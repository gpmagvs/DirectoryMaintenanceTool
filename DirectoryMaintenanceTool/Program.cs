using NLog;
using DirectoryMaintenanceTool;
using DirectoryMaintenanceTool.Configuration;
using System.Text.Json;

// 初始化 NLog
LogManager.LoadConfiguration("nlog.config");
var logger = LogManager.GetCurrentClassLogger();

try
{
    string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
    if (!File.Exists(configPath))
    {
        logger.Error($"錯誤: 找不到配置檔 '{configPath}'");
        return 1;
    }

    string jsonContent = File.ReadAllText(configPath);
    var config = JsonSerializer.Deserialize<AppConfig>(jsonContent);

    if (config == null || config.Folders == null || config.Folders.Count == 0)
    {
        logger.Error("錯誤: 配置檔格式錯誤或未設定資料夾");
        return 1;
    }

    return ProcessFolders(config);
}
catch (Exception ex)
{
    logger.Error(ex, "讀取配置檔時發生錯誤");
    return 1;
}

static int ProcessFolders(AppConfig config)
{
    var logger = LogManager.GetCurrentClassLogger();

    try
    {
        foreach (var folder in config.Folders)
        {
            if (!Directory.Exists(folder.Directory))
            {
                logger.Error($"錯誤: 目錄 '{folder.Directory}' 不存在");
                continue;
            }

            if (folder.DeleteThresholdDays < 0)
            {
                logger.Error("錯誤: 建立時間閾值必須是大於或等於0的整數");
                continue;
            }

            DateTime thresholdDate = DateTime.Now.AddDays(-folder.DeleteThresholdDays);
            logger.Info("========================================");
            logger.Info($"開始處理目錄: {folder.Directory}");
            logger.Info("========================================");

            logger.Info("[階段一] 刪除過期目錄");
            logger.Info($"刪除創建時間早於 {thresholdDate:yyyy-MM-dd} 的子目錄");
            logger.Info("----------------------------------------");
            var deleteProcessor = new DirectoryDeleteProcessor(folder.Directory, thresholdDate);
            deleteProcessor.ProcessDirectories();

            logger.Info("[階段二] 壓縮歷史目錄");
            logger.Info("壓縮除了今日創建的子目錄");
            logger.Info("----------------------------------------");
            var processor = new DirectoryCompressProcessor(folder.Directory);
            processor.ProcessDirectories();

            logger.Info("========================================");
            logger.Info("所有處理程序完成");
            logger.Info("========================================");
        }
        return 0;
    }
    catch (Exception ex)
    {
        logger.Error(ex, "程序執行過程中發生錯誤");
        return 1;
    }
}
