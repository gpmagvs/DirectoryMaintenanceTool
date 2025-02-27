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
        private readonly DateTime _thresholdDate = DateTime.MinValue;

        public DirectoryDeleteProcessor(string rootPath, DateTime thresholdDate)
        {
            _thresholdDate = thresholdDate;
        }

        public void ProcessDirectories()
        {

        }
    }
}
