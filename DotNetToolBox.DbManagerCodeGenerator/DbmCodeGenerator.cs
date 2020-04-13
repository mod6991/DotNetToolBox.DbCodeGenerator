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
        private List<DbItem> _dbItems;
        private CodeGenerationSettings _codeGenerationSettings;
        private string _objectsNamespace;
        private string _dbLayerNamespace;
        private string _dbLayerObjectName;
        
        private string _outputPath;

        public DbmCodeGenerator(List<DbItem> dbItems, CodeGenerationSettings codeGenerationSettings, string objectsNamespace, string dbLayerNamespace, string dbLayerObjectName, string outputPath)
        {
            _dbItems = dbItems;
            _codeGenerationSettings = codeGenerationSettings;
            _objectsNamespace = objectsNamespace;
            _dbLayerNamespace = dbLayerNamespace;
            _dbLayerObjectName = dbLayerObjectName;
            _outputPath = outputPath;
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
