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
using MySql.Data.MySqlClient;
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
        public string? İsim { get; set; }
        public string? Plaka { get; set; }
        public string? Giriş { get; set; }
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
            String mySqlConnectionString = "server=localhost;uid=root;database=dogusblok;";
            MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);
            mySqlConn.Open();
            
            MySqlCommand selectTable = new MySqlCommand(@"SELECT * FROM person", mySqlConn);
            MySqlDataReader myDataReader = selectTable.ExecuteReader();

            List<Person> items = new List<Person>();
            while (myDataReader.Read())
            {
                items.Add(new Person() {İsim = myDataReader[0].ToString(), Plaka = myDataReader[1].ToString(), Giriş = myDataReader[2].ToString()});
            }
            
            MainListView.ItemsSource = items;
            mySqlConn.Close();
            
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(MainListView.ItemsSource);
            view.Filter = UserFilterSearch;
        }
        
        private void ImportExcelToGridView(string FilePath)
        {
            string connectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source='{FilePath}';Extended Properties='excel 8.0;HDR=Yes;IMEX=1'";
            OleDbConnection con_excel = new OleDbConnection(connectionString);
            OleDbCommand cmd_excel = new OleDbCommand();
            OleDbDataAdapter ole_dataAdapter = new OleDbDataAdapter();
            DataTable dt = new DataTable();
            cmd_excel.Connection = con_excel;
            
            String mySqlConnectionString = "server=localhost;uid=root;database=dogusblok;";
            MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);
            mySqlConn.Open();
            List<Person> items = new List<Person>();

            try
            {
                MySqlCommand Create_table =
                    new MySqlCommand(
                        @"CREATE TABLE person (name VARCHAR(100), licensePlate VARCHAR(20), entry VARCHAR(10));", mySqlConn);
                Create_table.ExecuteNonQuery();

                con_excel.Open();
                DataTable dataExcel = con_excel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string sheetName = dataExcel.Rows[0]["TABLE_NAME"].ToString();

                cmd_excel.CommandText = "Select * from [" + sheetName + "]";
                ole_dataAdapter.SelectCommand = cmd_excel;
                ole_dataAdapter.Fill(dt);

                Debug.WriteLine($"Veritabanı veri sayısı : {dt.Rows.Count}");

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    items.Add(new Person() { İsim =  dt.Rows[i].Field<string>(0), Plaka = dt.Rows[i].Field<string>(1), Giriş = dt.Rows[i].Field<string>(2)});
                    MySqlCommand InsertInto =
                        new MySqlCommand(
                            @$"INSERT INTO person (name, licensePlate, entry) VALUES ('{dt.Rows[i].Field<string>(0)}','{dt.Rows[i].Field<string>(1)}','{dt.Rows[i].Field<string>(2)}');", mySqlConn);
                    InsertInto.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Tablo Oluşturulmuş.");
                MySqlCommand selectTable = new MySqlCommand(@"SELECT * FROM person", mySqlConn);
                MySqlDataReader myDataReader = selectTable.ExecuteReader();

                while (myDataReader.Read())
                {
                    items.Add(new Person() {İsim = myDataReader[0].ToString(), Plaka = myDataReader[1].ToString(), Giriş = myDataReader[2].ToString()});
                }

            }
            MainListView.ItemsSource = items;
            con_excel.Close();
            mySqlConn.Close();
            
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(MainListView.ItemsSource);
            view.Filter = UserFilterSearch;
        }
        
        private void BtnSearch_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                CollectionViewSource.GetDefaultView(MainListView.ItemsSource).Refresh();
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"ERR01: Uygun Kaynak Bulunamadı!!!");
            }
        }

        private bool UserFilterSearch(object item)
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
            try
            {
                CollectionViewSource.GetDefaultView(MainListView.ItemsSource).Refresh();
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"ERR01: Uygun Kaynak Bulunamadı!!!");
            }
        }
    }
}