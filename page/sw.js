// Service Worker for Bing Wallpaper Collector PWA
const CACHE_NAME = "bing-wallpaper-v1.1.0";
const DATA_CACHE_NAME = "bing-wallpaper-data-v1.1.0";

// 需要缓存的静态资源
const STATIC_FILES = [
  "/BingWallpaperCollector/",
  "/BingWallpaperCollector/index.html",
  "/BingWallpaperCollector/promo.html",
  "/BingWallpaperCollector/offline.html",
  "/BingWallpaperCollector/assets/css/style.css",
  "/BingWallpaperCollector/assets/js/app.js",
  "/BingWallpaperCollector/assets/js/data-loader.js",
  "/BingWallpaperCollector/data-index.js",
  "/BingWallpaperCollector/manifest.json",
  "/BingWallpaperCollector/assets/images/icon-192x192.png",
  "/BingWallpaperCollector/assets/images/icon-512x512.png",
];

// 数据文件的URL模式
const DATA_URL_PATTERNS = [
  /\/BingWallpaperCollector\/archive\/.*\.json$/,
  /\/BingWallpaperCollector\/data-index\.js$/,
];

// 外部资源模式（Bing图片）
const EXTERNAL_IMAGE_PATTERNS = [
  /^https:\/\/www\.bing\.com\/.*\.(jpg|jpeg|webp)$/,
  /^https:\/\/.*\.bing\.net\/.*\.(jpg|jpeg|webp)$/,
];

// 安装事件 - 预缓存静态资源
self.addEventListener("install", (event) => {
  console.log("[SW] 安装中...");
  event.waitUntil(
    caches
      .open(CACHE_NAME)
      .then((cache) => {
        console.log("[SW] 预缓存静态文件");
        return cache.addAll(STATIC_FILES);
      })
      .catch((err) => {
        console.warn("[SW] 预缓存失败，某些文件可能不存在:", err);
      })
  );
  self.skipWaiting();
});

// 激活事件 - 清理旧缓存
self.addEventListener("activate", (event) => {
  console.log("[SW] 激活中...");
  event.waitUntil(
    caches.keys().then((cacheNames) => {
      return Promise.all(
        cacheNames.map((cacheName) => {
          if (cacheName !== CACHE_NAME && cacheName !== DATA_CACHE_NAME) {
            console.log("[SW] 清理旧缓存:", cacheName);
            return caches.delete(cacheName);
          }
        })
      );
    })
  );
  self.clients.claim();
});

