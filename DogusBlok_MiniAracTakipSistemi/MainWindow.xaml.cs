using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Common;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Color = System.Drawing.Color;
using Path = System.IO.Path;

namespace DogusBlok_MiniAracTakipSistemi
{
    public class Person
    {
        public string İsim { get; set; }
        public string Plaka { get; set; }
        public string Giriş { get; set; }
    }
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var appDirectory = AppContext.BaseDirectory;
            var excelList = Directory.GetFiles(appDirectory, "*.xlsx", SearchOption.AllDirectories);

            if (excelList?.Any() ?? false)
            {
                ImportExcelToGridView(excelList[0]);
            }
            else
            {
                ImportDatabaseToGridView();    
            }
        }

        private void ImportDatabaseToGridView()
        {
            
        }
        
        private void ImportExcelToGridView(string FilePath)
        {
            string connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{FilePath}';Extended Properties='excel 8.0;HDR=Yes;IMEX=1'";
            OleDbConnection con_excel = new OleDbConnection(connectionString);
            OleDbCommand cmd_excel = new OleDbCommand();
            OleDbDataAdapter ole_dataAdapter = new OleDbDataAdapter();
            DataTable dt = new DataTable();
            cmd_excel.Connection = con_excel;
            
            con_excel.Open();
            DataTable dataExcel = con_excel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            string sheetName = dataExcel.Rows[0]["TABLE_NAME"].ToString();

            cmd_excel.CommandText = "Select * from [" + sheetName + "]";
            ole_dataAdapter.SelectCommand = cmd_excel;
            ole_dataAdapter.Fill(dt);

            Debug.WriteLine($"Veritabanı veri sayısı : {dt.Rows.Count}");

            List<Person> items = new List<Person>();
            for (int i = 0; i < dt.Rows.Count; i++)
                items.Add(new Person() { İsim =  dt.Rows[i].Field<string>(0), Plaka = dt.Rows[i].Field<string>(1), Giriş = dt.Rows[i].Field<string>(2)});
            
            MainListView.ItemsSource = items;
            con_excel.Close();
            
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(MainListView.ItemsSource);
            view.Filter = UserFilter;
        }
        
        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(MainListView.ItemsSource).Refresh();
        }

        private bool UserFilter(object item)
        {
            if (String.IsNullOrEmpty(txtFilter.Text))
            {
                return true;
            }
            else
                return ((item as Person).İsim.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0) 
                       || ((item as Person).Giriş.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0) 
                       || ((item as Person).Plaka.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void TxtFilter_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(MainListView.ItemsSource).Refresh();
        }
    }
}