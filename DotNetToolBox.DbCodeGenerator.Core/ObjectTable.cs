using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetToolBox.DbCodeGenerator.Core
{
    public class ObjectTable
    {
        public ObjectTable()
        {

        }

        public ObjectTable(string objectName, string tableName, string query)
        {
            ObjectName = objectName;
            TableName = tableName;
            Query = query;
        }

        public string ObjectName { get; set; }
        public string TableName { get; set; }
        public string Query { get; set; }
    }
}
