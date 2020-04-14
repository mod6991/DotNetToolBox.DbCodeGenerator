using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetToolBox.DbCodeGenerator.Core
{
    public class DbItem
    {
        public DbItem()
        {
            Fields = new List<DbField>();
        }

        public string ObjectName { get; set; }
        public string TableName { get; set; }
        public string Query { get; set; }
        public List<DbField> Fields { get; set; }
        public bool UseSelectAll { get; set; }
        public bool UseSelectById { get; set; }
        public bool UseInsert { get; set; }
        public bool UseUpdate{ get; set; }
        public bool UseDelete { get; set; }
    }
}
