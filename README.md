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

![1](https://user-images.githubusercontent.com/18140418/211635743-90322abe-9466-44fb-9595-0bff6be8f5cc.png)
![2](https://user-images.githubusercontent.com/18140418/211636140-4e0126e1-8baf-43ca-b518-9b9f87c638df.png)
![3](https://user-images.githubusercontent.com/18140418/211636153-6d8a6b13-fc92-4bbc-ac2c-3a1438e1700a.png)
![4](https://user-images.githubusercontent.com/18140418/211636164-4f40a893-751c-4d2d-8bad-56145068e67b.png)
![5](https://user-images.githubusercontent.com/18140418/211636171-33a432df-da25-4ac3-8007-19ace647286f.png)
![6](https://user-images.githubusercontent.com/18140418/211636179-97fffb0f-5890-45b0-afd3-a7cbea7cb4e7.png)
