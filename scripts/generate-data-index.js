#!/usr/bin/env node

/**
 * 生成数据索引文件脚本
 * 扫描 archive 目录中的JSON文件，生成 page/data-index.js
 */

const fs = require("fs");
const path = require("path");

// 配置
const ARCHIVE_DIR = path.join(__dirname, "..", "archive");
const OUTPUT_FILE = path.join(__dirname, "..", "page", "data-index.js");

/**
 * 扫描 archive 目录获取所有数据文件信息
 */
function scanArchiveDirectory() {
  console.log("🔍 开始扫描 archive 目录...");

  if (!fs.existsSync(ARCHIVE_DIR)) {
    throw new Error(`Archive 目录不存在: ${ARCHIVE_DIR}`);
  }

  const countries = [];
  const dates = new Set();
  const availableData = {};
  let totalFiles = 0;

  // 读取所有国家目录
  const countryDirs = fs
    .readdirSync(ARCHIVE_DIR, { withFileTypes: true })
    .filter((dirent) => dirent.isDirectory())
    .map((dirent) => dirent.name);

  console.log(`📁 发现 ${countryDirs.length} 个国家目录:`, countryDirs);

  // 扫描每个国家目录
  for (const country of countryDirs) {
    const countryPath = path.join(ARCHIVE_DIR, country);
    const jsonFiles = fs
      .readdirSync(countryPath)
      .filter((file) => file.endsWith(".json"))
      .map((file) => file.replace(".json", ""))
      .sort((a, b) => new Date(b) - new Date(a)); // 日期降序

    if (jsonFiles.length > 0) {
      countries.push(country);
      availableData[country] = jsonFiles;
      totalFiles += jsonFiles.length;

      // 收集所有日期
      jsonFiles.forEach((date) => dates.add(date));

      console.log(`✅ ${country}: ${jsonFiles.length} 个文件`);
    }
  }

  // 转换日期集合为排序数组
  const sortedDates = Array.from(dates).sort(
    (a, b) => new Date(b) - new Date(a)
  );

  console.log(
    `📊 扫描完成: ${countries.length} 个国家, ${sortedDates.length} 个日期, ${totalFiles} 个文件`
  );
  console.log(
    `📅 日期范围: ${sortedDates[sortedDates.length - 1]} 到 ${sortedDates[0]}`
  );

  return {
    countries: countries.sort(),
    dates: sortedDates,
    totalFiles,
    availableData,
  };
}

/**
 * 生成数据索引文件内容
 */
function generateDataIndex(data) {
  const generated = new Date().toISOString();

  const indexContent = `// 本地数据索引文件
// 生成时间: ${generated}

window.WALLPAPER_DATA_INDEX = ${JSON.stringify(
    {
      generated:
        generated.split("T")[0] +
        "T" +
        generated.split("T")[1].split(".")[0] +
        ".000Z",
      countries: data.countries,
      dates: data.dates,
      totalFiles: data.totalFiles,
      availableData: data.availableData,
    },
    null,
    2
  )};

console.log("📊 本地数据索引加载完成:", {
  countries: ${data.countries.length},
  dates: ${data.dates.length},
  totalFiles: ${data.totalFiles}
});`;

  return indexContent;
}

/**
 * 写入数据索引文件
 */
function writeDataIndex(content) {
  try {
    // 确保输出目录存在
    const outputDir = path.dirname(OUTPUT_FILE);
    if (!fs.existsSync(outputDir)) {
      fs.mkdirSync(outputDir, { recursive: true });
    }

    fs.writeFileSync(OUTPUT_FILE, content, "utf8");
    console.log(`✅ 数据索引文件已生成: ${OUTPUT_FILE}`);
  } catch (error) {
    throw new Error(`写入文件失败: ${error.message}`);
  }
}

/**
 * 主函数
 */
function main() {
  try {
    console.log("🚀 开始生成数据索引文件...");
    console.log("==================================");

    // 扫描数据目录
    const data = scanArchiveDirectory();

    // 生成索引内容
    console.log("\n📝 生成索引内容...");
    const indexContent = generateDataIndex(data);

    // 写入文件
    console.log("\n💾 写入索引文件...");
    writeDataIndex(indexContent);

    console.log("\n🎉 数据索引生成完成！");
    console.log("==================================");
    console.log(`📊 统计信息:`);
    console.log(`   - 国家数量: ${data.countries.length}`);
    console.log(`   - 日期数量: ${data.dates.length}`);
    console.log(`   - 文件总数: ${data.totalFiles}`);
    console.log(`   - 最新日期: ${data.dates[0]}`);
    console.log(`   - 最早日期: ${data.dates[data.dates.length - 1]}`);
  } catch (error) {
    console.error("❌ 生成数据索引时发生错误:", error.message);
    process.exit(1);
  }
}

// 运行主函数
if (require.main === module) {
  main();
}

module.exports = { scanArchiveDirectory, generateDataIndex, writeDataIndex };
