using DirectoryMaintenanceTool;
using System.IO.Compression;
using NLog;

public class DirectoryCompressProcessor : IDirectoryProcessor
{
    private readonly string _rootPath;
    private readonly DateTime _thresholdDate = DateTime.MaxValue;
    private int _processedCount = 0;
    private int _compressedCount = 0;
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

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
        try
        {
            var directories = Directory.GetDirectories(_rootPath, "*", SearchOption.TopDirectoryOnly);
            Logger.Info($"找到 {directories.Length} 個子目錄需要處理,開始處理...");

            foreach (var dir in directories)
            {
                ProcessSingleDirectory(dir);
            }

            // 輸出處理摘要
            Logger.Info($"處理完成摘要:總共掃描: {_processedCount} 個目錄,已壓縮: {_compressedCount} 個目錄,保留: {_processedCount - _compressedCount} 個目錄");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "處理目錄時發生錯誤");
        }
    }

    private void ProcessSingleDirectory(string directoryPath)
    {
        try
        {
            _processedCount++;
            var dirInfo = new DirectoryInfo(directoryPath);

            // 檢查目錄建立時間是否早於閾值
            if (dirInfo.CreationTime >= _thresholdDate)
            {
                Logger.Info($"[保留] {dirInfo.Name}");
                Logger.Info($"       原因: 建立時間較新 ({dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss})");
                return;
            }

            if (dirInfo.CreationTime.Date == DateTime.Now.Date)
            {
                Logger.Info($"[保留] {dirInfo.Name}");
                Logger.Info($"       原因: 該目錄建立時間為今日 ({dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss})");
                return;
            }

            // 檢查是否已經有對應的壓縮檔
            string zipPath = $"{directoryPath}.zip";
            if (File.Exists(zipPath))
            {
                Logger.Info($"[保留] {dirInfo.Name}");
                Logger.Info($"       原因: 已經有壓縮檔");
                return;
            }

            try
            {
                // 計算目錄大小
                long dirSize = GetDirectorySize(dirInfo);
                Logger.Info($"[壓縮] {dirInfo.Name}");
                Logger.Info($"       建立時間: {dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                Logger.Info($"       原始大小: {FormatFileSize(dirSize)}");

                // 執行壓縮
                ZipFile.CreateFromDirectory(directoryPath, zipPath, CompressionLevel.Optimal, false);

                // 驗證壓縮檔
                if (ValidateZipFile(zipPath))
                {
                    // 取得壓縮檔大小
                    var zipInfo = new FileInfo(zipPath);
                    Logger.Info($"       壓縮大小: {FormatFileSize(zipInfo.Length)}");
                    Logger.Info($"       壓縮比例: {((1 - (double)zipInfo.Length / dirSize) * 100):F2}%");

                    // 刪除原始目錄
                    Directory.Delete(directoryPath, true);
                    _compressedCount++;
                    Logger.Info($"       狀態: 壓縮成功");
                }
                else
                {
                    File.Delete(zipPath);
                    Logger.Error($"       狀態: 壓縮檔驗證失敗");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"       狀態: 壓縮失敗 - {directoryPath}");
                if (File.Exists(zipPath))
                {
                    File.Delete(zipPath);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, $"處理目錄時發生錯誤: {directoryPath}");
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

    private long GetDirectorySize(DirectoryInfo dirInfo)
    {
        try
        {
            return dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
        }
        catch
        {
            return 0;
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}