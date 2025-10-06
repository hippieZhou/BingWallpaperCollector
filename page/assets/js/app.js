// 主应用程序类
class WallpaperApp {
  constructor() {
    this.currentView = "gallery";
    this.currentWallpapers = [];
    this.filters = {
      country: "",
      date: "",
      search: "",
    };

    this.init();
  }

  // 初始化应用程序
  async init() {
    this.showLoading();
    this.bindEvents();

    try {
      await this.loadData();
      await this.initializeFilters();
      this.showGalleryView();
      this.updateStats();
    } catch (error) {
      console.error("应用程序初始化失败:", error);
      this.showError("数据加载失败，请刷新页面重试。");
    } finally {
      this.hideLoading();
    }
  }

  // 绑定事件监听器
  bindEvents() {
    // 导航按钮
    document.querySelectorAll(".nav-btn").forEach((btn) => {
      btn.addEventListener("click", (e) => {
        const view = e.currentTarget.dataset.view;
        this.switchView(view);
      });
    });

    // 筛选器
    document
      .getElementById("country-filter")
      .addEventListener("change", (e) => {
        this.filters.country = e.target.value;
        this.applyFilters();
      });

    document.getElementById("date-filter").addEventListener("change", (e) => {
      this.filters.date = e.target.value;
      this.applyFilters();
    });

    document.getElementById("search-input").addEventListener("input", (e) => {
      this.filters.search = e.target.value;
      this.debounceSearch();
    });

    // 清除筛选器按钮
    this.addClearFiltersButton();

    // 筛选器控件在所有视图中都可见

    // 模态框
    document
      .getElementById("wallpaper-modal")
      .addEventListener("click", (e) => {
        if (e.target.id === "wallpaper-modal") {
          this.closeModal();
        }
      });

    document.querySelector(".modal-close").addEventListener("click", () => {
      this.closeModal();
    });

    // 下载按钮
    document.getElementById("download-btn").addEventListener("click", (e) => {
      this.toggleDownloadPanel();
    });

    // 复制链接和分享按钮
    document.getElementById("copy-link-btn").addEventListener("click", () => {
      this.copyCurrentWallpaperLink();
    });

    document.getElementById("share-btn").addEventListener("click", () => {
      this.shareCurrentWallpaper();
    });

    // ESC键关闭模态框
    document.addEventListener("keydown", (e) => {
      if (e.key === "Escape") {
        this.closeModal();
      }
    });
  }

  // 加载数据
  async loadData() {
    await window.dataLoader.loadAllData((progress) => {
      this.updateLoadingProgress(progress);
    });
  }

  // 更新加载进度
  updateLoadingProgress(progress) {
    const spinner = document.querySelector(".loading-spinner p");
    if (spinner) {
      spinner.textContent = `正在加载壁纸数据... ${progress.percentage}% (${progress.current})`;
    }
  }

  // 初始化筛选器
  async initializeFilters() {
    console.log("🔧 开始初始化筛选器...");
    console.log("📊 WALLPAPER_DATA_INDEX:", window.WALLPAPER_DATA_INDEX);
    
    const countryFilter = document.getElementById("country-filter");
    const dateFilter = document.getElementById("date-filter");

    // 填充国家筛选器 - 显示所有支持的国家
    const countryStats = window.dataLoader.getCountryStats();
    const countryInfo = window.dataLoader.getCountryInfo();

    // 遍历所有支持的国家，确保都显示在下拉列表中
    Object.entries(countryInfo).forEach(([country, info]) => {
      const option = document.createElement("option");
      option.value = country;

      // 获取统计数据（如果有的话）
      const stats = countryStats[country];
      const count = stats ? stats.count : 0;

      if (count > 0) {
        option.textContent = `${info.flag} ${info.name} (${count})`;
      } else {
        option.textContent = `${info.flag} ${info.name} (无数据)`;
        option.style.color = "#999";
      }

      countryFilter.appendChild(option);
    });

    // 填充日期筛选器 - 使用实际数据中可用的日期
    const actualDates = window.dataLoader.getActualDataDates();
    
    // 只显示最近的8个日期
    const recentDates = actualDates.slice(0, 8);
    
    recentDates.forEach((date) => {
      const option = document.createElement("option");
      option.value = date;
      option.textContent = this.formatDate(date);
      dateFilter.appendChild(option);
    });
  }

