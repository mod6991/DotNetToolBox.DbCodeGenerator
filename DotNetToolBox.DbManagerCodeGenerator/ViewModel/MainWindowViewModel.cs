using DotNetToolBox.Database;
using DotNetToolBox.DbCodeGenerator.Core;
using DotNetToolBox.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Text;
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
        private bool _useUpdate;
        private bool _useDelete;
        private string _objectName;
        private string _tableName;
        private string _query;
        private DbItem _selectedDbItem;
        private CodeGenerationSettings _codeGenerationSettings;

        public MainWindowViewModel(Window window)
            : base(window)
        {
            ResetCommand                = new RelayCommand((param) => Reset(),              (param) => ReturnTrue());
            GenerateCommand             = new RelayCommand((param) => Generate(),           (param) => ReturnTrue());
            ExitCommand                 = new RelayCommand((param) => Exit(),               (param) => ReturnTrue());
            AddObjectTableCommand       = new RelayCommand((param) => AddObjectTable(),     (param) => ReturnTrue());
            RemoveObjectTableCommand    = new RelayCommand((param) => RemoveObjectTable(),  (param) => ReturnTrue());

            DbItemList = new ObservableCollection<DbItem>();

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
            get { return _connectionString; }
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

        public bool UseUpdate
        {
            get { return _useUpdate; }
            set
            {
                _useUpdate = value;
                OnPropertyChanged(nameof(UseUpdate));
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

        public ObservableCollection<DbItem> DbItemList { get; }

        public DbItem SelectedDbItem
        {
            get { return _selectedDbItem; }
            set
            {
                _selectedDbItem = value;
                OnPropertyChanged(nameof(SelectedDbItem));
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
            LoadObjectSpecificSettings();

            _codeGenerationSettings = new CodeGenerationSettings
            {
                CSharpFilesCodePage = Int32.Parse(ConfigurationManager.AppSettings["CSharpFilesCodePage"]),
                CSharpIndentType = ConfigurationManager.AppSettings["CSharpIndentType"],
                CSharpIndentSize = Int32.Parse(ConfigurationManager.AppSettings["CSharpIndentSize"]),
                SqlIndentType = ConfigurationManager.AppSettings["SqlIndentType"],
                SqlIndentSize = Int32.Parse(ConfigurationManager.AppSettings["SqlIndentSize"])
            };
        }

        private void LoadObjectSpecificSettings()
        {
            UseSelectAll = bool.Parse(ConfigurationManager.AppSettings["DefaultUseSelectAll"]);
            UseSelectById = bool.Parse(ConfigurationManager.AppSettings["DefaultUseSelectById"]);
            UseInsert = bool.Parse(ConfigurationManager.AppSettings["DefaultUseInsert"]);
            UseUpdate= bool.Parse(ConfigurationManager.AppSettings["DefaultUseUpdate"]);
            UseDelete = bool.Parse(ConfigurationManager.AppSettings["DefaultUseDelete"]);
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
                DbItemList.Clear();
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

                db.Open();

                foreach(DbItem dbi in DbItemList)
                {
                    DataTable dt = new DataTable();
                    db.FillDataTableWithRequest(dbi.Query, null, dt);

                    foreach (DataColumn col in dt.Columns)
                        dbi.Fields.Add(new DbField(col.DataType.Name, col.ColumnName));
                }

                db.Close();

                System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DbmCodeGenerator dbm = new DbmCodeGenerator(DbItemList, _codeGenerationSettings, ObjectsNamespace, DbLayerNamespace, DbLayerObjectName, ParameterPrefix, dialog.SelectedPath);
                    dbm.Generate();
                }

                MessageBox.Show("Code generation succeeded !", "Done", MessageBoxButton.OK, MessageBoxImage.Information);
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
                DbItem dbi = new DbItem();
                dbi.ObjectName = ObjectName;
                dbi.TableName = TableName;
                dbi.Query = Query;
                dbi.UseSelectAll = UseSelectAll;
                dbi.UseSelectById = UseSelectById;
                dbi.UseInsert = UseInsert;
                dbi.UseUpdate= UseUpdate;
                dbi.UseDelete = UseDelete;

                DbItemList.Add(dbi);
                ObjectName = string.Empty;
                TableName = string.Empty;
                Query = string.Empty;
                LoadObjectSpecificSettings();
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
                if (SelectedDbItem != null)
                    DbItemList.Remove(SelectedDbItem);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
