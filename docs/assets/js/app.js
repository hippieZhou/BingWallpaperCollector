// 主应用程序类
class WallpaperApp {
    constructor() {
        this.currentView = 'gallery';
        this.currentWallpapers = [];
        this.filters = {
            country: '',
            date: '',
            search: ''
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
            console.error('应用程序初始化失败:', error);
            this.showError('数据加载失败，请刷新页面重试。');
        } finally {
            this.hideLoading();
        }
    }

    // 绑定事件监听器
    bindEvents() {
        // 导航按钮
        document.querySelectorAll('.nav-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                const view = e.currentTarget.dataset.view;
                this.switchView(view);
            });
        });

        // 筛选器
        document.getElementById('country-filter').addEventListener('change', (e) => {
            this.filters.country = e.target.value;
            this.applyFilters();
        });

        document.getElementById('date-filter').addEventListener('change', (e) => {
            this.filters.date = e.target.value;
            this.applyFilters();
        });

        document.getElementById('search-input').addEventListener('input', (e) => {
            this.filters.search = e.target.value;
            this.debounceSearch();
        });

        // 模态框
        document.getElementById('wallpaper-modal').addEventListener('click', (e) => {
            if (e.target.id === 'wallpaper-modal') {
                this.closeModal();
            }
        });

        document.querySelector('.modal-close').addEventListener('click', () => {
            this.closeModal();
        });

        // 复制链接和分享按钮
        document.getElementById('copy-link-btn').addEventListener('click', () => {
            this.copyCurrentWallpaperLink();
        });

        document.getElementById('share-btn').addEventListener('click', () => {
            this.shareCurrentWallpaper();
        });

        // ESC键关闭模态框
        document.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
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
        const spinner = document.querySelector('.loading-spinner p');
        if (spinner) {
            spinner.textContent = `正在加载壁纸数据... ${progress.percentage}% (${progress.current})`;
        }
    }

    // 初始化筛选器
    async initializeFilters() {
        const countryFilter = document.getElementById('country-filter');
        const dateFilter = document.getElementById('date-filter');

        // 填充国家筛选器 - 显示所有支持的国家
        const countryStats = window.dataLoader.getCountryStats();
        const countryInfo = window.dataLoader.getCountryInfo();
        
        // 遍历所有支持的国家，确保都显示在下拉列表中
        Object.entries(countryInfo).forEach(([country, info]) => {
            const option = document.createElement('option');
            option.value = country;
            
            // 获取统计数据（如果有的话）
            const stats = countryStats[country];
            const count = stats ? stats.count : 0;
            
            if (count > 0) {
                option.textContent = `${info.flag} ${info.name} (${count})`;
            } else {
                option.textContent = `${info.flag} ${info.name} (无数据)`;
                option.style.color = '#999';
            }
            
            countryFilter.appendChild(option);
        });

        // 填充日期筛选器 - 使用UI显示的8天范围
        const uiDates = await window.dataLoader.getAvailableDates();
        uiDates.forEach(date => {
            const option = document.createElement('option');
            option.value = date;
            
            // 检查这个日期是否有实际数据
            const hasData = window.dataLoader.wallpapers.some(w => w.date === date);
            const dateText = this.formatDate(date);
            
            if (hasData) {
                option.textContent = dateText;
            } else {
                option.textContent = `${dateText} (无数据)`;
                option.style.color = '#999';
            }
            
            dateFilter.appendChild(option);
        });
    }

    // 格式化日期显示
    formatDate(dateString) {
        const date = new Date(dateString + 'T00:00:00');
        return date.toLocaleDateString('zh-CN', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }

    // 切换视图
    switchView(viewName) {
        // 更新导航按钮状态
        document.querySelectorAll('.nav-btn').forEach(btn => {
            btn.classList.remove('active');
        });
        document.querySelector(`[data-view="${viewName}"]`).classList.add('active');

        // 隐藏所有视图
        document.querySelectorAll('.view').forEach(view => {
            view.classList.remove('active');
        });

        // 显示目标视图
        document.getElementById(`${viewName}-view`).classList.add('active');

        this.currentView = viewName;

        // 根据视图加载内容
        switch (viewName) {
            case 'gallery':
                this.showGalleryView();
                break;
            case 'countries':
                this.showCountriesView();
                break;
            case 'timeline':
                this.showTimelineView(); // 异步方法，但不需要await
                break;
        }
    }

    // 显示画廊视图
    showGalleryView() {
        this.currentWallpapers = window.dataLoader.getLatestWallpapers();
        this.renderWallpaperGrid(this.currentWallpapers);
    }

    // 显示国家视图
    showCountriesView() {
        const countryStats = window.dataLoader.getCountryStats();
        this.renderCountriesGrid(countryStats);
    }

    // 显示时间轴视图
    async showTimelineView() {
        // 获取UI显示的日期范围和实际数据
        const uiDates = await window.dataLoader.getAvailableDates();
        const wallpapersByDate = window.dataLoader.getWallpapersByDate();
        
        // 创建包含所有UI日期的完整时间轴数据
        const completeTimeline = {};
        uiDates.forEach(date => {
            completeTimeline[date] = wallpapersByDate[date] || [];
        });
        
        this.renderTimeline(completeTimeline);
    }

    // 渲染壁纸网格
    renderWallpaperGrid(wallpapers) {
        const grid = document.getElementById('wallpaper-grid');
        grid.innerHTML = '';

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
        const card = document.createElement('div');
        card.className = 'wallpaper-card';
        card.addEventListener('click', () => this.openModal(wallpaper));

        card.innerHTML = `
            <div class="wallpaper-image">
                <img src="${wallpaper.thumbnailUrl}" alt="${wallpaper.title}" loading="lazy">
                <div class="image-overlay"></div>
            </div>
            <div class="wallpaper-info">
                <h3 class="wallpaper-title">${wallpaper.title}</h3>
                <p class="wallpaper-description">${wallpaper.description}</p>
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
        const grid = document.getElementById('countries-grid');
        grid.innerHTML = '';

        const countryInfo = window.dataLoader.getCountryInfo();

        // 显示所有支持的国家
        Object.entries(countryInfo).forEach(([country, basicInfo]) => {
            const stats = countryStats[country];
            const count = stats ? stats.count : 0;
            const datesCount = stats ? stats.dates.length : 0;

            const card = document.createElement('div');
            card.className = 'country-card';
            card.addEventListener('click', () => this.filterByCountry(country));

            // 根据是否有数据设置不同的样式
            if (count === 0) {
                card.style.opacity = '0.6';
                card.style.cursor = 'pointer';
            }

            card.innerHTML = `
                <div class="country-flag">${basicInfo.flag}</div>
                <div class="country-name">${basicInfo.name}</div>
                <div class="country-code">${basicInfo.code}</div>
                <div class="country-stats">
                    <div class="stat">
                        <span class="stat-number" style="color: ${count > 0 ? '#667eea' : '#ccc'}">${count}</span>
                        <span class="stat-label">壁纸</span>
                    </div>
                    <div class="stat">
                        <span class="stat-number" style="color: ${datesCount > 0 ? '#667eea' : '#ccc'}">${datesCount}</span>
                        <span class="stat-label">天数</span>
                    </div>
                </div>
                ${count === 0 ? '<div style="text-align: center; margin-top: 1rem; font-size: 0.8rem; color: #999;">暂无数据</div>' : ''}
            `;

            grid.appendChild(card);
        });
    }

    // 渲染时间轴
    renderTimeline(wallpapersByDate) {
        const container = document.getElementById('timeline-container');
        container.innerHTML = '';

        Object.entries(wallpapersByDate).forEach(([date, wallpapers]) => {
            const dateSection = document.createElement('div');
            dateSection.innerHTML = `
                <div class="timeline-date">${this.formatDate(date)} ${wallpapers.length > 0 ? `(${wallpapers.length} 张)` : '(无数据)'}</div>
                <div class="timeline-wallpapers" id="timeline-${date}"></div>
            `;
            container.appendChild(dateSection);

            const wallpapersGrid = dateSection.querySelector('.timeline-wallpapers');
            
            if (wallpapers.length > 0) {
                wallpapers.forEach(wallpaper => {
                    const card = this.createWallpaperCard(wallpaper);
                    wallpapersGrid.appendChild(card);
                });
            } else {
                // 显示无数据提示
                const noDataCard = document.createElement('div');
                noDataCard.className = 'no-data-message';
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

    // 应用筛选器
    applyFilters() {
        const results = window.dataLoader.searchWallpapers(this.filters.search, {
            country: this.filters.country,
            date: this.filters.date
        });

        this.currentWallpapers = results;
        
        if (this.currentView === 'gallery') {
            this.renderWallpaperGrid(results);
            
            // 如果没有结果，显示特殊提示
            if (results.length === 0) {
                if (this.filters.date && this.filters.country) {
                    this.showNoDataForCountryAndDate(this.filters.country, this.filters.date);
                } else if (this.filters.date) {
                    this.showNoDataForDate(this.filters.date);
                } else if (this.filters.country) {
                    this.showNoDataForCountry(this.filters.country);
                }
            }
        }

        this.updateStats(results.length);
    }

    // 显示指定日期无数据的提示
    showNoDataForDate(date) {
        const grid = document.getElementById('wallpaper-grid');
        
        const noDataMessage = document.createElement('div');
        noDataMessage.className = 'no-data-message';
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
        const grid = document.getElementById('wallpaper-grid');
        
        const countryInfo = window.dataLoader.getCountryInfo()[country];
        const noDataMessage = document.createElement('div');
        noDataMessage.className = 'no-data-message';
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
        const grid = document.getElementById('wallpaper-grid');
        
        const countryInfo = window.dataLoader.getCountryInfo()[country];
        const formattedDate = this.formatDate(date);
        const noDataMessage = document.createElement('div');
        noDataMessage.className = 'no-data-message';
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
        document.getElementById('country-filter').value = country;
        this.filters.country = country;
        this.switchView('gallery');
        this.applyFilters();
    }

    // 打开模态框
    openModal(wallpaper) {
        const modal = document.getElementById('wallpaper-modal');
        const title = document.getElementById('modal-title');
        const country = document.getElementById('modal-country');
        const date = document.getElementById('modal-date');
        const image = document.getElementById('modal-image');
        const description = document.getElementById('modal-description');
        const copyright = document.getElementById('modal-copyright');
        const resolutionButtons = document.getElementById('resolution-buttons');

        title.textContent = wallpaper.title;
        country.textContent = `${wallpaper.countryInfo.flag} ${wallpaper.countryInfo.name}`;
        country.className = 'country-tag';
        date.textContent = wallpaper.displayDate;
        date.className = 'date-tag';
        image.src = wallpaper.fullImageUrl;
        image.alt = wallpaper.title;
        description.textContent = wallpaper.description;
        copyright.innerHTML = `© ${wallpaper.copyright}`;

        // 渲染分辨率按钮
        resolutionButtons.innerHTML = '';
        if (wallpaper.imageResolutions) {
            wallpaper.imageResolutions.forEach(resolution => {
                const btn = document.createElement('a');
                btn.href = resolution.url;
                btn.target = '_blank';
                btn.className = 'resolution-btn';
                btn.innerHTML = `
                    <span>${resolution.resolution}</span>
                    <span>${resolution.size}</span>
                `;
                resolutionButtons.appendChild(btn);
            });
        }

        // 存储当前壁纸用于分享
        this.currentModalWallpaper = wallpaper;

        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    // 关闭模态框
    closeModal() {
        const modal = document.getElementById('wallpaper-modal');
        modal.classList.remove('active');
        document.body.style.overflow = '';
        this.currentModalWallpaper = null;
    }

    // 复制当前壁纸链接
    copyCurrentWallpaperLink() {
        if (!this.currentModalWallpaper) return;

        const url = this.currentModalWallpaper.fullImageUrl;
        navigator.clipboard.writeText(url).then(() => {
            this.showToast('链接已复制到剪贴板');
        }).catch(() => {
            this.showToast('复制失败，请手动复制');
        });
    }

    // 分享当前壁纸
    shareCurrentWallpaper() {
        if (!this.currentModalWallpaper) return;

        const wallpaper = this.currentModalWallpaper;
        const shareData = {
            title: `${wallpaper.title} - 必应壁纸`,
            text: `${wallpaper.description} (${wallpaper.countryInfo.name})`,
            url: wallpaper.fullImageUrl
        };

        if (navigator.share) {
            navigator.share(shareData).catch(console.error);
        } else {
            // 回退到复制链接
            this.copyCurrentWallpaperLink();
        }
    }

    // 更新统计信息
    updateStats(filteredCount = null) {
        const totalCount = document.getElementById('total-count');
        const countryCount = document.getElementById('country-count');
        const lastUpdate = document.getElementById('last-update');

        const displayCount = filteredCount !== null ? filteredCount : window.dataLoader.wallpapers.length;
        totalCount.textContent = displayCount;
        countryCount.textContent = window.dataLoader.countries.length;

        // 设置最后更新时间
        if (window.dataLoader.wallpapers.length > 0) {
            const latestDate = Math.max(...window.dataLoader.wallpapers.map(w => new Date(w.date)));
            lastUpdate.textContent = new Date(latestDate).toLocaleDateString('zh-CN');
        }
    }

    // 显示加载状态
    showLoading() {
        document.getElementById('loading').classList.remove('hidden');
    }

    // 隐藏加载状态
    hideLoading() {
        document.getElementById('loading').classList.add('hidden');
    }

    // 显示错误信息
    showError(message) {
        const loading = document.getElementById('loading');
        loading.innerHTML = `
            <div class="loading-spinner">
                <i class="fas fa-exclamation-triangle" style="color: #e74c3c;"></i>
                <p style="color: #e74c3c;">${message}</p>
                <button onclick="location.reload()" style="margin-top: 1rem; padding: 0.5rem 1rem; border: none; border-radius: 5px; background: #667eea; color: white; cursor: pointer;">重新加载</button>
            </div>
        `;
    }

    // 显示提示消息
    showToast(message) {
        // 创建或获取toast容器
        let toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toast-container';
            toastContainer.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 10001;
                display: flex;
                flex-direction: column;
                gap: 10px;
            `;
            document.body.appendChild(toastContainer);
        }

        // 创建toast元素
        const toast = document.createElement('div');
        toast.style.cssText = `
            background: rgba(0, 0, 0, 0.8);
            color: white;
            padding: 1rem 1.5rem;
            border-radius: 8px;
            font-size: 0.9rem;
            transform: translateX(100%);
            transition: transform 0.3s ease;
        `;
        toast.textContent = message;
        
        toastContainer.appendChild(toast);

        // 动画显示
        setTimeout(() => {
            toast.style.transform = 'translateX(0)';
        }, 100);

        // 自动隐藏
        setTimeout(() => {
            toast.style.transform = 'translateX(100%)';
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        }, 3000);
    }
}

// 当DOM加载完成后初始化应用程序
document.addEventListener('DOMContentLoaded', () => {
    window.app = new WallpaperApp();
});

// 全局错误处理
window.addEventListener('error', (e) => {
    console.error('全局错误:', e.error);
});

window.addEventListener('unhandledrejection', (e) => {
    console.error('未处理的Promise拒绝:', e.reason);
});
