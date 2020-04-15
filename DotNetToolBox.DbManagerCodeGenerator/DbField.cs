using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetToolBox.DbManagerCodeGenerator
{
    public class DbField
    {
        private static readonly char[] US = new char[] { '_' };

        public DbField(string dataType, string dbFieldName)
        {
            DataType = dataType;
            DbFieldName = dbFieldName;

            SetNames();
        }

        public string DataType { get; set; }
        public string DbFieldName { get; set; }
        public string PropertyName { get; set; }
        public string ParameterName { get; set; }

        private void SetNames()
        {
            string baseName = string.Empty;
            string[] splitted = DbFieldName.Split(US, StringSplitOptions.RemoveEmptyEntries);

            foreach (string s in splitted)
                baseName += char.ToUpper(s[0]) + s.Substring(1).ToLower();

            PropertyName = char.ToUpper(baseName[0]) + baseName.Substring(1);
            ParameterName = char.ToLower(baseName[0]) + baseName.Substring(1);
        }
    }
}
