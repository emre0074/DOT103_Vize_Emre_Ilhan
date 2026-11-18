using System;

/*  =====================================================================
 *  DOT 103 VİZE PROJESİ - LABİRENTTEN KAÇIŞ
 *  Seçenek B (Dallanan Zindan Odaları) + Seçenek A (Sıra Tabanlı Boss Savaşı)
 *
 *  HİKAYE: Prenses Ayla, büyücü Morvak tarafından labirentin sonundaki Kara Kale'ye
 *  kaçırıldı. Oyuncu 10 odalık labirentten geçer, 4. odada Ork'u, 7. odada Ejderha'yı,
 *  10. odada kaleyi koruyan Kara Şövalye'yi yener, prensesi kurtarır ve Kral onu ödüllendirir.
 *
 *  KILAVUZDAKİ 8 KURALIN KODDAKİ YERLERİ:
 *  Kural 1 - Game Loop ..........: Main metodu -> while (oyunAktif)
 *  Kural 2 - Durum Değişkenleri .: can, mana, atk, zirh, altin, iksir, okSayisi ...
 *  Kural 3 - Random .............: static Random zar (sadece bir kez new), zar.Next(1, 21) = d20
 *  Kural 4 - do while ve Girdi ..: SecimAl, KapiOdasi, Magaza metotları
 *  Kural 5 - break / continue ...: Savas metodu (düşman ölünce break, sersemleyince continue)
 *  Kural 6 - Fail-Safe ..........: MAX_OYUN_TURU = 10 (oyun 10 turda biter)
 *                                  MAX_TUR = 20 (bir savaş en fazla 20 tur sürer)
 *  Kural 7 - Dizi ...............: silahAdlari, canavarAdlari, iyiKapilar, tumSonlar, sorular ...
 *  Kural 8 - Metotlar ...........: DurumEkrani, HasarAl, Savas, Magaza, KapiyiAc ...
 *  ===================================================================== */

namespace LabirenttenKacis
{
    internal class Program
    {
        // ================== SABİTLER ==================
        const int MAX_OYUN_TURU = 10;  // KURAL 6: Oyun 10 turdan oluşur (her tur 1 oda)
        const int MAX_TUR = 20;        // KURAL 6: Bir savaş en fazla 20 tur sürer
        const int ORK_TURU = 4;        // 4. odada ara boss: Ork
        const int EJDERHA_TURU = 7;    // 7. odada ara boss: Ejderha
        const int YETENEK_MANA = 20;   // Özel yetenek kaç mana harcar
        const int KUTU_GENISLIGI = 60; // ekrandaki kutuların iç genişliği

        // Kapı türleri: İYİ kapılar
        const int KAPI_KORIDOR = 0;    // sessiz koridor, güvenli
        const int KAPI_AC_ASLAN = 1;   // 2 aydır aç aslan -> açlıktan ölmüş, güvenli!
        const int KAPI_SORU = 2;       // soru soran kapı
        const int KAPI_HAZINE = 3;     // gerçek hazine
        const int KAPI_CESME = 4;      // şifa çeşmesi
        const int KAPI_CANAVAR = 5;    // küçük canavar savaşı, ödüllü
        const int KAPI_TUCCAR = 6;     // gezgin tüccar (mağaza)
        // Kapı türleri: TEHLİKELİ kapılar
        const int KAPI_LAV = 7;        // lav -> anında ölüm
        const int KAPI_BUZ = 8;        // eksi 200 derece
        const int KAPI_CUKUR = 9;      // derin çukur
        const int KAPI_ASLAN = 10;     // tok ama sinirli aslan
        const int KAPI_ZEHIR = 11;     // zehirli gaz
        const int KAPI_DIKEN = 12;     // dikenli duvarlar
        const int KAPI_SU = 13;        // su dolan oda
        const int KAPI_MIMIK = 14;     // sahte hazine sandığı

        // ================== KURAL 2: DURUM DEĞİŞKENLERİ ==================
        static string oyuncuAdi = "";
        static string sinifAdi = "";
        static int sinifNo;            // 1 = Savaşçı, 2 = Okçu, 3 = Gezgin
        static string yetenekAdi = "";
        static int can;
        static int maxCan;
        static int mana;
        static int maxMana;
        static int atk;                // temel saldırı gücü
        static int zirh;               // gelen hasardan düşülür
        static int altin;              // para
        static int iksir;
        static int okSayisi;           // arbalet için
        static int pusula;             // tehlikeli bir kapıyı gösterir
        static int dirilisTilsimi;     // ölünce 1 kere diriltir
        static int zehir;              // kaç hamle daha zehir hasarı alınacak
        static bool sonSansKullanildi; // ölürken 1 kez "son şans" zarı atılır
        static int oyunTuru;           // kaçıncı odadayız (1-20)
        static bool oyunAktif;
        static string sonAdi = "";     // oyun hangi sonla bitti
        static int yenilenBoss;
        static int yenilenCanavar;

        // Zorluk ayarları
        static string zorlukAdi = "";
        static int hasarYuzdesi;       // gelen hasar yüzde kaç olacak
        static int skorYuzdesi;        // zor modda daha çok puan

        static int rekorSkor = 0;      // tekrar oynansa da sıfırlanmaz

        // ================== KURAL 7: DİZİLER ==================
        static int[] iyiKapilar = { KAPI_KORIDOR, KAPI_AC_ASLAN, KAPI_SORU, KAPI_HAZINE, KAPI_CESME, KAPI_CANAVAR, KAPI_TUCCAR };
        static int[] tehlikeliKapilar = { KAPI_LAV, KAPI_BUZ, KAPI_CUKUR, KAPI_ASLAN, KAPI_ZEHIR, KAPI_DIKEN, KAPI_SU, KAPI_MIMIK };

        // Silahlar: aynı index aynı silahı gösterir
        static string[] silahAdlari = { "Paslı Hançer", "Demir Kılıç", "Arbalet", "Ejder Kılıcı" };
        static int[] silahHasari = { 4, 10, 14, 20 };
        static int[] silahFiyati = { 0, 50, 70, 150 };
        static bool[] silahVar = new bool[4];
        static int seciliSilah;

        // Kapıların arkasından çıkabilecek küçük canavarlar
        static string[] canavarAdlari = { "Goblin", "İskelet Savaşçı", "Dev Örümcek", "Mağara Trolü" };
        static int[] canavarCani = { 25, 35, 40, 60 };
        static int[] canavarAtk = { 5, 7, 8, 11 };
        static int[] canavarOdulu = { 15, 20, 25, 40 };

        // Labirentin 3 bölgesi ve hikayeleri
        static string[] bolgeAdlari = { "KARANLIK GİRİŞ", "DERİN MAĞARALAR", "KARA KALE'NİN GÖLGESİ" };
        static string[] bolgeHikayeleri =
        {
            "Labirentin ilk koridorları. Duvarlarda eski maceracıların tırnak izleri var.\nKapıların ne sakladığı henüz açıkça belli.",
            "Taş merdivenlerden derinlere indin. Hava ağırlaştı.\nArtık kapılar sırlarını kolay vermiyor, ipuçlarını dikkatli oku.",
            "Uzakta Kara Kale'nin kuleleri görünüyor. Prenses orada bir yerde.\nMorvak'ın en sinsi tuzakları burada, ipuçları birbirine benziyor!"
        };

        // Oyundaki tüm sonlar ve hangilerinin bulunduğu
        static string[] tumSonlar =
        {
            "DÜĞÜN SONU", "HAZİNE SONU", "KOMUTAN SONU",
            "KÜL SONU", "SONSUZ DÜŞÜŞ SONU", "BUZ HEYKELİ SONU", "ASLAN YEMİ SONU", "BİLMECE KURBANI SONU",
            "ZEHİR SONU", "DİKENLİ DUVAR SONU", "BOĞULMA SONU", "AÇGÖZLÜLÜK SONU",
            "YENİLGİ SONU", "SONSUZ DÜELLO SONU", "ENKAZ SONU"
        };
        static bool[] bulunanSonlar = new bool[15];

        // Soru soran kapının soruları
        static string[] sorular =
        {
            "Şehirlerim var ama evim yok, ormanlarım var ama ağacım yok,\n   denizlerim var ama balığım yok. Ben neyim?",
            "Ne kadar çok alırsan arkanda o kadar çok bırakırsın. Nedir?",
            "Bir çobanın 17 koyunu var. 9'u hariç hepsi kaçıyor. Kaç koyun kaldı?",
            "Kullanılmadan önce kırılması gereken şey nedir?",
            "Bir yarışta ikinci sıradakini geçtin. Şimdi kaçıncısın?",
            "Bir sayının yarısının yarısı 5 ise o sayı kaçtır?"
        };
        static string[] secenekler =
        {
            "1) Rüya   2) Harita   3) Ayna",
            "1) Adım   2) Para     3) Nefes",
            "1) 8      2) 9        3) 17",
            "1) Cam    2) Söz      3) Yumurta",
            "1) İkinci 2) Birinci  3) Üçüncü",
            "1) 20     2) 10       3) 25"
        };
        static int[] dogruCevaplar = { 2, 1, 2, 3, 1, 1 };

        // KURAL 3: Random döngülerin DIŞINDA sadece bir kez oluşturuluyor
        static Random zar = new Random();