// 请求拦截 - 实现缓存策略
self.addEventListener("fetch", (event) => {
  const { request } = event;
  const url = new URL(request.url);

  // 跳过非 HTTP/HTTPS 请求
  if (!url.protocol.startsWith("http")) {
    return;
  }

  // 1. 静态资源 - 缓存优先策略
  if (STATIC_FILES.includes(url.pathname)) {
    event.respondWith(
      caches.match(request).then((response) => {
        if (response) {
          console.log("[SW] 从缓存返回静态文件:", url.pathname);
          return response;
        }
        return fetchAndCache(request, CACHE_NAME);
      })
    );
    return;
  }

  // 2. 数据文件 - 始终从网络获取最新数据，不使用缓存
  if (DATA_URL_PATTERNS.some((pattern) => pattern.test(url.href))) {
    console.log("[SW] 数据文件请求，始终从网络获取:", url.pathname);
    event.respondWith(fetchWithCacheBusting(request));
    return;
  }

  // 3. 外部图片 - 智能缓存策略
  if (EXTERNAL_IMAGE_PATTERNS.some((pattern) => pattern.test(url.href))) {
    event.respondWith(
      caches
        .match(request)
        .then((response) => {
          // 检查缓存是否存在且未过期
          if (response) {
            const cachedAt = response.headers.get("sw-cached-at");
            const maxAge = response.headers.get("sw-max-age");

            if (cachedAt && maxAge) {
              const age = Date.now() - parseInt(cachedAt);
              const maxAgeMs = parseInt(maxAge);

              // 如果缓存未过期，返回缓存
              if (age < maxAgeMs) {
                console.log(
                  "[SW] 返回缓存的图片:",
                  url.pathname,
                  `(缓存${Math.round(age / 1000 / 60)}分钟)`
                );
                return response;
              } else {
                console.log("[SW] 图片缓存已过期，重新获取:", url.pathname);
              }
            } else {
              // 旧版本缓存，直接使用但标记为需要更新
              console.log("[SW] 使用旧版本缓存，后台更新:", url.pathname);
              // 后台更新缓存
              fetchAndCache(request, DATA_CACHE_NAME, {
                maxAge: 1000 * 60 * 60 * 2, // 2小时缓存
              }).catch(() => {}); // 静默处理错误
              return response;
            }
          }

          // 没有缓存或缓存过期，重新获取
          return fetchAndCache(request, DATA_CACHE_NAME, {
            maxAge: 1000 * 60 * 60 * 2, // 2小时缓存（从7天改为2小时）
          });
        })
        .catch(() => {
          // 图片加载失败时返回占位图
          return new Response(
            '<svg width="400" height="240" xmlns="http://www.w3.org/2000/svg"><rect width="100%" height="100%" fill="#f0f0f0"/><text x="50%" y="50%" text-anchor="middle" dy=".3em" fill="#666">图片暂不可用</text></svg>',
            { headers: { "Content-Type": "image/svg+xml" } }
          );
        })
    );
    return;
  }

  // 4. 导航请求 - 网络优先，离线时返回缓存页面
  if (request.mode === "navigate") {
    event.respondWith(
      fetch(request, { referrerPolicy: "no-referrer" })
        .then((response) => {
          // 成功时缓存页面
          if (response.ok) {
            const responseClone = response.clone();
            caches.open(CACHE_NAME).then((cache) => {
              cache.put(request, responseClone);
            });
          }
          return response;
        })
        .catch(() => {
          // 离线时返回缓存页面或离线页面
          return caches.match(request).then((response) => {
            return (
              response || caches.match("/BingWallpaperCollector/offline.html")
            );
          });
        })
    );
    return;
  }

  // 5. 其他请求 - 网络优先
  event.respondWith(
    (async () => {
      try {
        // 检查是否是Bing图片URL，如果是则设置no-referrer策略
        const url = new URL(request.url);
        let fetchOptions = {};

        if (url.hostname === "www.bing.com" || url.hostname === "bing.com") {
          fetchOptions = {
            referrerPolicy: "no-referrer",
            mode: "cors",
            credentials: "omit",
          };
        }

        return await fetch(request, fetchOptions);
      } catch (error) {
        // 网络失败时回退到缓存
        return await caches.match(request);
      }
    })()
  );
});

// 工具函数：无缓存获取数据文件，添加时间戳防止缓存
async function fetchWithCacheBusting(request) {
  try {
    // 为URL添加时间戳参数防止缓存
    const url = new URL(request.url);
    url.searchParams.set("t", Date.now().toString());
    url.searchParams.set("nocache", "1");

    // 创建新的请求，添加缓存控制头
    const newRequest = new Request(url.toString(), {
      method: request.method,
      headers: {
        ...request.headers,
        "Cache-Control": "no-cache, no-store, must-revalidate",
        Pragma: "no-cache",
        Expires: "0",
      },
      body: request.body,
      credentials: request.credentials,
      cache: "no-cache",
    });

    console.log("[SW] 从网络获取数据文件（无缓存）:", url.toString());
    const response = await fetch(newRequest);

    if (response.ok) {
      // 添加防缓存响应头
      const headers = new Headers(response.headers);
      headers.set("Cache-Control", "no-cache, no-store, must-revalidate");
      headers.set("Pragma", "no-cache");
      headers.set("Expires", "0");
      headers.set("Last-Modified", new Date().toUTCString());

      return new Response(response.body, {
        status: response.status,
        statusText: response.statusText,
        headers: headers,
      });
    } else {
      throw new Error(`Network response not ok: ${response.status}`);
    }
  } catch (error) {
    console.error("[SW] 获取数据文件失败:", request.url, error);
    // 数据文件获取失败时，不回退到缓存，直接返回错误
    throw error;
  }
}

