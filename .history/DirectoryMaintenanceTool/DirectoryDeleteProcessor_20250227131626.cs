using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DirectoryMaintenanceTool
{
    public class DirectoryDeleteProcessor : IDirectoryProcessor
    {
        private readonly string _rootPath;
        private readonly DateTime _thresholdDate;
        private int _processedCount = 0;
        private int _deletedCount = 0;

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
                Console.WriteLine($"找到 {directories.Length} 個子目錄需要處理");
                Console.WriteLine("開始處理...\n");

                foreach (var dir in directories)
                {
                    ProcessSingleDirectory(dir);
                }

                // 輸出處理摘要
                Console.WriteLine("\n處理完成摘要:");
                Console.WriteLine($"總共掃描: {_processedCount} 個目錄");
                Console.WriteLine($"已刪除: {_deletedCount} 個目錄");
                Console.WriteLine($"保留: {_processedCount - _deletedCount} 個目錄");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"處理目錄時發生錯誤: {ex.Message}");
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
                    Console.WriteLine($"[保留] {dirInfo.Name}");
                    Console.WriteLine($"       建立時間: {dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");
                    return;
                }

                // 刪除符合條件的目錄
                Console.WriteLine($"[刪除] {dirInfo.Name}");
                Console.WriteLine($"       建立時間: {dirInfo.CreationTime:yyyy-MM-dd HH:mm:ss}");

                long dirSize = GetDirectorySize(dirInfo);
                Console.WriteLine($"       大小: {FormatFileSize(dirSize)}");

                Directory.Delete(directoryPath, true);
                _deletedCount++;
                Console.WriteLine($"       狀態: 刪除成功\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"       狀態: 刪除失敗 - {ex.Message}\n");
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