        // ================== KURAL 1: ANA OYUN DÖNGÜSÜ ==================
        static void Main(string[] args)
        {
            GirisEkrani(); // açılış introsu sadece bir kez gösterilir

            int tekrar;
            do
            {
                OyunuSifirla();
                HikayeEkrani();
                ZorlukSec();
                KarakterSec();

                // GAME LOOP: her döngü 1 tur = 1 oda. Oyun bitince oyunAktif false olur.
                while (oyunAktif)
                {
                    oyunTuru++;

                    // Yeni bir bölgeye girince hikayesi anlatılır
                    if (oyunTuru == 1) BolgeHikayesi(0);
                    if (oyunTuru == 5) BolgeHikayesi(1);
                    if (oyunTuru == 8) BolgeHikayesi(2);

                    if (oyunTuru == ORK_TURU)
                    {
                        Savas("Mağara Orku", 85, 10, 80, 1, true);
                        if (oyunAktif) Magaza();
                    }
                    else if (oyunTuru == EJDERHA_TURU)
                    {
                        Savas("Kızıl Ejderha", 140, 15, 130, 2, true);
                        if (oyunAktif) Magaza();
                    }
                    else if (oyunTuru == MAX_OYUN_TURU)
                    {
                        // 10. tur: Kara Kale ve final boss
                        KaleyeVaris();
                        Magaza();
                        Savas("Kara Şövalye", 200, 19, 250, 3, true);
                        if (oyunAktif)
                        {
                            PrensesVeKral();
                            oyunAktif = false; // oyun kazanılarak bitti
                        }
                    }
                    else
                    {
                        KapiOdasi(); // normal oda: kapı seçimi
                    }
                }

                OyunSonuEkrani();

                Console.WriteLine("\nTekrar oynamak ister misin?  1) Evet  2) Hayır");
                tekrar = SecimAl(1, 2);
            } while (tekrar == 1);

            RenkliYaz("\nOynadığın için teşekkürler!", ConsoleColor.Cyan);
            RenkliYaz("Yapımcı: EMRE İLHAN", ConsoleColor.Yellow);
            Console.WriteLine("\n(Çıkmak için Enter'a bas)");
            Console.ReadLine(); // pencere hemen kapanmasın diye bekler
        }

        // ================== AÇILIŞ İNTROSU ==================
        // Logo satırları bir string dizisinde tutulur, for döngüsü satırları sırayla yazar.
        static void GirisEkrani()
        {
            YeniEkran();

            string[] logo =
            {
                " _         _    ____ ___ ____  _____ _   _ _____ ",
                "| |       / \\  | __ )_ _|  _ \\| ____| \\ | |_   _|",
                "| |      / _ \\ |  _ \\| || |_) |  _| |  \\| | | |  ",
                "| |___  / ___ \\| |_) | ||  _ <| |___| |\\  | | |  ",
                "|_____|/_/   \\_\\____/___|_| \\_\\_____|_| \\_| |_|  "
            };
            // Her logo satırının rengi: yukarıdan aşağı sarıdan kırmızıya, ateş gibi
            ConsoleColor[] logoRenkleri =
            {
                ConsoleColor.Yellow, ConsoleColor.Yellow, ConsoleColor.DarkYellow, ConsoleColor.Red, ConsoleColor.DarkRed
            };

            Console.WriteLine();
            for (int i = 0; i < logo.Length; i++)
            {
                RenkliYaz("   " + logo[i], logoRenkleri[i]);
            }

            Console.WriteLine();
            RenkliYaz("            ~ K A Ç I Ş :  Prensesin Kurtarılışı ~", ConsoleColor.White);
            Console.WriteLine();

            // Süs çizgisi: for döngüsüyle '=' ve '-' sırayla yazılır
            string susCizgisi = "   ";
            for (int i = 0; i < 50; i++)
            {
                if (i % 2 == 0) susCizgisi = susCizgisi + "=";
                else susCizgisi = susCizgisi + "-";
            }
            RenkliYaz(susCizgisi, ConsoleColor.DarkGray);

            // Kale resmi
            RenkliYaz("                     |>>>                   |>>>", ConsoleColor.Red);
            RenkliYaz("                     |                      |", ConsoleColor.Gray);
            RenkliYaz("                 _  _|_  _              _  _|_  _", ConsoleColor.Gray);
            RenkliYaz("                |;|_|;|_|;|            |;|_|;|_|;|", ConsoleColor.Gray);
            RenkliYaz("                \\ .  .  . /____________\\ .  .  . /", ConsoleColor.Gray);
            RenkliYaz("                 |:  .  : |   ______   | :  .  :|", ConsoleColor.DarkGray);
            RenkliYaz("                 |  .  :  |  |  ||  |  |  :  .  |", ConsoleColor.DarkGray);
            RenkliYaz("                 |:  .    |  |  ||  |  |    .  :|", ConsoleColor.DarkGray);
            RenkliYaz("                 |________|__|__||__|__|________|", ConsoleColor.DarkGray);
            Console.WriteLine();

            // Yapımcı kutusu
            RenkliYaz("            +-----------------------------------+", ConsoleColor.DarkCyan);
            RenkliYaz("            |       Bir EMRE İLHAN Yapımı       |", ConsoleColor.Cyan);
            RenkliYaz("            |                                   |", ConsoleColor.DarkCyan);
            RenkliYaz("            |   DOT 103 - Programlamaya Giriş   |", ConsoleColor.Gray);
            RenkliYaz("            |        Vize Projesi - 2026        |", ConsoleColor.Gray);
            RenkliYaz("            +-----------------------------------+", ConsoleColor.DarkCyan);
            Console.WriteLine();

            RenkliYaz("        Karanlık labirent seni bekliyor, kahraman...", ConsoleColor.DarkYellow);
            RenkliYaz("\n               >>> Başlamak için Enter'a bas <<<", ConsoleColor.Green);
            Console.ReadLine();
        }

        // ================== OYUN BAŞLANGICI ==================
        static void OyunuSifirla()
        {
            can = 100;
            maxCan = 100;
            mana = 50;
            maxMana = 50;
            atk = 8;
            zirh = 0;
            altin = 30;
            iksir = 2;
            okSayisi = 0;
            pusula = 0;
            dirilisTilsimi = 0;
            zehir = 0;
            sonSansKullanildi = false;
            oyunTuru = 0;
            oyunAktif = true;
            sonAdi = "";
            yenilenBoss = 0;
            yenilenCanavar = 0;

            for (int i = 0; i < silahVar.Length; i++)
            {
                silahVar[i] = false;
            }
            silahVar[0] = true; // oyuna paslı hançer ile başlıyoruz
            seciliSilah = 0;
        }

        static void HikayeEkrani()
        {
            YeniEkran();
            Baslik("LABİRENTTEN KAÇIŞ  ~  Prensesin Kurtarılışı");
            if (rekorSkor > 0)
            {
                Console.WriteLine("   Şu anki rekor: " + rekorSkor);
            }
            Console.WriteLine();

            do
            {
                Console.WriteLine("Kahramanının adı: ");
                oyuncuAdi = Console.ReadLine();
                if (oyuncuAdi == "" || oyuncuAdi == null)
                {
                    Console.WriteLine("İsim boş olamaz!");
                }
            } while (oyuncuAdi == "" || oyuncuAdi == null);

            YeniEkran();
            Baslik("~  H İ K A Y E  ~");
            Console.WriteLine();
            Console.WriteLine("Bir gece kara bulutlar Kral Doruk'un sarayını kapladı.");
            Console.WriteLine("Kötü büyücü Morvak, Prenses Ayla'yı kaçırıp labirentin sonundaki");
            Console.WriteLine("KARA KALE'ye kapattı. Kaleyi yenilmez KARA ŞÖVALYE koruyor.");
            Console.WriteLine();
            Console.WriteLine("Kral ilan etti: \"Kızımı kurtaran, istediği ödülü alacak!\"");
            Console.WriteLine("Yüzlerce şövalye labirente girdi. Hiçbiri geri dönmedi.");
            Console.WriteLine();
            Console.WriteLine("Şimdi sıra sende, " + oyuncuAdi + ".");
            Console.WriteLine("Labirent " + MAX_OYUN_TURU + " odadan oluşuyor. Her odada kapılar var:");
            Console.WriteLine("bazılarının ardında hazine, bazılarında lav, buz, zehir ve canavarlar...");
            Console.WriteLine(ORK_TURU + ". odada bir ORK, " + EJDERHA_TURU + ". odada bir EJDERHA yolunu kesecek.");
            Console.WriteLine(MAX_OYUN_TURU + ". odada ise Kara Kale ve onun bekçisi seni bekliyor.");
            Console.WriteLine();
            Console.WriteLine("Cebinde paslı bir hançer, iki iksir ve biraz altın var. Yolculuk başlıyor...");
            Bekle();
        }

        static void ZorlukSec()
        {
            YeniEkran();
            Baslik("ZORLUK SEÇ");
            KutuSatiri("[1] Kolay  - Daha az hasar alırsın     (skor x0.5)");
            KutuSatiri("[2] Normal - Dengeli oyun              (skor x1)");
            KutuSatiri("[3] Kabus  - Her şey çok acıtır        (skor x2)");
            KutuCizgisi("=");

            // Hafta 5: switch case dallanması
            int secim = SecimAl(1, 3);
            switch (secim)
            {
                case 1:
                    zorlukAdi = "Kolay"; hasarYuzdesi = 70; skorYuzdesi = 50;
                    break;
                case 2:
                    zorlukAdi = "Normal"; hasarYuzdesi = 100; skorYuzdesi = 100;
                    break;
                case 3:
                    zorlukAdi = "Kabus"; hasarYuzdesi = 135; skorYuzdesi = 200;
                    break;
            }
        }

        static void KarakterSec()
        {
            YeniEkran();
            Baslik("KARAKTER SINIFI SEÇ");
            KutuSatiri("[1] SAVAŞÇI - 130 can, +2 saldırı, 2 zırh");
            KutuSatiri("    Yetenek: Kükreyen Darbe (ıskalamayan çift hasar)");
            KutuCizgisi("-");
            KutuSatiri("[2] OKÇU    - 90 can, Arbalet ve 10 okla başlar");
            KutuSatiri("    Yetenek: Ok Yağmuru (3 ok, büyük ihtimalle sersemletir)");
            KutuCizgisi("-");
            KutuSatiri("[3] GEZGİN  - 80 altın, 2 Pusula, +1 iksir, 70 mana");
            KutuSatiri("    Yetenek: Şifa Işığı (+35 can, zehri temizler)");
            KutuCizgisi("=");

            sinifNo = SecimAl(1, 3);
            switch (sinifNo)
            {
                case 1:
                    sinifAdi = "Savaşçı";
                    yetenekAdi = "Kükreyen Darbe";
                    maxCan = 130; can = 130; atk += 2; zirh = 2;
                    break;
                case 2:
                    sinifAdi = "Okçu";
                    yetenekAdi = "Ok Yağmuru";
                    maxCan = 90; can = 90;
                    silahVar[2] = true; seciliSilah = 2; okSayisi = 10;
                    break;
                case 3:
                    sinifAdi = "Gezgin";
                    yetenekAdi = "Şifa Işığı";
                    altin = 80; pusula = 2; iksir++;
                    maxMana = 70; mana = 70;
                    break;
            }

            Console.WriteLine("\nBol şans " + sinifAdi + " " + oyuncuAdi + "...");
            Bekle();
        }

