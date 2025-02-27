// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

#if DEBUG
if (args.Length == 0)
{
    // 在偵錯模式下，如果沒有提供參數，使用預設測試參數
    args = new string[]
    {
        @"C:\AGVSystemLog\VMSLog",  // 測試目錄路徑
        "30"               // 測試天數閾值
    };
    Console.WriteLine("偵錯模式: 使用測試參數");
}
#endif

if (args.Length != 2)
{
    Console.WriteLine("使用方式: DirectoryMaintenanceTool <目錄路徑> <建立時間閾值(天)>");
    return 1;
}

string directoryPath = args[0];
if (!Directory.Exists(directoryPath))
{
    Console.WriteLine($"錯誤: 目錄 '{directoryPath}' 不存在");
    return 1;
}

if (!int.TryParse(args[1], out int daysThreshold) || daysThreshold < 0)
{
    Console.WriteLine("錯誤: 建立時間閾值必須是大於或等於0的整數");
    return 1;
}

try
{
    DateTime thresholdDate = DateTime.Now.AddDays(-daysThreshold);
    Console.WriteLine($"正在處理目錄: {directoryPath}");
    Console.WriteLine($"檢查建立時間早於: {thresholdDate:yyyy-MM-dd}");

    var processor = new DirectoryProcessor(directoryPath, thresholdDate);
    processor.ProcessDirectories();

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"發生錯誤: {ex.Message}");
    return 1;
}
