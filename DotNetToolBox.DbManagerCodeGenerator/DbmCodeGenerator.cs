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
        private string _objectsNamespace;
        private string _dbLayerNamespace;
        private string _dbLayerObjectName;
        private string _parameterPrefix;
        private Encoding _encoding;
        private string _outputPath;

        public DbmCodeGenerator(IEnumerable<DbItem> dbItems, int csharpFilesCodePage, string objectsNamespace, string dbLayerNamespace, string dbLayerObjectName, string parameterPrefix, string outputPath)
        {
            _dbItems = dbItems;
            _objectsNamespace = objectsNamespace;
            _dbLayerNamespace = dbLayerNamespace;
            _dbLayerObjectName = dbLayerObjectName;
            _parameterPrefix = parameterPrefix;
            _outputPath = outputPath;
            _encoding = Encoding.GetEncoding(csharpFilesCodePage);
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
                        sw.WriteLine($"    public class {dbi.ObjectName} : IDbObject");
                        sw.WriteLine($"    {{"); //start class

                        foreach(DbField field in dbi.Fields)
                            sw.WriteLine($"        public {field.DataType} {field.PropertyName} {{ get; set; }}");

                        sw.WriteLine();

                        sw.WriteLine($"        public List<DbObjectMapping> GetMapping()");
                        sw.WriteLine($"        {{"); //start GetMapping
                        sw.WriteLine($"            return new List<DbObjectMapping>");
                        sw.WriteLine($"            {{");

                        for(int i=0,l=dbi.Fields.Count; i < l; i++)
                        {
                            if(i < l - 1)
                                sw.WriteLine($"                new DbObjectMapping(\"{dbi.Fields[i].PropertyName}\", \"{dbi.Fields[i].DbFieldName}\"),");
                            else
                                sw.WriteLine($"                new DbObjectMapping(\"{dbi.Fields[i].PropertyName}\", \"{dbi.Fields[i].DbFieldName}\")");
                        }

                        sw.WriteLine($"            }};");
                        sw.WriteLine($"        }}"); //end GetMapping

                        sw.WriteLine($"    }}"); //end class
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
                        sw.WriteLine($"    public class {dbi.ObjectName} : IDbObject");
                        sw.WriteLine($"    {{"); //start class

                        foreach (DbField field in dbi.Fields)
                            sw.WriteLine($"        public {field.DataType} {field.PropertyName} {{ get; set; }}");

                        sw.WriteLine($"    }}"); //end class
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
                            sw.WriteLine($"  <Request Name=\"SelectAll{dbi.ObjectName}s\">");
                            sw.WriteLine($"    SELECT");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.WriteLine($"      {dbi.Fields[i].DbFieldName},");
                                else
                                    sw.WriteLine($"      {dbi.Fields[i].DbFieldName}");
                            }

                            sw.WriteLine($"    FROM");
                            sw.WriteLine($"      {dbi.TableName}");
                            sw.WriteLine($"  </Request>");
                        }

                        if (dbi.UseSelectById)
                        {
                            sw.WriteLine($"  <Request Name=\"Select{dbi.ObjectName}ById\">");
                            sw.WriteLine($"    SELECT");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.WriteLine($"      {dbi.Fields[i].DbFieldName},");
                                else
                                    sw.WriteLine($"      {dbi.Fields[i].DbFieldName}");
                            }

                            sw.WriteLine($"    FROM");
                            sw.WriteLine($"      {dbi.TableName}");
                            sw.WriteLine($"    WHERE");
                            sw.WriteLine($"      {dbi.Fields[0].DbFieldName} = {_parameterPrefix}{dbi.Fields[0].ParameterName}");
                            sw.WriteLine($"  </Request>");
                        }

                        if (dbi.UseInsert)
                        {
                            sw.WriteLine($"  <Request Name=\"Insert{dbi.ObjectName}\">");
                            sw.Write($"    INSERT INTO {dbi.TableName} (");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.Write($"{dbi.Fields[i].DbFieldName}, ");
                                else
                                    sw.WriteLine($"{dbi.Fields[i].DbFieldName})");
                            }

                            sw.Write($"    VALUES (");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.Write($"{_parameterPrefix}{dbi.Fields[i].ParameterName}, ");
                                else
                                    sw.WriteLine($"{_parameterPrefix}{dbi.Fields[i].ParameterName})");
                            }

                            sw.WriteLine($"  </Request>");
                        }

                        if (dbi.UseUpdate)
                        {
                            sw.WriteLine($"  <Request Name=\"Update{dbi.ObjectName}\">");
                            sw.WriteLine($"    UPDATE {dbi.TableName} SET");

                            for (int i = 0, l = dbi.Fields.Count; i < l; i++)
                            {
                                if (i < l - 1)
                                    sw.WriteLine($"      {dbi.Fields[i].DbFieldName} = {_parameterPrefix}{dbi.Fields[i].ParameterName},");
                                else
                                    sw.WriteLine($"      {dbi.Fields[i].DbFieldName} = {_parameterPrefix}{dbi.Fields[i].ParameterName}");
                            }

                            sw.WriteLine($"    WHERE");
                            sw.WriteLine($"      {dbi.Fields[0].DbFieldName} = {_parameterPrefix}{dbi.Fields[0].ParameterName}");
                            sw.WriteLine($"  </Request>");
                        }

                        if (dbi.UseDelete)
                        {
                            sw.WriteLine($"  <Request Name=\"Delete{dbi.ObjectName}\">");
                            sw.WriteLine($"    DELETE FROM {dbi.TableName}");
                            sw.WriteLine($"    WHERE");
                            sw.WriteLine($"      {dbi.Fields[0].DbFieldName} = {_parameterPrefix}{dbi.Fields[0].ParameterName}");
                            sw.WriteLine($"  </Request>");
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
                    sw.WriteLine($"    public partial class {_dbLayerObjectName}");
                    sw.WriteLine($"    {{"); //start class

                    sw.WriteLine($"        private string _connectionString;");
                    sw.WriteLine($"        private string _provider;");
                    sw.WriteLine($"        private string _requestDirectory;");
                    sw.WriteLine($"        private ILog _logger;");
                    sw.WriteLine($"        private DbManager _db;");
                    sw.WriteLine();

                    //Constructor
                    sw.WriteLine($"        public {_dbLayerObjectName}(string connectionString, string provider, string requestDirectory, ILog logger)");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            _connectionString = connectionString;");
                    sw.WriteLine($"            _provider = provider;");
                    sw.WriteLine($"            _requestDirectory = requestDirectory;");
                    sw.WriteLine($"            _logger = logger;");
                    sw.WriteLine($"            _db = new DbManager(_connectionString, _provider);");
                    sw.WriteLine();
                    sw.WriteLine($"            RegisterDbObjects();");
                    sw.WriteLine($"            ReadRequestFiles();");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //Properties
                    sw.WriteLine($"        public DbManager DbManager");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            get {{ return _db; }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //Open
                    sw.WriteLine($"        public void Open()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            _db.Open();");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //Close
                    sw.WriteLine($"        public void Close()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            _db.Close();");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //RegisterDbObjects
                    sw.WriteLine($"        private void RegisterDbObjects()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            try");
                    sw.WriteLine($"            {{");
                    foreach(DbItem dbi in _dbItems)
                        sw.WriteLine($"                _db.RegisterDbObject(typeof({dbi.ObjectName}));");
                    sw.WriteLine($"            }}");
                    sw.WriteLine($"            catch (Exception ex)");
                    sw.WriteLine($"            {{");
                    sw.WriteLine($"                _logger.Fatal(\"An error occured while registering db objects\", ex);");
                    sw.WriteLine($"            }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();
                    
                    //ReadRequestFiles
                    sw.WriteLine($"        private void ReadRequestFiles()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            try");
                    sw.WriteLine($"            {{");
                    foreach(DbItem dbi in _dbItems)
                        sw.WriteLine($"                _db.AddRequestFile(\"{dbi.ObjectName}\", Path.Combine(_requestDirectory, \"{dbi.ObjectName}.xml\"));");
                    sw.WriteLine($"            }}");
                    sw.WriteLine($"            catch (Exception ex)");
                    sw.WriteLine($"            {{");
                    sw.WriteLine($"                _logger.Fatal(\"An error occured while reading request files\", ex);");
                    sw.WriteLine($"            }}");
                    sw.WriteLine($"        }}");
                    
                    sw.WriteLine($"    }}"); //end class
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
                    sw.WriteLine($"    public partial class {_dbLayerObjectName}");
                    sw.WriteLine($"    {{"); //start class

                    sw.WriteLine($"        private bool _disposed;");
                    sw.WriteLine($"        private string _connectionString;");
                    sw.WriteLine($"        private string _provider;");
                    sw.WriteLine($"        private DbProviderFactory _factory;");
                    sw.WriteLine($"        private DbConnection _connection;");
                    sw.WriteLine($"        private DbTransaction _transaction;");
                    sw.WriteLine($"        private Dictionary<string, Dictionary<string, string>> _requests;");
                    sw.WriteLine();

                    //Constructor
                    sw.WriteLine($"        public {_dbLayerObjectName}(string connectionString, string provider)");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            _connectionString = connectionString;");
                    sw.WriteLine($"            _provider = provider;");
                    sw.WriteLine($"            _factory = DbProviderFactories.GetFactory(_provider);");
                    sw.WriteLine($"            _connection = _factory.CreateConnection();");
                    sw.WriteLine($"            _connection.ConnectionString = _connectionString;");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //Destructor
                    sw.WriteLine($"        ~{_dbLayerObjectName}()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            Dispose(false);");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //Properties
                    sw.WriteLine($"        public bool Disposed");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            get {{ return _disposed; }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();
                    sw.WriteLine($"        public string ConnectionString");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            get {{ return _connectionString; }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();
                    sw.WriteLine($"        public string Provider");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            get {{ return _provider; }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();
                    sw.WriteLine($"        public DbProviderFactory Factory");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            get {{ return _factory; }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();
                    sw.WriteLine($"        public DbConnection Connection");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            get {{ return _connection; }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();
                    sw.WriteLine($"        public DbTransaction Transaction");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            get {{ return _transaction; }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();
                    sw.WriteLine($"        public Dictionary<string, Dictionary<string, string>> Requests");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            get {{ return _requests; }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //Open
                    sw.WriteLine($"        public void Open()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            if (_disposed)");
                    sw.WriteLine($"                throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"            _db.Open();");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //Close
                    sw.WriteLine($"        public void Close()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            if (_disposed)");
                    sw.WriteLine($"                throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"            _db.Close();");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //BeginTransaction
                    sw.WriteLine($"        public void BeginTransaction()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            if (_disposed)");
                    sw.WriteLine($"                throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"            _transaction = _connection.BeginTransaction();");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //EndTransaction
                    sw.WriteLine($"        public void EndTransaction(bool commit)");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            if (_disposed)");
                    sw.WriteLine($"                throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"            if (_transaction != null)");
                    sw.WriteLine($"            {{");
                    sw.WriteLine($"                if (commit)");
                    sw.WriteLine($"                    _transaction.Commit();");
                    sw.WriteLine($"                else");
                    sw.WriteLine($"                    _transaction.Rollback();");
                    sw.WriteLine();
                    sw.WriteLine($"                _transaction = null;");
                    sw.WriteLine($"            }}");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //CreateParameter
                    sw.WriteLine($"        public void CreateParameter()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            if (_disposed)");
                    sw.WriteLine($"                throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"            return _factory.CreateParameter();");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //CreateParameter2
                    sw.WriteLine($"        public void CreateParameter(string name, object value, ParameterDirection paramDirection = ParameterDirection.Input)");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            if (_disposed)");
                    sw.WriteLine($"                throw new ObjectDisposedException(typeof({_dbLayerObjectName}).FullName);");
                    sw.WriteLine();
                    sw.WriteLine($"            DbParameter param = _factory.CreateParameter();");
                    sw.WriteLine($"            param.ParameterName = name;");
                    sw.WriteLine($"            param.Value = value;");
                    sw.WriteLine($"            param.Direction = paramDirection;");
                    sw.WriteLine($"            return param;");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //Dispose
                    sw.WriteLine($"        public void Dispose()");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            Dispose(true);");
                    sw.WriteLine($"            GC.SuppressFinalize(this);");
                    sw.WriteLine($"        }}");
                    sw.WriteLine();

                    //Dispose2
                    sw.WriteLine($"        public void Dispose(bool disposing)");
                    sw.WriteLine($"        {{");
                    sw.WriteLine($"            if (_disposed)");
                    sw.WriteLine($"                return;");
                    sw.WriteLine();
                    sw.WriteLine($"            if (disposing)");
                    sw.WriteLine($"            {{"); //start if disposing
                    sw.WriteLine($"                if (_connection != null)");
                    sw.WriteLine($"                {{");
                    sw.WriteLine($"                    _connection.Dispose();");
                    sw.WriteLine($"                    _connection = null;");
                    sw.WriteLine($"                }}");
                    sw.WriteLine();
                    sw.WriteLine($"                _transaction = null;");
                    sw.WriteLine($"                _factory = null;");
                    sw.WriteLine($"                _connectionString = null;");
                    sw.WriteLine($"                _provider = null;");
                    sw.WriteLine($"            }}"); //end if disposing
                    sw.WriteLine();
                    sw.WriteLine($"            _disposed = true;");
                    sw.WriteLine($"        }}");

                    sw.WriteLine($"    }}"); //end class
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
                        sw.WriteLine($"    public partial class {_dbLayerObjectName}");
                        sw.WriteLine($"    {{"); //start class

                        if (dbi.UseSelectAll)
                        {
                            sw.WriteLine($"        public List<{dbi.ObjectName}> SelectAll{dbi.ObjectName}s()");
                            sw.WriteLine($"        {{");
                            sw.WriteLine($"            List<{dbi.ObjectName}> list = _db[\"{dbi.ObjectName}\"].FillObjects<{dbi.ObjectName}>(\"SelectAll{dbi.ObjectName}s\", null);");
                            sw.WriteLine($"            return list;");
                            sw.WriteLine($"        }}");
                            first = false;
                        }

                        if (dbi.UseSelectById)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"        public {dbi.ObjectName} Select{dbi.ObjectName}ById({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"        {{");
                            sw.WriteLine($"            List<DbParameter> parameters = new List<DbParameter>();");
                            sw.WriteLine($"            parameters.Add(_db.CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine($"            List<{dbi.ObjectName}> list = _db[\"{dbi.ObjectName}\"].FillObjects<{dbi.ObjectName}>(\"Select{dbi.ObjectName}ById\", parameters);");
                            sw.WriteLine($"            if (list.Count == 0)");
                            sw.WriteLine($"                return null;");
                            sw.WriteLine($"            return list[0];");
                            sw.WriteLine($"        }}");
                            first = false;
                        }

                        if (dbi.UseInsert)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"        public void Insert{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"        {{");
                            sw.WriteLine($"            List<DbParameter> parameters = new List<DbParameter>();");

                            foreach (DbField field in dbi.Fields)
                                sw.WriteLine($"            parameters.Add(_db.CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine($"            _db[\"{dbi.ObjectName}\"].ExecuteNonQuery(\"Insert{dbi.ObjectName}\", parameters);");
                            sw.WriteLine($"        }}");
                            first = false;
                        }

                        if (dbi.UseUpdate)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"        public void Update{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"        {{");
                            sw.WriteLine($"            List<DbParameter> parameters = new List<DbParameter>();");

                            foreach (DbField field in dbi.Fields)
                                sw.WriteLine($"            parameters.Add(_db.CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine($"            _db[\"{dbi.ObjectName}\"].ExecuteNonQuery(\"Update{dbi.ObjectName}\", parameters);");
                            sw.WriteLine($"        }}");
                            first = false;
                        }

                        if (dbi.UseDelete)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"        public void Delete{dbi.ObjectName}({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"        {{");
                            sw.WriteLine($"            List<DbParameter> parameters = new List<DbParameter>();");
                            sw.WriteLine($"            parameters.Add(_db.CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine($"            _db[\"{dbi.ObjectName}\"].ExecuteNonQuery(\"Delete{dbi.ObjectName}\", parameters);");
                            sw.WriteLine($"        }}");
                            first = false;
                        }

                        sw.WriteLine($"    }}"); //end class
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
                        sw.WriteLine($"    public partial class {_dbLayerObjectName}");
                        sw.WriteLine($"    {{"); //start class

                        if (dbi.UseSelectAll)
                        {
                            sw.WriteLine($"        public List<{dbi.ObjectName}> SelectAll{dbi.ObjectName}s()");
                            sw.WriteLine($"        {{"); //start SelectAll
                            sw.WriteLine($"            if (Disposed)");
                            sw.WriteLine($"                throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"            List<{dbi.ObjectName}> list = new List<{dbi.ObjectName}>();");
                            sw.WriteLine();
                            sw.WriteLine($"            using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"            {{"); //start DbCommand
                            sw.WriteLine($"                command.CommandType = CommandType.Text;");
                            sw.WriteLine($"                command.CommandText = Requests[\"{dbi.ObjectName}\"][\"SelectAll{dbi.ObjectName}s\"];");
                            sw.WriteLine();
                            sw.WriteLine($"                using (DbDataReader reader = command.ExecuteReader())");
                            sw.WriteLine($"                {{"); //start DbDataReader
                            sw.WriteLine($"                    while (reader.Read())");
                            sw.WriteLine($"                    {{"); //start reader.Read
                            sw.WriteLine($"                        {dbi.ObjectName} obj = new {dbi.ObjectName}();");

                            foreach (DbField field in dbi.Fields)
                            {
                                sw.WriteLine($"                        if (!(reader[\"{field.DbFieldName}\"] is DBNull))");
                                sw.WriteLine($"                            obj.{field.PropertyName} = ({field.DataType})reader[\"{field.DbFieldName}\"];");
                            }

                            sw.WriteLine($"                        list.Add(obj);");
                            sw.WriteLine($"                    }}"); //end reader.Read
                            sw.WriteLine();
                            sw.WriteLine($"                    reader.Close();");
                            sw.WriteLine($"                }}"); //end DbDataReader
                            sw.WriteLine($"            }}"); //end DbCommand
                            sw.WriteLine();
                            sw.WriteLine($"            return list;");
                            sw.WriteLine($"        }}"); //end SelectAll
                            first = false;
                        }

                        if (dbi.UseSelectById)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"        public {dbi.ObjectName} Select{dbi.ObjectName}ById({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"        {{"); //start SelectById
                            sw.WriteLine($"            if (Disposed)");
                            sw.WriteLine($"                throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"            List<{dbi.ObjectName}> list = new List<{dbi.ObjectName}>();");
                            sw.WriteLine();
                            sw.WriteLine($"            using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"            {{"); //start DbCommand
                            sw.WriteLine($"                command.CommandType = CommandType.Text;");
                            sw.WriteLine($"                command.CommandText = Requests[\"{dbi.ObjectName}\"][\"Select{dbi.ObjectName}ById\"];");
                            sw.WriteLine();
                            sw.WriteLine($"                command.Parameters.Add(CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine();
                            sw.WriteLine($"                using (DbDataReader reader = command.ExecuteReader())");
                            sw.WriteLine($"                {{"); //start DbDataReader
                            sw.WriteLine($"                    while (reader.Read())");
                            sw.WriteLine($"                    {{"); //start reader.Read
                            sw.WriteLine($"                        {dbi.ObjectName} obj = new {dbi.ObjectName}();");

                            foreach (DbField field in dbi.Fields)
                            {
                                sw.WriteLine($"                        if (!(reader[\"{field.DbFieldName}\"] is DBNull))");
                                sw.WriteLine($"                            obj.{field.PropertyName} = ({field.DataType})reader[\"{field.DbFieldName}\"];");
                            }

                            sw.WriteLine($"                        list.Add(obj);");
                            sw.WriteLine($"                    }}"); //end reader.Read
                            sw.WriteLine();
                            sw.WriteLine($"                    reader.Close();");
                            sw.WriteLine($"                }}"); //end DbDataReader
                            sw.WriteLine($"            }}"); //end DbCommand
                            sw.WriteLine();
                            sw.WriteLine($"            if (list.Count == 0)");
                            sw.WriteLine($"                return null;");
                            sw.WriteLine($"            return list[0];");
                            sw.WriteLine($"        }}"); //end SelectById
                            first = false;
                        }

                        if (dbi.UseInsert)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"        public int Insert{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"        {{"); //start Insert
                            sw.WriteLine($"            if (Disposed)");
                            sw.WriteLine($"                throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"            using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"            {{"); //start DbCommand
                            sw.WriteLine($"                command.CommandType = CommandType.Text;");
                            sw.WriteLine($"                command.CommandText = Requests[\"{dbi.ObjectName}\"][\"Insert{dbi.ObjectName}\"];");
                            sw.WriteLine();
                            sw.WriteLine($"                if (Transaction != null)");
                            sw.WriteLine($"                    command.Transaction = Transaction;");
                            sw.WriteLine();

                            foreach(DbField field in dbi.Fields)
                                sw.WriteLine($"                command.Parameters.Add(CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine();
                            sw.WriteLine($"                return command.ExecuteNonQuery();");
                            sw.WriteLine($"            }}"); //start DbCommand
                            sw.WriteLine($"        }}"); //end Insert
                            first = false;
                        }

                        if (dbi.UseUpdate)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"        public int Update{dbi.ObjectName}({dbi.ObjectName} {minObjName})");
                            sw.WriteLine($"        {{"); //start Update
                            sw.WriteLine($"            if (Disposed)");
                            sw.WriteLine($"                throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"            using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"            {{"); //start DbCommand
                            sw.WriteLine($"                command.CommandType = CommandType.Text;");
                            sw.WriteLine($"                command.CommandText = Requests[\"{dbi.ObjectName}\"][\"Update{dbi.ObjectName}\"];");
                            sw.WriteLine();
                            sw.WriteLine($"                if (Transaction != null)");
                            sw.WriteLine($"                    command.Transaction = Transaction;");
                            sw.WriteLine();

                            foreach (DbField field in dbi.Fields)
                                sw.WriteLine($"                command.Parameters.Add(CreateParameter(\"{field.ParameterName}\", {minObjName}.{field.PropertyName}));");

                            sw.WriteLine();
                            sw.WriteLine($"                return command.ExecuteNonQuery();");
                            sw.WriteLine($"            }}"); //start DbCommand
                            sw.WriteLine($"        }}"); //end Update
                            first = false;
                        }

                        if (dbi.UseDelete)
                        {
                            if (!first)
                                sw.WriteLine();

                            sw.WriteLine($"        public int Delete{dbi.ObjectName}({dbi.Fields[0].DataType} {dbi.Fields[0].ParameterName})");
                            sw.WriteLine($"        {{"); //start Delete
                            sw.WriteLine($"            if (Disposed)");
                            sw.WriteLine($"                throw new ObjectDisposedException(typeof(DbManager).FullName);");
                            sw.WriteLine();
                            sw.WriteLine($"            using (DbCommand command = Connection.CreateCommand())");
                            sw.WriteLine($"            {{"); //start DbCommand
                            sw.WriteLine($"                command.CommandType = CommandType.Text;");
                            sw.WriteLine($"                command.CommandText = Requests[\"{dbi.ObjectName}\"][\"Delete{dbi.ObjectName}\"];");
                            sw.WriteLine();
                            sw.WriteLine($"                if (Transaction != null)");
                            sw.WriteLine($"                    command.Transaction = Transaction;");
                            sw.WriteLine();
                            sw.WriteLine($"                command.Parameters.Add(CreateParameter(\"{dbi.Fields[0].ParameterName}\", {dbi.Fields[0].ParameterName}));");
                            sw.WriteLine();
                            sw.WriteLine($"                return command.ExecuteNonQuery();");
                            sw.WriteLine($"            }}"); //start DbCommand
                            sw.WriteLine($"        }}"); //end Delete
                            first = false;
                        }

                        sw.WriteLine($"    }}"); //end class
                        sw.WriteLine("}"); //end namespace
                    }
                }
            }
        }
    }
}
