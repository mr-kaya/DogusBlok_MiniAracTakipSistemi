using System;
using System.Diagnostics;
using System.Windows;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg.Attr;

namespace DogusBlok_MiniAracTakipSistemi;

public partial class AddWindow : Window
{
    public AddWindow()
    {
        InitializeComponent();

        GenelTerimler mainTerimler = new GenelTerimler();
        string mySqlConnectionString = $"server={mainTerimler.myIp};port=3306;uid={mainTerimler.userName};database={mainTerimler.databaseName};";
        MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);
        
    }

    private void ButtonEkle_OnClick(object sender, RoutedEventArgs e)
    {
        string[] hataTutucu = new string[2];
        if (String.IsNullOrEmpty(FirmaTextBox.Text))
            hataTutucu[0] = "Firma Adı Alanı Boş Bırakılamaz!";
        else
            hataTutucu[0] = "";

        if (String.IsNullOrEmpty(SevkDatePicker.Text))
            hataTutucu[1] = "Sevk Tarih Alanı Boş Bırakılamaz!";
        else
            hataTutucu[1] = "";

        HataTutucuTextBox.Text = $"{hataTutucu[0]}\n{hataTutucu[1]}";

        if (String.IsNullOrEmpty(hataTutucu[0]) && String.IsNullOrEmpty(hataTutucu[1]))
        {
            GenelTerimler mainTerimler = new GenelTerimler();
            string mySqlConnectionString = $"server={mainTerimler.myIp};port=3306;uid={mainTerimler.userName};database={mainTerimler.databaseName};";
            MySqlConnection mySqlConn = new MySqlConnection(mySqlConnectionString);
            
            Debug.WriteLine($"Veriler Başarılı Şekilde Database'e Yazdırıldı.");
        }
    }

    private void Buttonİptal_OnClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}