        static void BolgeHikayesi(int bolgeNo)
        {
            YeniEkran();
            Baslik("*** " + bolgeAdlari[bolgeNo] + " ***");
            Console.WriteLine();
            Console.WriteLine(bolgeHikayeleri[bolgeNo]);
            Bekle();
        }

        // ================== KURAL 8: EKRAN METOTLARI ==================
        // Oyuncunun tüm bilgilerini ekrana çizer
        static void DurumEkrani()
        {
            YeniEkran();
            Baslik("ODA " + oyunTuru + "/" + MAX_OYUN_TURU + "   ~   Zorluk: " + zorlukAdi);
            RenkliKutuSatiri("Kahraman : " + oyuncuAdi + " (" + sinifAdi + ")", ConsoleColor.White);
            RenkliKutuSatiri("Yolculuk : " + BarCiz(oyunTuru, MAX_OYUN_TURU) + " Oda " + oyunTuru + "/" + MAX_OYUN_TURU, ConsoleColor.DarkYellow);
            RenkliKutuSatiri("Sıradaki : " + SiradakiBoss(), ConsoleColor.DarkRed);
            KutuCizgisi("-");
            RenkliKutuSatiri("CAN      : " + BarCiz(can, maxCan) + " " + can + "/" + maxCan + CanUyarisi(), CanRengi());
            RenkliKutuSatiri("MANA     : " + BarCiz(mana, maxMana) + " " + mana + "/" + maxMana, ConsoleColor.Cyan);
            KutuCizgisi("-");
            RenkliKutuSatiri("Silah    : " + silahAdlari[seciliSilah] + " (+" + silahHasari[seciliSilah] + " hasar)", ConsoleColor.White);
            KutuSatiri("Güç      : Saldırı " + atk + " | Zırh " + zirh + " | Toplam Hasar " + (atk + silahHasari[seciliSilah]));
            RenkliKutuSatiri("Altın    : " + altin, ConsoleColor.Yellow);
            KutuSatiri("Çanta    : İksir " + iksir + " | Ok " + okSayisi + " | Pusula " + pusula + " | Tılsım " + dirilisTilsimi);
            RenkliKutuSatiri("Yetenek  : " + yetenekAdi + " (" + YETENEK_MANA + " mana)", ConsoleColor.Magenta);
            if (sonSansKullanildi) KutuSatiri("Son Şans : Kullanıldı");
            else KutuSatiri("Son Şans : Hazır (ölürken 1 kez zar atılır)");
            if (zehir > 0)
            {
                KutuCizgisi("-");
                RenkliKutuSatiri("ZEHİRLİSİN! " + zehir + " hamle can kaybedersin. İksir iyileştirir.", ConsoleColor.Magenta);
            }
            KutuCizgisi("=");
        }

        static string SiradakiBoss()
        {
            if (oyunTuru <= ORK_TURU) return "Mağara Orku (" + ORK_TURU + ". oda)";
            else if (oyunTuru <= EJDERHA_TURU) return "Kızıl Ejderha (" + EJDERHA_TURU + ". oda)";
            else return "Kara Şövalye - Kalenin Bekçisi (" + MAX_OYUN_TURU + ". oda)";
        }

        // Canın azaldıkça yanına uyarı yazısı eklenir (if / else if)
        static string CanUyarisi()
        {
            if (can > maxCan / 2) return "";
            else if (can > maxCan / 4) return "  (Yaralı)";
            else return "  (KRİTİK!)";
        }

        // 20 kutucuklu bir bar yazısı hazırlar (can, mana, yolculuk ve düşman canı için ortak)
        // for döngüsü her turda yazının sonuna bir kutucuk ekler
        static string BarCiz(int mevcut, int enFazla)
        {
            int dolu = mevcut * 20 / enFazla;
            if (dolu < 0) dolu = 0;

            string bar = "[";
            for (int i = 0; i < 20; i++)
            {
                if (i < dolu) bar = bar + "#";
                else bar = bar + ".";
            }
            bar = bar + "]";
            return bar;
        }

        // Hafta 7: iç içe for -> dıştaki döngü satırlar, içteki döngü kapılar
        static void KapilariCiz(int kapiSayisi)
        {
            string[] satirlar = { " ______  ", "|      | ", "|  ?   | ", "|    o | ", "|______| " };

            for (int satir = 0; satir < satirlar.Length; satir++)
            {
                string cizgi = ""; // bu satırdaki tüm kapılar yan yana bu yazıya eklenir
                for (int kapi = 0; kapi < kapiSayisi; kapi++)
                {
                    if (satir == 2)
                        cizgi = cizgi + "|  " + (kapi + 1) + "   | "; // ortadaki satıra kapı numarası
                    else
                        cizgi = cizgi + satirlar[satir];
                }
                RenkliYaz(cizgi, ConsoleColor.DarkYellow);
            }
        }

        // Yeni ekrana geçerken for döngüsüyle araya 3 boş satır koyar
        static void YeniEkran()
        {
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine();
            }
        }

        // ================== GÖRÜNÜM METOTLARI (KUTULAR) ==================
        // Bir parçayı istenen sayıda yan yana ekler. Örnek: Tekrarla("=", 5) -> "====="
        static string Tekrarla(string parca, int adet)
        {
            string sonuc = "";
            for (int i = 0; i < adet; i++)
            {
                sonuc = sonuc + parca;
            }
            return sonuc;
        }

        // Yazıyı istenen renkte yazar, sonra rengi normale döndürür
        // Console.ForegroundColor: yazı rengi, Console.ResetColor: varsayılan renge dönüş
        static void RenkliYaz(string metin, ConsoleColor renk)
        {
            Console.ForegroundColor = renk;
            Console.WriteLine(metin);
            Console.ResetColor();
        }

        // Kutunun yatay çizgisi: +=====...=====+ veya +-----...-----+
        static void KutuCizgisi(string parca)
        {
            RenkliYaz("+" + Tekrarla(parca, KUTU_GENISLIGI) + "+", ConsoleColor.DarkCyan);
        }

        // Yazıyı kutunun içine koyar, sağ tarafı boşlukla doldurup kenarı kapatır
        static void KutuSatiri(string metin)
        {
            RenkliKutuSatiri(metin, ConsoleColor.Gray);
        }

        // Aynı kutu satırı ama istenen renkte
        static void RenkliKutuSatiri(string metin, ConsoleColor renk)
        {
            int bosluk = KUTU_GENISLIGI - 2 - metin.Length;
            if (bosluk < 0) bosluk = 0; // yazı çok uzunsa boşluk eklenmez
            RenkliYaz("| " + metin + Tekrarla(" ", bosluk) + " |", renk);
        }

        // Canın azaldıkça satır yeşilden sarıya, sonra kırmızıya döner
        static ConsoleColor CanRengi()
        {
            if (can > maxCan / 2) return ConsoleColor.Green;
            else if (can > maxCan / 4) return ConsoleColor.Yellow;
            else return ConsoleColor.Red;
        }

        // Ortalanmış başlık kutusu çizer
        static void Baslik(string metin)
        {
            int sol = (KUTU_GENISLIGI - metin.Length) / 2;
            if (sol < 0) sol = 0;
            int sag = KUTU_GENISLIGI - metin.Length - sol;
            if (sag < 0) sag = 0;

            RenkliYaz("+" + Tekrarla("=", KUTU_GENISLIGI) + "+", ConsoleColor.DarkYellow);
            RenkliYaz("|" + Tekrarla(" ", sol) + metin + Tekrarla(" ", sag) + "|", ConsoleColor.Yellow);
            RenkliYaz("+" + Tekrarla("=", KUTU_GENISLIGI) + "+", ConsoleColor.DarkYellow);
        }

        static void Bekle()
        {
            RenkliYaz("\n                >> Devam etmek için Enter'a bas <<", ConsoleColor.DarkGray);
            Console.ReadLine();
        }

        // ================== KURAL 4: GİRDİ GÜVENLİĞİ ==================
        // Girilen yazıyı min..max arasındaki her sayıyla tek tek karşılaştırır.
        // Eşleşme yoksa uyarır ve do while sayesinde tekrar sorar, program çökmez.
        static int SecimAl(int min, int max)
        {
            int secim;
            do
            {
                secim = -1; // -1 = henüz geçerli bir seçim yok
                Console.WriteLine(">> Seçimin:");
                string girdi = Console.ReadLine();

                for (int i = min; i <= max; i++)
                {
                    if (girdi == i.ToString())
                    {
                        secim = i;
                        break;
                    }
                }

                if (secim == -1)
                {
                    RenkliYaz("Geçersiz seçim! " + min + " ile " + max + " arasında bir sayı gir.", ConsoleColor.Red);
                }
            } while (secim == -1);

            return secim;
        }

        // ================== KAPI ODALARI ==================
        // Zorluk seviyesi: 1. tur = 1, 2-3 = 2, 4-5 = 3, 6-7 = 4, 8-9 = 5
        static int Seviye()
        {
            return 1 + oyunTuru / 2;
        }

        static void KapiOdasi()
        {
            int[] kapilar = KapilariOlustur();
            bool[] isaretli = new bool[kapilar.Length]; // pusulanın işaretledikleri
            bool kapiAcildi = false;

            do
            {
                DurumEkrani();

                // Zehirliysen her hamlede can kaybedersin
                ZehirEtkisi();
                if (!oyunAktif)
                {
                    Bekle();
                    return;
                }

                if (oyunTuru == 1)
                {
                    Console.WriteLine("Duvarda bir yazı var: \"Her odada en az bir kapı seni yaşatır.\"");
                }
                Console.WriteLine();
                KapilariCiz(kapilar.Length);
                Console.WriteLine();

                for (int i = 0; i < kapilar.Length; i++)
                {
                    string satir = " [" + (i + 1) + "] Kapı: " + KapiAciklamasi(kapilar[i]);
                    if (isaretli[i])
                    {
                        satir = satir + "  [PUSULA: TEHLİKE!]";
                    }
                    if (isaretli[i]) RenkliYaz(satir, ConsoleColor.Red); else Console.WriteLine(satir);
                }
                Console.WriteLine(" " + Tekrarla("-", 40));
                Console.WriteLine(" [0] İksir iç (+40 can, zehri temizler)");
                Console.WriteLine(" [" + (kapilar.Length + 1) + "] Büyülü Pusulayı kullan (" + pusula + " adet)");

                int secim = SecimAl(0, kapilar.Length + 1);

                if (secim == 0)
                {
                    IksirIc();
                    Bekle();
                    continue; // kapı açmadan menüye geri dön
                }
                if (secim == kapilar.Length + 1)
                {
                    PusulaKullan(kapilar, isaretli);
                    Bekle();
                    continue;
                }

                // Gerilim anı: kapıyı açmadan önce bekle
                Console.WriteLine();
                RenkliYaz(secim + ". kapının kolunu tuttun... Kalbin hızla çarpıyor...", ConsoleColor.DarkYellow);
                Console.WriteLine("(Kapıyı açmak için Enter'a bas)");
                Console.ReadLine();

                KapiyiAc(kapilar[secim - 1]);
                kapiAcildi = true;
                Bekle();
            } while (!kapiAcildi);
        }

