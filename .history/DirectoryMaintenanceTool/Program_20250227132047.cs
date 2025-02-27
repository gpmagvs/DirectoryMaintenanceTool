using NLog;
using DirectoryMaintenanceTool;

// 初始化 NLog
LogManager.LoadConfiguration("nlog.config");
var logger = LogManager.GetCurrentClassLogger();

#if DEBUG
if (args.Length == 0)
{
    // 在偵錯模式下，如果沒有提供參數，使用預設測試參數
    args = new string[]
    {
        @"C:\GPM_SECS_SYSTEM_LOG",  // 測試目錄路徑
        "30"               // 測試天數閾值
    };
    logger.Info("偵錯模式: 使用測試參數");
}
#endif

if (args.Length != 2)
{
    logger.Error("使用方式: DirectoryMaintenanceTool <目錄路徑> <建立時間閾值(天)>");
    return 1;
}

string directoryPath = args[0];
if (!Directory.Exists(directoryPath))
{
    logger.Error($"錯誤: 目錄 '{directoryPath}' 不存在");
    return 1;
}

if (!int.TryParse(args[1], out int daysThreshold) || daysThreshold < 0)
{
    logger.Error("錯誤: 建立時間閾值必須是大於或等於0的整數");
    return 1;
}

try
{
    DateTime thresholdDate = DateTime.Now.AddDays(-daysThreshold);
    logger.Info("========================================");
    logger.Info($"開始處理目錄: {directoryPath}");
    logger.Info("========================================");

    logger.Info("\n[階段一] 刪除過期目錄");
    logger.Info($"刪除創建時間早於 {thresholdDate:yyyy-MM-dd} 的子目錄");
    logger.Info("----------------------------------------\n");
    var deleteProcessor = new DirectoryDeleteProcessor(directoryPath, thresholdDate);
    deleteProcessor.ProcessDirectories();

    logger.Info("\n[階段二] 壓縮歷史目錄");
    logger.Info("壓縮除了今日創建的子目錄");
    logger.Info("----------------------------------------\n");
    var processor = new DirectoryCompressProcessor(directoryPath);
    processor.ProcessDirectories();

    logger.Info("\n========================================");
    logger.Info("所有處理程序完成");
    logger.Info("========================================\n");
    return 0;
}
catch (Exception ex)
{
    logger.Error(ex, "程序執行過程中發生錯誤");
    return 1;
}