// 工具函数：获取并缓存资源
async function fetchAndCache(request, cacheName, options = {}) {
  const { fallbackToCache = false, maxAge = 0 } = options;

  try {
    // 检查是否是Bing图片URL，如果是则设置no-referrer策略
    const url = new URL(request.url);
    let fetchOptions = {};

    if (url.hostname === "www.bing.com" || url.hostname === "bing.com") {
      fetchOptions = {
        referrerPolicy: "no-referrer",
        mode: "cors",
        credentials: "omit",
      };
    }

    const response = await fetch(request, fetchOptions);

    if (response.ok) {
      // 只缓存GET请求，跳过HEAD等其他请求方法
      if (request.method === "GET") {
        // 克隆响应用于缓存
        const responseClone = response.clone();
        const cache = await caches.open(cacheName);

        // 设置缓存头（如果指定了maxAge）
        if (maxAge > 0) {
          const headers = new Headers(responseClone.headers);
          headers.set("sw-cached-at", Date.now().toString());
          headers.set("sw-max-age", maxAge.toString());

          const cachedResponse = new Response(responseClone.body, {
            status: responseClone.status,
            statusText: responseClone.statusText,
            headers: headers,
          });

          cache.put(request, cachedResponse);
        } else {
          cache.put(request, responseClone);
        }

        console.log("[SW] 缓存新资源:", request.url);
      } else {
        console.log("[SW] 跳过非GET请求的缓存:", request.method, request.url);
      }
      return response;
    }

    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
  } catch (error) {
    console.log("[SW] 网络请求失败:", request.url, error.message);

    // 对于Bing图片请求失败，不要记录详细错误避免过多日志
    const url = new URL(request.url);
    if (url.hostname === "www.bing.com" || url.hostname === "bing.com") {
      // 静默处理Bing图片请求失败
    }

    if (fallbackToCache) {
      const cachedResponse = await caches.match(request);
      if (cachedResponse) {
        // 检查缓存是否过期
        const cachedAt = cachedResponse.headers.get("sw-cached-at");
        const maxAge = cachedResponse.headers.get("sw-max-age");

        if (cachedAt && maxAge) {
          const age = Date.now() - parseInt(cachedAt);
          if (age > parseInt(maxAge)) {
            console.log("[SW] 缓存已过期:", request.url);
            throw error;
          }
        }

        console.log("[SW] 从缓存返回:", request.url);
        return cachedResponse;
      }
    }

    throw error;
  }
}

// 后台同步 - 用于离线时的数据同步
self.addEventListener("sync", (event) => {
  if (event.tag === "background-sync") {
    console.log("[SW] 后台同步触发");
    event.waitUntil(doBackgroundSync());
  }
});

async function doBackgroundSync() {
  try {
    // 这里可以添加后台数据同步逻辑
    console.log("[SW] 执行后台数据同步");

    // 例如：更新数据索引
    const response = await fetch("/BingWallpaperCollector/data-index.js");
    if (response.ok) {
      const cache = await caches.open(DATA_CACHE_NAME);
      cache.put("/BingWallpaperCollector/data-index.js", response);
    }
  } catch (error) {
    console.log("[SW] 后台同步失败:", error);
  }
}

// 版本更新检查
self.addEventListener("message", (event) => {
  if (event.data && event.data.type === "SKIP_WAITING") {
    self.skipWaiting();
  }

  // 手动检查数据更新
  if (event.data && event.data.type === "CHECK_DATA_UPDATE") {
    checkForDataUpdates();
  }

  // 清除图片缓存
  if (event.data && event.data.type === "CLEAR_IMAGE_CACHE") {
    clearImageCache();
  }

  // 强制刷新所有缓存
  if (event.data && event.data.type === "FORCE_REFRESH_CACHE") {
    forceRefreshAllCache();
  }
});

// 检查数据更新
async function checkForDataUpdates() {
  try {
    console.log("[SW] 检查数据更新...");

    const cache = await caches.open(CACHE_NAME);

    // 首先检查更新时间戳
    const latestTimestampResponse = await fetch(
      "/BingWallpaperCollector/last-update.json?t=" + Date.now(),
      {
        cache: "no-cache",
      }
    );

    if (latestTimestampResponse.ok) {
      const latestTimestamp = await latestTimestampResponse.json();
      const cachedTimestampResponse = await cache.match(
        "/BingWallpaperCollector/last-update.json"
      );

      let shouldUpdate = false;
      let isFirstCheck = false;

      if (cachedTimestampResponse) {
        const cachedTimestamp = await cachedTimestampResponse.json();

        // 比较时间戳
        if (latestTimestamp.lastUpdate > cachedTimestamp.lastUpdate) {
          console.log("[SW] 时间戳显示有数据更新！");
          console.log(
            "[SW] 缓存时间:",
            new Date(cachedTimestamp.lastUpdate * 1000).toISOString()
          );
          console.log(
            "[SW] 最新时间:",
            new Date(latestTimestamp.lastUpdate * 1000).toISOString()
          );
          shouldUpdate = true;
        }
      } else {
        console.log("[SW] 首次检查更新时间戳");
        isFirstCheck = true;
        shouldUpdate = true;
      }

      if (shouldUpdate) {
        // 更新时间戳缓存
        await cache.put(
          "/BingWallpaperCollector/last-update.json",
          latestTimestampResponse.clone()
        );

        // 更新数据索引缓存
        const latestIndexResponse = await fetch(
          "/BingWallpaperCollector/data-index.js?t=" + Date.now(),
          {
            cache: "no-cache",
          }
        );

        if (latestIndexResponse.ok) {
          await cache.put(
            "/BingWallpaperCollector/data-index.js",
            latestIndexResponse.clone()
          );

          if (!isFirstCheck) {
            // 通知所有客户端数据已更新
            const clients = await self.clients.matchAll();
            clients.forEach((client) => {
              client.postMessage({
                type: "DATA_UPDATED",
                message: "发现新的壁纸数据！",
                timestamp: latestTimestamp.lastUpdate,
                updateTime: latestTimestamp.updateTime,
              });
            });
          }

          console.log("[SW] 数据缓存已更新");
        }
      } else {
        console.log("[SW] 数据无更新");
      }
    } else {
      // 如果没有时间戳文件，回退到检查数据索引文件
      console.log("[SW] 未找到时间戳文件，回退到检查数据索引");
      await checkDataIndexForUpdates(cache);
    }
  } catch (error) {
    console.error("[SW] 检查数据更新失败:", error);
    // 错误时回退到检查数据索引文件
    try {
      const cache = await caches.open(CACHE_NAME);
      await checkDataIndexForUpdates(cache);
    } catch (fallbackError) {
      console.error("[SW] 回退检查也失败:", fallbackError);
    }
  }
}