        // Her odada kapılar rastgele oluşur: 1 iyi kapı + geri kalanı tehlikeli kapılar
        static int[] KapilariOlustur()
        {
            int kapiSayisi;
            if (oyunTuru <= 3) kapiSayisi = 3;
            else if (oyunTuru <= 6) kapiSayisi = 4;
            else kapiSayisi = 5;

            int[] kapilar = new int[kapiSayisi];

            // İlk kapı her zaman iyi kapılardan biri
            kapilar[0] = iyiKapilar[zar.Next(0, iyiKapilar.Length)];

            // Diğer kapılar tehlikeli kapılardan, aynısı iki kez gelmesin
            for (int i = 1; i < kapiSayisi; i++)
            {
                int yeniKapi;
                bool ayniVar;
                do
                {
                    yeniKapi = tehlikeliKapilar[zar.Next(0, tehlikeliKapilar.Length)];
                    ayniVar = false;
                    for (int j = 0; j < i; j++)
                    {
                        if (kapilar[j] == yeniKapi) ayniVar = true;
                    }
                } while (ayniVar);
                kapilar[i] = yeniKapi;
            }

            KapilariKaristir(kapilar); // iyi kapı hep 1. sırada olmasın
            return kapilar;
        }

        // Kapıların yerini rastgele değiştirir
        static void KapilariKaristir(int[] kapilar)
        {
            for (int i = kapilar.Length - 1; i > 0; i--)
            {
                int j = zar.Next(0, i + 1); // Next üst sınırı dahil etmez, bu yüzden i + 1
                int gecici = kapilar[i];
                kapilar[i] = kapilar[j];
                kapilar[j] = gecici;
            }
        }

        static bool TehlikeliMi(int kapiTuru)
        {
            for (int i = 0; i < tehlikeliKapilar.Length; i++)
            {
                if (tehlikeliKapilar[i] == kapiTuru) return true;
            }
            return false;
        }

        // Pusula, henüz işaretlenmemiş tehlikeli kapılardan birini rastgele gösterir
        static void PusulaKullan(int[] kapilar, bool[] isaretli)
        {
            if (pusula <= 0)
            {
                Console.WriteLine("Hiç pusulan yok! Mağazadan alabilirsin.");
                return;
            }

            // Önce kaç aday kapı olduğunu say
            int adaySayisi = 0;
            for (int i = 0; i < kapilar.Length; i++)
            {
                if (TehlikeliMi(kapilar[i]) && !isaretli[i])
                {
                    adaySayisi++;
                }
            }

            if (adaySayisi == 0)
            {
                Console.WriteLine("Pusulanın iğnesi sakin. Gösterecek başka tehlike yok.");
                return;
            }

            // Adaylardan rastgele birini seç
            int hedef = zar.Next(0, adaySayisi);
            int sayac = 0;
            for (int i = 0; i < kapilar.Length; i++)
            {
                if (TehlikeliMi(kapilar[i]) && !isaretli[i])
                {
                    if (sayac == hedef)
                    {
                        isaretli[i] = true;
                        Console.WriteLine("Pusulanın iğnesi titredi ve " + (i + 1) + ". kapıyı gösterdi: TEHLİKE!");
                        break;
                    }
                    sayac++;
                }
            }
            pusula--;
        }

        // 1-3. turda kapılar açıkça yazılı, 5-6'da ipucu var, 8-9'da ipuçları birbirine benziyor
        static string KapiAciklamasi(int kapiTuru)
        {
            if (oyunTuru <= 3)
            {
                switch (kapiTuru)
                {
                    case KAPI_KORIDOR: return "Sessiz, boş bir koridor. Duvarlarda meşaleler yanıyor.";
                    case KAPI_AC_ASLAN: return "Ardında 2 aydır hiçbir şey yememiş bir aslan var.";
                    case KAPI_SORU: return "Üzerinde taştan bir yüz olan, soru soran kapı.";
                    case KAPI_HAZINE: return "Ardından altın parıltısı geliyor, bir hazine odası!";
                    case KAPI_CESME: return "Ardında şırıl şırıl akan bir şifa çeşmesi var.";
                    case KAPI_CANAVAR: return "Ardında homurdanan bir canavar var. Yenersen ödül alırsın.";
                    case KAPI_TUCCAR: return "Ardında fener tutan yaşlı bir tüccar bekliyor.";
                    case KAPI_LAV: return "Ardı kızgın lav dolu bir oda.";
                    case KAPI_BUZ: return "Ardı eksi 200 derecelik bir buz odası.";
                    case KAPI_CUKUR: return "Ardı dibi görünmeyen derin bir çukur.";
                    case KAPI_ASLAN: return "Ardında yeni yemek yemiş ama çok sinirli bir aslan var.";
                    case KAPI_ZEHIR: return "Ardı yeşil, zehirli gazla dolu bir oda.";
                    case KAPI_DIKEN: return "Duvarları dikenli, birbirine yaklaşan bir oda.";
                    case KAPI_SU: return "Tavanından su akan, yavaş yavaş dolan bir oda.";
                    case KAPI_MIMIK: return "Ortasında tek bir sandık var. Sandığın üstünde diş izleri var...";
                }
            }
            else if (oyunTuru <= 6)
            {
                switch (kapiTuru)
                {
                    case KAPI_KORIDOR: return "Ardı sessiz, aralıktan meşale ışığı sızıyor.";
                    case KAPI_AC_ASLAN: return "Önünde eski pati izleri var ama içeriden hiç ses gelmiyor.";
                    case KAPI_SORU: return "Taştan bir yüz var, gözleri seni takip ediyor.";
                    case KAPI_HAZINE: return "Anahtar deliğinden altın yığınları görünüyor.";
                    case KAPI_CESME: return "Ardından tatlı bir su sesi ve nane kokusu geliyor.";
                    case KAPI_CANAVAR: return "Ardından homurtu ve pençe sesleri geliyor.";
                    case KAPI_TUCCAR: return "Ardından birinin para saydığını ve ıslık çaldığını duyuyorsun.";
                    case KAPI_LAV: return "Altından turuncu bir ışık sızıyor, tokmak çok sıcak.";
                    case KAPI_BUZ: return "Kapı kolu kırağı tutmuş, nefesin buhara dönüyor.";
                    case KAPI_CUKUR: return "Arkasından uğultu ve çok uzaklardan taş düşme sesi geliyor.";
                    case KAPI_ASLAN: return "Arkasından tok bir kükreme ve kemik çıtırtısı geliyor.";
                    case KAPI_ZEHIR: return "Kenarlarından yeşilimsi bir duman sızıyor, burnun yanıyor.";
                    case KAPI_DIKEN: return "Arkasından taş sürtünmesi ve metal gıcırtısı geliyor.";
                    case KAPI_SU: return "Altından su sızıyor, ayakların ıslandı.";
                    case KAPI_MIMIK: return "Ardında tek başına bir sandık var... sanki nefes alıyor.";
                }
            }
            else
            {
                // Son bölge: ipuçları birbirine benziyor, dikkatli olmalısın
                switch (kapiTuru)
                {
                    case KAPI_KORIDOR: return "Kapı sıradan görünüyor.";
                    case KAPI_AC_ASLAN: return "Kapının önünde eski kemikler var, içerisi çok sessiz.";
                    case KAPI_SORU: return "Kapının üzerinde silik yazılar var.";
                    case KAPI_HAZINE: return "Kapının ardından şıngırtı geliyor.";
                    case KAPI_CESME: return "Kapı nemli, ardından hafif bir şırıltı geliyor.";
                    case KAPI_CANAVAR: return "Kapının ardında bir şey kıpırdıyor.";
                    case KAPI_TUCCAR: return "Kapının ardından mırıltılar geliyor.";
                    case KAPI_LAV: return "Kapı ılık, ardından hafif bir parıltı var.";
                    case KAPI_BUZ: return "Kapıdan serin bir hava geliyor, kenarları beyaz.";
                    case KAPI_CUKUR: return "Kapıdan hafif bir esinti geliyor.";
                    case KAPI_ASLAN: return "Kapının önünde kocaman pati izleri var.";
                    case KAPI_ZEHIR: return "Kapıdan tatlı, çiçeksi bir koku geliyor.";
                    case KAPI_DIKEN: return "Kapının ardından ritmik bir tıkırtı geliyor.";
                    case KAPI_SU: return "Kapının altı ıslak, damlama sesi geliyor.";
                    case KAPI_MIMIK: return "Kapının ardından para şıngırtısı geliyor.";
                }
            }
            return "Gizemli bir kapı.";
        }

