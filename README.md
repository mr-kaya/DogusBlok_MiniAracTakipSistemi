# DogusBlok_MiniAracTakipSistemi
1. Programı çalıştırdığında root.txt dosyası oluşturulacak.

2. İçindeki veriler :
    myIp=//MSSQL Server Name
    databaseName=//Veritabanı İsmi
    userName=//Verileri Okuma ve Yazma İzni Verilmiş Kullanıcı Adı
    userPassword=//İzinleri Verilmiş Kullanıcının Şifresi
    mainTable=//Bağlanılacak Veritabanının Tablo İsmi

3. Veritabanı içindeki veriler :
CREATE TABLE [dbo].[sevkiyat](
	[Firma] [nvarchar](100) NOT NULL,
	[Satis_Sekli] [nvarchar](50) NULL,
	[Sevk_Tarih] [date] NOT NULL,
	[Mamul_Cins] [nvarchar](100) NULL,
	[Mamul_Aded] [nvarchar](100) NULL,
	[Plaka] [nvarchar](100) NULL,
	[Notlar] [text] NULL,
	[Arac_Sevk_Durumu] [nvarchar](50) NULL,
	[Teslim] [nvarchar](15) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[sevkiyat] ADD  CONSTRAINT [DF__sevkiyat__Teslim__49C3F6B7]  DEFAULT ('Fabrika Teslim') FOR [Teslim]
GO

4. Progrmada kullanılan NuGet Paketleri :
    ClosedXML
    Extended.Wpf.Toolkit
    Microsoft.Data.SqlClient
