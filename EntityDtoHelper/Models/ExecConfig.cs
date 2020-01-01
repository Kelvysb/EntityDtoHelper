namespace EntityHelper.Models
{
    public class ExecConfig
    {
        public Connection connection { get; set; }
        public bool AllTables { get; set; }
        public bool IncludeViews { get; set; }
        public string Table { get; set; }
        public string DtoFolder { get; set; }
        public string EntityFolder { get; set; }
        public string DtoNamespace { get; set; }
        public string EntityNamespace { get; set; }
        public bool UseEntityFramework { get; set; }
    }
}