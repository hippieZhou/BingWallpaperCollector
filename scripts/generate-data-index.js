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
  console.log(`📍 扫描路径: ${ARCHIVE_DIR}`);

  if (!fs.existsSync(ARCHIVE_DIR)) {
    console.error(`❌ Archive 目录不存在: ${ARCHIVE_DIR}`);
    console.log("当前工作目录:", process.cwd());
    console.log("尝试查找 archive 目录...");

    // 列出当前目录的内容
    try {
      const currentDirContents = fs.readdirSync(process.cwd());
      console.log("当前目录内容:", currentDirContents);

      // 检查是否有 archive 相关目录
      const archiveRelated = currentDirContents.filter((item) =>
        item.toLowerCase().includes("archive")
      );
      console.log("发现的 archive 相关项:", archiveRelated);
    } catch (err) {
      console.error("无法读取当前目录:", err.message);
    }

    throw new Error(`Archive 目录不存在: ${ARCHIVE_DIR}`);
  }

  const countries = [];
  const dates = new Set();
  const availableData = {};
  let totalFiles = 0;

  console.log("✅ Archive 目录存在，开始扫描...");

  // 读取所有国家目录
  let countryDirs;
  try {
    countryDirs = fs
      .readdirSync(ARCHIVE_DIR, { withFileTypes: true })
      .filter((dirent) => dirent.isDirectory())
      .map((dirent) => dirent.name);
  } catch (err) {
    console.error(`❌ 无法读取 archive 目录: ${err.message}`);
    throw err;
  }

  console.log(`📁 发现 ${countryDirs.length} 个国家目录:`, countryDirs);

  if (countryDirs.length === 0) {
    console.warn("⚠️ 警告: 未发现任何国家目录");
    return {
      countries: [],
      dates: [],
      totalFiles: 0,
      availableData: {},
    };
  }

  // 扫描每个国家目录
  for (const country of countryDirs) {
    const countryPath = path.join(ARCHIVE_DIR, country);
    console.log(`🔍 正在扫描国家目录: ${country}`);
    console.log(`   路径: ${countryPath}`);

    let jsonFiles;
    try {
      // 检查国家目录是否可读
      const allFiles = fs.readdirSync(countryPath);
      console.log(`   目录中所有文件: ${allFiles.length} 个`);

      jsonFiles = allFiles
        .filter((file) => file.endsWith(".json"))
        .map((file) => file.replace(".json", ""))
        .sort((a, b) => new Date(b) - new Date(a)); // 日期降序

      console.log(`   JSON文件: ${jsonFiles.length} 个`, jsonFiles.slice(0, 3));
    } catch (err) {
      console.error(`❌ 无法读取国家目录 ${country}: ${err.message}`);
      continue;
    }

    if (jsonFiles.length > 0) {
      countries.push(country);
      availableData[country] = jsonFiles;
      totalFiles += jsonFiles.length;

      // 收集所有日期
      jsonFiles.forEach((date) => dates.add(date));

      console.log(
        `✅ ${country}: ${jsonFiles.length} 个文件 (${jsonFiles[0]} 到 ${
          jsonFiles[jsonFiles.length - 1]
        })`
      );
    } else {
      console.log(`⚠️  ${country}: 无有效JSON文件`);
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
    console.log(`📍 运行环境信息:`);
    console.log(`   - Node.js 版本: ${process.version}`);
    console.log(`   - 当前工作目录: ${process.cwd()}`);
    console.log(`   - 脚本目录: ${__dirname}`);
    console.log(`   - Archive 目录: ${ARCHIVE_DIR}`);
    console.log(`   - 输出文件: ${OUTPUT_FILE}`);
    console.log("==================================");

    // 扫描数据目录
    console.log("\n🔍 第一步：扫描数据目录");
    const data = scanArchiveDirectory();

    if (data.totalFiles === 0) {
      console.warn("⚠️ 警告: 未发现任何数据文件");
      console.log("生成空的数据索引文件");
    }

    // 生成索引内容
    console.log("\n📝 第二步：生成索引内容");
    const indexContent = generateDataIndex(data);
    console.log(`生成的内容长度: ${indexContent.length} 字符`);

    // 写入文件
    console.log("\n💾 第三步：写入索引文件");
    writeDataIndex(indexContent);

    // 验证生成的文件
    console.log("\n✅ 第四步：验证生成结果");
    if (fs.existsSync(OUTPUT_FILE)) {
      const fileStats = fs.statSync(OUTPUT_FILE);
      console.log(`文件大小: ${fileStats.size} 字节`);
      console.log(`修改时间: ${fileStats.mtime}`);

      // 读取并验证文件内容
      const savedContent = fs.readFileSync(OUTPUT_FILE, "utf8");
      if (savedContent.includes("WALLPAPER_DATA_INDEX")) {
        console.log("✅ 文件内容验证通过");
      } else {
        console.error("❌ 文件内容验证失败");
        throw new Error("生成的文件内容不正确");
      }
    } else {
      throw new Error("输出文件未创建成功");
    }

    console.log("\n🎉 数据索引生成完成！");
    console.log("==================================");
    console.log(`📊 最终统计信息:`);
    console.log(`   - 国家数量: ${data.countries.length}`);
    console.log(`   - 日期数量: ${data.dates.length}`);
    console.log(`   - 文件总数: ${data.totalFiles}`);
    if (data.dates.length > 0) {
      console.log(`   - 最新日期: ${data.dates[0]}`);
      console.log(`   - 最早日期: ${data.dates[data.dates.length - 1]}`);
    }
    console.log(`   - 输出文件: ${OUTPUT_FILE}`);
    console.log("==================================");
  } catch (error) {
    console.error("\n❌ 生成数据索引时发生错误:");
    console.error(`错误类型: ${error.constructor.name}`);
    console.error(`错误消息: ${error.message}`);
    if (error.stack) {
      console.error("错误堆栈:", error.stack);
    }
    console.error("==================================");
    process.exit(1);
  }
}

// 运行主函数
if (require.main === module) {
  main();
}

module.exports = { scanArchiveDirectory, generateDataIndex, writeDataIndex };