  // 格式化日期显示
  formatDate(dateString) {
    const date = new Date(dateString + "T00:00:00");
    return date.toLocaleDateString("zh-CN", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  }

  // 切换视图
  switchView(viewName) {
    // 更新导航按钮状态
    document.querySelectorAll(".nav-btn").forEach((btn) => {
      btn.classList.remove("active");
    });
    document.querySelector(`[data-view="${viewName}"]`).classList.add("active");

    // 隐藏所有视图
    document.querySelectorAll(".view").forEach((view) => {
      view.classList.remove("active");
    });

    // 显示目标视图
    document.getElementById(`${viewName}-view`).classList.add("active");

    this.currentView = viewName;

    // 筛选器控件在所有视图中都可见

    // 根据视图加载内容
    switch (viewName) {
      case "gallery":
        this.showGalleryView();
        break;
      case "countries":
        this.showCountriesView();
        break;
      case "timeline":
        this.showTimelineView(); // 异步方法，但不需要await
        break;
    }
  }

  // 显示画廊视图
  showGalleryView() {
    // 如果有筛选条件，应用筛选
    if (this.hasActiveFilters()) {
      this.applyFilters();
    } else {
      this.currentWallpapers = window.dataLoader.getLatestWallpapers();
      this.renderWallpaperGrid(this.currentWallpapers);
    }
  }

  // 显示国家视图
  showCountriesView() {
    // 如果有筛选条件，应用筛选
    if (this.hasActiveFilters()) {
      this.applyFilters();
    } else {
      const countryStats = window.dataLoader.getCountryStats();
      this.renderCountriesGrid(countryStats);
    }
  }

  // 显示时间轴视图
  async showTimelineView() {
    // 如果有筛选条件，应用筛选
    if (this.hasActiveFilters()) {
      this.applyFilters();
    } else {
      // 获取UI显示的日期范围和实际数据
      const uiDates = await window.dataLoader.getAvailableDates();
      const wallpapersByDate = window.dataLoader.getWallpapersByDate();

      // 创建包含所有UI日期的完整时间轴数据
      const completeTimeline = {};
      uiDates.forEach((date) => {
        completeTimeline[date] = wallpapersByDate[date] || [];
      });

      this.renderTimeline(completeTimeline);
    }
  }

  // 渲染壁纸网格
  renderWallpaperGrid(wallpapers) {
    const grid = document.getElementById("wallpaper-grid");
    grid.innerHTML = "";

    if (wallpapers.length === 0) {
      grid.innerHTML = '<div class="no-results">没有找到匹配的壁纸。</div>';
      return;
    }

    wallpapers.forEach((wallpaper, index) => {
      const card = this.createWallpaperCard(wallpaper);
      card.style.animationDelay = `${index * 0.1}s`;
      grid.appendChild(card);
    });
  }

  // 创建壁纸卡片
  createWallpaperCard(wallpaper) {
    const card = document.createElement("div");
    card.className = "wallpaper-card";
    card.addEventListener("click", () => this.openModal(wallpaper));

    card.innerHTML = `
            <div class="wallpaper-image">
                <img src="${wallpaper.thumbnailUrl}" alt="${wallpaper.title}" loading="lazy" referrerpolicy="no-referrer">
                <div class="image-overlay"></div>
            </div>
            <div class="wallpaper-info">
                <div class="wallpaper-meta">
                    <span class="country-tag">${wallpaper.countryInfo.flag} ${wallpaper.countryInfo.name}</span>
                    <span class="date-tag">${wallpaper.displayDate}</span>
                </div>
            </div>
        `;

    return card;
  }

  // 渲染国家网格
  renderCountriesGrid(countryStats) {
    const grid = document.getElementById("countries-grid");
    grid.innerHTML = "";

    const countryInfo = window.dataLoader.getCountryInfo();

    // 显示所有支持的国家
    Object.entries(countryInfo).forEach(([country, basicInfo]) => {
      const stats = countryStats[country];
      const count = stats ? stats.count : 0;
      const datesCount = stats ? stats.dates.length : 0;

      const card = document.createElement("div");
      card.className = "country-card";
      card.addEventListener("click", () => this.filterByCountry(country));

      // 根据是否有数据设置不同的样式
      if (count === 0) {
        card.style.opacity = "0.6";
        card.style.cursor = "pointer";
      }

      card.innerHTML = `
                <div class="country-flag">${basicInfo.flag}</div>
                <div class="country-name">${basicInfo.name}</div>
                <div class="country-code">${basicInfo.code}</div>
                <div class="country-stats">
                    <div class="stat">
                        <span class="stat-number" style="color: ${
                          count > 0 ? "#667eea" : "#ccc"
                        }">${count}</span>
                        <span class="stat-label">壁纸</span>
                    </div>
                    <div class="stat">
                        <span class="stat-number" style="color: ${
                          datesCount > 0 ? "#667eea" : "#ccc"
                        }">${datesCount}</span>
                        <span class="stat-label">天数</span>
                    </div>
                </div>
                ${
                  count === 0
                    ? '<div style="text-align: center; margin-top: 1rem; font-size: 0.8rem; color: #999;">暂无数据</div>'
                    : ""
                }
            `;

      grid.appendChild(card);
    });
  }

  // 渲染时间轴
  renderTimeline(wallpapersByDate) {
    const container = document.getElementById("timeline-container");
    container.innerHTML = "";

    Object.entries(wallpapersByDate).forEach(([date, wallpapers]) => {
      const dateSection = document.createElement("div");
      dateSection.innerHTML = `
                <div class="timeline-date">${this.formatDate(date)} ${
        wallpapers.length > 0 ? `(${wallpapers.length} 张)` : "(无数据)"
      }</div>
                <div class="timeline-wallpapers" id="timeline-${date}"></div>
            `;
      container.appendChild(dateSection);

      const wallpapersGrid = dateSection.querySelector(".timeline-wallpapers");

      if (wallpapers.length > 0) {
        wallpapers.forEach((wallpaper) => {
          const card = this.createWallpaperCard(wallpaper);
          wallpapersGrid.appendChild(card);
        });
      } else {
        // 显示无数据提示
        const noDataCard = document.createElement("div");
        noDataCard.className = "no-data-message";
        noDataCard.style.cssText = `
                    text-align: center;
                    padding: 2rem;
                    color: #999;
                    background: #f8f9fa;
                    border-radius: 8px;
                    border: 2px dashed #dee2e6;
                `;
        noDataCard.innerHTML = `
                    <i class="fas fa-calendar-times" style="font-size: 2rem; margin-bottom: 1rem; color: #ccc;"></i>
                    <p>这一天暂无壁纸数据</p>
                    <p style="font-size: 0.8rem;">数据可能还未收集或正在更新中</p>
                `;
        wallpapersGrid.appendChild(noDataCard);
      }
    });
  }

  // 应用筛选器 (支持所有视图)
  applyFilters() {
    const results = window.dataLoader.searchWallpapers(this.filters.search, {
      country: this.filters.country,
      date: this.filters.date,
    });

    this.currentWallpapers = results;

    // 根据当前视图渲染筛选后的结果
    switch (this.currentView) {
      case "gallery":
        this.renderWallpaperGrid(results);
        // 如果没有结果，显示特殊提示
        if (results.length === 0) {
          if (this.filters.date && this.filters.country) {
            this.showNoDataForCountryAndDate(
              this.filters.country,
              this.filters.date
            );
          } else if (this.filters.date) {
            this.showNoDataForDate(this.filters.date);
          } else if (this.filters.country) {
            this.showNoDataForCountry(this.filters.country);
          }
        }
        break;
      case "countries":
        this.renderFilteredCountriesView(results);
        break;
      case "timeline":
        this.renderFilteredTimelineView(results);
        break;
    }

    this.updateStats(results.length);
    this.updateClearFiltersButton();
  }

  // 渲染筛选后的国家视图
  renderFilteredCountriesView(filteredWallpapers) {
    // 从筛选后的壁纸中统计国家信息
    const countryStats = {};

    filteredWallpapers.forEach((wallpaper) => {
      if (!countryStats[wallpaper.country]) {
        countryStats[wallpaper.country] = {
          count: 0,
          dates: new Set(),
        };
      }
      countryStats[wallpaper.country].count++;
      countryStats[wallpaper.country].dates.add(wallpaper.date);
    });

    // 转换为渲染格式
    const filteredCountryStats = Object.entries(countryStats).map(
      ([country, stats]) => [country, stats.count, stats.dates.size]
    );

    this.renderCountriesGrid(filteredCountryStats);
  }

  // 渲染筛选后的时间轴视图
  renderFilteredTimelineView(filteredWallpapers) {
    // 按日期分组筛选后的壁纸
    const filteredWallpapersByDate = {};

    filteredWallpapers.forEach((wallpaper) => {
      if (!filteredWallpapersByDate[wallpaper.date]) {
        filteredWallpapersByDate[wallpaper.date] = [];
      }
      filteredWallpapersByDate[wallpaper.date].push(wallpaper);
    });

    // 如果有日期筛选，只显示筛选的日期
    if (this.filters.date) {
      const filteredTimeline = {};
      filteredTimeline[this.filters.date] =
        filteredWallpapersByDate[this.filters.date] || [];
      this.renderTimeline(filteredTimeline);
    } else {
      // 没有日期筛选时，显示所有有数据的日期（按时间排序）
      const sortedDates = Object.keys(filteredWallpapersByDate).sort(
        (a, b) => new Date(b) - new Date(a)
      );
      const sortedTimeline = {};
      sortedDates.forEach((date) => {
        sortedTimeline[date] = filteredWallpapersByDate[date];
      });
      this.renderTimeline(sortedTimeline);
    }
  }

  // 添加清除筛选器按钮
  addClearFiltersButton() {
    const filterGroup = document.querySelector(".filter-group");
    if (filterGroup && !document.getElementById("clear-filters-btn")) {
      const clearButton = document.createElement("button");
      clearButton.id = "clear-filters-btn";
      clearButton.className = "clear-filters-btn";
      clearButton.innerHTML = '<i class="fas fa-times"></i> 清除筛选';
      clearButton.style.cssText = `
                background: #e74c3c;
                color: white;
                border: none;
                padding: 8px 12px;
                border-radius: 4px;
                cursor: pointer;
                font-size: 0.9rem;
                margin-left: auto;
                display: none;
                align-items: center;
                gap: 5px;
            `;
      clearButton.addEventListener("click", () => this.clearAllFilters());
      filterGroup.appendChild(clearButton);
    }
  }

  // 更新清除筛选器按钮的显示状态
  updateClearFiltersButton() {
    const clearButton = document.getElementById("clear-filters-btn");
    if (clearButton) {
      clearButton.style.display = this.hasActiveFilters() ? "flex" : "none";
    }
  }

  // 清除所有筛选器
  clearAllFilters() {
    // 重置筛选器状态
    this.filters.country = "";
    this.filters.date = "";
    this.filters.search = "";

    // 重置UI控件
    document.getElementById("country-filter").value = "";
    document.getElementById("date-filter").value = "";
    document.getElementById("search-input").value = "";

    // 根据当前视图重新加载数据
    switch (this.currentView) {
      case "gallery":
        this.showGalleryView();
        break;
      case "countries":
        this.showCountriesView();
        break;
      case "timeline":
        this.showTimelineView();
        break;
    }

    // 确保筛选器控件状态正确
    this.toggleFilterControls(this.currentView);

    // 更新统计和按钮状态
    this.updateStats();
    this.updateClearFiltersButton();
  }

  // 检查是否有激活的筛选器
  hasActiveFilters() {
    return this.filters.country || this.filters.date || this.filters.search;
  }

  // 显示指定日期无数据的提示
  showNoDataForDate(date) {
    const grid = document.getElementById("wallpaper-grid");

    const noDataMessage = document.createElement("div");
    noDataMessage.className = "no-data-message";
    noDataMessage.style.cssText = `
            text-align: center;
            padding: 4rem;
            color: #666;
            background: white;
            border-radius: 16px;
            box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
            grid-column: 1 / -1;
        `;

    const formattedDate = this.formatDate(date);
    noDataMessage.innerHTML = `
            <i class="fas fa-calendar-times" style="font-size: 4rem; margin-bottom: 2rem; color: #ddd;"></i>
            <h3 style="margin-bottom: 1rem; color: #333;">${formattedDate} 暂无数据</h3>
            <p style="margin-bottom: 2rem; color: #666;">
                这一天的壁纸信息可能还未收集，或者此日期超出了数据收集范围。
            </p>
            <div style="background: #f8f9fa; padding: 1rem; border-radius: 8px; margin: 0 auto; max-width: 400px;">
                <p style="font-size: 0.9rem; color: #666; margin: 0;">
                    <i class="fas fa-info-circle"></i> 
                    数据通常会在每天自动收集，请稍后再试或选择其他日期。
                </p>
            </div>
        `;

    grid.appendChild(noDataMessage);
  }

  // 显示指定国家无数据的提示
  showNoDataForCountry(country) {
    const grid = document.getElementById("wallpaper-grid");

    const countryInfo = window.dataLoader.getCountryInfo()[country];
    const noDataMessage = document.createElement("div");
    noDataMessage.className = "no-data-message";
    noDataMessage.style.cssText = `
            text-align: center;
            padding: 4rem;
            color: #666;
            background: white;
            border-radius: 16px;
            box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
            grid-column: 1 / -1;
        `;

    noDataMessage.innerHTML = `
            <div style="font-size: 4rem; margin-bottom: 2rem;">${countryInfo.flag}</div>
            <h3 style="margin-bottom: 1rem; color: #333;">${countryInfo.name} 暂无数据</h3>
            <p style="margin-bottom: 2rem; color: #666;">
                ${countryInfo.name} (${countryInfo.code}) 的壁纸信息可能还未收集，或者正在更新中。
            </p>
            <div style="background: #f8f9fa; padding: 1rem; border-radius: 8px; margin: 0 auto; max-width: 400px;">
                <p style="font-size: 0.9rem; color: #666; margin: 0;">
                    <i class="fas fa-info-circle"></i> 
                    我们支持 14 个国家/地区的壁纸收集，数据会定期更新。
                </p>
            </div>
        `;

    grid.appendChild(noDataMessage);
  }

  // 显示指定国家和日期无数据的提示
  showNoDataForCountryAndDate(country, date) {
    const grid = document.getElementById("wallpaper-grid");

    const countryInfo = window.dataLoader.getCountryInfo()[country];
    const formattedDate = this.formatDate(date);
    const noDataMessage = document.createElement("div");
    noDataMessage.className = "no-data-message";
    noDataMessage.style.cssText = `
            text-align: center;
            padding: 4rem;
            color: #666;
            background: white;
            border-radius: 16px;
            box-shadow: 0 8px 30px rgba(0, 0, 0, 0.12);
            grid-column: 1 / -1;
        `;

    noDataMessage.innerHTML = `
            <div style="display: flex; justify-content: center; align-items: center; gap: 1rem; margin-bottom: 2rem;">
                <div style="font-size: 3rem;">${countryInfo.flag}</div>
                <i class="fas fa-calendar-times" style="font-size: 3rem; color: #ddd;"></i>
            </div>
            <h3 style="margin-bottom: 1rem; color: #333;">${countryInfo.name} - ${formattedDate}</h3>
            <p style="margin-bottom: 2rem; color: #666;">
                ${formattedDate} 这天还没有来自 ${countryInfo.name} 的壁纸数据。
            </p>
            <div style="background: #f8f9fa; padding: 1rem; border-radius: 8px; margin: 0 auto; max-width: 400px;">
                <p style="font-size: 0.9rem; color: #666; margin: 0;">
                    <i class="fas fa-lightbulb"></i> 
                    尝试选择其他日期或国家，或稍后再来查看更新。
                </p>
            </div>
        `;

    grid.appendChild(noDataMessage);
  }

  // 搜索防抖
  debounceSearch() {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.applyFilters();
    }, 300);
  }

  // 根据国家筛选
  filterByCountry(country) {
    document.getElementById("country-filter").value = country;
    this.filters.country = country;
    this.switchView("gallery");
    this.applyFilters();
  }

  // 打开模态框
  openModal(wallpaper) {
    const modal = document.getElementById("wallpaper-modal");
    const title = document.getElementById("modal-title");
    const country = document.getElementById("modal-country");
    const date = document.getElementById("modal-date");
    const image = document.getElementById("modal-image");
    const description = document.getElementById("modal-description");
    const copyright = document.getElementById("modal-copyright");
    const resolutionButtons = document.getElementById("resolution-buttons");

    if (title) {
      title.textContent = wallpaper.title;
    }
    if (country) {
      country.textContent = `${wallpaper.countryInfo.flag} ${wallpaper.countryInfo.name}`;
      country.className = "country-tag";
    }
    if (date) {
      date.textContent = wallpaper.displayDate;
      date.className = "date-tag";
    }
    if (image) {
      image.src = wallpaper.fullImageUrl;
      image.alt = wallpaper.title;
    }
    if (description) {
      description.textContent = wallpaper.description;
    }
    if (copyright) {
      copyright.innerHTML = `© ${wallpaper.copyright}`;
    }

    // 设置主下载按钮
    this.setupDownloadButton(wallpaper);

    // 生成分辨率选项面板
    this.setupDownloadPanel(wallpaper);

    // 存储当前壁纸用于分享
    this.currentModalWallpaper = wallpaper;

    modal.classList.add("active");
    document.body.style.overflow = "hidden";
  }

  // 关闭模态框
  closeModal() {
    const modal = document.getElementById("wallpaper-modal");
    modal.classList.remove("active");
    document.body.style.overflow = "";

    // 隐藏下载面板
    this.hideDownloadPanel();

    this.currentModalWallpaper = null;
  }

  // 复制当前壁纸链接
  copyCurrentWallpaperLink() {
    if (!this.currentModalWallpaper) return;

    const url = this.currentModalWallpaper.fullImageUrl;
    navigator.clipboard
      .writeText(url)
      .then(() => {
        this.showToast("链接已复制到剪贴板");
      })
      .catch(() => {
        this.showToast("复制失败，请手动复制");
      });
  }

  // 分享当前壁纸
  shareCurrentWallpaper() {
    if (!this.currentModalWallpaper) return;

    const wallpaper = this.currentModalWallpaper;
    const shareData = {
      title: `${wallpaper.title} - 必应壁纸`,
      text: `${wallpaper.description} (${wallpaper.countryInfo.name})`,
      url: wallpaper.fullImageUrl,
    };

    if (navigator.share) {
      navigator.share(shareData).catch(console.error);
    } else {
      // 回退到复制链接
      this.copyCurrentWallpaperLink();
    }
  }

  // 设置主下载按钮
  setupDownloadButton(wallpaper) {
    const downloadBtn = document.getElementById("download-btn");
    const resolutionSpan = downloadBtn
      ? downloadBtn.querySelector(".btn-resolution")
      : null;

    if (wallpaper.imageResolutions && wallpaper.imageResolutions.length > 0) {
      // 找到最高分辨率（优先UHD 4K）
      const bestResolution = this.getBestResolution(wallpaper.imageResolutions);
      const is4K = bestResolution.resolution === "UHD";

      // 更新按钮显示的分辨率
      if (resolutionSpan) {
        resolutionSpan.textContent = is4K ? "4K" : bestResolution.resolution;
      }

      // 存储默认下载的分辨率
      if (downloadBtn) {
        downloadBtn.setAttribute(
          "data-default-resolution",
          JSON.stringify(bestResolution)
        );
      }
    }
  }

  // 设置下载面板
  setupDownloadPanel(wallpaper) {
    const resolutionOptions = document.getElementById("resolution-options");
    resolutionOptions.innerHTML = "";

    if (wallpaper.imageResolutions) {
      // 按分辨率重要性排序（4K优先）
      const sortedResolutions = [...wallpaper.imageResolutions].sort((a, b) => {
        const priorities = { UHD: 1, "Full HD": 2, HD: 3, Standard: 4 };
        return (
          (priorities[a.resolution] || 5) - (priorities[b.resolution] || 5)
        );
      });

      sortedResolutions.forEach((resolution) => {
        const option = document.createElement("div");
        const is4K = resolution.resolution === "UHD";

        option.className = "resolution-option";
        if (is4K) {
          option.classList.add("is-4k");
        }

        option.innerHTML = `
                    <div class="resolution-info">
                        <div class="resolution-name">
                            ${resolution.resolution}
                            ${
                              is4K
                                ? '<span class="resolution-badge">4K</span>'
                                : ""
                            }
                        </div>
                        <div class="resolution-size">${resolution.size}</div>
                    </div>
                    <i class="fas fa-download download-icon"></i>
                `;

        // 绑定点击下载事件
        option.addEventListener("click", () => {
          this.downloadWallpaper(resolution, wallpaper);
          this.hideDownloadPanel();
        });

        resolutionOptions.appendChild(option);
      });
    }
  }

  // 获取最佳分辨率（优先4K）
  getBestResolution(resolutions) {
    const priorities = { UHD: 1, "Full HD": 2, HD: 3, Standard: 4 };
    return resolutions.sort(
      (a, b) =>
        (priorities[a.resolution] || 5) - (priorities[b.resolution] || 5)
    )[0];
  }

  // 切换下载面板显示
  toggleDownloadPanel() {
    const panel = document.getElementById("download-panel");

    if (panel.classList.contains("hidden")) {
      this.showDownloadPanel();
    } else {
      this.hideDownloadPanel();
    }
  }

  // 显示下载面板
  showDownloadPanel() {
    const panel = document.getElementById("download-panel");
    const downloadBtn = document.getElementById("download-btn");

    panel.classList.remove("hidden");
    panel.classList.add("show");

    // 更新按钮状态
    const btnText = downloadBtn.querySelector(".btn-text");
    const btnIcon = downloadBtn.querySelector(".fas");

    if (btnText) {
      btnText.textContent = "选择分辨率";
    }
    if (btnIcon) {
      btnIcon.className = "fas fa-chevron-up";
    }

    // 如果有默认分辨率，也添加快速下载选项
    const defaultResolution = downloadBtn.getAttribute(
      "data-default-resolution"
    );
    if (defaultResolution && this.currentModalWallpaper) {
      const resolution = JSON.parse(defaultResolution);
      const quickDownload = document.createElement("div");
      quickDownload.className = "quick-download";
      quickDownload.innerHTML = `
                <button class="resolution-option quick-download-btn">
                    <div class="resolution-info">
                        <div class="resolution-name">⚡ 快速下载 ${resolution.resolution}</div>
                        <div class="resolution-size">推荐分辨率</div>
                    </div>
                    <i class="fas fa-bolt download-icon"></i>
                </button>
            `;

      quickDownload.querySelector("button").addEventListener("click", () => {
        this.downloadWallpaper(resolution, this.currentModalWallpaper);
        this.hideDownloadPanel();
      });

      panel.querySelector("#resolution-options").prepend(quickDownload);
    }
  }

  // 隐藏下载面板
  hideDownloadPanel() {
    const panel = document.getElementById("download-panel");
    const downloadBtn = document.getElementById("download-btn");

    panel.classList.add("hidden");
    panel.classList.remove("show");

    // 恢复按钮状态
    const btnText = downloadBtn.querySelector(".btn-text");
    const btnIcon = downloadBtn.querySelector(".fas");

    if (btnText) {
      btnText.textContent = "下载壁纸";
    }
    if (btnIcon) {
      btnIcon.className = "fas fa-download";
    }

    // 清除快速下载选项
    const quickDownload = panel.querySelector(".quick-download");
    if (quickDownload) {
      quickDownload.remove();
    }
  }

  // 下载壁纸
  async downloadWallpaper(resolution, wallpaper) {
    const button = event.currentTarget;
    const downloadText = button.querySelector(".download-text");
    const downloadStatus = button.querySelector(".download-status");
    const downloadIcon = button.querySelector(".fas");

    try {
      // 更新按钮状态
      button.disabled = true;
      button.classList.add("downloading");

      if (downloadIcon) {
        downloadIcon.className = "fas fa-spinner fa-spin";
      }
      if (downloadText) {
        downloadText.textContent = "下载中...";
      }
      if (downloadStatus) {
        downloadStatus.textContent = "";
      }

      // 生成文件名
      const fileName = this.generateFileName(wallpaper, resolution);

      // 显示下载进度提示
      this.showToast(`开始下载 ${resolution.resolution} 分辨率壁纸...`);

      // 使用 fetch 下载图片
      const response = await fetch(resolution.url);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }

      // 获取图片数据
      const blob = await response.blob();

      // 创建下载链接
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.style.display = "none";
      a.href = url;
      a.download = fileName;

      // 触发下载
      document.body.appendChild(a);
      a.click();

      // 清理
      setTimeout(() => {
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      }, 100);

      // 成功状态
      if (downloadIcon) {
        downloadIcon.className = "fas fa-check";
      }
      if (downloadText) {
        downloadText.textContent = "下载完成";
      }
      if (downloadStatus) {
        downloadStatus.textContent = "✓";
      }
      button.classList.remove("downloading");
      button.classList.add("download-success");

      // 显示成功消息
      const is4K = resolution.resolution === "UHD";
      this.showToast(
        `${is4K ? "4K " : ""}壁纸下载完成！(${resolution.size})`,
        "success"
      );

      // 重置按钮状态
      setTimeout(() => {
        button.disabled = false;
        if (downloadIcon) {
          downloadIcon.className = "fas fa-download";
        }
        if (downloadText) {
          downloadText.textContent = "下载";
        }
        if (downloadStatus) {
          downloadStatus.textContent = "";
        }
        button.classList.remove("download-success");
      }, 3000);
    } catch (error) {
      console.error("下载失败:", error);

      // 错误状态
      if (downloadIcon) {
        downloadIcon.className = "fas fa-exclamation-triangle";
      }
      if (downloadText) {
        downloadText.textContent = "下载失败";
      }
      if (downloadStatus) {
        downloadStatus.textContent = "!";
      }
      button.classList.remove("downloading");
      button.classList.add("download-error");

      // 显示错误消息
      this.showToast("下载失败，请稍后重试", "error");

      // 重置按钮状态
      setTimeout(() => {
        button.disabled = false;
        if (downloadIcon) {
          downloadIcon.className = "fas fa-download";
        }
        if (downloadText) {
          downloadText.textContent = "下载";
        }
        if (downloadStatus) {
          downloadStatus.textContent = "";
        }
        button.classList.remove("download-error");
      }, 3000);
    }
  }

  // 生成下载文件名
  generateFileName(wallpaper, resolution) {
    // 清理标题中的特殊字符
    const cleanTitle = wallpaper.title
      .replace(/[<>:"/\\|?*]/g, "") // 移除文件名不允许的字符
      .replace(/\s+/g, "-") // 空格替换为短横线
      .substring(0, 50); // 限制长度

    // 格式化日期
    const date = wallpaper.date || new Date().toISOString().split("T")[0];

    // 分辨率标识
    const resolutionTag =
      resolution.resolution === "UHD" ? "4K-UHD" : resolution.resolution;

    // 构建文件名
    return `Bing-${cleanTitle}-${date}-${resolutionTag}.jpg`;
  }

  // 更新统计信息
  updateStats(filteredCount = null) {
    const totalCount = document.getElementById("total-count");
    const countryCount = document.getElementById("country-count");
    const lastUpdate = document.getElementById("last-update");

    const displayCount =
      filteredCount !== null
        ? filteredCount
        : window.dataLoader.wallpapers.length;

    if (totalCount) {
      totalCount.textContent = displayCount;
    }
    if (countryCount) {
      countryCount.textContent = window.dataLoader.countries.length;
    }

    // 设置最后更新时间
    if (lastUpdate && window.dataLoader.wallpapers.length > 0) {
      const latestDate = Math.max(
        ...window.dataLoader.wallpapers.map((w) => new Date(w.date))
      );
      lastUpdate.textContent = new Date(latestDate).toLocaleDateString("zh-CN");
    }
  }

  // 显示加载状态
  showLoading() {
    document.getElementById("loading").classList.remove("hidden");
  }

  // 隐藏加载状态
  hideLoading() {
    document.getElementById("loading").classList.add("hidden");
  }

  // 显示错误信息
  showError(message) {
    const loading = document.getElementById("loading");
    loading.innerHTML = `
            <div class="loading-spinner">
                <i class="fas fa-exclamation-triangle" style="color: #e74c3c;"></i>
                <p style="color: #e74c3c;">${message}</p>
                <button onclick="location.reload()" style="margin-top: 1rem; padding: 0.5rem 1rem; border: none; border-radius: 5px; background: #667eea; color: white; cursor: pointer;">重新加载</button>
            </div>
        `;
  }

  // 刷新当前视图（用于缓存清除后重新加载）
  refreshCurrentView() {
    console.log("🔄 刷新当前视图:", this.currentView);

    // 根据当前视图重新加载内容
    switch (this.currentView) {
      case "gallery":
        this.showGalleryView();
        break;
      case "countries":
        this.showCountriesView();
        break;
      case "timeline":
        this.showTimelineView();
        break;
    }

    // 显示刷新提示
    this.showToast("页面内容已刷新，图片缓存已清除", "success");
  }

  // 显示提示消息
  showToast(message, type = "info") {
    // 创建或获取toast容器
    let toastContainer = document.getElementById("toast-container");
    if (!toastContainer) {
      toastContainer = document.createElement("div");
      toastContainer.id = "toast-container";
      toastContainer.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 10001;
                display: flex;
                flex-direction: column;
                gap: 10px;
                max-width: 350px;
            `;
      document.body.appendChild(toastContainer);
    }

    // 根据类型设置样式和图标
    const typeStyles = {
      info: {
        bg: "rgba(0, 0, 0, 0.8)",
        icon: "fas fa-info-circle",
        color: "white",
      },
      success: {
        bg: "rgba(72, 187, 120, 0.9)",
        icon: "fas fa-check-circle",
        color: "white",
      },
      error: {
        bg: "rgba(245, 101, 101, 0.9)",
        icon: "fas fa-exclamation-circle",
        color: "white",
      },
      warning: {
        bg: "rgba(236, 201, 75, 0.9)",
        icon: "fas fa-exclamation-triangle",
        color: "white",
      },
    };

    const style = typeStyles[type] || typeStyles.info;

    // 创建toast元素
    const toast = document.createElement("div");
    toast.className = `toast toast-${type}`;
    toast.style.cssText = `
            background: ${style.bg};
            color: ${style.color};
            padding: 1rem 1.5rem;
            border-radius: 8px;
            font-size: 0.9rem;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            display: flex;
            align-items: center;
            gap: 0.75rem;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            backdrop-filter: blur(10px);
        `;

    toast.innerHTML = `
            <i class="${style.icon}" style="font-size: 1.1rem; flex-shrink: 0;"></i>
            <span style="flex: 1;">${message}</span>
        `;

    toastContainer.appendChild(toast);

    // 动画显示
    setTimeout(() => {
      toast.style.transform = "translateX(0)";
    }, 100);

    // 根据类型设置不同的显示时间
    const duration = type === "error" ? 5000 : type === "success" ? 4000 : 3000;

    // 自动隐藏
    setTimeout(() => {
      toast.style.transform = "translateX(100%)";
      setTimeout(() => {
        if (toast.parentNode) {
          toast.parentNode.removeChild(toast);
        }
      }, 300);
    }, duration);
  }
}

// 当DOM加载完成后初始化应用程序
document.addEventListener("DOMContentLoaded", () => {
  window.app = new WallpaperApp();
});

// 全局错误处理
window.addEventListener("error", (e) => {
  console.error("全局错误:", e.error);
});

window.addEventListener("unhandledrejection", (e) => {
  console.error("未处理的Promise拒绝:", e.reason);
});
