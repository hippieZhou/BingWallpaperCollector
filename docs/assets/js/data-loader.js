// 数据加载和管理模块
class DataLoader {
    constructor() {
        this.wallpapers = [];
        this.countries = [];
        this.dates = [];
        this.loading = false;
    }

    // 国家代码到中文名称的映射
    getCountryInfo() {
        return {
            'China': { name: '中国', flag: '🇨🇳', code: 'zh-CN' },
            'UnitedStates': { name: '美国', flag: '🇺🇸', code: 'en-US' },
            'UnitedKingdom': { name: '英国', flag: '🇬🇧', code: 'en-GB' },
            'Japan': { name: '日本', flag: '🇯🇵', code: 'ja-JP' },
            'Germany': { name: '德国', flag: '🇩🇪', code: 'de-DE' },
            'France': { name: '法国', flag: '🇫🇷', code: 'fr-FR' },
            'Spain': { name: '西班牙', flag: '🇪🇸', code: 'es-ES' },
            'Italy': { name: '意大利', flag: '🇮🇹', code: 'it-IT' },
            'Russia': { name: '俄罗斯', flag: '🇷🇺', code: 'ru-RU' },
            'SouthKorea': { name: '韩国', flag: '🇰🇷', code: 'ko-KR' },
            'Brazil': { name: '巴西', flag: '🇧🇷', code: 'pt-BR' },
            'Australia': { name: '澳大利亚', flag: '🇦🇺', code: 'en-AU' },
            'Canada': { name: '加拿大', flag: '🇨🇦', code: 'en-CA' },
            'India': { name: '印度', flag: '🇮🇳', code: 'en-IN' }
        };
    }

    // 获取可用的日期列表
    async getAvailableDates() {
        // 首先尝试从数据索引获取
        if (window.WALLPAPER_DATA_INDEX && window.WALLPAPER_DATA_INDEX.dates) {
            console.log('从数据索引获取日期列表:', window.WALLPAPER_DATA_INDEX.dates);
            return window.WALLPAPER_DATA_INDEX.dates;
        }
        
        // 回退到已知的可用日期（基于实际数据）
        const knownDates = [
            '2025-08-28',
            '2025-08-27'
        ];
        
        console.log('使用预设的已知日期:', knownDates);
        return knownDates;
    }

    // 检查文件是否存在
    async fileExists(url) {
        try {
            const response = await fetch(url, { method: 'HEAD' });
            return response.ok;
        } catch (error) {
            return false;
        }
    }

    // 获取正确的基础路径
    getBasePath() {
        // 检测是否在GitHub Pages环境
        if (window.location.hostname.includes('github.io')) {
            // GitHub Pages路径: /BingWallpaperCollector/
            return '/BingWallpaperCollector';
        }
        // 本地开发环境
        return '';
    }

    // 加载单个壁纸数据
    async loadWallpaperData(country, date) {
        const basePath = this.getBasePath();
        const url = `${basePath}/BingWallpaperData/${country}/${date}/wallpaper_info.json`;
        
        try {
            // 检查文件是否存在
            if (!(await this.fileExists(url))) {
                // 静默处理文件不存在的情况
                return null;
            }

            const response = await fetch(url);
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.json();
            const countryInfo = this.getCountryInfo()[country];
            
            return {
                ...data,
                countryInfo: countryInfo,
                displayDate: this.formatDate(date),
                fullImageUrl: this.getBestImageUrl(data.imageResolutions),
                thumbnailUrl: this.getThumbnailUrl(data.imageResolutions)
            };
        } catch (error) {
            console.error(`加载壁纸数据失败 (${country}/${date}):`, error);
            return null;
        }
    }

    // 获取最佳图片URL（用于显示）
    getBestImageUrl(imageResolutions) {
        if (!imageResolutions || imageResolutions.length === 0) {
            return '/assets/images/placeholder.jpg';
        }
        
        // 优先选择HD分辨率
        const hdImage = imageResolutions.find(img => img.resolution === 'HD');
        if (hdImage) return hdImage.url;
        
        // 其次选择Full HD
        const fullHdImage = imageResolutions.find(img => img.resolution === 'Full HD');
        if (fullHdImage) return fullHdImage.url;
        
        // 最后选择第一个可用的
        return imageResolutions[0].url;
    }

    // 获取缩略图URL
    getThumbnailUrl(imageResolutions) {
        if (!imageResolutions || imageResolutions.length === 0) {
            return '/assets/images/placeholder.jpg';
        }
        
        // 优先选择Standard分辨率作为缩略图
        const standardImage = imageResolutions.find(img => img.resolution === 'Standard');
        if (standardImage) return standardImage.url;
        
        return this.getBestImageUrl(imageResolutions);
    }

