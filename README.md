# Labirentten Kaçış - Prensesin Kurtarılışı

**DOT 103 Programlamaya Giriş - Vize Projesi**
Hazırlayan: Emre İlhan

C# konsol oyunu. Seçenek B (Dallanan Zindan Odaları) temel alınmış, boss savaşları için Seçenek A'daki sıra tabanlı dövüş eklenmiştir.

## Oyunun Konusu
Kötü büyücü Morvak, Prenses Ayla'yı labirentin sonundaki Kara Kale'ye kapattı.
Oyuncu 10 odalık labirentte kapılar arasında seçim yaparak ilerler:
- Kapıların ardında hazine, şifa çeşmesi, tüccar, bilmece veya lav, buz, zehir, çukur, aslan gibi tuzaklar vardır.
- 4. odada **Mağara Orku**, 7. odada **Kızıl Ejderha**, 10. odada kaleyi koruyan **Kara Şövalye** ile savaşılır.
- Boss yenilince mağazadan silah (Kılıç, Arbalet, Ejder Kılıcı), iksir ve zırh alınabilir.
- Kara Şövalye yenilirse prenses kurtarılır ve kral oyuncuyu ödüllendirir. Oyunun 15 farklı sonu vardır.

## Nasıl Çalıştırılır
Visual Studio ile `ödev.slnx` dosyasını açıp **Ctrl+F5** ile çalıştırın.

## 8 Kuralın Koddaki Karşılığı
| Kural | Kodda |
|---|---|
| 1. Game Loop | `Main` içindeki `while (oyunAktif)` |
| 2. Durum değişkenleri | `can`, `atk`, `altin`, `iksir`, `mana`, `zirh` |
| 3. Random | `static Random zar = new Random();` döngü dışında bir kez, `zar.Next(1, 21)` ile d20 |
| 4. do while ve girdi güvenliği | `SecimAl`, `KapiOdasi`, `Magaza` |
| 5. break / continue | `Savas`: düşman ölünce `break`, sersemleyince `continue` |
| 6. Fail-Safe | `MAX_TUR = 20` savaş tur sınırı, `MAX_OYUN_TURU = 10` oda sınırı |
| 7. Dizi | `silahAdlari`, `silahHasari`, `canavarAdlari`, `tumSonlar` ... |
| 8. Metotlar | `DurumEkrani`, `HasarAl`, `Savas`, `Magaza`, `KapiyiAc` ... |
