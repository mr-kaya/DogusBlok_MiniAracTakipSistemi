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
        public string? _Satış_Şekli { get; set; }
        public string _Sevk_Tarih { get; set; }
        public string? _Mamul_Cins { get; set; }
        public string? _Mamul_Adet { get; set; }
        public string? _Plaka { get; set; }
        public string? _Notlar { get; set; }
        public string? _Araç_Sevk_Durumu { get; set; }
    }
    
    public class GenelTerimler
    {
        public static string? myIp { get; set; }
        public static string? databaseName { get; set; }
        public static string? userName { get; set; }
        public static string? mainTable { get; set; }
    }
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            ImportDatabaseToGridView();
        }

        public void ImportDatabaseToGridView()
        {
            GenelTerimler.myIp = "192.168.1.105";
            GenelTerimler.databaseName = "dogusblok";
            GenelTerimler.userName = "root2";
            GenelTerimler.mainTable = "sevkiyatDeneme";
            
            ListeGöster();
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
            if (txtFilter.Text.Length < 3)
            {
                return true;
            }
            else //Arama Kısmı Denenecek!! !!! !!! !!! !!! !!!
                return ((item as SevkiyatClass)._Firma_İsim.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0) 
                       || ((item as SevkiyatClass)._Satış_Şekli.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0) 
                       || ((item as SevkiyatClass)._Sevk_Tarih.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Mamul_Cins.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Plaka.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Notlar.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Araç_Sevk_Durumu.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0);
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

        public void ListeGöster()
        {
            String mySqlConnectionString = $"server={GenelTerimler.myIp};port=3306;uid={GenelTerimler.userName};database={GenelTerimler.databaseName};";
            MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);
            List<SevkiyatClass> items = new List<SevkiyatClass>();
            
            try
            {
                mySqlConn.Open();
            }
            catch (Exception err)
            {
                MessageBox.Show($"1. Kullandığınız bilgisayarın internet bağlantısını kontrol edin.\n" +
                                $"2. Lütfen Server IP'sinin {GenelTerimler.myIp} olduğundan emin olun. ('Çözüm 1 Server Bağlantı Hatası.mkv' Videosunu izleyin.)\n" +
                                $"3. Sunucuda Wampserver programı aktif şekilde çalıştığını kontrol edin. (Çözüm 2 Server Bağlantı Hatası.mkv' Videosunu izleyin.)",
                        "Server Bağlantı Hatası!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            MySqlCommand selectTable = new MySqlCommand(@$"SELECT * FROM {GenelTerimler.mainTable}", mySqlConn);
            MySqlDataReader myDataReader = selectTable.ExecuteReader();

            

            while ( myDataReader.Read())
            {
                items.Add(new SevkiyatClass() { _Firma_İsim = myDataReader[1].ToString(), _Satış_Şekli = myDataReader[2].ToString(), 
                                                    _Sevk_Tarih = Convert.ToDateTime(myDataReader[3]).ToString("yyyy-MM-dd"), _Mamul_Cins = myDataReader[4].ToString(),
                                                    _Mamul_Adet = myDataReader[5].ToString(), _Plaka = myDataReader[6].ToString(),
                                                    _Notlar = myDataReader[7].ToString(), _Araç_Sevk_Durumu = myDataReader[8].ToString()
                });
            }
            
            MainListView.ItemsSource = items;
            mySqlConn.Close();
            
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(MainListView.ItemsSource);
            view.Filter = UserFilterSearch;
        }
        
        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            AddPageTabItem.IsSelected = true;
        }

        private void ButtonYenile_OnClick(object sender, RoutedEventArgs e)
        {
            ListeGöster();
        }

        private void Buttonİptal_OnClick(object sender, RoutedEventArgs e)
        {
            MainPageTabItem.IsSelected = true;
        }

        private void ButtonEkle_OnClick(object sender, RoutedEventArgs e)
        {
            string[] hataTutucu = new string[3];
            if (String.IsNullOrEmpty(FirmaTextBox.Text))
                hataTutucu[0] = "Firma Adı Alanı Boş Bırakılamaz!";
            else
                hataTutucu[0] = "";

            if (String.IsNullOrEmpty(SevkDatePicker.Text))
                hataTutucu[1] = "Sevk Tarih Alanı Boş Bırakılamaz!";
            else
                hataTutucu[1] = "";
            /*
            * Efsane Sistem : Mamul Cinsi = Mamul Adedi (Yani MamulCins[1] = MamulAdet[1])
            */
            string[] mamulCinsStringList = MamulCinsTextBox.Text.Split(
                new string[]{Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries
            );

            string[] mamulAdetStringList = MamulAdetTextBox.Text.Split(
                new string[]{Environment.NewLine},
                StringSplitOptions.RemoveEmptyEntries
            );

            if (mamulCinsStringList.Length != mamulAdetStringList.Length)
                hataTutucu[2] =
                    $"Mamulün Cinsi {mamulCinsStringList.Length} tane, Mamulün Adedi {mamulAdetStringList.Length} tane";
            else
                hataTutucu[2] = "";

            HataTutucuTextBox.Text = $"{hataTutucu[0]}\n{hataTutucu[1]}\n{hataTutucu[2]}";
            if (String.IsNullOrEmpty(hataTutucu[0]) && String.IsNullOrEmpty(hataTutucu[1]) && String.IsNullOrEmpty(hataTutucu[2]))
            {
                string mySqlConnectionString = $"server={GenelTerimler.myIp};port=3306;uid={GenelTerimler.userName};database={GenelTerimler.databaseName};";
                MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);
            
                try
                {
                    mySqlConn.Open();
                }
                catch (Exception err)
                {
                    MessageBox.Show($"1. Kullandığınız bilgisayarın internet bağlantısını kontrol edin.\n" +
                                    $"2. Sunucuda Wampserver programı aktif şekilde çalıştığını kontrol edin. (Çözüm 2 Server Bağlantı Hatası.mkv' Videosunu izleyin.)",
                        "Server Bağlantı Hatası!", MessageBoxButton.OK, MessageBoxImage.Error);
                    throw;
                }

                string sevkiyatTarih = Convert.ToDateTime(SevkDatePicker.Text).ToString("yyyy-MM-dd");
                MySqlCommand sqlAddCommand = new MySqlCommand($@"INSERT INTO {GenelTerimler.mainTable} (Firma, Satis_Sekli, Sevk_Tarih, Mamul_Cins, Mamul_Aded, Plaka, Notlar, Arac_Sevk_Durumu) VALUES ('{FirmaTextBox.Text}', '{SatışŞekliIntegerUpDown.Text}', '{sevkiyatTarih}', '{MamulCinsTextBox.Text}', '{MamulAdetTextBox.Text}', '{PlakaTextBox.Text}', '{NotTextBox.Text}', '{AraçSevkDurumuComboBox.Text}')", mySqlConn);
                sqlAddCommand.ExecuteNonQuery();
                mySqlConn.Close();

                FirmaTextBox.Text = SatışŞekliIntegerUpDown.Text = SevkDatePicker.Text = 
                    MamulCinsTextBox.Text = MamulAdetTextBox.Text = PlakaTextBox.Text =
                        NotTextBox.Text = AraçSevkDurumuComboBox.Text = "";

                MainPageTabItem.IsSelected = true;
                
                ListeGöster();
            }
        }
    }
}