        // Seçilen kapıyı açar ve arkasındaki olayı yaşatır
        static void KapiyiAc(int kapiTuru)
        {
            Console.WriteLine();
            int d20 = zar.Next(1, 21); // d20 zarı: 1-20 arası (21 dahil değil)
            int kazanc;

            switch (kapiTuru)
            {
                // ---------- İYİ KAPILAR ----------
                case KAPI_KORIDOR:
                    kazanc = 10 + zar.Next(0, 11);
                    altin += kazanc;
                    RenkliYaz("Sessiz bir koridordan güvenle geçtin. Yerde " + kazanc + " altın buldun.", ConsoleColor.Green);
                    break;

                case KAPI_AC_ASLAN:
                    kazanc = 30 + oyunTuru * 4;
                    altin += kazanc;
                    Console.WriteLine("Aslanın odasına girdin... Aslan yerde yatıyor.");
                    RenkliYaz("2 ay yemek yemeyen bir aslan çoktan açlıktan ölmüş! Akıllıca seçim.", ConsoleColor.Green);
                    RenkliYaz("Aslanın yanındaki eski çantada " + kazanc + " altın vardı!", ConsoleColor.Green);
                    break;

                case KAPI_SORU:
                    SoruSor();
                    break;

                case KAPI_HAZINE:
                    kazanc = 30 + oyunTuru * 6 + zar.Next(0, 16);
                    altin += kazanc;
                    RenkliYaz("Gerçek bir hazine odası! +" + kazanc + " altın!", ConsoleColor.Green);
                    if (d20 >= 12)
                    {
                        iksir++;
                        RenkliYaz("Sandığın dibinde bir iksir de vardı: +1 iksir", ConsoleColor.Green);
                    }
                    break;

                case KAPI_CESME:
                    can = maxCan;
                    mana = maxMana;
                    zehir = 0;
                    RenkliYaz("Şifa çeşmesinden içtin. Canın ve manan tamamen doldu!", ConsoleColor.Green);
                    break;

                case KAPI_CANAVAR:
                    // Erken turlarda zayıf, ileride güçlü canavarlar çıkar
                    int enFazla = 2 + oyunTuru / 4;
                    if (enFazla > canavarAdlari.Length) enFazla = canavarAdlari.Length;
                    int c = zar.Next(0, enFazla);
                    Savas(canavarAdlari[c], canavarCani[c], canavarAtk[c], canavarOdulu[c], 0, false);
                    break;

                case KAPI_TUCCAR:
                    RenkliYaz("Yaşlı tüccar fenerini kaldırdı: \"Hoş geldin yolcu!\"", ConsoleColor.Yellow);
                    Bekle();
                    Magaza();
                    break;

                // ---------- TEHLİKELİ KAPILAR ----------
                case KAPI_LAV:
                    RenkliYaz("Kapıyı açar açmaz kızgın lav üzerine aktı!", ConsoleColor.Red);
                    Olum("KÜL SONU", "Labirentten bir avuç kül olarak çıktın.");
                    break;

                case KAPI_CUKUR:
                    RenkliYaz("Adım attığın yerde zemin yok! Derin bir çukura düşüyorsun...", ConsoleColor.Red);
                    RenkliYaz("Tutunmak için zar atılıyor (15 ve üstü gerekli)... Zar: " + d20, ConsoleColor.Cyan);
                    if (d20 >= 15)
                    {
                        RenkliYaz("Son anda bir kayaya tutundun ve yukarı tırmandın!", ConsoleColor.Green);
                        HasarAl(15 + Seviye() * 5, "SONSUZ DÜŞÜŞ SONU", "Tırmanırken gücün tükendi ve karanlığa düştün.");
                    }
                    else
                    {
                        Olum("SONSUZ DÜŞÜŞ SONU", "Hâlâ düşüyorsun... Bu çukurun dibi yok.");
                    }
                    break;

                case KAPI_BUZ:
                    RenkliYaz("Eksi 200 derece! Vücudun anında donmaya başladı!", ConsoleColor.Red);
                    HasarAl(35 + Seviye() * 8, "BUZ HEYKELİ SONU", "Labirentin ortasında bir buz heykeline dönüştün.");
                    if (oyunAktif) RenkliYaz("Titreyerek odanın öbür ucundan çıktın.", ConsoleColor.Green);
                    break;

                case KAPI_ASLAN:
                    RenkliYaz("Bu aslan aç değil ama çok sinirli! Üzerine atladı!", ConsoleColor.Red);
                    RenkliYaz("Kaçmak için zar atılıyor (12 ve üstü)... Zar: " + d20, ConsoleColor.Cyan);
                    int aslanHasari = 25 + Seviye() * 8;
                    if (d20 >= 12)
                    {
                        RenkliYaz("Yana sıçradın, sadece pençe yedin.", ConsoleColor.Green);
                        aslanHasari = aslanHasari / 2;
                    }
                    HasarAl(aslanHasari, "ASLAN YEMİ SONU", "Aslan bugün çok güzel bir akşam yemeği yedi.");
                    break;

                case KAPI_ZEHIR:
                    RenkliYaz("Kapıyı açınca yeşil bir gaz yüzüne çarptı!", ConsoleColor.Red);
                    zehir = 3;
                    HasarAl(10 + Seviye() * 5, "ZEHİR SONU", "Zehirli gaz ciğerlerini yaktı.");
                    if (oyunAktif) RenkliYaz("ZEHİRLENDİN! 3 hamle boyunca can kaybedeceksin. İksir zehri temizler.", ConsoleColor.Magenta);
                    break;

                case KAPI_DIKEN:
                    RenkliYaz("İçeri girer girmez kapı kapandı ve dikenli duvarlar yaklaşmaya başladı!", ConsoleColor.Red);
                    RenkliYaz("Karşıdaki kapıyı kırmak için zar atılıyor (10 ve üstü)... Zar: " + d20, ConsoleColor.Cyan);
                    if (d20 >= 10)
                    {
                        RenkliYaz("Omzunla kapıyı kırdın ve son anda dışarı fırladın!", ConsoleColor.Green);
                        HasarAl(10 + Seviye() * 3, "DİKENLİ DUVAR SONU", "Dikenler seni yakaladı.");
                    }
                    else
                    {
                        RenkliYaz("Kapı kırılmadı! Dikenler bedenine battı!", ConsoleColor.Red);
                        HasarAl(30 + Seviye() * 8, "DİKENLİ DUVAR SONU", "Duvarlar kapandı ve seni ezdi.");
                    }
                    break;

                case KAPI_SU:
                    RenkliYaz("Kapı arkandan kilitlendi ve tavandan su boşalmaya başladı!", ConsoleColor.Red);
                    RenkliYaz("Su vanasını bulmak için zar atılıyor (11 ve üstü)... Zar: " + d20, ConsoleColor.Cyan);
                    if (d20 >= 11)
                    {
                        RenkliYaz("Vanayı buldun ve çevirdin! Su çekildi, kapı açıldı.", ConsoleColor.Green);
                    }
                    else
                    {
                        RenkliYaz("Vanayı bulamadın, su boğazına kadar yükseldi!", ConsoleColor.Red);
                        HasarAl(20 + Seviye() * 7, "BOĞULMA SONU", "Su tavana kadar yükseldi...");
                        if (oyunAktif && iksir > 0)
                        {
                            iksir--;
                            RenkliYaz("Çırpınırken çantandan bir iksir düştü ve kayboldu! (-1 iksir)", ConsoleColor.Red);
                        }
                    }
                    break;

                case KAPI_MIMIK:
                    RenkliYaz("Sandığa koştun... Sandığın DİŞLERİ var! Bu bir MİMİK!", ConsoleColor.Red);
                    int calinan = altin / 2;
                    altin -= calinan;
                    RenkliYaz("Mimik kesendeki altınların yarısını yuttu: -" + calinan + " altın", ConsoleColor.Red);
                    HasarAl(15 + Seviye() * 5, "AÇGÖZLÜLÜK SONU", "Mimik seni de yuttu. Açgözlülüğün sonu budur.");
                    break;
            }
        }

        static void SoruSor()
        {
            // 5. odaya kadar kolay sorular (0-2), sonra zor sorular (3-5)
            int soruNo;
            if (oyunTuru <= 5) soruNo = zar.Next(0, 3);
            else soruNo = zar.Next(3, 6);

            Console.WriteLine("Taştan yüz konuşmaya başladı:");
            Console.WriteLine("\"Geçmek istiyorsan bilmeceme cevap ver!\"");
            RenkliYaz("\n   " + sorular[soruNo], ConsoleColor.Cyan);
            RenkliYaz("   " + secenekler[soruNo], ConsoleColor.Cyan);

            int cevap = SecimAl(1, 3);

            if (cevap == dogruCevaplar[soruNo])
            {
                int odul = 25 + oyunTuru * 4;
                altin += odul;
                pusula++;
                RenkliYaz("\"Doğru... Akıllı birisin.\" Taştan yüz sana " + odul + " altın ve bir Büyülü Pusula verdi!", ConsoleColor.Green);
            }
            else
            {
                RenkliYaz("\"YANLIŞ!\" Kapıdan yıldırım çıktı ve seni çarptı!", ConsoleColor.Red);
                HasarAl(15 + Seviye() * 7, "BİLMECE KURBANI SONU", "Taştan yüz seni de taşa çevirdi.");
            }
        }

        // ================== HASAR, ZEHİR VE ÖLÜM ==================
        // Zorluk yüzdesini uygular, zırhı düşer, can biterse ölüm sonunu çağırır
        static void HasarAl(int miktar, string olumSonu, string olumMesaji)
        {
            int gercekHasar = miktar * hasarYuzdesi / 100 - zirh;
            if (gercekHasar < 1) gercekHasar = 1;

            can -= gercekHasar;
            RenkliYaz("-" + gercekHasar + " can!  (Kalan can: " + can + ")", ConsoleColor.Red);

            if (can <= 0)
            {
                can = 0;
                Olum(olumSonu, olumMesaji);
            }
        }

        static void ZehirEtkisi()
        {
            if (zehir > 0)
            {
                zehir--;
                RenkliYaz("Zehir damarlarında yanıyor...", ConsoleColor.Magenta);
                HasarAl(5, "ZEHİR SONU", "Zehir kalbine ulaştı.");
            }
        }

        static void Olum(string olumSonu, string olumMesaji)
        {
            RenkliYaz(olumMesaji, ConsoleColor.DarkRed);

            if (dirilisTilsimi > 0)
            {
                dirilisTilsimi--;
                can = maxCan / 2;
                zehir = 0;
                RenkliYaz("Boynundaki Diriliş Tılsımı parladı ve paramparça oldu! Hayata döndün.", ConsoleColor.Green);
                return;
            }

            // Oyunda 1 kez: ölümün eşiğinde son şans zarı
            if (!sonSansKullanildi)
            {
                sonSansKullanildi = true;
                RenkliYaz("\nGözlerin kararıyor... Aklına Prenses Ayla geliyor...", ConsoleColor.DarkYellow);
                RenkliYaz("SON ŞANS ZARI (12 ve üstü gerekli) - atmak için Enter'a bas", ConsoleColor.Yellow);
                Console.ReadLine();
                int d20 = zar.Next(1, 21);
                RenkliYaz("Zar: " + d20, ConsoleColor.Cyan);
                if (d20 >= 12)
                {
                    can = 20;
                    zehir = 0;
                    RenkliYaz("!!! AYAĞA KALKTIN !!! Ölümün eşiğinden döndün! (20 can)", ConsoleColor.Green);
                    return;
                }
                RenkliYaz("Gücün yetmedi...", ConsoleColor.Red);
            }

            sonAdi = olumSonu;
            oyunAktif = false;
        }

