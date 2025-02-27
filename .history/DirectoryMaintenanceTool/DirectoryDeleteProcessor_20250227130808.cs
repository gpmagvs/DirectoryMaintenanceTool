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
                foreach (var dir in directories)
                {
                    ProcessSingleDirectory(dir);
                }
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
                var dirInfo = new DirectoryInfo(directoryPath);

                // 檢查目錄建立時間是否早於閾值
                if (dirInfo.CreationTime >= _thresholdDate)
                {
                    Console.WriteLine($"跳過目錄 {dirInfo.Name} - 建立時間較新 ({dirInfo.CreationTime:yyyy-MM-dd})");
                    return;
                }

                // 刪除符合條件的目錄
                Console.WriteLine($"正在刪除目錄: {dirInfo.Name} (建立時間: {dirInfo.CreationTime:yyyy-MM-dd})");
                Directory.Delete(directoryPath, true);
                Console.WriteLine($"成功刪除目錄: {dirInfo.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除目錄時發生錯誤: {ex.Message}");
            }
        }
    }
}
