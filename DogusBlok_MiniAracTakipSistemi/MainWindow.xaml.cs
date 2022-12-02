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
        public int? _Sıra { get; set; }
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
        public static string? charset { get; set; }
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
            GenelTerimler.databaseName = "dogusblok2";
            GenelTerimler.userName = "root2";
            GenelTerimler.mainTable = "sevkiyatDeneme";
            GenelTerimler.charset = "utf8";

            BaşlangıçDatePicker.SelectedDate = DateTime.Now;
            BitişDatePicker.SelectedDate = DateTime.Now;

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
                //Arama Kısmı Denenecek!! !!! !!! !!! !!! !!!
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
            String mySqlConnectionString = $"server={GenelTerimler.myIp};port=3306;uid={GenelTerimler.userName};database={GenelTerimler.databaseName};charset={GenelTerimler.charset};";
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

            try
            {
                if (Convert.ToDateTime(BaşlangıçDatePicker.Text) > Convert.ToDateTime(BitişDatePicker.Text))
                {
                    BitişDatePicker.Text = BaşlangıçDatePicker.Text;
                }
                
                MySqlCommand selectTable = new MySqlCommand(@$"SELECT * FROM {GenelTerimler.mainTable} WHERE 
                Sevk_Tarih>='{Convert.ToDateTime(BaşlangıçDatePicker.Text).ToString("yyyy-MM-dd")}' AND Sevk_Tarih<='{Convert.ToDateTime(BitişDatePicker.Text).ToString("yyyy-MM-dd")}'", mySqlConn);
                MySqlDataReader myDataReader = selectTable.ExecuteReader();
                
                int i = 1;
                while ( myDataReader.Read())
                {
                    items.Add(new SevkiyatClass() { _Sıra = i,
                        _Firma_İsim = myDataReader[0].ToString(), _Satış_Şekli = myDataReader[1].ToString(), 
                        _Sevk_Tarih = Convert.ToDateTime(myDataReader[2]).ToString("dd.MM.yyyy dddd"), _Mamul_Cins = myDataReader[3].ToString(),
                        _Mamul_Adet = myDataReader[4].ToString(), _Plaka = myDataReader[5].ToString(),
                        _Notlar = myDataReader[6].ToString(), _Araç_Sevk_Durumu = myDataReader[7].ToString()
                    });
                    i++;
                }
            
                MainListView.ItemsSource = items;
                mySqlConn.Close();

                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(MainListView.ItemsSource);
                view.Filter = UserFilterSearch;
            }
            catch (Exception e)
            {
                
            }
        }
        
        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            AddPageTabItem.IsSelected = true;
        }

        private void BtnDelete_OnClick(object sender, RoutedEventArgs e)
        {
            SevkiyatClass sevkiyatClass = MainListView.SelectedItem as SevkiyatClass;

            try
            {
                if (!String.IsNullOrEmpty(sevkiyatClass._Firma_İsim))
                {
                    var dialogResult = MessageBox.Show("Seçtiğiniz sevkiyat verisi tamamen silinecek. Emin misiniz?", "Sil Onay",
                        MessageBoxButton.YesNo);
                    if (dialogResult.ToString() == "Yes")
                    {
                        String mySqlConnectionString = $"server={GenelTerimler.myIp};port=3306;uid={GenelTerimler.userName};database={GenelTerimler.databaseName};charset={GenelTerimler.charset};";
                        MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);
                        try
                        {
                            mySqlConn.Open();
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show($"1. Kullandığınız bilgisayarın internet bağlantısını kontrol edin.\n" +
                                            $"2. Lütfen Server IP'sinin {GenelTerimler.myIp} olduğundan emin olun. ('Çözüm 1 Server Bağlantı Hatası.mkv' Videosunu izleyin.)\n" +
                                            $"3. Sunucuda Wampserver programı aktif şekilde çalıştığını kontrol edin. (Çözüm 2 Server Bağlantı Hatası.mkv' Videosunu izleyin.)",
                                "Server Bağlantı Hatası!", MessageBoxButton.OK, MessageBoxImage.Error);
                            throw;
                        }
                        
                        MySqlCommand sqlSil = new MySqlCommand($@"DELETE FROM {GenelTerimler.mainTable} WHERE 
                                Firma='{sevkiyatClass._Firma_İsim}' AND Satis_Sekli='{sevkiyatClass._Satış_Şekli}' AND Sevk_Tarih='{Convert.ToDateTime(sevkiyatClass._Sevk_Tarih).ToString("yyyy-MM-dd")}' AND
                                Mamul_Cins='{sevkiyatClass._Mamul_Cins}' AND Mamul_Aded='{sevkiyatClass._Mamul_Adet}' AND Plaka='{sevkiyatClass._Plaka}' AND
                                Notlar='{sevkiyatClass._Notlar}' AND Arac_Sevk_Durumu='{sevkiyatClass._Araç_Sevk_Durumu}'", mySqlConn);
                        int i = sqlSil.ExecuteNonQuery();
                        mySqlConn.Close();

                        if (i != 1)
                        {
                            MessageBox.Show("Veriniz silinemedi.", "Bilgi", MessageBoxButton.OK);
                        }

                        ListeGöster();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Silmek için önce tablodan seçim yapınız.","Bilgi", MessageBoxButton.OK);
            }
        }
        
        private void DataGridRow_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            SevkiyatClass rowSevkiyat = MainListView.SelectedItem as SevkiyatClass;

            UpdateTabItem.IsSelected = true;
            FirmaTextBoxUpdate.Text = rowSevkiyat._Firma_İsim;
            SatışŞekliIntegerUpDownUpdate.Text = rowSevkiyat._Satış_Şekli;
            SevkDatePickerUpdate.Text = rowSevkiyat._Sevk_Tarih;
            MamulCinsTextBoxUpdate.Text = rowSevkiyat._Mamul_Cins;
            MamulAdetTextBoxUpdate.Text = rowSevkiyat._Mamul_Adet;
            PlakaTextBoxUpdate.Text = rowSevkiyat._Plaka;
            NotTextBoxUpdate.Text = rowSevkiyat._Notlar;
            AraçSevkDurumuComboBoxUpdate.Text = rowSevkiyat._Araç_Sevk_Durumu;
        }
        
        private void ButtonGüncelle_OnClick(object sender, RoutedEventArgs e)
        {
            string[] hataTutucu = new string[3];
            if (String.IsNullOrEmpty(FirmaTextBoxUpdate.Text))
                hataTutucu[0] = "Firma Adı Alanı Boş Bırakılamaz!";
            else
                hataTutucu[0] = "";

            if (String.IsNullOrEmpty(SevkDatePickerUpdate.Text))
                hataTutucu[1] = "Sevk Tarih Alanı Boş Bırakılamaz!";
            else
                hataTutucu[1] = "";
            /*
            * Efsane Sistem : Mamul Cinsi = Mamul Adedi (Yani MamulCins[1] = MamulAdet[1])
            */
            string[] mamulCinsStringList = MamulCinsTextBoxUpdate.Text.Split(
                new string[]{Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries
            );

            string[] mamulAdetStringList = MamulAdetTextBoxUpdate.Text.Split(
                new string[]{Environment.NewLine},
                StringSplitOptions.RemoveEmptyEntries
            );

            if (mamulCinsStringList.Length != mamulAdetStringList.Length)
                hataTutucu[2] =
                    $"Mamulün Cinsi {mamulCinsStringList.Length} tane, Mamulün Adedi {mamulAdetStringList.Length} tane";
            else
                hataTutucu[2] = "";

            HataTutucuTextBoxUpdate.Text = $"{hataTutucu[0]}\n{hataTutucu[1]}\n{hataTutucu[2]}";
            if (String.IsNullOrEmpty(hataTutucu[0]) && String.IsNullOrEmpty(hataTutucu[1]) &&
                String.IsNullOrEmpty(hataTutucu[2]))
            {
                string mySqlConnectionString =
                    $"server={GenelTerimler.myIp};port=3306;uid={GenelTerimler.userName};database={GenelTerimler.databaseName};charset={GenelTerimler.charset};";
                MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);

                char[] firmaİsimChar = FirmaTextBoxUpdate.Text.ToCharArray();
                if (char.IsLower(firmaİsimChar[0]))
                {
                    firmaİsimChar[0] = char.ToUpper(firmaİsimChar[0]);
                }

                for (int i = 1; i < firmaİsimChar.Length; i++)
                {
                    if (firmaİsimChar[i - 1] == ' ')
                    {
                        if (char.IsLower(firmaİsimChar[i]))
                        {
                            firmaİsimChar[i] = char.ToUpper(firmaİsimChar[i]);
                        }
                    }
                }

                string firmaİsim = new string(firmaİsimChar.ToArray());

                string plakaNumarası = PlakaTextBoxUpdate.Text;
                if (!String.IsNullOrEmpty(PlakaTextBoxUpdate.Text))
                {
                    plakaNumarası = PlakaTextBoxUpdate.Text.Replace(" ", "").ToUpper();
                }

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

                SevkiyatClass rowSevkiyat = MainListView.SelectedItem as SevkiyatClass;
                MySqlCommand updateCommand = new MySqlCommand($@"UPDATE {GenelTerimler.mainTable} SET 
                           Firma='{firmaİsim}', Satis_Sekli='{SatışŞekliIntegerUpDownUpdate.Text}', Sevk_Tarih='{Convert.ToDateTime(SevkDatePickerUpdate.Text).ToString("yyyy-MM-dd")}', 
                           Mamul_Cins='{MamulCinsTextBoxUpdate.Text}', Mamul_Aded='{MamulAdetTextBoxUpdate.Text}', Plaka='{plakaNumarası}',
                           Notlar='{NotTextBoxUpdate.Text}', Arac_Sevk_Durumu='{AraçSevkDurumuComboBoxUpdate.Text}' 
                           WHERE 
                                Firma='{rowSevkiyat._Firma_İsim}' AND Satis_Sekli='{rowSevkiyat._Satış_Şekli}' AND Sevk_Tarih='{Convert.ToDateTime(rowSevkiyat._Sevk_Tarih).ToString("yyyy-MM-dd")}' AND 
                                Mamul_Cins='{rowSevkiyat._Mamul_Cins}' AND Mamul_Aded='{rowSevkiyat._Mamul_Adet}' AND Plaka='{rowSevkiyat._Plaka}' AND
                                Notlar='{rowSevkiyat._Notlar}' AND Arac_Sevk_Durumu='{rowSevkiyat._Araç_Sevk_Durumu}'", mySqlConn) ;
                updateCommand.ExecuteNonQuery();
                mySqlConn.Close();

                MainPageTabItem.IsSelected = true;
                ListeGöster();
            }
        }

        private void ButtonYenile_OnClick(object sender, RoutedEventArgs e)
        {
            ListeGöster();
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
                string mySqlConnectionString = $"server={GenelTerimler.myIp};port=3306;uid={GenelTerimler.userName};database={GenelTerimler.databaseName};charset={GenelTerimler.charset};";
                MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);

                char[] firmaİsimChar = FirmaTextBox.Text.ToCharArray();
                if (char.IsLower(firmaİsimChar[0]))
                {
                    firmaİsimChar[0] = char.ToUpper(firmaİsimChar[0]);
                }

                for (int i = 1; i < firmaİsimChar.Length; i++)
                {
                    if (firmaİsimChar[i-1] == ' ')
                    {
                        if (char.IsLower(firmaİsimChar[i]))
                        {
                            firmaİsimChar[i] = char.ToUpper(firmaİsimChar[i]);
                        }
                    }
                }

                string firmaİsim = new string(firmaİsimChar.ToArray());
                
                string plakaNumarası = PlakaTextBox.Text;
                if (!String.IsNullOrEmpty(PlakaTextBox.Text))
                {
                    plakaNumarası = PlakaTextBox.Text.Replace(" ", "").ToUpper();
                }

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
                
                MySqlCommand sqlAddCommand = new MySqlCommand($@"INSERT INTO {GenelTerimler.mainTable} (Firma, Satis_Sekli, Sevk_Tarih, Mamul_Cins, Mamul_Aded, Plaka, Notlar, Arac_Sevk_Durumu) VALUES ('{firmaİsim}', '{SatışŞekliIntegerUpDown.Text}', '{Convert.ToDateTime(SevkDatePicker.Text).ToString("yyyy-MM-dd")}', '{MamulCinsTextBox.Text}', '{MamulAdetTextBox.Text}', '{plakaNumarası}', '{NotTextBox.Text}', '{AraçSevkDurumuComboBox.Text}')", mySqlConn);
                sqlAddCommand.ExecuteNonQuery();
                mySqlConn.Close();

                FirmaTextBox.Text = SatışŞekliIntegerUpDown.Text = SevkDatePicker.Text = 
                    MamulCinsTextBox.Text = MamulAdetTextBox.Text = PlakaTextBox.Text =
                        NotTextBox.Text = AraçSevkDurumuComboBox.Text = "";

                MainPageTabItem.IsSelected = true;
                
                ListeGöster();
            }
            
        }
        
        private void Buttonİptal_OnClick(object sender, RoutedEventArgs e)
        {
            FirmaTextBox.Text = SatışŞekliIntegerUpDown.Text = SevkDatePicker.Text = 
                MamulCinsTextBox.Text = MamulAdetTextBox.Text = PlakaTextBox.Text =
                    NotTextBox.Text = AraçSevkDurumuComboBox.Text = "";
            
            MainPageTabItem.IsSelected = true;
        }
        
        private void Date_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender.ToString() != "")
            {
                ListeGöster();
            }

        }
    }
}