        static void IksirIc()
        {
            if (iksir <= 0)
            {
                RenkliYaz("Hiç iksirin kalmadı!", ConsoleColor.Red);
                return;
            }
            if (can == maxCan && zehir == 0)
            {
                Console.WriteLine("Canın zaten dolu, iksiri boşa harcamadın.");
                return;
            }

            iksir--;
            can += 40;
            if (can > maxCan) can = maxCan;
            zehir = 0;
            Console.WriteLine("İksiri içtin. Canın: " + can + " (zehir de temizlendi)");
        }

        // ================== SAVAŞ SİSTEMİ (Seçenek A) ==================
        // tip: 0 = normal canavar, 1 = Ork (öfkelenir), 2 = Ejderha (ateş püskürür), 3 = Kara Şövalye (2. aşama)
        static void Savas(string dusmanAdi, int dusmanCan, int dusmanAtk, int odul, int tip, bool bossMu)
        {
            int dusmanMaxCan = dusmanCan;

            DurumEkrani();
            if (bossMu)
            {
                mana = maxMana; // boss savaşına dolu mana ile başlanır
                Console.WriteLine();
                Baslik("!!!   B O S S   S A V A Ş I   !!!");
                BossResmi(tip);
                Console.WriteLine("Yolu kesen: " + dusmanAdi);
                if (tip == 1) Console.WriteLine("Ork sopasını yere vurdu. (İpucu: Canı yarıya inince öfkelenir.)");
                if (tip == 2) Console.WriteLine("Ejderha kanatlarını açtı. (İpucu: Her 3 turda ateş püskürür, nefes alırken SAVUN!)");
                if (tip == 3) Console.WriteLine("\"Prensese ulaşmak için önce beni geçmelisin!\" (İpucu: Canı yarıya inince kara büyüyle iyileşip ikinci aşamaya geçer.)");
            }
            else
            {
                Console.WriteLine("\nKapının ardından bir " + dusmanAdi + " fırladı!");
            }
            Console.WriteLine("Bu savaş en fazla " + MAX_TUR + " tur sürer!");
            Bekle();

            int tur = 0;
            bool dusmanSersem = false;
            bool ofkeli = false;      // ork ve şövalye canı yarıya inince öfkelenir
            bool atesNefesi = false;  // ejderha bu tur ateş püskürecek mi
            int kombo = 0;            // üst üste kaç kez vuruldu

            // KURAL 6: Fail-safe -> tur sayısı MAX_TUR'u geçemez
            while (tur < MAX_TUR)
            {
                tur++;
                bool savunuyor = false;

                // Her tur 5 mana dolar
                mana += 5;
                if (mana > maxMana) mana = maxMana;

                DurumEkrani();
                RenkliKutuSatiri("DÜŞMAN   : " + dusmanAdi, ConsoleColor.Red);
                RenkliKutuSatiri("CANI     : " + BarCiz(dusmanCan, dusmanMaxCan) + " " + dusmanCan + "/" + dusmanMaxCan, ConsoleColor.Red);
                RenkliKutuSatiri("Savaş Turu " + tur + "/" + MAX_TUR + "      Kombo: x" + kombo, ConsoleColor.Yellow);
                KutuCizgisi("=");

                // Düşmanın canı %20'nin altına inince sendeler
                bool dusmanZayif = dusmanCan <= dusmanMaxCan / 5;
                if (dusmanZayif)
                {
                    Console.WriteLine(dusmanAdi + " SENDELİYOR! Bitirici vuruş için kritik şansın arttı!");
                }
                if (MAX_TUR - tur < 5)
                {
                    Console.WriteLine("Acele et! Savaşın bitmesine " + (MAX_TUR - tur) + " tur kaldı!");
                }

                ZehirEtkisi();
                if (!oyunAktif)
                {
                    Bekle();
                    return;
                }

                // Ejderha her 3. turda ateş püskürür, oyuncu önceden uyarılır
                if (tip == 2 && tur % 3 == 0)
                {
                    atesNefesi = true;
                    RenkliYaz("\n!!! Ejderha derin bir nefes alıyor, ağzında alevler parlıyor !!!", ConsoleColor.Red);
                }

                Console.WriteLine();
                Console.WriteLine("   [1] Saldır        [2] Savun          [3] İksir iç");
                Console.WriteLine("   [4] " + yetenekAdi + " (" + YETENEK_MANA + " mana)     [5] Silah değiştir");

                int secim = SecimAl(1, 5);
                Console.WriteLine();

                switch (secim)
                {
                    case 1:
                        int vurus = OyuncuHasariHesapla(dusmanZayif);
                        if (vurus > 0)
                        {
                            // Kombo: üst üste her vuruşta +3 hasar (en fazla x5)
                            if (kombo < 5) kombo++;
                            if (kombo >= 2)
                            {
                                int bonus = (kombo - 1) * 3;
                                vurus += bonus;
                                Console.WriteLine("KOMBO x" + kombo + "! (+" + bonus + " hasar)");
                            }
                        }
                        else
                        {
                            kombo = 0; // ıskalayınca kombo bozulur
                        }
                        dusmanCan -= vurus;
                        if (vurus > 0) Console.WriteLine(dusmanAdi + " " + vurus + " hasar aldı.");

                        // Arbaletle vurunca sersemletme şansı
                        if (vurus > 0 && seciliSilah == 2 && zar.Next(1, 21) >= 15)
                        {
                            dusmanSersem = true;
                            Console.WriteLine("Ok dizine saplandı! " + dusmanAdi + " sersemledi!");
                        }
                        break;
                    case 2:
                        savunuyor = true;
                        kombo = 0; // saldırmayınca kombo bozulur
                        Console.WriteLine("Kalkanını kaldırdın...");
                        if (zar.Next(1, 21) >= 11)
                        {
                            dusmanSersem = true;
                            Console.WriteLine("Mükemmel savunma! Saldırıyı geri püskürttün, düşman sersemledi.");
                        }
                        break;
                    case 3:
                        kombo = 0;
                        IksirIc();
                        break;
                    case 4:
                        int yetenekHasari = OzelYetenek();
                        if (yetenekHasari > 0)
                        {
                            dusmanCan -= yetenekHasari;
                            Console.WriteLine(dusmanAdi + " " + yetenekHasari + " hasar aldı.");
                            // Okçunun ok yağmuru yüksek ihtimalle sersemletir
                            if (sinifNo == 2 && zar.Next(1, 21) >= 8)
                            {
                                dusmanSersem = true;
                                Console.WriteLine(dusmanAdi + " oklardan sersemledi!");
                            }
                        }
                        break;
                    case 5:
                        kombo = 0;
                        SilahDegistir();
                        break;
                }

                // Savaşın ortasında sürpriz olay (%10 ihtimal)
                if (zar.Next(1, 11) == 1)
                {
                    Console.WriteLine();
                    switch (zar.Next(1, 5))
                    {
                        case 1:
                            Console.WriteLine("SÜRPRİZ! Tavandan kayalar düştü! İkiniz de 8 hasar aldınız!");
                            dusmanCan -= 8;
                            HasarAl(8, "YENİLGİ SONU", "Kayaların altında kaldın.");
                            break;
                        case 2:
                            Console.WriteLine("SÜRPRİZ! " + dusmanAdi + " kan gölünde kaydı ve yere kapaklandı!");
                            dusmanSersem = true;
                            break;
                        case 3:
                            Console.WriteLine("SÜRPRİZ! Yerde yuvarlanan bir iksir şişesi yakaladın! +1 iksir");
                            iksir++;
                            break;
                        case 4:
                            Console.WriteLine("SÜRPRİZ! Duvardaki meşale devrildi, " + dusmanAdi + " alev aldı! 12 hasar!");
                            dusmanCan -= 12;
                            break;
                    }
                }

                // KURAL 5: Düşman öldüyse döngüyü hemen kır (break)
                if (dusmanCan <= 0)
                {
                    break;
                }
                if (!oyunAktif)
                {
                    Bekle();
                    return; // sürpriz kayalar oyuncuyu öldürdüyse
                }

                // Canı yarıya inince öfkelenme (sadece bir kere olur)
                if (!ofkeli && dusmanCan <= dusmanMaxCan / 2 && (tip == 1 || tip == 3))
                {
                    ofkeli = true;
                    if (tip == 1)
                    {
                        dusmanAtk += 5;
                        Console.WriteLine("Ork kükredi ve sopasını havaya kaldırdı! ÖFKELENDİ! (+5 saldırı)");
                    }
                    else
                    {
                        dusmanAtk += 7;
                        dusmanCan += 30;
                        Console.WriteLine("Kara Şövalye'nin miğferi düştü, gözleri kızıl yanıyor... İKİNCİ AŞAMA!");
                        Console.WriteLine("Kara büyüyle yaralarını kapattı! (+30 can, +7 saldırı)");
                    }
                }

                // KURAL 5: Düşman sersemlediyse onun turunu atla (continue)
                if (dusmanSersem)
                {
                    dusmanSersem = false;
                    atesNefesi = false; // sersemleyince nefesi de boşa gitti
                    Console.WriteLine(dusmanAdi + " bu tur saldıramadı.");
                    Bekle();
                    continue;
                }

                // Düşmanın saldırısı
                if (atesNefesi)
                {
                    atesNefesi = false;
                    if (savunuyor)
                    {
                        Console.WriteLine("Kalkanının arkasına saklandın, alevler etrafından geçti! Hasar yok.");
                    }
                    else
                    {
                        Console.WriteLine("Ejderha ATEŞ PÜSKÜRDÜ! Her yer alev!");
                        HasarAl(dusmanAtk * 2 + zar.Next(0, 6), "YENİLGİ SONU", "Ejderhanın alevleri seni yuttu.");
                    }
                }
                else
                {
                    int dusmanVurus = DusmanHasariHesapla(dusmanAtk);
                    if (savunuyor)
                    {
                        dusmanVurus = dusmanVurus / 2;
                        Console.WriteLine("Savunman hasarı yarıya indirdi.");
                    }

                    if (dusmanVurus == 0)
                    {
                        Console.WriteLine(dusmanAdi + " ıskaladı!");
                    }
                    else
                    {
                        Console.WriteLine(dusmanAdi + " sana saldırdı!");
                        HasarAl(dusmanVurus, "YENİLGİ SONU", dusmanAdi + " seni yendi. Labirent yeni bir ruh kazandı.");
                    }
                }

                if (!oyunAktif)
                {
                    Bekle();
                    return; // oyuncu öldü
                }
                Bekle();
            }

            // Döngü bitti: ya düşman öldü ya da tur sınırı doldu
            if (dusmanCan <= 0)
            {
                Console.WriteLine("\n" + dusmanAdi + " yere yığıldı! ZAFER!");
                altin += odul;
                if (bossMu) yenilenBoss++;
                else yenilenCanavar++;
                Console.WriteLine("Ödül: +" + odul + " altın!");
                Bekle();
            }
            else if (bossMu)
            {
                // Fail-safe: 20 tur geçti ve kimse ölmedi -> berabere, oyun biter
                Console.WriteLine("\n" + MAX_TUR + " tur geçti, ikiniz de yorgunluktan yere yığıldınız...");
                Console.WriteLine("Savaş berabere bitti. Labirentte sonsuza dek düello etmeye mahkumsunuz.");
                sonAdi = "SONSUZ DÜELLO SONU";
                oyunAktif = false;
                Bekle();
            }
            else
            {
                // Küçük canavarla berabere kalınca canavar kaçar, oyun devam eder
                Console.WriteLine("\n" + MAX_TUR + " tur geçti. " + dusmanAdi + " yoruldu ve karanlığa kaçtı. Ödül yok.");
                Bekle();
            }
        }

