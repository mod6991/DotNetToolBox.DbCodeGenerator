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
        public List<DbField> Fields { get; set; }
        public bool UseSelectAll { get; set; }
        public bool UseSelectById { get; set; }
        public bool UseInsert { get; set; }
        public bool UseUpdateSingle { get; set; }
        public bool UseUpdateAll { get; set; }
        public bool UseDelete { get; set; }
    }
}
