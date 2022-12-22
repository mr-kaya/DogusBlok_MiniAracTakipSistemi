using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ClosedXML.Excel;
using Microsoft.Win32;
using Window = System.Windows.Window;

namespace DogusBlok_MiniAracTakipSistemi
{
    public class SevkiyatClass
    {
        public int? Sıra { get; set; }
        public string? _Firma_İsim { get; set; }
        public string? _Teslim { get; set; }
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
        public static string? userPassword { get; set; }
        public static string? mainTable { get; set; }
    }

    public partial class MainWindow : Window
    {
        private static List<SevkiyatClass> items;
        
        public MainWindow()
        {
            InitializeComponent();

            GenelTerimler.myIp = "DESKTOP-Q1VKCF3";
            GenelTerimler.databaseName = "DogusBlokMiniSevkiyatTakip";
            GenelTerimler.userName = "root";
            GenelTerimler.userPassword = "baron099";
            GenelTerimler.mainTable = "sevkiyat";

            ImportDatabaseToGridView();
        }

        private void ImportDatabaseToGridView()
        {
            string path = @"root.txt";
            FileInfo fi = new FileInfo(path);
            
            if (!fi.Exists)
            {
                // Create the file.
                using (FileStream fs = fi.Create())
                {
                    Byte[] info =
                        new UTF8Encoding(true).GetBytes($"myIp={GenelTerimler.myIp}\n" +
                                                        $"databaseName={GenelTerimler.databaseName}\n" +
                                                        $"userName={GenelTerimler.userName}\n" +
                                                        $"userPassword={GenelTerimler.userPassword}\n" +
                                                        $"mainTable={GenelTerimler.mainTable}");
                    fs.Write(info, 0, info.Length);
                    fs.Close();
                }
            }
            
            using (StreamReader sr = fi.OpenText())
            {
                string[] lines = new string[5];
                int i = 0;
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    Debug.WriteLine($"sıra={s}");
                    lines[i] = s;
                    i++;
                }
                
                string[] kontrolListe = new string[2];

                kontrolListe = lines[0].Split('=');
                GenelTerimler.myIp = kontrolListe[1];

                kontrolListe = lines[1].Split('=');
                GenelTerimler.databaseName = kontrolListe[1];

                kontrolListe = lines[2].Split('=');
                GenelTerimler.userName = kontrolListe[1];

                kontrolListe = lines[3].Split('=');
                GenelTerimler.userPassword = kontrolListe[1];

                kontrolListe = lines[4].Split('=');
                GenelTerimler.mainTable = kontrolListe[1];
            }

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
                return ((item as SevkiyatClass)._Firma_İsim.IndexOf(txtFilter.Text, StringComparison.CurrentCultureIgnoreCase) >= 0) 
                       || ((item as SevkiyatClass)._Satış_Şekli.ToString().IndexOf(txtFilter.Text, StringComparison.CurrentCultureIgnoreCase) >= 0) 
                       || ((item as SevkiyatClass)._Sevk_Tarih.IndexOf(txtFilter.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Mamul_Cins.IndexOf(txtFilter.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Mamul_Adet.IndexOf(txtFilter.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Plaka.IndexOf(txtFilter.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Notlar.IndexOf(txtFilter.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                       || ((item as SevkiyatClass)._Araç_Sevk_Durumu.IndexOf(txtFilter.Text, StringComparison.CurrentCultureIgnoreCase) >= 0);
        }

        private void TxtFilter_OnTextChanged(object sender, TextChangedEventArgs e) 
        {
            try
            {
                CollectionViewSource.GetDefaultView(MainListView.ItemsSource).Refresh();
            }
            catch (Exception exception)
            {
                // ignore
            }
        }

        public void ListeGöster()
        {
            String mySqlConnectionString = $"server={GenelTerimler.myIp};database={GenelTerimler.databaseName};uid={GenelTerimler.userName};password={GenelTerimler.userPassword};Encrypt=False;";
            SqlConnection mySqlConn = new SqlConnection(mySqlConnectionString);
            items = new List<SevkiyatClass>();

            try
            {
                mySqlConn.Open();
            }
            catch (Exception err)
            {
                MessageBox.Show($"{err}" +
                    $"1. Kullandığınız bilgisayarın internet bağlantısını kontrol edin.\n",
                            "Server Bağlantı Hatası!", MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }

            try
            {
                SqlCommand selectTable = new SqlCommand(@$"SELECT * FROM {GenelTerimler.mainTable} WHERE 
                Sevk_Tarih>='{Convert.ToDateTime(BaşlangıçDatePicker.Text).ToString("yyyy-MM-dd")}' AND Sevk_Tarih<='{Convert.ToDateTime(BitişDatePicker.Text).ToString("yyyy-MM-dd")}' ORDER BY 
                Teslim ASC, Sevk_Tarih ASC", mySqlConn);
                SqlDataReader myDataReader = selectTable.ExecuteReader();
            
                bool boşlukKarar = true;
                int i = 1;
                while ( myDataReader.Read())
                {
                    if (myDataReader[1].ToString() == "İnşaat Teslim" && boşlukKarar)
                    {
                        for (int j = 0; j < 2; j++) //2 satırlık boşluk.
                        {
                            items.Add(new SevkiyatClass()
                            {
                                Sıra = i,
                                _Firma_İsim = "",
                                _Teslim = "",
                                _Satış_Şekli = "", 
                                _Sevk_Tarih = "", 
                                _Mamul_Cins = "",
                                _Mamul_Adet = "", 
                                _Plaka = "",
                                _Notlar = "", 
                                _Araç_Sevk_Durumu = ""
                            });

                            i++;
                        }
                        boşlukKarar = false;
                    }
                    
                    items.Add(new SevkiyatClass() { Sıra = i,
                        _Firma_İsim = myDataReader[0].ToString(),
                        _Teslim = myDataReader[1].ToString(),
                        _Satış_Şekli = myDataReader[2].ToString(), 
                        _Sevk_Tarih = Convert.ToDateTime(myDataReader[3]).ToString("dd.MM.yyyy dddd"), 
                        _Mamul_Cins = myDataReader[4].ToString(),
                        _Mamul_Adet = myDataReader[5].ToString(), 
                        _Plaka = myDataReader[6].ToString(),
                        _Notlar = myDataReader[7].ToString(), 
                        _Araç_Sevk_Durumu = myDataReader[8].ToString()
                    });
                    
                    i++;
                }
            
                
                
                MainListView.ItemsSource = items;
                mySqlConn.Close();

                CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(MainListView.ItemsSource);
                view.Filter = UserFilterSearch;
                
                if (Convert.ToDateTime(BaşlangıçDatePicker.Text) > Convert.ToDateTime(BitişDatePicker.Text))
                {
                    BitişDatePicker.Text = BaşlangıçDatePicker.Text;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"catch yakala = {e}");
            }
            
        }
        
        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            SevkDatePicker.SelectedDate = DateTime.Now;
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
                        String mySqlConnectionString = $"server={GenelTerimler.myIp};database={GenelTerimler.databaseName};uid={GenelTerimler.userName};password={GenelTerimler.userPassword};Encrypt=False;";
                        SqlConnection mySqlConn = new SqlConnection(mySqlConnectionString);
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
                        
                        SqlCommand sqlSil = new SqlCommand($@"DELETE FROM {GenelTerimler.mainTable} WHERE 
                                Firma LIKE '{sevkiyatClass._Firma_İsim}' AND Teslim LIKE '{sevkiyatClass._Teslim}' AND Satis_Sekli LIKE '{sevkiyatClass._Satış_Şekli}' AND Sevk_Tarih LIKE '{Convert.ToDateTime(sevkiyatClass._Sevk_Tarih).ToString("yyyy-MM-dd")}' AND
                                Mamul_Cins LIKE '{sevkiyatClass._Mamul_Cins}' AND Mamul_Aded LIKE '{sevkiyatClass._Mamul_Adet}' AND Plaka LIKE '{sevkiyatClass._Plaka}' AND
                                Notlar LIKE '{sevkiyatClass._Notlar}' AND Arac_Sevk_Durumu LIKE '{sevkiyatClass._Araç_Sevk_Durumu}'", mySqlConn);
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

            if (rowSevkiyat?._Firma_İsim == "") return;
            UpdateTabItem.IsSelected = true;
            FirmaTextBoxUpdate.Text = rowSevkiyat._Firma_İsim;
            SatışŞekliIntegerUpDownUpdate.Text = rowSevkiyat._Satış_Şekli;
            SevkDatePickerUpdate.Text = rowSevkiyat._Sevk_Tarih;
            MamulCinsTextBoxUpdate.Text = rowSevkiyat._Mamul_Cins;
            MamulAdetTextBoxUpdate.Text = rowSevkiyat._Mamul_Adet;
            PlakaTextBoxUpdate.Text = rowSevkiyat._Plaka;
            NotTextBoxUpdate.Text = rowSevkiyat._Notlar;
            AraçSevkDurumuComboBoxUpdate.Text = rowSevkiyat._Araç_Sevk_Durumu;

            if (FabrikaTeslimRadioButtonUpdate.Content.ToString() == rowSevkiyat._Teslim)
            {
                InsaatTeslimRadioButtonUpdate.IsChecked = false;
                FabrikaTeslimRadioButtonUpdate.IsChecked = true;
            }
            else
            {
                FabrikaTeslimRadioButtonUpdate.IsChecked = false;
                InsaatTeslimRadioButtonUpdate.IsChecked = true;
            }
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
                    $"server={GenelTerimler.myIp};database={GenelTerimler.databaseName};uid={GenelTerimler.userName};password={GenelTerimler.userPassword};Encrypt=False;";
                SqlConnection mySqlConn = new SqlConnection(mySqlConnectionString);

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
                
                string? teslimRadioButton;
                if (InsaatTeslimRadioButtonUpdate.IsChecked == true)
                {
                    teslimRadioButton = InsaatTeslimRadioButtonUpdate.Content.ToString();
                }
                else
                {
                    teslimRadioButton = FabrikaTeslimRadioButtonUpdate.Content.ToString();
                }

                string firmaİsim = new string(firmaİsimChar.ToArray());
                firmaİsim = firmaİsim.Replace("'","\"");

                string plakaNumarası = PlakaTextBoxUpdate.Text;
                if (!String.IsNullOrEmpty(PlakaTextBoxUpdate.Text))
                {
                    plakaNumarası = plakaNumarası.Replace("'","");
                    plakaNumarası = plakaNumarası.Replace(" ", "").ToUpper();
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
                SqlCommand updateCommand = new SqlCommand($@"UPDATE {GenelTerimler.mainTable} SET 
                           Firma='{firmaİsim}', Teslim='{teslimRadioButton}', Satis_Sekli='{SatışŞekliIntegerUpDownUpdate.Text}', Sevk_Tarih='{Convert.ToDateTime(SevkDatePickerUpdate.Text).ToString("yyyy-MM-dd")}', 
                           Mamul_Cins='{MamulCinsTextBoxUpdate.Text.Replace("'","\"")}', Mamul_Aded='{MamulAdetTextBoxUpdate.Text.Replace("'","\"")}', Plaka='{plakaNumarası}',
                           Notlar='{NotTextBoxUpdate.Text.Replace("'","\"")}', Arac_Sevk_Durumu='{AraçSevkDurumuComboBoxUpdate.Text}' 
                           WHERE 
                                Firma LIKE '{rowSevkiyat._Firma_İsim}' AND Teslim LIKE '{rowSevkiyat._Teslim}' AND Satis_Sekli LIKE '{rowSevkiyat._Satış_Şekli}' AND Sevk_Tarih LIKE '{Convert.ToDateTime(rowSevkiyat._Sevk_Tarih).ToString("yyyy-MM-dd")}' AND 
                                Mamul_Cins LIKE '{rowSevkiyat._Mamul_Cins}' AND Mamul_Aded LIKE '{rowSevkiyat._Mamul_Adet}' AND Plaka LIKE '{rowSevkiyat._Plaka}' AND
                                Notlar LIKE '{rowSevkiyat._Notlar}' AND Arac_Sevk_Durumu LIKE '{rowSevkiyat._Araç_Sevk_Durumu}'", mySqlConn) ;
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
                string mySqlConnectionString = $"server={GenelTerimler.myIp};database={GenelTerimler.databaseName};uid={GenelTerimler.userName};password={GenelTerimler.userPassword};Encrypt=False;";
                SqlConnection mySqlConn = new SqlConnection(mySqlConnectionString);

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
                firmaİsim = firmaİsim.Replace("'","\"");
                
                string plakaNumarası = PlakaTextBox.Text;
                if (!String.IsNullOrEmpty(PlakaTextBox.Text))
                {
                    plakaNumarası = plakaNumarası.Replace("'","");
                    plakaNumarası = plakaNumarası.Replace(" ", "").ToUpper();
                }

                string? teslimRadioButton;
                if (InsaatTeslimRadioButton.IsChecked == true)
                {
                    teslimRadioButton = InsaatTeslimRadioButton.Content.ToString();
                }
                else
                {
                    teslimRadioButton = FabrikaTeslimRadioButton.Content.ToString();
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

                SqlCommand sqlAddCommand = new SqlCommand($@"INSERT INTO {GenelTerimler.mainTable} (Firma, Teslim, Satis_Sekli, Sevk_Tarih, Mamul_Cins, Mamul_Aded, Plaka, Notlar, Arac_Sevk_Durumu) VALUES ('{firmaİsim}', '{teslimRadioButton}', '{SatışŞekliIntegerUpDown.Text}', '{Convert.ToDateTime(SevkDatePicker.Text).ToString("yyyy-MM-dd")}', '{MamulCinsTextBox.Text.Replace("'","\"")}', '{MamulAdetTextBox.Text.Replace("'","\"")}', '{plakaNumarası}', '{NotTextBox.Text.Replace("'","\"")}', '{AraçSevkDurumuComboBox.Text}')", mySqlConn);
                sqlAddCommand.ExecuteNonQuery();
                mySqlConn.Close();

                FirmaTextBox.Text = SatışŞekliIntegerUpDown.Text = SevkDatePicker.Text = 
                    MamulCinsTextBox.Text = MamulAdetTextBox.Text = PlakaTextBox.Text =
                        NotTextBox.Text = AraçSevkDurumuComboBox.Text = "";
                InsaatTeslimRadioButton.IsChecked = false;
                FabrikaTeslimRadioButton.IsChecked = true;

                MainPageTabItem.IsSelected = true;
                
                ListeGöster();
            }
            
        }
        
        private void Buttonİptal_OnClick(object sender, RoutedEventArgs e)
        {
            FirmaTextBox.Text = SatışŞekliIntegerUpDown.Text = SevkDatePicker.Text = 
                MamulCinsTextBox.Text = MamulAdetTextBox.Text = PlakaTextBox.Text =
                    NotTextBox.Text = AraçSevkDurumuComboBox.Text = "";
            
            InsaatTeslimRadioButton.IsChecked = false;
            FabrikaTeslimRadioButton.IsChecked = true;
            
            MainPageTabItem.IsSelected = true;
        }
        
        private void Date_OnSelectedDateChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender != null && sender.ToString() != "")
            {
                ListeGöster();
            }

        }

        
        
        private async void BtnExcelCikti_OnButtonClick(object sender, RoutedEventArgs e)
        {
            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(MainListView.ItemsSource);
            SaveFileDialog sfd = new SaveFileDialog() { Filter = "Excel Workbook|*.xlsx" };
    
            Debug.WriteLine($"göster = {view.CurrentItem}");
            
            if (sfd.ShowDialog() != DialogResult.HasValue)
            {
                try
                {
                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.AddRange(new DataColumn[10]
                        {
                            new DataColumn("#", typeof(int)),
                            new DataColumn("Firma", typeof(string)),
                            new DataColumn("Teslimat", typeof(string)),
                            new DataColumn("Satış Şekli", typeof(string)),
                            new DataColumn("Sevk Tarihi", typeof(string)),
                            new DataColumn("Mamul Cinsi", typeof(string)),
                            new DataColumn("Mamul Adedi", typeof(string)),
                            new DataColumn("Plakalar", typeof(string)),
                            new DataColumn("Notlar", typeof(string)),
                            new DataColumn("Sevk Durumu", typeof(string))
                        });

                        foreach (var s in items)
                        {
                            if (s._Araç_Sevk_Durumu == "")
                            {
                                s._Araç_Sevk_Durumu = "Gelmedi";
                            }
                            
                            dt.Rows.Add(s.Sıra, s._Firma_İsim, s._Teslim, s._Satış_Şekli, s._Sevk_Tarih, s._Mamul_Cins,
                                s._Mamul_Adet, s._Plaka, s._Notlar, s._Araç_Sevk_Durumu);
                        }

                        workbook.Worksheets.Add(dt, "Doğuş Blok");
                        workbook.SaveAs(sfd.FileName);
                    }

                    MessageBox.Show("Excel Dosyanız Kaydedilmiştir.", "Mesaj", MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception exception)
                {
                    MessageBox.Show($"{exception}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }


        private void BtnPrint_OnClick(object sender, RoutedEventArgs e)
        {
            DataTable dt = new DataTable();

            dt.Columns.AddRange(new DataColumn[9]
            {
                new DataColumn("Sayı", typeof(int)),
                new DataColumn("Firma", typeof(string)),
                new DataColumn("Teslim", typeof(string)),
                new DataColumn("SatışŞekli", typeof(string)),
                new DataColumn("SevkTarihi", typeof(string)),
                new DataColumn("MamulCinsi", typeof(string)),
                new DataColumn("MamulAdedi", typeof(string)),
                new DataColumn("Plakalar", typeof(string)),
                new DataColumn("SevkDurumu", typeof(string))
            });
            foreach (var s in items)
            {
                if (s._Araç_Sevk_Durumu == "")
                {
                    s._Araç_Sevk_Durumu = "Gelmedi";
                }
                
                dt.Rows.Add(s.Sıra, s._Firma_İsim, s._Teslim, s._Satış_Şekli, s._Sevk_Tarih, s._Mamul_Cins,
                    s._Mamul_Adet, s._Plaka, s._Araç_Sevk_Durumu);
            }
            
            dg.DataContext = dt.DefaultView;

            PrintDG print = new PrintDG();
            print.printDG(dg, $"{BaşlangıçDatePicker.Text} / {BitişDatePicker.Text}   Doğuş Blok Sevkiyat Çıktısı"); 
        }
    }
    
    public  class PrintDG
    {
        public void printDG(DataGrid dataGrid, string title)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                FlowDocument fd = new FlowDocument();

                Paragraph p = new Paragraph(new Run(title));
                p.FontStyle = dataGrid.FontStyle;
                p.FontFamily = dataGrid.FontFamily;
                p.FontSize = 18;
                fd.Blocks.Add(p);

                Table table = new Table();
                TableRowGroup tableRowGroup = new TableRowGroup();
                TableRow r = new TableRow();
                fd.PageWidth = printDialog.PrintableAreaWidth;
                fd.PageHeight = printDialog.PrintableAreaHeight;
                fd.BringIntoView();

                fd.TextAlignment = TextAlignment.Center;
                fd.ColumnWidth = 500;
                table.CellSpacing = 0;

                var headerList = dataGrid.Columns.Select(e => e.Header.ToString()).ToList();
                List<dynamic> bindList = new List<dynamic>();

                for (int j = 0; j < headerList.Count; j++)
                {
                    r.Cells.Add(new TableCell(new Paragraph(new Run(headerList[j]))));
                    r.Cells[j].ColumnSpan = 4;
                    r.Cells[j].Padding = new Thickness(4);
                    r.Cells[j].BorderBrush = Brushes.Black;
                    r.Cells[j].FontWeight = FontWeights.Bold;
                    r.Cells[j].Background = Brushes.DarkGray;
                    r.Cells[j].Foreground = Brushes.White;
                    r.Cells[j].BorderThickness = new Thickness(1, 1, 1, 1);

                    var binding = (dataGrid.Columns[j] as DataGridBoundColumn)?.Binding as Binding;
                    if (binding != null)
                    {
                        bindList.Add(binding.Path.Path);
                    }
                }

                tableRowGroup.Rows.Add(r);
                table.RowGroups.Add(tableRowGroup);

                foreach (DataRowView row in dataGrid.Items)
                {
                    table.BorderBrush = Brushes.Gray;
                    table.BorderThickness = new Thickness(1, 1, 0, 0);
                    table.FontStyle = dataGrid.FontStyle;
                    table.FontFamily = dataGrid.FontFamily;
                    table.FontSize = 13;
                    tableRowGroup = new TableRowGroup();
                    r = new TableRow();

                    for (int j = 0; j < row.Row.ItemArray.Length; j++)
                    {
                        r.Cells.Add(new TableCell(new Paragraph(new Run(row.Row.ItemArray[j].ToString()))));

                        r.Cells[j].ColumnSpan = 4;
                        r.Cells[j].Padding = new Thickness(4);
                        r.Cells[j].BorderBrush = Brushes.DarkGray;
                        r.Cells[j].BorderThickness = new Thickness(0, 0, 1, 1);
                    }
                    tableRowGroup.Rows.Add(r);
                    table.RowGroups.Add(tableRowGroup);
                }
                
                fd.Blocks.Add(table);
                try
                {
                    printDialog.PrintDocument(((IDocumentPaginatorSource)fd).DocumentPaginator, "");
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        "Kaydetmeye çalıştığınız PDF dosyasıyla aynı isimde bir PDF dosyası bilgisayarınızda açık. \nLütfen açık olan PDF dosyasını kapatın ve tekrar deneyin.",
                        "Hata!",MessageBoxButton.OK, MessageBoxImage.Stop);
                    Console.WriteLine(e);
                    
                }
            }
        }
    }
}