        // d20 zarı: 1 = ıska, 20 = kritik (kılıçlarda 18 ve üstü kritik)
        // Düşman sendeliyorsa kritik sınırı 4 düşer (bitirici vuruş)
        static int OyuncuHasariHesapla(bool dusmanZayif)
        {
            // Arbaletin oku yoksa sadece yumrukla vurulur
            if (seciliSilah == 2)
            {
                if (okSayisi <= 0)
                {
                    Console.WriteLine("Okun kalmadı! Yumrukla vurdun.");
                    return atk / 2;
                }
                okSayisi--;
            }

            int d20 = zar.Next(1, 21);
            Console.WriteLine("Saldırı zarı: " + d20);

            if (d20 == 1)
            {
                RenkliYaz("Iskaladın!", ConsoleColor.DarkGray);
                return 0;
            }

            int hasar = atk + silahHasari[seciliSilah] + zar.Next(0, 6);

            int kritikSiniri = 20;
            if (seciliSilah == 1 || seciliSilah == 3) kritikSiniri = 18; // kılıçlar daha keskin
            if (dusmanZayif) kritikSiniri -= 4;

            if (d20 >= kritikSiniri)
            {
                hasar = hasar * 2;
                if (dusmanZayif) Console.WriteLine("*** BİTİRİCİ VURUŞ! ***");
                else RenkliYaz("KRİTİK VURUŞ!", ConsoleColor.Yellow);
            }
            return hasar;
        }

        // Sınıfa göre özel yetenek. Düşmana verilen hasarı döndürür.
        static int OzelYetenek()
        {
            if (mana < YETENEK_MANA)
            {
                Console.WriteLine("Yeterli manan yok! (" + mana + "/" + YETENEK_MANA + ") Turun boşa gitti.");
                return 0;
            }
            mana -= YETENEK_MANA;

            int hasar = 0;
            switch (sinifNo)
            {
                case 1: // Savaşçı: asla ıskalamayan çift hasar
                    hasar = (atk + silahHasari[seciliSilah] + 5) * 2;
                    Console.WriteLine("KÜKREYEN DARBE! Tüm gücünle vurdun!");
                    break;
                case 2: // Okçu: ok harcamadan 3 ok atar
                    for (int i = 1; i <= 3; i++)
                    {
                        int okHasari = atk / 2 + zar.Next(4, 9);
                        hasar += okHasari;
                        Console.WriteLine(i + ". ok: " + okHasari + " hasar");
                    }
                    Console.WriteLine("OK YAĞMURU!");
                    break;
                case 3: // Gezgin: iyileşir, hasar vermez
                    can += 35;
                    if (can > maxCan) can = maxCan;
                    zehir = 0;
                    Console.WriteLine("ŞİFA IŞIĞI! Yaraların kapandı. Canın: " + can);
                    break;
            }
            return hasar;
        }

        // Boss'ların resmini satır satır çizer (her satır dizinin bir elemanı)
        static void BossResmi(int tip)
        {
            string[] resim;
            switch (tip)
            {
                case 1: // Ork
                    resim = new string[]
                    {
                        "        .-\"\"\"\"-.",
                        "       /  O  O  \\",
                        "      |    /\\    |",
                        "      |  /VVVV\\  |",
                        "       \\  \\__/  /",
                        "        '------'",
                        "    ==[ MAĞARA ORKU ]=="
                    };
                    break;
                case 2: // Ejderha
                    resim = new string[]
                    {
                        "     /\\                 /\\",
                        "    /  \\    _____     /  \\",
                        "   / /\\ \\  / O O \\   / /\\ \\",
                        "  /_/  \\_\\|  ___  |-/_/  \\_\\",
                        "           \\VVVVV/ ~~~ ~~~ >>>",
                        "            \\___/",
                        "    ==[ KIZIL EJDERHA ]=="
                    };
                    break;
                default: // Kara Şövalye
                    resim = new string[]
                    {
                        "          _____",
                        "         |[___]|",
                        "         | - - |      |",
                        "       __|_____|__    |",
                        "      |  |  X  |  |===+==",
                        "      |  |_____|  |   |",
                        "         |  |  |",
                        "        _|  |  |_",
                        "    ==[ KARA ŞÖVALYE ]=="
                    };
                    break;
            }

            for (int i = 0; i < resim.Length; i++)
            {
                RenkliYaz(resim[i], ConsoleColor.Red);
            }
        }

        static int DusmanHasariHesapla(int dusmanAtk)
        {
            int d20 = zar.Next(1, 21);
            if (d20 <= 2)
            {
                return 0; // ıska
            }

            int hasar = dusmanAtk + zar.Next(0, 6);
            if (d20 == 20)
            {
                hasar = hasar + hasar / 2;
                Console.WriteLine("Düşmandan güçlü bir darbe!");
            }
            return hasar;
        }

        static void SilahDegistir()
        {
            Console.WriteLine("Sahip olduğun silahlar:");
            for (int i = 0; i < silahAdlari.Length; i++)
            {
                if (silahVar[i])
                {
                    Console.WriteLine((i + 1) + ") " + silahAdlari[i] + " (+" + silahHasari[i] + ")");
                }
            }

            int secim;
            do
            {
                secim = SecimAl(1, silahAdlari.Length);
                if (!silahVar[secim - 1])
                {
                    Console.WriteLine("Bu silaha sahip değilsin!");
                }
            } while (!silahVar[secim - 1]);

            seciliSilah = secim - 1;
            Console.WriteLine(silahAdlari[seciliSilah] + " artık elinde.");
        }

        // ================== MAĞAZA ==================
        static void Magaza()
        {
            int secim;
            do
            {
                DurumEkrani();
                Console.WriteLine();
                Baslik("$$$  TÜCCARIN DÜKKANI  $$$");
                Console.WriteLine("Tüccar: \"Altının varsa malım var!\"   (Kesende " + altin + " altın var)\n");
                Console.WriteLine(" 1) Can İksiri (+40 can, zehri geçirir) 20 altın");
                Console.WriteLine(" 2) Ok Paketi (5 ok) .................. 15 altın");
                Console.WriteLine(" 3) " + silahAdlari[1] + " (+10 hasar) ......... " + silahFiyati[1] + " altın" + SahipMi(1));
                Console.WriteLine(" 4) " + silahAdlari[2] + " (+14, 5 ok hediye) ..... " + silahFiyati[2] + " altın" + SahipMi(2));
                Console.WriteLine(" 5) " + silahAdlari[3] + " (+20 hasar) ........ " + silahFiyati[3] + " altın" + SahipMi(3));
                Console.WriteLine(" 6) Deri Zırh (+3 zırh) ............... 45 altın");
                Console.WriteLine(" 7) Güç Antrenmanı (+3 saldırı) ....... 40 altın");
                Console.WriteLine(" 8) Kalp Taşı (+25 maks. can) ......... 50 altın");
                Console.WriteLine(" 9) Büyü Kitabı (+20 maks. mana) ...... 40 altın");
                Console.WriteLine("10) Diriliş Tılsımı (1 kez diriltir) .. 100 altın");
                Console.WriteLine("11) Büyülü Pusula (tehlike gösterir) .. 30 altın");
                Console.WriteLine("12) Silah değiştir");
                Console.WriteLine(" 0) Tüccardan ayrıl ve yola devam et");

                secim = SecimAl(0, 12);

                switch (secim)
                {
                    case 1:
                        if (OdemeYap(20)) { iksir++; RenkliYaz("+1 iksir.", ConsoleColor.Green); }
                        break;
                    case 2:
                        if (OdemeYap(15)) { okSayisi += 5; RenkliYaz("+5 ok.", ConsoleColor.Green); }
                        break;
                    case 3:
                    case 4:
                    case 5:
                        int silahNo = secim - 2; // 3 -> 1, 4 -> 2, 5 -> 3
                        if (silahVar[silahNo])
                        {
                            Console.WriteLine("Bu silah zaten sende var.");
                        }
                        else if (OdemeYap(silahFiyati[silahNo]))
                        {
                            silahVar[silahNo] = true;
                            seciliSilah = silahNo;
                            if (silahNo == 2) okSayisi += 5;
                            Console.WriteLine(silahAdlari[silahNo] + " satın alındı ve eline aldın!");
                        }
                        break;
                    case 6:
                        if (OdemeYap(45)) { zirh += 3; Console.WriteLine("Zırhın artık " + zirh); }
                        break;
                    case 7:
                        if (OdemeYap(40)) { atk += 3; Console.WriteLine("Saldırı gücün artık " + atk); }
                        break;
                    case 8:
                        if (OdemeYap(50))
                        {
                            maxCan += 25;
                            can += 25;
                            Console.WriteLine("Maksimum canın artık " + maxCan);
                        }
                        break;
                    case 9:
                        if (OdemeYap(40))
                        {
                            maxMana += 20;
                            mana = maxMana;
                            Console.WriteLine("Maksimum manan artık " + maxMana);
                        }
                        break;
                    case 10:
                        if (dirilisTilsimi >= 1)
                        {
                            Console.WriteLine("Aynı anda sadece 1 tılsım taşıyabilirsin.");
                        }
                        else if (OdemeYap(100))
                        {
                            dirilisTilsimi = 1;
                            Console.WriteLine("Diriliş Tılsımı boynunda parlıyor.");
                        }
                        break;
                    case 11:
                        if (OdemeYap(30)) { pusula++; RenkliYaz("+1 Büyülü Pusula.", ConsoleColor.Green); }
                        break;
                    case 12:
                        SilahDegistir();
                        break;
                }

                if (secim != 0) Bekle();
            } while (secim != 0);
        }

