using DotNetToolBox.Database;
using DotNetToolBox.DbCodeGenerator.Core;
using DotNetToolBox.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace DotNetToolBox.DbManagerCodeGenerator.ViewModel
{
    public class MainWindowViewModel : ViewModelBase<Window>
    {
        private string _title;
        private string _connectionString;
        private string _provider;
        private string _parameterPrefix;
        private string _objectsNamespace;
        private string _dbLayerNamespace;
        private string _dbLayerObjectName;
        private bool _useSelectAll;
        private bool _useSelectById;
        private bool _useInsert;
        private bool _useUpdateSingle;
        private bool _useUpdateAll;
        private bool _useDelete;
        private string _objectName;
        private string _tableName;
        private string _query;
        private ObjectTable _selectedObjectTable;
        private CodeGenerationSettings _codeGenerationSettings;

        public MainWindowViewModel(Window window)
            : base(window)
        {
            ResetCommand                = new RelayCommand((param) => Reset(),              (param) => ReturnTrue());
            GenerateCommand             = new RelayCommand((param) => Generate(),           (param) => ReturnTrue());
            ExitCommand                 = new RelayCommand((param) => Exit(),               (param) => ReturnTrue());
            AddObjectTableCommand       = new RelayCommand((param) => AddObjectTable(),     (param) => ReturnTrue());
            RemoveObjectTableCommand    = new RelayCommand((param) => RemoveObjectTable(),  (param) => ReturnTrue());

            ObjectTableList = new ObservableCollection<ObjectTable>();

            LoadAppSettings();
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        public string ConnectionString
        {
            get { return _connectionString; ; }
            set
            {
                _connectionString = value;
                OnPropertyChanged(nameof(ConnectionString));
            }
        }

        public string Provider
        {
            get { return _provider; }
            set
            {
                _provider = value;
                OnPropertyChanged(nameof(Provider));
            }
        }

        public string ParameterPrefix
        {
            get { return _parameterPrefix; }
            set
            {
                _parameterPrefix = value;
                OnPropertyChanged(nameof(ParameterPrefix));
            }
        }

        public string ObjectsNamespace
        {
            get { return _objectsNamespace; }
            set
            {
                _objectsNamespace = value;
                OnPropertyChanged(nameof(ObjectsNamespace));
            }
        }

        public string DbLayerNamespace
        {
            get { return _dbLayerNamespace; }
            set
            {
                _dbLayerNamespace = value;
                OnPropertyChanged(nameof(DbLayerNamespace));
            }
        }

        public string DbLayerObjectName
        {
            get { return _dbLayerObjectName; }
            set
            {
                _dbLayerObjectName = value;
                OnPropertyChanged(nameof(DbLayerObjectName));
            }
        }

        public bool UseSelectAll
        {
            get { return _useSelectAll; }
            set
            {
                _useSelectAll = value;
                OnPropertyChanged(nameof(UseSelectAll));
            }
        }

        public bool UseSelectById
        {
            get { return _useSelectById; }
            set
            {
                _useSelectById = value;
                OnPropertyChanged(nameof(UseSelectById));
            }
        }

        public bool UseInsert
        {
            get { return _useInsert; }
            set
            {
                _useInsert = value;
                OnPropertyChanged(nameof(UseInsert));
            }
        }

        public bool UseUpdateSingle
        {
            get { return _useUpdateSingle; }
            set
            {
                _useUpdateSingle = value;
                OnPropertyChanged(nameof(UseUpdateSingle));
            }
        }

        public bool UseUpdateAll
        {
            get { return _useUpdateAll; }
            set
            {
                _useUpdateAll = value;
                OnPropertyChanged(nameof(UseUpdateAll));
            }
        }

        public bool UseDelete
        {
            get { return _useDelete; }
            set
            {
                _useDelete = value;
                OnPropertyChanged(nameof(UseDelete));
            }
        }

        public string ObjectName
        {
            get { return _objectName; }
            set
            {
                _objectName = value;
                OnPropertyChanged(nameof(ObjectName));
            }
        }

        public string TableName
        {
            get { return _tableName; }
            set
            {
                _tableName = value;
                OnPropertyChanged(nameof(TableName));
            }
        }

        public string Query
        {
            get { return _query; }
            set
            {
                _query = value;
                OnPropertyChanged(nameof(Query));
            }
        }

        public ObservableCollection<ObjectTable> ObjectTableList { get; }

        public ObjectTable SelectedObjectTable
        {
            get { return _selectedObjectTable; }
            set
            {
                _selectedObjectTable = value;
                OnPropertyChanged(nameof(SelectedObjectTable));
            }
        }


        public ICommand ResetCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand AddObjectTableCommand { get; }
        public ICommand RemoveObjectTableCommand { get; }

        private void LoadAppSettings()
        {
            ConnectionString = ConfigurationManager.AppSettings["DefaultConnectionString"];
            Provider = ConfigurationManager.AppSettings["DefaultProvider"];
            ParameterPrefix = ConfigurationManager.AppSettings["DefaultParameterPrefix"];
            ObjectsNamespace = ConfigurationManager.AppSettings["DefaultObjectsNamespace"];
            DbLayerNamespace = ConfigurationManager.AppSettings["DefaultDbLayerNamespace"];
            DbLayerObjectName = ConfigurationManager.AppSettings["DefaultDbLayerObjectName"];
            UseSelectAll = bool.Parse(ConfigurationManager.AppSettings["DefaultUseSelectAll"]);
            UseSelectById = bool.Parse(ConfigurationManager.AppSettings["DefaultUseSelectById"]);
            UseInsert = bool.Parse(ConfigurationManager.AppSettings["DefaultUseInsert"]);
            UseUpdateSingle = bool.Parse(ConfigurationManager.AppSettings["DefaultUseUpdateSingle"]);
            UseUpdateAll = bool.Parse(ConfigurationManager.AppSettings["DefaultUseUpdateAll"]);
            UseDelete = bool.Parse(ConfigurationManager.AppSettings["DefaultUseDelete"]);

            _codeGenerationSettings = new CodeGenerationSettings
            {
                CSharpFilesCodePage = ConfigurationManager.AppSettings["CSharpFilesCodePage"],
                CSharpIndentType = ConfigurationManager.AppSettings["CSharpIndentType"],
                CSharpIndentSize = Int32.Parse(ConfigurationManager.AppSettings["CSharpIndentSize"]),
                SqlIndentType = ConfigurationManager.AppSettings["SqlIndentType"],
                SqlIndentSize = Int32.Parse(ConfigurationManager.AppSettings["SqlIndentSize"])
            };
        }

        private bool ReturnTrue()
        {
            return true;
        }

        private void Reset()
        {
            try
            {
                LoadAppSettings();
                ObjectTableList.Clear();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Generate()
        {
            DbManager db = null;

            try
            {
                if (_connectionString.Contains("%USERNAME%") || _connectionString.Contains("%PASSWORD%"))
                {
                    View.PasswordWindow passWindow = new View.PasswordWindow();
                    PasswordWindowViewModel passVM = new PasswordWindowViewModel(passWindow);
                    passWindow.DataContext = passVM;
                    passWindow.Owner = VisualObject;
                    bool? sd = passWindow.ShowDialog();

                    string connectionString = _connectionString.Replace("%USERNAME%", passVM.Username).Replace("%PASSWORD%", passVM.Password);

                    db = new DbManager(connectionString, _provider);
                }
                else
                    db = new DbManager(ConnectionString, Provider);

                List<DbItem> dbItems = new List<DbItem>();

                db.Open();

                foreach(ObjectTable ot in ObjectTableList)
                {
                    DbItem dbItem = new DbItem();
                    dbItem.ObjectName = ot.ObjectName;
                    dbItem.TableName = ot.TableName;
                    dbItem.UseSelectAll = UseSelectAll;
                    dbItem.UseSelectById = UseSelectById;
                    dbItem.UseInsert = UseInsert;
                    dbItem.UseUpdateSingle = UseUpdateSingle;
                    dbItem.UseUpdateAll = UseUpdateAll;
                    dbItem.UseDelete = UseDelete;

                    DataTable dt = new DataTable();
                    db.FillDataTableWithRequest(ot.Query, null, dt);

                    foreach (DataColumn col in dt.Columns)
                        dbItem.Fields.Add(new DbField(col.DataType.Name, col.ColumnName));

                    dbItems.Add(dbItem);
                }

                db.Close();

                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DbmCodeGenerator dbm = new DbmCodeGenerator(dbItems, _codeGenerationSettings, ObjectsNamespace, DbLayerNamespace, DbLayerObjectName, dialog.SelectedPath);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (db != null)
                    db.Dispose();
            }
        }

        private void Exit()
        {
            try
            {
                VisualObject.Close();
                Application.Current.Shutdown();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddObjectTable()
        {
            try
            {
                ObjectTableList.Add(new ObjectTable(ObjectName, TableName, Query));
                ObjectName = string.Empty;
                TableName = string.Empty;
                Query = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveObjectTable()
        {
            try
            {
                if (SelectedObjectTable != null)
                    ObjectTableList.Remove(SelectedObjectTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
