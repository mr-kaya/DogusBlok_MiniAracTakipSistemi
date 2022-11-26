using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg.Attr;
using static DogusBlok_MiniAracTakipSistemi.GenelTerimler;

namespace DogusBlok_MiniAracTakipSistemi;

public partial class AddWindow : Window
{

    public AddWindow()
    {
        InitializeComponent();

        
    }

    public void ButtonEkle_OnClick(object sender, RoutedEventArgs e)
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
            string mySqlConnectionString = $"server={myIp};port=3306;uid={userName};database={databaseName};";
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
            MySqlCommand sqlAddCommand = new MySqlCommand($@"INSERT INTO {mainTable} (Firma, Satis_Sekli, Sevk_Tarih, Mamul_Cins, Mamul_Aded, Plaka, Notlar, Arac_Sevk_Durumu) VALUES ('{FirmaTextBox.Text}', '{SatışŞekliIntegerUpDown.Text}', '{sevkiyatTarih}', '{MamulCinsTextBox.Text}', '{MamulAdetTextBox.Text}', '{PlakaTextBox.Text}', '{NotTextBox.Text}', '{AraçSevkDurumuComboBox.Text}')", mySqlConn);
            sqlAddCommand.ExecuteNonQuery();
            mySqlConn.Close();
            
            this.Close();
        }
    }

    private void Buttonİptal_OnClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}