        static bool OdemeYap(int fiyat)
        {
            if (altin < fiyat)
            {
                RenkliYaz("Yeterli altının yok! (" + altin + "/" + fiyat + ")", ConsoleColor.Red);
                return false;
            }
            altin -= fiyat;
            return true;
        }

        static string SahipMi(int silahNo)
        {
            if (silahVar[silahNo]) return "  [SAHİPSİN]";
            return "";
        }

        // ================== FİNAL: KALE, PRENSES VE KRAL ==================
        static void KaleyeVaris()
        {
            YeniEkran();
            Baslik("K A R A   K A L E");
            Console.WriteLine();
            Console.WriteLine("Son kapıyı açtın ve labirentten dışarı, kayalık bir vadiye çıktın.");
            Console.WriteLine("Karşında simsiyah taşlardan yapılmış KARA KALE yükseliyor.");
            Console.WriteLine("En yüksek kulenin penceresinden bir mendil sallanıyor: Prenses Ayla!");
            Console.WriteLine();
            Console.WriteLine("Kale kapısının önünde kara zırhlı, dev bir şövalye duruyor.");
            Console.WriteLine("Elindeki kılıç ay ışığında parlıyor...");
            Console.WriteLine();
            Console.WriteLine("Kapının yanında son bir tüccar çadırı var. Savaştan önce hazırlan!");
            Bekle();
        }

        // Çöken kaleden 3 adımda kaçış. Her adımda 3 yoldan biri güvenli.
        static void KaleKacisi()
        {
            string[] yollar = { "Sol koridor", "Orta merdiven", "Sağdaki pencere" };

            for (int adim = 1; adim <= 3; adim++)
            {
                YeniEkran();
                Baslik("!!! KALE ÇÖKÜYOR !!!   Kaçış " + adim + "/3");
                Console.WriteLine("Tavan çatırdıyor, taşlar yağıyor! Prensesi elinden tuttun ve koşuyorsun!\n");
                Console.WriteLine("Can: " + BarCiz(can, maxCan) + " " + can + "/" + maxCan + "\n");

                int guvenliYol = zar.Next(1, 4); // 1, 2 veya 3

                // Prenses bazen doğru yolu fısıldar
                if (zar.Next(1, 3) == 1)
                {
                    Console.WriteLine("Prenses Ayla: \"Bu kaleyi biliyorum! " + yollar[guvenliYol - 1] + " güvenli!\"");
                }
                else
                {
                    Console.WriteLine("Prenses Ayla: \"Bilmiyorum! İçgüdülerine güven!\"");
                }

                for (int i = 0; i < yollar.Length; i++)
                {
                    Console.WriteLine((i + 1) + ") " + yollar[i]);
                }

                int secim = SecimAl(1, 3);
                if (secim == guvenliYol)
                {
                    Console.WriteLine("Doğru yol! Arkanızda her şey çöktü ama siz geçtiniz!");
                }
                else
                {
                    Console.WriteLine("Yol çöktü! Taşlar üstüne yağdı, prensesi korumak için siper oldun!");
                    HasarAl(8 + adim * 3, "ENKAZ SONU", "Kara Kale, seni de enkazının altına gömdü.");
                    if (!oyunAktif)
                    {
                        Bekle();
                        return;
                    }
                }
                Bekle();
            }

            YeniEkran();
            Console.WriteLine("Son anda kale kapısından dışarı fırladınız!");
            Console.WriteLine("Arkanızda Kara Kale büyük bir gürültüyle toz bulutunun içinde yok oldu!");
            Bekle();
        }

        static void PrensesVeKral()
        {
            YeniEkran();
            Baslik("<3  PRENSES KURTARILDI  <3");
            Console.WriteLine();
            Console.WriteLine("Kara Şövalye'nin zırhından altın bir anahtar düştü.");
            Console.WriteLine("Kulenin döner merdivenlerini koşarak çıktın ve kapıyı açtın.");
            Console.WriteLine();
            Console.WriteLine("Prenses Ayla: \"Seni bekliyordum, " + oyuncuAdi + "! Beni kurtardın!\"");
            Console.WriteLine();
            Console.WriteLine("Prenses sana sakladığı bir şifa iksirini içirdi. (+30 can)");
            can += 30;
            if (can > maxCan) can = maxCan;
            Console.WriteLine();
            Console.WriteLine("Tam o anda yer sarsılmaya başladı... Morvak'ın büyüsü bozuldu, KALE ÇÖKÜYOR!");
            Bekle();

            KaleKacisi();
            if (!oyunAktif) return; // enkazın altında kaldıysa

            YeniEkran();
            Console.WriteLine("Prensesle birlikte atlara bindiniz ve şafakla birlikte saraya ulaştınız.");
            Bekle();

            YeniEkran();
            Baslik("KRALIN SARAYI");
            Console.WriteLine();
            Console.WriteLine("Bütün halk sarayın önünde toplanmış, adını haykırıyor.");
            Console.WriteLine("Kral Doruk tahtından kalktı ve sana sarıldı.");
            Console.WriteLine();
            Console.WriteLine("Kral Doruk: \"Kızımı kurtardın. Söz verdiğim gibi istediğin ödülü seç!\"");
            Console.WriteLine();
            Console.WriteLine("1) Prenses Ayla ile evlenmek ve tahtın varisi olmak");
            Console.WriteLine("2) Kraliyet hazinesinden 1000 altın");
            Console.WriteLine("3) Kraliyet ordusunun komutanı olmak");

            int secim = SecimAl(1, 3);
            Console.WriteLine();
            switch (secim)
            {
                case 1:
                    sonAdi = "DÜĞÜN SONU";
                    Console.WriteLine("Kırk gün kırk gece düğün yapıldı. Sen artık krallığın veliahtısın!");
                    break;
                case 2:
                    sonAdi = "HAZİNE SONU";
                    altin += 1000;
                    Console.WriteLine("Kral sana 1000 altın verdi. Dünyanın en zengin maceracısı oldun!");
                    break;
                case 3:
                    sonAdi = "KOMUTAN SONU";
                    Console.WriteLine("Kraliyet ordusunun başına geçtin. Morvak'ın kalıntılarını avlamaya hazırsın!");
                    break;
            }
            Bekle();
        }

        // ================== OYUN SONU ==================
        static void OyunSonuEkrani()
        {
            YeniEkran();
            bool kazandi = sonAdi == "DÜĞÜN SONU" || sonAdi == "HAZİNE SONU" || sonAdi == "KOMUTAN SONU";

            if (kazandi)
            {
                Baslik("*** PRENSESİ KURTARDIN VE LABİRENTTEN KAÇTIN! ***");
                RenkliYaz(oyuncuAdi + " adı yüzyıllarca şarkılarda söylenecek.", ConsoleColor.Green);
            }
            else
            {
                Baslik("O Y U N   B İ T T İ");
                RenkliYaz(oyuncuAdi + ", " + oyunTuru + ". odada labirente yenik düştün.", ConsoleColor.Red);
                Console.WriteLine("Prenses Ayla hâlâ Kara Kale'de kurtarılmayı bekliyor...");
            }

            Console.WriteLine();
            KutuCizgisi("-");
            KutuSatiri("Ulaştığın son   : " + sonAdi);
            KutuSatiri("Ulaşılan oda    : " + oyunTuru + "/" + MAX_OYUN_TURU);
            KutuSatiri("Yenilen boss    : " + yenilenBoss + "     Yenilen canavar: " + yenilenCanavar);
            KutuSatiri("Altın           : " + altin);
            KutuCizgisi("-");

            SkorHesapla(kazandi);
            SonlariGoster();
        }

        static void SkorHesapla(bool kazandi)
        {
            int skor = altin + oyunTuru * 60 + yenilenBoss * 150 + yenilenCanavar * 40 + can + iksir * 10;
            if (kazandi) skor += 500;
            skor = skor * skorYuzdesi / 100; // zorluk çarpanı

            RenkliYaz("\nSKOR: " + skor + "  (" + zorlukAdi + ")", ConsoleColor.Yellow);
            if (skor > rekorSkor)
            {
                rekorSkor = skor;
                RenkliYaz("*** YENİ REKOR! ***", ConsoleColor.Green);
            }
            else
            {
                Console.WriteLine("Rekor: " + rekorSkor);
            }
        }

        // Bu sonu "bulundu" olarak işaretler ve tüm son koleksiyonunu listeler
        static void SonlariGoster()
        {
            int bulunanSayisi = 0;
            for (int i = 0; i < tumSonlar.Length; i++)
            {
                if (tumSonlar[i] == sonAdi)
                {
                    bulunanSonlar[i] = true;
                }
                if (bulunanSonlar[i])
                {
                    bulunanSayisi++;
                }
            }

            Console.WriteLine();
            Baslik("SON KOLEKSİYONU: " + bulunanSayisi + "/" + tumSonlar.Length);
            for (int i = 0; i < tumSonlar.Length; i++)
            {
                if (bulunanSonlar[i])
                    RenkliKutuSatiri("[X] " + tumSonlar[i], ConsoleColor.Green);
                else
                    RenkliKutuSatiri("[ ] ???", ConsoleColor.DarkGray);
            }
            KutuCizgisi("=");
        }
    }
}
