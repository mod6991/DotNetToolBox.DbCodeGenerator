using DotNetToolBox.DbCodeGenerator.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetToolBox.DbManagerCodeGenerator
{
    public class DbmCodeGenerator
    {
        private IEnumerable<DbItem> _dbItems;
        private CodeGenerationSettings _codeGenerationSettings;
        private string _objectsNamespace;
        private string _dbLayerNamespace;
        private string _dbLayerObjectName;
        private Encoding _encoding;
        
        private string _outputPath;

        public DbmCodeGenerator(IEnumerable<DbItem> dbItems, CodeGenerationSettings codeGenerationSettings, string objectsNamespace, string dbLayerNamespace, string dbLayerObjectName, string outputPath)
        {
            _dbItems = dbItems;
            _codeGenerationSettings = codeGenerationSettings;
            _objectsNamespace = objectsNamespace;
            _dbLayerNamespace = dbLayerNamespace;
            _dbLayerObjectName = dbLayerObjectName;
            _outputPath = outputPath;
            _encoding = Encoding.GetEncoding(_codeGenerationSettings.CSharpFilesCodePage);
            
            if (_codeGenerationSettings.CSharpIndentType != "SPACES" && _codeGenerationSettings.CSharpIndentType != "TABS")
                throw new NotSupportedException($"CSharpIndentType '{_codeGenerationSettings.CSharpIndentType}' not supported");
            if (_codeGenerationSettings.CSharpIndentSize < 0)
                throw new NotSupportedException($"CSharpIndentSize cannot be negative");
            if (_codeGenerationSettings.SqlIndentType != "SPACES" && _codeGenerationSettings.SqlIndentType != "TABS")
                throw new NotSupportedException($"SqlIndentType '{_codeGenerationSettings.SqlIndentType}' not supported");
            if (_codeGenerationSettings.SqlIndentSize < 0)
                throw new NotSupportedException($"SqlIndentSize cannot be negative");
        }

        public void Generate()
        {
            GenerateObjects();
            GenerateDbLayerHome();
            GenerateDbLayerItems();
            GenerateQueries();
        }

        private void GenerateObjects()
        {
            string objectsDir = Path.Combine(_outputPath, "Objects");
            if (!Directory.Exists(objectsDir))
                Directory.CreateDirectory(objectsDir);

            foreach(DbItem dbi in _dbItems)
            {
                string file = Path.Combine(objectsDir, $"{dbi.ObjectName}.cs");

                using(FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, _encoding))
                    {
                        sw.WriteLine("using System;");
                        sw.WriteLine("using System.Collections.Generic;");
                        sw.WriteLine("using DotNetToolBox.Database;");
                        sw.WriteLine();
                        sw.WriteLine($"namespace {_objectsNamespace}");
                        sw.WriteLine("{"); //start namespace
                        sw.WriteLine($"{WriteIndentCs(1)}public class {dbi.ObjectName} : IDbObject");
                        sw.WriteLine($"{WriteIndentCs(1)}{{"); //start class

                        foreach(DbField field in dbi.Fields)
                            sw.WriteLine($"{WriteIndentCs(2)}public {field.DataType} {field.PropertyName} {{ get; set; }}");

                        sw.WriteLine();

                        sw.WriteLine($"{WriteIndentCs(2)}public List<DbObjectMapping> GetMapping()");
                        sw.WriteLine($"{WriteIndentCs(2)}{{"); //start GetMapping
                        sw.WriteLine($"{WriteIndentCs(3)}List<DbObjectMapping> mapping = new List<DbObjectMapping>();");

                        foreach(DbField field in dbi.Fields)
                            sw.WriteLine($"{WriteIndentCs(3)}mapping.Add(new DbObjectMapping(\"{field.PropertyName}\",\"{field.DbFieldName}\"));");

                        sw.WriteLine($"{WriteIndentCs(3)}return mapping;");
                        sw.WriteLine($"{WriteIndentCs(2)}}}"); //end GetMapping

                        sw.WriteLine($"{WriteIndentCs(1)}}}"); //end class
                        sw.WriteLine("}"); //end namespace
                    }
                }
            }
        }

        private void GenerateDbLayerHome()
        {

        }

        private void GenerateDbLayerItems()
        {

        }

        private void GenerateQueries()
        {
            //string queriesDir = Path.Combine(_outputPath, "Queries");
            //if (!Directory.Exists(queriesDir))
            //    Directory.CreateDirectory(queriesDir);
        }

        private string WriteIndentCs(int nbIndent)
        {
            return new string(_codeGenerationSettings.CSharpIndentType == "SPACES" ? ' ' : '\t', _codeGenerationSettings.CSharpIndentSize * nbIndent);
        }
    }
}