    // 格式化日期
    formatDate(dateString) {
        const date = new Date(dateString + 'T00:00:00');
        return date.toLocaleDateString('zh-CN', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    }

    // 加载所有壁纸数据
    async loadAllData(progressCallback = null) {
        if (this.loading) {
            console.log('数据正在加载中...');
            return this.wallpapers;
        }

        this.loading = true;
        this.wallpapers = [];
        
        const countries = Object.keys(this.getCountryInfo());
        const dates = await this.getAvailableDates();
        
        const total = countries.length * dates.length;
        let loaded = 0;

        console.log(`开始加载数据: ${countries.length} 个国家 × ${dates.length} 个日期 = ${total} 个文件`);

        // 并发加载数据，但限制并发数
        const concurrencyLimit = 5;
        const promises = [];

        for (const country of countries) {
            for (const date of dates) {
                const promise = this.loadWallpaperData(country, date)
                    .then(data => {
                        loaded++;
                        if (progressCallback) {
                            progressCallback({
                                loaded,
                                total,
                                percentage: Math.round((loaded / total) * 100),
                                current: `${country}/${date}`
                            });
                        }
                        return data;
                    });
                
                promises.push(promise);
                
                // 控制并发数
                if (promises.length >= concurrencyLimit) {
                    await Promise.all(promises);
                    promises.length = 0; // 清空数组
                    
                    // 短暂延迟，避免过快的请求
                    await new Promise(resolve => setTimeout(resolve, 100));
                }
            }
        }

        // 等待剩余的请求完成
        if (promises.length > 0) {
            await Promise.all(promises);
        }

        // 收集所有有效数据
        const allResults = await Promise.all(promises);
        this.wallpapers = allResults.filter(data => data !== null);
        
        // 提取国家和日期列表
        this.countries = [...new Set(this.wallpapers.map(w => w.country))];
        this.dates = [...new Set(this.wallpapers.map(w => w.date))].sort().reverse();
        
        console.log(`数据加载完成: ${this.wallpapers.length} 张壁纸`);
        console.log(`可用国家: ${this.countries.length} 个`);
        console.log(`可用日期: ${this.dates.length} 个`);
        
        this.loading = false;
        return this.wallpapers;
    }

    // 获取国家统计数据
    getCountryStats() {
        const countryInfo = this.getCountryInfo();
        const stats = {};
        
        Object.keys(countryInfo).forEach(country => {
            const countryWallpapers = this.wallpapers.filter(w => w.country === country);
            stats[country] = {
                ...countryInfo[country],
                count: countryWallpapers.length,
                wallpapers: countryWallpapers,
                dates: [...new Set(countryWallpapers.map(w => w.date))].sort().reverse()
            };
        });
        
        return stats;
    }

    // 搜索壁纸
    searchWallpapers(query, filters = {}) {
        let results = [...this.wallpapers];
        
        // 文本搜索
        if (query && query.trim()) {
            const searchTerm = query.toLowerCase();
            results = results.filter(wallpaper => 
                wallpaper.title.toLowerCase().includes(searchTerm) ||
                wallpaper.description.toLowerCase().includes(searchTerm) ||
                wallpaper.copyright.toLowerCase().includes(searchTerm)
            );
        }
        
        // 国家筛选
        if (filters.country) {
            results = results.filter(wallpaper => wallpaper.country === filters.country);
        }
        
        // 日期筛选
        if (filters.date) {
            results = results.filter(wallpaper => wallpaper.date === filters.date);
        }
        
        return results;
    }

    // 按日期分组壁纸
    getWallpapersByDate() {
        const grouped = {};
        
        this.wallpapers.forEach(wallpaper => {
            if (!grouped[wallpaper.date]) {
                grouped[wallpaper.date] = [];
            }
            grouped[wallpaper.date].push(wallpaper);
        });
        
        // 按日期排序（最新在前）
        const sortedDates = Object.keys(grouped).sort().reverse();
        const result = {};
        
        sortedDates.forEach(date => {
            result[date] = grouped[date].sort((a, b) => 
                a.countryInfo.name.localeCompare(b.countryInfo.name)
            );
        });
        
        return result;
    }

    // 获取随机壁纸
    getRandomWallpapers(count = 6) {
        const shuffled = [...this.wallpapers].sort(() => 0.5 - Math.random());
        return shuffled.slice(0, count);
    }

    // 获取最新壁纸
    getLatestWallpapers(count = 12) {
        return [...this.wallpapers]
            .sort((a, b) => new Date(b.date) - new Date(a.date))
            .slice(0, count);
    }
}

// 创建全局数据加载器实例
window.dataLoader = new DataLoader();
