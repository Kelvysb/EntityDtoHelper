using BDataBaseStandard;

namespace EntityHelper.Models
{

    public class Connection
    {
        public string Name { get; set; }
        public DataBase.enmDataBaseType Type { get; set; }
        public string ConnectionString { get; set; }
        public string  Server { get; set; }
        public string  DataBase { get; set; }
        public string  User { get; set; }
        public string  Password { get; set; }

    }
}