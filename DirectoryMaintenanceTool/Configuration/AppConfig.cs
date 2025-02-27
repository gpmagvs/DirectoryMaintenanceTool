namespace DirectoryMaintenanceTool.Configuration
{
    public class FolderConfig
    {
        public string Directory { get; set; } = string.Empty;
        public int DeleteThresholdDays { get; set; }
    }

    public class AppConfig
    {
        public List<FolderConfig> Folders { get; set; } = new List<FolderConfig>();
    }
}