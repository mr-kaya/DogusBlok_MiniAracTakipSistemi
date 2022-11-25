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
using Google.Protobuf.WellKnownTypes;
using Color = System.Drawing.Color;
using Path = System.IO.Path;

namespace DogusBlok_MiniAracTakipSistemi
{
    public class SevkiyatClass
    {
        public string? _Firma_İsim { get; set; }
        public int? _Özel_Giriş { get; set; }
        public string _Sevk_Tarih { get; set; }
        public string? _Mamul_Cins { get; set; }
        public string? _Mamul_Adet { get; set; }
        public string? _Plaka { get; set; }
        public string? _Notlar { get; set; }
        public int? _Araç_Sevk_Durumu { get; set; }
    }

    public class GenelTerimler
    {
        public string? myIp { get; set; }
        public string? databaseName { get; set; }
        public string? userName { get; set; }
        public string? mainTable { get; set; }
    }
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ImportDatabaseToGridView();
        }

        private void ImportDatabaseToGridView()
        {
            GenelTerimler mainTerimler = new ()
            {
                myIp = "192.168.1.105",
                databaseName = "dogusblok",
                userName = "root2",
                mainTable = "sevkiyatDeneme"
            };

            String mySqlConnectionString = $"server={mainTerimler.myIp};port=3306;uid={mainTerimler.userName};database={mainTerimler.databaseName};";
            MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);

            try
            {
                mySqlConn.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show($"1. Kullandığınız bilgisayarın internet bağlantısını kontrol edin.\n" +
                                $"2. Lütfen Server IP'sinin {mainTerimler.myIp} olduğundan emin olun. ('Çözüm 1 Server Bağlantı Hatası.mkv' Videosunu izleyin.)\n" +
                                $"3. Sunucuda Wampserver programı aktif şekilde çalıştığını kontrol edin. (Çözüm 2 Server Bağlantı Hatası.mkv' Videosunu izleyin.)",
                        "Server Bağlantı Hatası!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            MySqlCommand selectTable = new MySqlCommand(@$"SELECT * FROM {mainTerimler.mainTable}", mySqlConn);
            MySqlDataReader myDataReader = selectTable.ExecuteReader();

            List<SevkiyatClass> items = new List<SevkiyatClass>();
            while (myDataReader.Read())
            {
                string tarihString = Convert.ToDateTime(myDataReader[3]).ToString("yyyy-MM-dd");
                DateTime tarihDateTime = Convert.ToDateTime(tarihString);
                
                items.Add(new SevkiyatClass() { _Firma_İsim = myDataReader[1].ToString(), _Özel_Giriş = (sbyte) myDataReader[2], 
                                                    _Sevk_Tarih = Convert.ToDateTime(myDataReader[3]).ToString("yyyy-MM-dd"), _Mamul_Cins = myDataReader[4].ToString(),
                                                    _Mamul_Adet = myDataReader[5].ToString(), _Plaka = myDataReader[6].ToString(),
                                                    _Notlar = myDataReader[7].ToString(), _Araç_Sevk_Durumu = (sbyte) myDataReader[8]
                });
            }
            
            MainListView.ItemsSource = items;
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
            else //Arama Kısmı Denenecek!! !!! !!! !!! !!! !!!
                return ((item as SevkiyatClass)._Firma_İsim.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0) 
                       || ((item as SevkiyatClass)._Özel_Giriş.ToString().IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0) 
                       || ((item as SevkiyatClass)._Sevk_Tarih.ToString().IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Mamul_Cins.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Mamul_Adet.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Plaka.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Notlar.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Araç_Sevk_Durumu.ToString().IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
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

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            AddWindow addWindow = new AddWindow();
            addWindow.Show();
        }
    }
}