using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DotNetToolBox.DbManagerCodeGenerator
{
    public class DbmCodeGenerator
    {
        private IEnumerable<DbItem> _dbItems;
        private CodeGenerationSettings _codeGenerationSettings;
        private string _objectsNamespace;
        private string _dbLayerNamespace;
        private string _dbLayerObjectName;
        private string _parameterPrefix;
        private Encoding _encoding;
        
        private string _outputPath;

        public DbmCodeGenerator(IEnumerable<DbItem> dbItems, CodeGenerationSettings codeGenerationSettings, string objectsNamespace, string dbLayerNamespace, string dbLayerObjectName, string parameterPrefix, string outputPath)
        {
            _dbItems = dbItems;
            _codeGenerationSettings = codeGenerationSettings;
            _objectsNamespace = objectsNamespace;
            _dbLayerNamespace = dbLayerNamespace;
            _dbLayerObjectName = dbLayerObjectName;
            _parameterPrefix = parameterPrefix;
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

        public void Generate(bool useReflection)
        {
            GenerateObjects();
            
            if (useReflection)
            {
                GenerateDbLayerHome();
                GenerateDbLayerItems();
            }
            else
            {

            }

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
                        sw.WriteLine("using DotNetToolBox.Database;");
                        sw.WriteLine("using System;");
                        sw.WriteLine("using System.Collections.Generic;");
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
            string file = Path.Combine(_outputPath, $"{_dbLayerObjectName}.cs");

            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, _encoding))
                {
                    sw.WriteLine("using DotNetToolBox.Database;");
                    sw.WriteLine("using log4net;");
                    sw.WriteLine($"using {_objectsNamespace};");
                    sw.WriteLine("using System;");
                    sw.WriteLine("using System.IO;");
                    sw.WriteLine();
                    sw.WriteLine($"namespace {_dbLayerNamespace}");
                    sw.WriteLine("{"); //start namespace
                    sw.WriteLine($"{WriteIndentCs(1)}public partial class {_dbLayerObjectName}");
                    sw.WriteLine($"{WriteIndentCs(1)}{{"); //start class

                    sw.WriteLine($"{WriteIndentCs(2)}private string _connectionString;");
                    sw.WriteLine($"{WriteIndentCs(2)}private string _provider;");
                    sw.WriteLine($"{WriteIndentCs(2)}private string _requestDirectory;");
                    sw.WriteLine($"{WriteIndentCs(2)}private ILog _logger;");
                    sw.WriteLine($"{WriteIndentCs(2)}private DbManager _db;");
                    sw.WriteLine();

                    sw.WriteLine($"{WriteIndentCs(2)}public {_dbLayerObjectName}(string connectionString, string provider, string requestDirectory, ILog logger)");
                    sw.WriteLine($"{WriteIndentCs(2)}{{"); //start ctor
                    sw.WriteLine($"{WriteIndentCs(3)}_connectionString = connectionString;");
                    sw.WriteLine($"{WriteIndentCs(3)}_provider = provider;");
                    sw.WriteLine($"{WriteIndentCs(3)}_requestDirectory = requestDirectory;");
                    sw.WriteLine($"{WriteIndentCs(3)}_logger = logger;");
                    sw.WriteLine($"{WriteIndentCs(3)}_db = new DbManager(_connectionString, _provider);");
                    sw.WriteLine();
                    sw.WriteLine($"{WriteIndentCs(3)}RegisterDbObjects();");
                    sw.WriteLine($"{WriteIndentCs(3)}ReadRequestFiles();");
                    sw.WriteLine($"{WriteIndentCs(2)}}}"); //end ctor
                    sw.WriteLine();

                    sw.WriteLine($"{WriteIndentCs(2)}public string ConnectionString");
                    sw.WriteLine($"{WriteIndentCs(2)}{{");
                    sw.WriteLine($"{WriteIndentCs(3)}get {{ return _connectionString; }}");
                    sw.WriteLine($"{WriteIndentCs(3)}set {{ _connectionString = value; }}");
                    sw.WriteLine($"{WriteIndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{WriteIndentCs(2)}public string Provider");
                    sw.WriteLine($"{WriteIndentCs(2)}{{");
                    sw.WriteLine($"{WriteIndentCs(3)}get {{ return _provider; }}");
                    sw.WriteLine($"{WriteIndentCs(3)}set {{ _provider = value; }}");
                    sw.WriteLine($"{WriteIndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{WriteIndentCs(2)}public DbManager DbManager");
                    sw.WriteLine($"{WriteIndentCs(2)}{{");
                    sw.WriteLine($"{WriteIndentCs(3)}get {{ return _db; }}");
                    sw.WriteLine($"{WriteIndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{WriteIndentCs(2)}public void Open()");
                    sw.WriteLine($"{WriteIndentCs(2)}{{");
                    sw.WriteLine($"{WriteIndentCs(3)}_db.Open();");
                    sw.WriteLine($"{WriteIndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{WriteIndentCs(2)}public void Close()");
                    sw.WriteLine($"{WriteIndentCs(2)}{{");
                    sw.WriteLine($"{WriteIndentCs(3)}_db.Close();");
                    sw.WriteLine($"{WriteIndentCs(2)}}}");
                    sw.WriteLine();

                    sw.WriteLine($"{WriteIndentCs(2)}private void RegisterDbObjects()");
                    sw.WriteLine($"{WriteIndentCs(2)}{{"); //start RegisterDbObjects
                    sw.WriteLine($"{WriteIndentCs(3)}try");
                    sw.WriteLine($"{WriteIndentCs(3)}{{");
                    foreach(DbItem dbi in _dbItems)
                        sw.WriteLine($"{WriteIndentCs(4)}_db.RegisterDbObject(typeof({dbi.ObjectName}));");
                    sw.WriteLine($"{WriteIndentCs(3)}}}");
                    sw.WriteLine($"{WriteIndentCs(3)}catch (Exception ex)");
                    sw.WriteLine($"{WriteIndentCs(3)}{{");
                    sw.WriteLine($"{WriteIndentCs(4)}_logger.Fatal(\"An error occured while registering db objects\", ex);");
                    sw.WriteLine($"{WriteIndentCs(3)}}}");
                    sw.WriteLine($"{WriteIndentCs(2)}}}"); //end RegisterDbObjects
                    sw.WriteLine();
                    
                    sw.WriteLine($"{WriteIndentCs(2)}private void ReadRequestFiles()");
                    sw.WriteLine($"{WriteIndentCs(2)}{{"); //start RegisterDbObjects
                    sw.WriteLine($"{WriteIndentCs(3)}try");
                    sw.WriteLine($"{WriteIndentCs(3)}{{");
                    foreach(DbItem dbi in _dbItems)
                        sw.WriteLine($"{WriteIndentCs(4)}_db.AddRequestFile(\"{dbi.ObjectName}\", Path.Combine(_requestDirectory, \"{dbi.ObjectName}.xml\"));");
                    sw.WriteLine($"{WriteIndentCs(3)}}}");
                    sw.WriteLine($"{WriteIndentCs(3)}catch (Exception ex)");
                    sw.WriteLine($"{WriteIndentCs(3)}{{");
                    sw.WriteLine($"{WriteIndentCs(4)}_logger.Fatal(\"An error occured while reading request files\", ex);");
                    sw.WriteLine($"{WriteIndentCs(3)}}}");
                    sw.WriteLine($"{WriteIndentCs(2)}}}"); //end RegisterDbObjects
                    
                    sw.WriteLine($"{WriteIndentCs(1)}}}"); //end class
                    sw.WriteLine("}"); //end namespace
                }
            }
        }

        private void GenerateDbLayerItems()
        {
            foreach(DbItem dbi in _dbItems)
            {
                string file = Path.Combine(_outputPath, $"{_dbLayerObjectName}.{dbi.ObjectName}.cs");
                string minObjName = char.ToLower(dbi.ObjectName[0]) + dbi.ObjectName.Substring(1);

                using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, _encoding))
                    {
                        bool first = true;

                        sw.WriteLine($"using {_objectsNamespace};");
                        sw.WriteLine("using System;");
                        sw.WriteLine("using System.Collections.Generic;");
                        sw.WriteLine("using System.Data.Common;");
                        sw.WriteLine();
                        sw.WriteLine($"namespace {_dbLayerNamespace}");
                        sw.WriteLine("{"); //start namespace
                        sw.WriteLine($"{WriteIndentCs(1)}public partial class {_dbLayerObjectName}");
                        sw.WriteLine($"{WriteIndentCs(1)}{{"); //start class

                        if (dbi.UseSelectAll)
                        {
                            sw.WriteLine($"{WriteIndentCs(2)}public List<{dbi.ObjectName}> SelectAll{dbi.ObjectName}s()");
                            sw.WriteLine($"{WriteIndentCs(2)}{{"); //start SelectAll
                            sw.WriteLine($"{WriteIndentCs(3)}List<{dbi.ObjectName}> list = _db[\"{dbi.ObjectName}\"].FillObjects<{dbi.ObjectName}>(\"SelectAll{dbi.ObjectName}s\", null);");
                            sw.WriteLine($"{WriteIndentCs(3)}return list;");
                            sw.WriteLine($"{WriteIndentCs(2)}}}"); //end SelectAll
                            first = false;
                        }

                        if (dbi.UseSelectById)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{WriteIndentCs(2)}public {dbi.ObjectName} Select{dbi.ObjectName}ById({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"{WriteIndentCs(2)}{{"); //start SelectById
                            sw.WriteLine($"{WriteIndentCs(3)}List<DbParameter> parameters = new List<DbParameter>();");
                            sw.WriteLine($"{WriteIndentCs(3)}parameters.Add(_db.CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine($"{WriteIndentCs(3)}List<{dbi.ObjectName}> list = _db[\"{dbi.ObjectName}\"].FillObjects<{dbi.ObjectName}>(\"Select{dbi.ObjectName}ById\", parameters);");
                            sw.WriteLine($"{WriteIndentCs(3)}if (list.Count == 0)");
                            sw.WriteLine($"{WriteIndentCs(4)}return null;");
                            sw.WriteLine($"{WriteIndentCs(3)}return list[0];");
                            sw.WriteLine($"{WriteIndentCs(2)}}}"); //end SelectById
                            first = false;
                        }

                        if (dbi.UseInsert)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{WriteIndentCs(2)}public void Insert{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"{WriteIndentCs(2)}{{"); //start Insert
                            sw.WriteLine($"{WriteIndentCs(3)}List<DbParameter> parameters = new List<DbParameter>();");

                            foreach (DbField field in dbi.Fields)
                                sw.WriteLine($"{WriteIndentCs(3)}parameters.Add(_db.CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine($"{WriteIndentCs(3)}_db[\"{dbi.ObjectName}\"].ExecuteNonQuery(\"Insert{dbi.ObjectName}\", parameters);");
                            sw.WriteLine($"{WriteIndentCs(2)}}}"); //end Insert
                            first = false;
                        }

                        if (dbi.UseUpdate)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{WriteIndentCs(2)}public void Update{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"{WriteIndentCs(2)}{{"); //start Update
                            sw.WriteLine($"{WriteIndentCs(3)}List<DbParameter> parameters = new List<DbParameter>();");

                            foreach (DbField field in dbi.Fields)
                                sw.WriteLine($"{WriteIndentCs(3)}parameters.Add(_db.CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine($"{WriteIndentCs(3)}_db[\"{dbi.ObjectName}\"].ExecuteNonQuery(\"Update{dbi.ObjectName}\", parameters);");
                            sw.WriteLine($"{WriteIndentCs(2)}}}"); //end Update
                            first = false;
                        }

                        if (dbi.UseDelete)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{WriteIndentCs(2)}public void Delete{dbi.ObjectName}({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"{WriteIndentCs(2)}{{"); //start Delete
                            sw.WriteLine($"{WriteIndentCs(3)}List<DbParameter> parameters = new List<DbParameter>();");
                            sw.WriteLine($"{WriteIndentCs(3)}parameters.Add(_db.CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine($"{WriteIndentCs(3)}_db[\"{dbi.ObjectName}\"].ExecuteNonQuery(\"Delete{dbi.ObjectName}\", parameters);");
                            sw.WriteLine($"{WriteIndentCs(2)}}}"); //end Delete
                            first = false;
                        }

                        sw.WriteLine($"{WriteIndentCs(1)}}}"); //end class
                        sw.WriteLine("}"); //end namespace
                    }
                }
            }
        }

        private void GenerateQueries()
        {
            string queriesDir = Path.Combine(_outputPath, "Queries");
            if (!Directory.Exists(queriesDir))
                Directory.CreateDirectory(queriesDir);

            foreach(DbItem dbi in _dbItems)
            {
                string file = Path.Combine(queriesDir, $"{dbi.ObjectName}.xml");

                using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
                        sw.WriteLine("<Requests>");

                        if (dbi.UseSelectAll)
                        {
                            sw.WriteLine($"{WriteIndentSql(1)}<Request Name=\"SelectAll{dbi.ObjectName}s\">");
                            sw.WriteLine($"{WriteIndentSql(2)}SELECT");

                            for(int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.WriteLine($"{WriteIndentSql(3)}{dbi.Fields[i].DbFieldName},");
                                else
                                    sw.WriteLine($"{WriteIndentSql(3)}{dbi.Fields[i].DbFieldName}");
                            }

                            sw.WriteLine($"{WriteIndentSql(2)}FROM");
                            sw.WriteLine($"{WriteIndentSql(3)}{dbi.TableName}");
                            sw.WriteLine($"{WriteIndentSql(1)}</Request>");
                        }

                        if (dbi.UseSelectById)
                        {
                            sw.WriteLine($"{WriteIndentSql(1)}<Request Name=\"Select{dbi.ObjectName}ById\">");
                            sw.WriteLine($"{WriteIndentSql(2)}SELECT");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.WriteLine($"{WriteIndentSql(3)}{dbi.Fields[i].DbFieldName},");
                                else
                                    sw.WriteLine($"{WriteIndentSql(3)}{dbi.Fields[i].DbFieldName}");
                            }

                            sw.WriteLine($"{WriteIndentSql(2)}FROM");
                            sw.WriteLine($"{WriteIndentSql(3)}{dbi.TableName}");
                            sw.WriteLine($"{WriteIndentSql(2)}WHERE");
                            sw.WriteLine($"{WriteIndentSql(3)}{dbi.Fields[0].DbFieldName} = {_parameterPrefix}{dbi.Fields[0].ParameterName}");
                            sw.WriteLine($"{WriteIndentSql(1)}</Request>");
                        }

                        if (dbi.UseInsert)
                        {
                            sw.WriteLine($"{WriteIndentSql(1)}<Request Name=\"Insert{dbi.ObjectName}\">");
                            sw.Write($"{WriteIndentSql(2)}INSERT INTO {dbi.TableName} (");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.Write($"{dbi.Fields[i].DbFieldName}, ");
                                else
                                    sw.WriteLine($"{dbi.Fields[i].DbFieldName})");
                            }

                            sw.Write($"{WriteIndentSql(2)}VALUES (");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.Write($"{_parameterPrefix}{dbi.Fields[i].ParameterName}, ");
                                else
                                    sw.WriteLine($"{_parameterPrefix}{dbi.Fields[i].ParameterName})");
                            }

                            sw.WriteLine($"{WriteIndentSql(1)}</Request>");
                        }

                        if (dbi.UseUpdate)
                        {
                            sw.WriteLine($"{WriteIndentSql(1)}<Request Name=\"Update{dbi.ObjectName}\">");
                            sw.WriteLine($"{WriteIndentSql(2)}UPDATE {dbi.TableName} SET");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.WriteLine($"{WriteIndentSql(3)}{dbi.Fields[i].DbFieldName} = {_parameterPrefix}{dbi.Fields[i].ParameterName},");
                                else
                                    sw.WriteLine($"{WriteIndentSql(3)}{dbi.Fields[i].DbFieldName} = {_parameterPrefix}{dbi.Fields[i].ParameterName}");
                            }

                            sw.WriteLine($"{WriteIndentSql(2)}WHERE");
                            sw.WriteLine($"{WriteIndentSql(3)}{dbi.Fields[0].DbFieldName} = {_parameterPrefix}{dbi.Fields[0].ParameterName}");
                            sw.WriteLine($"{WriteIndentSql(1)}</Request>");
                        }

                        if (dbi.UseDelete)
                        {
                            sw.WriteLine($"{WriteIndentSql(1)}<Request Name=\"Delete{dbi.ObjectName}\">");
                            sw.WriteLine($"{WriteIndentSql(2)}DELETE FROM {dbi.TableName}");
                            sw.WriteLine($"{WriteIndentSql(2)}WHERE");
                            sw.WriteLine($"{WriteIndentSql(3)}{dbi.Fields[0].DbFieldName} = {_parameterPrefix}{dbi.Fields[0].ParameterName}");
                            sw.WriteLine($"{WriteIndentSql(1)}</Request>");
                        }

                        sw.WriteLine("</Requests>");
                    }
                }
            }
        }

        private string WriteIndentCs(int nbIndent)
        {
            return new string(_codeGenerationSettings.CSharpIndentType == "SPACES" ? ' ' : '\t', _codeGenerationSettings.CSharpIndentSize * nbIndent);
        }

        private string WriteIndentSql(int nbIndent)
        {
            return new string(_codeGenerationSettings.SqlIndentType == "SPACES" ? ' ' : '\t', _codeGenerationSettings.SqlIndentSize * nbIndent);
        }
    }
}
