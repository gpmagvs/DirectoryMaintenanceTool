using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace DirectoryMaintenanceTool
{
    public class DirectoryDeleteProcessor : IDirectoryProcessor
    {
        private readonly string _rootPath;
        private readonly DateTime _thresholdDate;
        private int _processedCount = 0;
        private int _deletedCount = 0;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public DirectoryDeleteProcessor(string rootPath, DateTime thresholdDate)
        {
            _rootPath = rootPath;
            _thresholdDate = thresholdDate;
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
                Logger.Info($"處理完成摘要:\n總共掃描: {_processedCount} 個目錄,已刪除: {_deletedCount} 個目錄,保留: {_processedCount - _deletedCount} 個目錄");
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
                    Logger.Info($"[保留] {dirInfo.Name}=> 建立時間: {dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                    return;
                }

                // 刪除符合條件的目錄
                Logger.Info($"[刪除] {dirInfo.Name} => 建立時間: {dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");

                long dirSize = GetDirectorySize(dirInfo);
                Logger.Info($"       大小: {FormatFileSize(dirSize)}");

                Directory.Delete(directoryPath, true);
                _deletedCount++;
                Logger.Info($"狀態: 刪除成功");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"狀態: 刪除失敗 - {directoryPath}");
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
}