// 回退方法：检查数据索引文件变化
async function checkDataIndexForUpdates(cache) {
  try {
    const cachedIndexResponse = await cache.match(
      "/BingWallpaperCollector/data-index.js"
    );
    const latestIndexResponse = await fetch(
      "/BingWallpaperCollector/data-index.js?t=" + Date.now(),
      {
        cache: "no-cache",
      }
    );

    if (!latestIndexResponse.ok) {
      console.log("[SW] 无法获取最新数据索引");
      return;
    }

    const latestIndexText = await latestIndexResponse.text();

    if (cachedIndexResponse) {
      const cachedIndexText = await cachedIndexResponse.text();

      if (cachedIndexText !== latestIndexText) {
        console.log("[SW] 数据索引有变化！");
        await cache.put(
          "/BingWallpaperCollector/data-index.js",
          latestIndexResponse.clone()
        );

        const clients = await self.clients.matchAll();
        clients.forEach((client) => {
          client.postMessage({
            type: "DATA_UPDATED",
            message: "发现新的壁纸数据！",
          });
        });
      }
    } else {
      await cache.put(
        "/BingWallpaperCollector/data-index.js",
        latestIndexResponse.clone()
      );
      console.log("[SW] 首次缓存数据索引");
    }
  } catch (error) {
    console.error("[SW] 检查数据索引失败:", error);
  }
}

// 清除图片缓存
async function clearImageCache() {
  try {
    console.log("[SW] 开始清除图片缓存...");
    const cache = await caches.open(DATA_CACHE_NAME);
    const requests = await cache.keys();

    let clearedCount = 0;
    for (const request of requests) {
      const url = new URL(request.url);
      // 检查是否是图片URL
      if (EXTERNAL_IMAGE_PATTERNS.some((pattern) => pattern.test(url.href))) {
        await cache.delete(request);
        clearedCount++;
      }
    }

    console.log(`[SW] 已清除 ${clearedCount} 个图片缓存`);

    // 通知所有客户端缓存已清除
    const clients = await self.clients.matchAll();
    clients.forEach((client) => {
      client.postMessage({
        type: "IMAGE_CACHE_CLEARED",
        message: `已清除 ${clearedCount} 个图片缓存`,
        count: clearedCount,
      });
    });
  } catch (error) {
    console.error("[SW] 清除图片缓存失败:", error);
  }
}

// 强制刷新所有缓存
async function forceRefreshAllCache() {
  try {
    console.log("[SW] 开始强制刷新所有缓存...");

    // 清除所有缓存
    const cacheNames = await caches.keys();
    await Promise.all(cacheNames.map((cacheName) => caches.delete(cacheName)));

    console.log("[SW] 所有缓存已清除，准备重新加载...");

    // 通知所有客户端进行硬刷新
    const clients = await self.clients.matchAll();
    clients.forEach((client) => {
      client.postMessage({
        type: "FORCE_RELOAD",
        message: "缓存已清除，正在重新加载...",
      });
    });
  } catch (error) {
    console.error("[SW] 强制刷新缓存失败:", error);
  }
}

// 定期检查数据更新（每小时一次）
setInterval(() => {
  checkForDataUpdates();
}, 60 * 60 * 1000);
