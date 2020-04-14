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


        }

        private void GenerateDbLayerHome()
        {

        }

        private void GenerateDbLayerItems()
        {

        }

        private void GenerateQueries()
        {
            string queriesDir = Path.Combine(_outputPath, "Queries");
            if (!Directory.Exists(queriesDir))
                Directory.CreateDirectory(queriesDir);
        }
    }
}
