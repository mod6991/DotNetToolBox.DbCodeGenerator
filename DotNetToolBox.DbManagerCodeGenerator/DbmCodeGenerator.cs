using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

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
            GenerateQueries();
            
            if (useReflection)
            {
                GenerateObjects();
                GenerateDbLayerHome();
                GenerateDbLayerItems();
            }
            else
            {
                GenerateObjectsWithoutReflection();
                GenerateDbLayerHomeWithoutReflection();
                GenerateDbLayerItemsWithoutReflection();
            }
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
                        sw.WriteLine($"{IndentCs(1)}public class {dbi.ObjectName} : IDbObject");
                        sw.WriteLine($"{IndentCs(1)}{{"); //start class

                        foreach(DbField field in dbi.Fields)
                            sw.WriteLine($"{IndentCs(2)}public {field.DataType} {field.PropertyName} {{ get; set; }}");

                        sw.WriteLine();

                        sw.WriteLine($"{IndentCs(2)}public List<DbObjectMapping> GetMapping()");
                        sw.WriteLine($"{IndentCs(2)}{{"); //start GetMapping
                        sw.WriteLine($"{IndentCs(3)}return new List<DbObjectMapping>");
                        sw.WriteLine($"{IndentCs(3)}{{");

                        for(int i=0,l=dbi.Fields.Count; i < l; i++)
                        {
                            if(i < l - 1)
                                sw.WriteLine($"{IndentCs(4)}new DbObjectMapping(\"{dbi.Fields[i].PropertyName}\", \"{dbi.Fields[i].DbFieldName}\"),");
                            else
                                sw.WriteLine($"{IndentCs(4)}new DbObjectMapping(\"{dbi.Fields[i].PropertyName}\", \"{dbi.Fields[i].DbFieldName}\")");
                        }

                        sw.WriteLine($"{IndentCs(3)}}};");
                        sw.WriteLine($"{IndentCs(2)}}}"); //end GetMapping

                        sw.WriteLine($"{IndentCs(1)}}}"); //end class
                        sw.WriteLine("}"); //end namespace
                    }
                }
            }
        }

        private void GenerateObjectsWithoutReflection()
        {
            string objectsDir = Path.Combine(_outputPath, "Objects");
            if (!Directory.Exists(objectsDir))
                Directory.CreateDirectory(objectsDir);

            foreach (DbItem dbi in _dbItems)
            {
                string file = Path.Combine(objectsDir, $"{dbi.ObjectName}.cs");

                using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
                {
                    using (StreamWriter sw = new StreamWriter(fs, _encoding))
                    {
                        sw.WriteLine("using System;");
                        sw.WriteLine();
                        sw.WriteLine($"namespace {_objectsNamespace}");
                        sw.WriteLine("{"); //start namespace
                        sw.WriteLine($"{IndentCs(1)}public class {dbi.ObjectName} : IDbObject");
                        sw.WriteLine($"{IndentCs(1)}{{"); //start class

                        foreach (DbField field in dbi.Fields)
                            sw.WriteLine($"{IndentCs(2)}public {field.DataType} {field.PropertyName} {{ get; set; }}");

                        sw.WriteLine($"{IndentCs(1)}}}"); //end class
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

            foreach (DbItem dbi in _dbItems)
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
                            sw.WriteLine($"{IndentSql(1)}<Request Name=\"SelectAll{dbi.ObjectName}s\">");
                            sw.WriteLine($"{IndentSql(2)}SELECT");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.WriteLine($"{IndentSql(3)}{dbi.Fields[i].DbFieldName},");
                                else
                                    sw.WriteLine($"{IndentSql(3)}{dbi.Fields[i].DbFieldName}");
                            }

                            sw.WriteLine($"{IndentSql(2)}FROM");
                            sw.WriteLine($"{IndentSql(3)}{dbi.TableName}");
                            sw.WriteLine($"{IndentSql(1)}</Request>");
                        }

                        if (dbi.UseSelectById)
                        {
                            sw.WriteLine($"{IndentSql(1)}<Request Name=\"Select{dbi.ObjectName}ById\">");
                            sw.WriteLine($"{IndentSql(2)}SELECT");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.WriteLine($"{IndentSql(3)}{dbi.Fields[i].DbFieldName},");
                                else
                                    sw.WriteLine($"{IndentSql(3)}{dbi.Fields[i].DbFieldName}");
                            }

                            sw.WriteLine($"{IndentSql(2)}FROM");
                            sw.WriteLine($"{IndentSql(3)}{dbi.TableName}");
                            sw.WriteLine($"{IndentSql(2)}WHERE");
                            sw.WriteLine($"{IndentSql(3)}{dbi.Fields[0].DbFieldName} = {_parameterPrefix}{dbi.Fields[0].ParameterName}");
                            sw.WriteLine($"{IndentSql(1)}</Request>");
                        }

                        if (dbi.UseInsert)
                        {
                            sw.WriteLine($"{IndentSql(1)}<Request Name=\"Insert{dbi.ObjectName}\">");
                            sw.Write($"{IndentSql(2)}INSERT INTO {dbi.TableName} (");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.Write($"{dbi.Fields[i].DbFieldName}, ");
                                else
                                    sw.WriteLine($"{dbi.Fields[i].DbFieldName})");
                            }

                            sw.Write($"{IndentSql(2)}VALUES (");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.Write($"{_parameterPrefix}{dbi.Fields[i].ParameterName}, ");
                                else
                                    sw.WriteLine($"{_parameterPrefix}{dbi.Fields[i].ParameterName})");
                            }

                            sw.WriteLine($"{IndentSql(1)}</Request>");
                        }

                        if (dbi.UseUpdate)
                        {
                            sw.WriteLine($"{IndentSql(1)}<Request Name=\"Update{dbi.ObjectName}\">");
                            sw.WriteLine($"{IndentSql(2)}UPDATE {dbi.TableName} SET");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.WriteLine($"{IndentSql(3)}{dbi.Fields[i].DbFieldName} = {_parameterPrefix}{dbi.Fields[i].ParameterName},");
                                else
                                    sw.WriteLine($"{IndentSql(3)}{dbi.Fields[i].DbFieldName} = {_parameterPrefix}{dbi.Fields[i].ParameterName}");
                            }

                            sw.WriteLine($"{IndentSql(2)}WHERE");
                            sw.WriteLine($"{IndentSql(3)}{dbi.Fields[0].DbFieldName} = {_parameterPrefix}{dbi.Fields[0].ParameterName}");
                            sw.WriteLine($"{IndentSql(1)}</Request>");
                        }

                        if (dbi.UseDelete)
                        {
                            sw.WriteLine($"{IndentSql(1)}<Request Name=\"Delete{dbi.ObjectName}\">");
                            sw.WriteLine($"{IndentSql(2)}DELETE FROM {dbi.TableName}");
                            sw.WriteLine($"{IndentSql(2)}WHERE");
                            sw.WriteLine($"{IndentSql(3)}{dbi.Fields[0].DbFieldName} = {_parameterPrefix}{dbi.Fields[0].ParameterName}");
                            sw.WriteLine($"{IndentSql(1)}</Request>");
                        }

                        sw.WriteLine("</Requests>");
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
                    sw.WriteLine($"{IndentCs(1)}public partial class {_dbLayerObjectName}");
                    sw.WriteLine($"{IndentCs(1)}{{"); //start class

                    sw.WriteLine($"{IndentCs(2)}private string _connectionString;");
                    sw.WriteLine($"{IndentCs(2)}private string _provider;");
                    sw.WriteLine($"{IndentCs(2)}private string _requestDirectory;");
                    sw.WriteLine($"{IndentCs(2)}private ILog _logger;");
                    sw.WriteLine($"{IndentCs(2)}private DbManager _db;");
                    sw.WriteLine();

                    //Constructor
                    sw.WriteLine($"{IndentCs(2)}public {_dbLayerObjectName}(string connectionString, string provider, string requestDirectory, ILog logger)");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}_connectionString = connectionString;");
                    sw.WriteLine($"{IndentCs(3)}_provider = provider;");
                    sw.WriteLine($"{IndentCs(3)}_requestDirectory = requestDirectory;");
                    sw.WriteLine($"{IndentCs(3)}_logger = logger;");
                    sw.WriteLine($"{IndentCs(3)}_db = new DbManager(_connectionString, _provider);");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(3)}RegisterDbObjects();");
                    sw.WriteLine($"{IndentCs(3)}ReadRequestFiles();");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //Properties
                    sw.WriteLine($"{IndentCs(2)}public DbManager DbManager");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}get {{ return _db; }}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //Open
                    sw.WriteLine($"{IndentCs(2)}public void Open()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}_db.Open();");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //Close
                    sw.WriteLine($"{IndentCs(2)}public void Close()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}_db.Close();");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //RegisterDbObjects
                    sw.WriteLine($"{IndentCs(2)}private void RegisterDbObjects()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}try");
                    sw.WriteLine($"{IndentCs(3)}{{");
                    foreach(DbItem dbi in _dbItems)
                        sw.WriteLine($"{IndentCs(4)}_db.RegisterDbObject(typeof({dbi.ObjectName}));");
                    sw.WriteLine($"{IndentCs(3)}}}");
                    sw.WriteLine($"{IndentCs(3)}catch (Exception ex)");
                    sw.WriteLine($"{IndentCs(3)}{{");
                    sw.WriteLine($"{IndentCs(4)}_logger.Fatal(\"An error occured while registering db objects\", ex);");
                    sw.WriteLine($"{IndentCs(3)}}}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();
                    
                    //ReadRequestFiles
                    sw.WriteLine($"{IndentCs(2)}private void ReadRequestFiles()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}try");
                    sw.WriteLine($"{IndentCs(3)}{{");
                    foreach(DbItem dbi in _dbItems)
                        sw.WriteLine($"{IndentCs(4)}_db.AddRequestFile(\"{dbi.ObjectName}\", Path.Combine(_requestDirectory, \"{dbi.ObjectName}.xml\"));");
                    sw.WriteLine($"{IndentCs(3)}}}");
                    sw.WriteLine($"{IndentCs(3)}catch (Exception ex)");
                    sw.WriteLine($"{IndentCs(3)}{{");
                    sw.WriteLine($"{IndentCs(4)}_logger.Fatal(\"An error occured while reading request files\", ex);");
                    sw.WriteLine($"{IndentCs(3)}}}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    
                    sw.WriteLine($"{IndentCs(1)}}}"); //end class
                    sw.WriteLine("}"); //end namespace
                }
            }
        }

        private void GenerateDbLayerHomeWithoutReflection()
        {
            string file = Path.Combine(_outputPath, $"{_dbLayerObjectName}.cs");

            using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                using (StreamWriter sw = new StreamWriter(fs, _encoding))
                {
                    sw.WriteLine($"using {_objectsNamespace};");
                    sw.WriteLine("using System;");
                    sw.WriteLine("using System.Data;");
                    sw.WriteLine("using System.Data.Common;");
                    sw.WriteLine("using System.IO;");
                    sw.WriteLine();
                    sw.WriteLine($"namespace {_dbLayerNamespace}");
                    sw.WriteLine("{"); //start namespace
                    sw.WriteLine($"{IndentCs(1)}public partial class {_dbLayerObjectName}");
                    sw.WriteLine($"{IndentCs(1)}{{"); //start class

                    sw.WriteLine($"{IndentCs(2)}private bool _disposed;");
                    sw.WriteLine($"{IndentCs(2)}private string _connectionString;");
                    sw.WriteLine($"{IndentCs(2)}private string _provider;");
                    sw.WriteLine($"{IndentCs(2)}private DbProviderFactory _factory;");
                    sw.WriteLine($"{IndentCs(2)}private DbConnection _connection;");
                    sw.WriteLine($"{IndentCs(2)}private DbTransaction _transaction;");
                    sw.WriteLine($"{IndentCs(2)}private Dictionary<string, Dictionary<string, string>> _requests;");
                    sw.WriteLine();

                    //Constructor
                    sw.WriteLine($"{IndentCs(2)}public {_dbLayerObjectName}(string connectionString, string provider)");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}_connectionString = connectionString;");
                    sw.WriteLine($"{IndentCs(3)}_provider = provider;");
                    sw.WriteLine($"{IndentCs(3)}_factory = DbProviderFactories.GetFactory(_provider);");
                    sw.WriteLine($"{IndentCs(3)}_connection = _factory.CreateConnection();");
                    sw.WriteLine($"{IndentCs(3)}_connection.ConnectionString = _connectionString;");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //Destructor
                    sw.WriteLine($"{IndentCs(2)}~{_dbLayerObjectName}()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}Dispose(false);");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //Properties
                    sw.WriteLine($"{IndentCs(2)}public bool Disposed");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}get {{ return _disposed; }}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(2)}public string ConnectionString");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}get {{ return _connectionString; }}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(2)}public string Provider");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}get {{ return _provider; }}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(2)}public DbProviderFactory Factory");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}get {{ return _factory; }}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(2)}public DbConnection Connection");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}get {{ return _connection; }}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(2)}public DbTransaction Transaction");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}get {{ return _transaction; }}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(2)}public Dictionary<string, Dictionary<string, string>> Requests");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}get {{ return _requests; }}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //Open
                    sw.WriteLine($"{IndentCs(2)}public void Open()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}if (_disposed)");
                    sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(3)}_db.Open();");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //Close
                    sw.WriteLine($"{IndentCs(2)}public void Close()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}if (_disposed)");
                    sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(3)}_db.Close();");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //BeginTransaction
                    sw.WriteLine($"{IndentCs(2)}public void BeginTransaction()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}if (_disposed)");
                    sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(3)}_transaction = _connection.BeginTransaction();");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //EndTransaction
                    sw.WriteLine($"{IndentCs(2)}public void EndTransaction(bool commit)");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}if (_disposed)");
                    sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(3)}if (_transaction != null)");
                    sw.WriteLine($"{IndentCs(3)}{{");
                    sw.WriteLine($"{IndentCs(4)}if (commit)");
                    sw.WriteLine($"{IndentCs(5)}_transaction.Commit();");
                    sw.WriteLine($"{IndentCs(4)}else");
                    sw.WriteLine($"{IndentCs(5)}_transaction.Rollback();");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(4)}_transaction = null;");
                    sw.WriteLine($"{IndentCs(3)}}}");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //CreateParameter
                    sw.WriteLine($"{IndentCs(2)}public void CreateParameter()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}if (_disposed)");
                    sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(3)}return _factory.CreateParameter();");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //CreateParameter2
                    sw.WriteLine($"{IndentCs(2)}public void CreateParameter(string name, object value, ParameterDirection paramDirection = ParameterDirection.Input)");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}if (_disposed)");
                    sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(3)}DbParameter param = _factory.CreateParameter();");
                    sw.WriteLine($"{IndentCs(3)}param.ParameterName = name;");
                    sw.WriteLine($"{IndentCs(3)}param.Value = value;");
                    sw.WriteLine($"{IndentCs(3)}param.Direction = paramDirection;");
                    sw.WriteLine($"{IndentCs(3)}return param;");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //Dispose
                    sw.WriteLine($"{IndentCs(2)}public void Dispose()");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}Dispose(true);");
                    sw.WriteLine($"{IndentCs(3)}GC.SuppressFinalize(this);");
                    sw.WriteLine($"{IndentCs(2)}}}");
                    sw.WriteLine();

                    //Dispose2
                    sw.WriteLine($"{IndentCs(2)}public void Dispose(bool disposing)");
                    sw.WriteLine($"{IndentCs(2)}{{");
                    sw.WriteLine($"{IndentCs(3)}if (_disposed)");
                    sw.WriteLine($"{IndentCs(4)}return;");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(3)}if (disposing)");
                    sw.WriteLine($"{IndentCs(3)}{{"); //start if disposing
                    sw.WriteLine($"{IndentCs(4)}if (_connection != null)");
                    sw.WriteLine($"{IndentCs(4)}{{");
                    sw.WriteLine($"{IndentCs(5)}_connection.Dispose();");
                    sw.WriteLine($"{IndentCs(5)}_connection = null;");
                    sw.WriteLine($"{IndentCs(4)}}}");
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(4)}_transaction = null;");
                    sw.WriteLine($"{IndentCs(4)}_factory = null;");
                    sw.WriteLine($"{IndentCs(4)}_connectionString = null;");
                    sw.WriteLine($"{IndentCs(4)}_provider = null;");
                    sw.WriteLine($"{IndentCs(3)}}}"); //end if disposing
                    sw.WriteLine();
                    sw.WriteLine($"{IndentCs(3)}_disposed = true;");
                    sw.WriteLine($"{IndentCs(2)}}}");

                    sw.WriteLine($"{IndentCs(1)}}}"); //end class
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
                        sw.WriteLine($"{IndentCs(1)}public partial class {_dbLayerObjectName}");
                        sw.WriteLine($"{IndentCs(1)}{{"); //start class

                        if (dbi.UseSelectAll)
                        {
                            sw.WriteLine($"{IndentCs(2)}public List<{dbi.ObjectName}> SelectAll{dbi.ObjectName}s()");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start SelectAll
                            sw.WriteLine($"{IndentCs(3)}List<{dbi.ObjectName}> list = _db[\"{dbi.ObjectName}\"].FillObjects<{dbi.ObjectName}>(\"SelectAll{dbi.ObjectName}s\", null);");
                            sw.WriteLine($"{IndentCs(3)}return list;");
                            sw.WriteLine($"{IndentCs(2)}}}"); //end SelectAll
                            first = false;
                        }

                        if (dbi.UseSelectById)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{IndentCs(2)}public {dbi.ObjectName} Select{dbi.ObjectName}ById({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start SelectById
                            sw.WriteLine($"{IndentCs(3)}List<DbParameter> parameters = new List<DbParameter>();");
                            sw.WriteLine($"{IndentCs(3)}parameters.Add(_db.CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine($"{IndentCs(3)}List<{dbi.ObjectName}> list = _db[\"{dbi.ObjectName}\"].FillObjects<{dbi.ObjectName}>(\"Select{dbi.ObjectName}ById\", parameters);");
                            sw.WriteLine($"{IndentCs(3)}if (list.Count == 0)");
                            sw.WriteLine($"{IndentCs(4)}return null;");
                            sw.WriteLine($"{IndentCs(3)}return list[0];");
                            sw.WriteLine($"{IndentCs(2)}}}"); //end SelectById
                            first = false;
                        }

                        if (dbi.UseInsert)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{IndentCs(2)}public void Insert{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start Insert
                            sw.WriteLine($"{IndentCs(3)}List<DbParameter> parameters = new List<DbParameter>();");

                            foreach (DbField field in dbi.Fields)
                                sw.WriteLine($"{IndentCs(3)}parameters.Add(_db.CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine($"{IndentCs(3)}_db[\"{dbi.ObjectName}\"].ExecuteNonQuery(\"Insert{dbi.ObjectName}\", parameters);");
                            sw.WriteLine($"{IndentCs(2)}}}"); //end Insert
                            first = false;
                        }

                        if (dbi.UseUpdate)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{IndentCs(2)}public void Update{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start Update
                            sw.WriteLine($"{IndentCs(3)}List<DbParameter> parameters = new List<DbParameter>();");

                            foreach (DbField field in dbi.Fields)
                                sw.WriteLine($"{IndentCs(3)}parameters.Add(_db.CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine($"{IndentCs(3)}_db[\"{dbi.ObjectName}\"].ExecuteNonQuery(\"Update{dbi.ObjectName}\", parameters);");
                            sw.WriteLine($"{IndentCs(2)}}}"); //end Update
                            first = false;
                        }

                        if (dbi.UseDelete)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{IndentCs(2)}public void Delete{dbi.ObjectName}({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start Delete
                            sw.WriteLine($"{IndentCs(3)}List<DbParameter> parameters = new List<DbParameter>();");
                            sw.WriteLine($"{IndentCs(3)}parameters.Add(_db.CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine($"{IndentCs(3)}_db[\"{dbi.ObjectName}\"].ExecuteNonQuery(\"Delete{dbi.ObjectName}\", parameters);");
                            sw.WriteLine($"{IndentCs(2)}}}"); //end Delete
                            first = false;
                        }

                        sw.WriteLine($"{IndentCs(1)}}}"); //end class
                        sw.WriteLine("}"); //end namespace
                    }
                }
            }
        }

        private void GenerateDbLayerItemsWithoutReflection()
        {
            foreach (DbItem dbi in _dbItems)
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
                        sw.WriteLine("using System.Data;");
                        sw.WriteLine("using System.Data.Common;");
                        sw.WriteLine();
                        sw.WriteLine($"namespace {_dbLayerNamespace}");
                        sw.WriteLine("{"); //start namespace
                        sw.WriteLine($"{IndentCs(1)}public partial class {_dbLayerObjectName}");
                        sw.WriteLine($"{IndentCs(1)}{{"); //start class

                        if (dbi.UseSelectAll)
                        {
                            sw.WriteLine($"{IndentCs(2)}public List<{dbi.ObjectName}> SelectAll{dbi.ObjectName}s()");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start SelectAll
                            sw.WriteLine($"{IndentCs(3)}if (Disposed)");
                            sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(3)}List<{dbi.ObjectName}> list = new List<{dbi.ObjectName}>();");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(3)}using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"{IndentCs(3)}{{"); //start DbCommand
                            sw.WriteLine($"{IndentCs(4)}command.CommandType = CommandType.Text;");
                            sw.WriteLine($"{IndentCs(4)}command.CommandText = Requests[\"{dbi.ObjectName}\"][\"SelectAll{dbi.ObjectName}s\"];");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}using (DbDataReader reader = command.ExecuteReader())");
                            sw.WriteLine($"{IndentCs(4)}{{"); //start DbDataReader
                            sw.WriteLine($"{IndentCs(5)}while (reader.Read())");
                            sw.WriteLine($"{IndentCs(5)}{{"); //start reader.Read
                            sw.WriteLine($"{IndentCs(6)}{dbi.ObjectName} obj = new {dbi.ObjectName}();");

                            foreach (DbField field in dbi.Fields)
                            {
                                sw.WriteLine($"{IndentCs(6)}if (!(reader[\"{field.DbFieldName}\"] is DBNull))");
                                sw.WriteLine($"{IndentCs(7)}obj.{field.PropertyName} = ({field.DataType})reader[\"{field.DbFieldName}\"];");
                            }

                            sw.WriteLine($"{IndentCs(6)}list.Add(obj);");
                            sw.WriteLine($"{IndentCs(5)}}}"); //end reader.Read
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(5)}reader.Close();");
                            sw.WriteLine($"{IndentCs(4)}}}"); //end DbDataReader
                            sw.WriteLine($"{IndentCs(3)}}}"); //end DbCommand
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(3)}return list;");
                            sw.WriteLine($"{IndentCs(2)}}}"); //end SelectAll
                            first = false;
                        }

                        if (dbi.UseSelectById)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{IndentCs(2)}public {dbi.ObjectName} Select{dbi.ObjectName}ById({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start SelectById
                            sw.WriteLine($"{IndentCs(3)}if (Disposed)");
                            sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(3)}List<{dbi.ObjectName}> list = new List<{dbi.ObjectName}>();");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(3)}using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"{IndentCs(3)}{{"); //start DbCommand
                            sw.WriteLine($"{IndentCs(4)}command.CommandType = CommandType.Text;");
                            sw.WriteLine($"{IndentCs(4)}command.CommandText = Requests[\"{dbi.ObjectName}\"][\"Select{dbi.ObjectName}ById\"];");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}command.Parameters.Add(CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}using (DbDataReader reader = command.ExecuteReader())");
                            sw.WriteLine($"{IndentCs(4)}{{"); //start DbDataReader
                            sw.WriteLine($"{IndentCs(5)}while (reader.Read())");
                            sw.WriteLine($"{IndentCs(5)}{{"); //start reader.Read
                            sw.WriteLine($"{IndentCs(6)}{dbi.ObjectName} obj = new {dbi.ObjectName}();");

                            foreach (DbField field in dbi.Fields)
                            {
                                sw.WriteLine($"{IndentCs(6)}if (!(reader[\"{field.DbFieldName}\"] is DBNull))");
                                sw.WriteLine($"{IndentCs(7)}obj.{field.PropertyName} = ({field.DataType})reader[\"{field.DbFieldName}\"];");
                            }

                            sw.WriteLine($"{IndentCs(6)}list.Add(obj);");
                            sw.WriteLine($"{IndentCs(5)}}}"); //end reader.Read
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(5)}reader.Close();");
                            sw.WriteLine($"{IndentCs(4)}}}"); //end DbDataReader
                            sw.WriteLine($"{IndentCs(3)}}}"); //end DbCommand
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(3)}if (list.Count == 0)");
                            sw.WriteLine($"{IndentCs(4)}return null;");
                            sw.WriteLine($"{IndentCs(3)}return list[0];");
                            sw.WriteLine($"{IndentCs(2)}}}"); //end SelectById
                            first = false;
                        }

                        if (dbi.UseInsert)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{IndentCs(2)}public int Insert{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start Insert
                            sw.WriteLine($"{IndentCs(3)}if (Disposed)");
                            sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(3)}using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"{IndentCs(3)}{{"); //start DbCommand
                            sw.WriteLine($"{IndentCs(4)}command.CommandType = CommandType.Text;");
                            sw.WriteLine($"{IndentCs(4)}command.CommandText = Requests[\"{dbi.ObjectName}\"][\"Insert{dbi.ObjectName}\"];");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}if (Transaction != null)");
                            sw.WriteLine($"{IndentCs(5)}command.Transaction = Transaction;");
                            sw.WriteLine();

                            foreach(DbField field in dbi.Fields)
                                sw.WriteLine($"{IndentCs(4)}command.Parameters.Add(CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}return command.ExecuteNonQuery();");
                            sw.WriteLine($"{IndentCs(3)}}}"); //start DbCommand
                            sw.WriteLine($"{IndentCs(2)}}}"); //end Insert
                            first = false;
                        }

                        if (dbi.UseUpdate)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{IndentCs(2)}public int Update{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start Update
                            sw.WriteLine($"{IndentCs(3)}if (Disposed)");
                            sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(3)}using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"{IndentCs(3)}{{"); //start DbCommand
                            sw.WriteLine($"{IndentCs(4)}command.CommandType = CommandType.Text;");
                            sw.WriteLine($"{IndentCs(4)}command.CommandText = Requests[\"{dbi.ObjectName}\"][\"Update{dbi.ObjectName}\"];");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}if (Transaction != null)");
                            sw.WriteLine($"{IndentCs(5)}command.Transaction = Transaction;");
                            sw.WriteLine();

                            foreach (DbField field in dbi.Fields)
                                sw.WriteLine($"{IndentCs(4)}command.Parameters.Add(CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}return command.ExecuteNonQuery();");
                            sw.WriteLine($"{IndentCs(3)}}}"); //start DbCommand
                            sw.WriteLine($"{IndentCs(2)}}}"); //end Update
                            first = false;
                        }

                        if (dbi.UseDelete)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"{IndentCs(2)}public int Delete{dbi.ObjectName}({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"{IndentCs(2)}{{"); //start Delete
                            sw.WriteLine($"{IndentCs(3)}if (Disposed)");
                            sw.WriteLine($"{IndentCs(4)}throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(3)}using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"{IndentCs(3)}{{"); //start DbCommand
                            sw.WriteLine($"{IndentCs(4)}command.CommandType = CommandType.Text;");
                            sw.WriteLine($"{IndentCs(4)}command.CommandText = Requests[\"{dbi.ObjectName}\"][\"Delete{dbi.ObjectName}\"];");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}if (Transaction != null)");
                            sw.WriteLine($"{IndentCs(5)}command.Transaction = Transaction;");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}command.Parameters.Add(CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine();
                            sw.WriteLine($"{IndentCs(4)}return command.ExecuteNonQuery();");
                            sw.WriteLine($"{IndentCs(3)}}}"); //start DbCommand
                            sw.WriteLine($"{IndentCs(2)}}}"); //end Delete
                            first = false;
                        }

                        sw.WriteLine($"{IndentCs(1)}}}"); //end class
                        sw.WriteLine("}"); //end namespace
                    }
                }
            }
        }

        private string IndentCs(int nbIndent)
        {
            return new string(_codeGenerationSettings.CSharpIndentType == "SPACES" ? ' ' : '\t', _codeGenerationSettings.CSharpIndentSize * nbIndent);
        }

        private string IndentSql(int nbIndent)
        {
            return new string(_codeGenerationSettings.SqlIndentType == "SPACES" ? ' ' : '\t', _codeGenerationSettings.SqlIndentSize * nbIndent);
        }
    }
}
