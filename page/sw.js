// Service Worker for Bing Wallpaper Collector PWA
const CACHE_NAME = "bing-wallpaper-v1.0.0";
const DATA_CACHE_NAME = "bing-wallpaper-data-v1.0.0";

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

  // 2. 数据文件 - 网络优先策略
  if (DATA_URL_PATTERNS.some((pattern) => pattern.test(url.href))) {
    event.respondWith(
      fetchAndCache(request, DATA_CACHE_NAME, {
        fallbackToCache: true,
        maxAge: 1000 * 60 * 60 * 2, // 2小时缓存
      })
    );
    return;
  }

  // 3. 外部图片 - 缓存优先，长期缓存
  if (EXTERNAL_IMAGE_PATTERNS.some((pattern) => pattern.test(url.href))) {
    event.respondWith(
      caches
        .match(request)
        .then((response) => {
          if (response) {
            return response;
          }
          return fetchAndCache(request, DATA_CACHE_NAME, {
            maxAge: 1000 * 60 * 60 * 24 * 7, // 7天缓存
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
      fetch(request)
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
    fetch(request).catch(() => {
      return caches.match(request);
    })
  );
});

// 工具函数：获取并缓存资源
async function fetchAndCache(request, cacheName, options = {}) {
  const { fallbackToCache = false, maxAge = 0 } = options;

  try {
    const response = await fetch(request);

    if (response.ok) {
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
      return response;
    }

    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
  } catch (error) {
    console.log("[SW] 网络请求失败:", request.url, error.message);

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

// 推送通知
self.addEventListener("push", (event) => {
  const options = {
    body: event.data ? event.data.text() : "Bing壁纸收集器有新更新",
    icon: "/BingWallpaperCollector/assets/images/icon-192x192.png",
    badge: "/BingWallpaperCollector/assets/images/icon-72x72.png",
    vibrate: [200, 100, 200],
    data: {
      url: "/BingWallpaperCollector/",
    },
    actions: [
      {
        action: "view",
        title: "查看",
        icon: "/BingWallpaperCollector/assets/images/icon-96x96.png",
      },
    ],
  };

  event.waitUntil(
    self.registration.showNotification("Bing壁纸收集器", options)
  );
});

// 处理通知点击
self.addEventListener("notificationclick", (event) => {
  event.notification.close();

  const url = event.notification.data?.url || "/BingWallpaperCollector/";

  event.waitUntil(
    clients.matchAll({ type: "window" }).then((clientList) => {
      // 如果已有窗口打开，则聚焦
      for (const client of clientList) {
        if (client.url === url && "focus" in client) {
          return client.focus();
        }
      }

      // 否则打开新窗口
      if (clients.openWindow) {
        return clients.openWindow(url);
      }
    })
  );
});

// 版本更新检查
self.addEventListener("message", (event) => {
  if (event.data && event.data.type === "SKIP_WAITING") {
    self.skipWaiting();
  }
});

