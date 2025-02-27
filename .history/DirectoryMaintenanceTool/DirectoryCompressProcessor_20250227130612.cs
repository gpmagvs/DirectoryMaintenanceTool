using DirectoryMaintenanceTool;
using System.IO.Compression;

public class DirectoryCompressProcessor : IDirectoryProcessor
{
    private readonly string _rootPath;
    private readonly DateTime _thresholdDate = DateTime.MaxValue;

    public DirectoryCompressProcessor(string rootPath, DateTime thresholdDate) : this(rootPath)
    {
        _thresholdDate = thresholdDate;
    }

    public DirectoryCompressProcessor(string rootPath)
    {
        _rootPath = rootPath;
    }

    public void ProcessDirectories()
    {
        var directories = Directory.GetDirectories(_rootPath, "*", SearchOption.TopDirectoryOnly);
        foreach (var dir in directories)
        {
            ProcessSingleDirectory(dir);
        }
    }

    private void ProcessSingleDirectory(string directoryPath)
    {
        var dirInfo = new DirectoryInfo(directoryPath);

        // 檢查目錄建立時間是否早於閾值
        if (dirInfo.CreationTime >= _thresholdDate)
        {
            Console.WriteLine($"跳過目錄 {dirInfo.Name} - 建立時間較新");
            return;
        }

        if (dirInfo.CreationTime.Date == DateTime.Now.Date)
        {
            Console.WriteLine($"跳過目錄 {dirInfo.Name} - 該目錄建立時間為今日");
            return;
        }

        // 檢查是否已經有對應的壓縮檔
        string zipPath = $"{directoryPath}.zip";
        if (File.Exists(zipPath))
        {
            Console.WriteLine($"跳過目錄 {dirInfo.Name} - 已經有壓縮檔");
            return;
        }

        try
        {
            // 執行壓縮
            Console.WriteLine($"正在壓縮目錄: {dirInfo.Name}");
            ZipFile.CreateFromDirectory(directoryPath, zipPath, CompressionLevel.Optimal, false);

            // 驗證壓縮檔
            if (ValidateZipFile(zipPath))
            {
                // 刪除原始目錄
                Directory.Delete(directoryPath, true);
                Console.WriteLine($"成功處理目錄: {dirInfo.Name}");
            }
            else
            {
                File.Delete(zipPath);
                Console.WriteLine($"壓縮檔驗證失敗: {dirInfo.Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"處理目錄 {dirInfo.Name} 時發生錯誤: {ex.Message}");
            if (File.Exists(zipPath))
            {
                File.Delete(zipPath);
            }
        }
    }

    private bool ValidateZipFile(string zipPath)
    {
        try
        {
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                // 檢查壓縮檔是否可以正常開啟和讀取
                return archive.Entries.Count > 0;
            }
        }
        catch
        {
            return false;
        }
    }
}