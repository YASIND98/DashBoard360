using DashboardTsy.Application.AiInsight.Responses;

namespace DashboardTsy.Infrastructure.AiInsight;

public static class MockAiInsightData
{
    // Servisin döndürdüğü biçim: markdown + değer vurgusu için inline <span style="color:...">.
    // Ön yüz katlanır bölümleri başlık numarasından kurar ("2.1. Özet" -> "2.1"); başlığın "## ",
    // "### " ya da "**...**" yazılmış olması fark etmez, servis üçünü de kullanıyor.
    // OpenSections boş gönderiliyor; bu durumda ön yüz DEFAULT_OPEN_SECTIONS listesine düşüyor.
    private const string BranchSummary = """
        # YÖNETİCİ ÖZETİ — ATRIUM / Avrupa-2

        **Tarih:** 2026-08-16

        ---

        ## 1. Yönetici Özeti

        Rapor, 2026-08-16 tarihinde elde edilen veriler kullanılarak Tümü işkolu için hazırlanmıştır.
        Rapordaki karşılaştırmalar, şubenin banka geneli ortalamaya göre durumunu göstermektedir.

        - Müşteri ürünleri bazında şube, toplam müşteri sayısında <span style="color:red">**-14.496**</span> adet ve aktif müşteri sayısında <span style="color:red">**-4.953**</span> adet ile banka ortalamasının gerisinde kalmakta; maaş müşteri adedi <span style="color:red">**-838**</span> adet ve emekli müşteri adedi <span style="color:red">**-611**</span> adet farkla bölge ve banka ortalamalarına kıyasla geride seyretmektedir. Buna karşın dijital penetrasyon oranı <span style="color:green">**%83,76**</span> ile banka ortalaması olan <span style="color:green">**%82,15**</span>'in <span style="color:green">**%1,61**</span> üzerinde gerçekleşerek dijital dönüşümde güçlü bir konum sergilemektedir.
        - Kredi kartı portföyünde toplam kart adedi <span style="color:red">**-5.050**</span> adet ve aktif kart adedi <span style="color:red">**-2.050**</span> adet ile banka ortalamasının gerisinde olmakla birlikte, kart aktiflik oranı <span style="color:green">**%53**</span> ile banka ortalaması olan <span style="color:green">**%48**</span>'in <span style="color:green">**%5**</span> üzerinde kalması mevcut kartların verimli kullanıldığını göstermektedir. 3 aylık satış HG% verisi belirtilmedi.
        - POS ağında toplam POS adedi <span style="color:red">**-174**</span> adet ve aktif POS adedi <span style="color:red">**-129**</span> adet ile banka ortalamasının oldukça gerisinde bulunurken, çeyrek bazlı POS cirosu <span style="color:red">**-159.632.600 ₺**</span> farkla banka ortalamasının altında seyretmektedir.
        - Bilanço büyüklüğü <span style="color:green">**%89,90**</span> ve çalışma büyüklüğü <span style="color:green">**%90,81**</span> hedef gerçekleşme oranlarıyla hedefe yakından yaklaşmakta; bilanço büyüklüğü banka ortalamasına göre <span style="color:green">**+173.088 ₺**</span>, çalışma büyüklüğü ise <span style="color:green">**+1.585.831 ₺**</span> farkla bölge ve banka ortalamalarının üzerinde değerlendirilmektedir.
        - Şubenin güçlü yönleri arasında dijital penetrasyon oranının banka ortalamasının üzerinde olması, net komisyon gelirinde banka ortalamasına kıyasla <span style="color:green">**+2**</span> birimlik bir farkla öne çıkması ve bilanço ile çalışma büyüklüklerinin hedefe yakın seyrederken bölge/banka ortalamalarını aşması sayılabilir.
        - Geliştirilmesi gereken alanlar ise toplam ve aktif müşteri sayılarının banka ortalamasının gerisinde kalması, maaş ve emekli müşteri segmentlerinde payının düşük olması ile net faiz gelirinde banka ortalamasına kıyasla <span style="color:red">**-28**</span> birimlik bir farkla hedef ve ortalama seviyelerinin altında seyretmesidir.

        ---

        ## 2. Ürünler

        **2.1. Özet**
        - Banka ortalamasına göre en fazla öne çıkan ürün **Vadeli Mevduatlar** olup şube değeri banka ortalamasının üzerinde <span style="color:green">**%165,07**</span> farkla gerçekleşmiştir.
        - Banka ortalamasına göre en fazla gerileyen ürünler arasında **Gayri Nakdi Krediler** (<span style="color:red">**%99,29**</span> geride), **POS Ciro** (<span style="color:red">**%83,94**</span> geride) ve **Nakdi Krediler** (<span style="color:red">**%81,54**</span> geride) öne çıkmaktadır.
        - Mevduat, yatırım fonları ve bilanço/çalışma büyüklüğü metriklerinde şube değerleri bölge ve banka ortalamalarını belirgin şekilde aşarken; müşteri sayıları, aktiflik oranları ve kredi hacimleri genel olarak geride seyretmektedir.

        **2.2. ✅ Güçlü Yönler**
        - **Mevduatlar (Solo)**: bölge <span style="color:green">**1.452.620**</span> ve banka <span style="color:green">**1.772.380**</span> ortalamalarının üzerinde; şube her iki bazda da güçlü konumda.
        - **Vadeli Mevduatlar**: bölge <span style="color:green">**1.379.390**</span> ve banka <span style="color:green">**1.583.390**</span> ortalamalarının üzerinde; şube her iki bazda da güçlü konumda.
        - **Yatırım Fonları**: bölge <span style="color:green">**1.065.800**</span> ve banka <span style="color:green">**1.196.480**</span> ortalamalarının üzerinde; şube her iki bazda da güçlü konumda.
        - **Çalışma Büyüklüğü**: bölge <span style="color:green">**1.588.600**</span> ve banka <span style="color:green">**1.585.830**</span> ortalamalarının üzerinde; şube her iki bazda da güçlü konumda.
        - **Anında Şifre Alım Oranı %**: bölge <span style="color:green">**18**</span> ve banka <span style="color:green">**24**</span> ortalamalarının üzerinde; şube her iki bazda da güçlü konumda.

        **2.3. ⚠️ Zayıf Yönler**
        - **POS Ciro**: bölge <span style="color:red">**-171.001.000**</span> ve banka <span style="color:red">**-159.633.000**</span> ortalamalarının gerisinde; şube her iki bazda da zayıf konumda.
        - **Nakdi Krediler**: bölge <span style="color:red">**-624.226**</span> ve banka <span style="color:red">**-1.010.950**</span> ortalamalarının gerisinde; şube her iki bazda da zayıf konumda.
        - **Gayri Nakdi Krediler**: bölge <span style="color:red">**-338.008**</span> ve banka <span style="color:red">**-448.255**</span> ortalamalarının gerisinde; şube her iki bazda da zayıf konumda.
        - **Bireysel Müşteri Bireysel KK Sahip Adet**: bölge <span style="color:red">**-2.948**</span> ve banka <span style="color:red">**-2.011**</span> ortalamalarının gerisinde; şube her iki bazda da zayıf konumda.
        - **Aktif Müşteri**: bölge <span style="color:red">**-5.114**</span> ve banka <span style="color:red">**-4.953**</span> ortalamalarının gerisinde; şube her iki bazda da zayıf konumda.

        ---

        ## 3. Hacim

        **3.1. Özet**
        - Banka ortalamasına göre en güçlü performans gösteren ürün **Mevduatlar (Solo)** olup, <span style="color:green">**1.772.380**</span> bin TL’lik pozitif banka farkı ile segmentte lider konumdadır.
        - Banka ortalamasına göre en geride kalan ürün **Nakdi Krediler**dir ve <span style="color:red">**-1.010.950**</span> bin TL’lik negatif banka farkı ile hedeflenen büyüme hızından geride kalmaktadır.
        - Genel hedef gerçekleşme (HG) oranları ürün bazında farklılık göstermekte; **Vadesiz Mevduatlar** <span style="color:green">**%514,56**</span> ile hedefi ciddi şekilde aşarken, **Yatırım Fonları** <span style="color:red">**%76,13**</span> ile hedef gerisinde kalmaktadır.
        - **Bilanço Büyüklüğü** metrikleri <span style="color:red">**%89,90**</span> HG oranı ile hedefe yakın seyretmekte, bölge ve banka ortalamalarına göre sırasıyla <span style="color:green">**352.044**</span> ve <span style="color:green">**173.088**</span> bin TL’lik pozitif farklar ile her iki ortalamayı da aşmakta, net büyüme <span style="color:green">**618.816**</span> bin TL ile olumludur.
        - **Çalışma Büyüklüğü** metrikleri <span style="color:red">**%90,81**</span> HG oranı ile hedefe yakınsamakta, bölge ve banka ortalamalarına kıyasla <span style="color:green">**1.588.600**</span> ve <span style="color:green">**1.585.830**</span> bin TL’lik güçlü pozitif farklar göstermekte, <span style="color:green">**672.228**</span> bin TL’lik net büyüme ile bilanço büyüklüğüne paralel olumlu bir seyir izlemektedir.

        **3.2. ✅ Güçlü Yönler**
        - **Mevduatlar (Solo)**: Bölge ortalamasına <span style="color:green">**1.452.620**</span> bin TL, banka ortalamasına ise <span style="color:green">**1.772.380**</span> bin TL farkla her iki kıyaslamada da belirgin bir üstünlük sağlamaktadır.
        - **Vadeli Mevduatlar**: Bölge ortalamasına <span style="color:green">**1.379.390**</span> bin TL, banka ortalamasına <span style="color:green">**1.583.390**</span> bin TL farkla portföyün en istikrarlı ve güçlü büyüme kalemidir.
        - **Çalışma Büyüklüğü**: Hem bölge (<span style="color:green">**1.588.600**</span> bin TL) hem de banka (<span style="color:green">**1.585.830**</span> bin TL) ortalamalarına kıyasla yüksek pozitif farklarla şubenin toplam hacimdeki en güçlü bileşenidir.
        - **Yatırım Fonları**: Bölge ortalamasına <span style="color:green">**1.065.800**</span> bin TL, banka ortalamasına <span style="color:green">**1.196.480**</span> bin TL farkla alternatif yatırım ürünlerinde belirgin bir pazar payı yakalamıştır.
        - **Vadeli TL**: Bölge ortalamasına <span style="color:green">**825.882**</span> bin TL, banka ortalamasına <span style="color:green">**928.020**</span> bin TL farkla vadeli mevduat segmentinde şube performansını destekleyen temel üründür.

        **3.3. ⚠️ Zayıf Yönler**
        - **Nakdi Krediler**: Bölge ortalamasına <span style="color:red">**-624.226**</span> bin TL, banka ortalamasına <span style="color:red">**-1.010.950**</span> bin TL farkla kredi portföyündeki en belirgin geriliği temsil etmektedir.
        - **Gayri Nakdi Krediler**: Bölge ortalamasına <span style="color:red">**-338.008**</span> bin TL, banka ortalamasına <span style="color:red">**-448.255**</span> bin TL farkla gayri nakdi kredi hacminde belirgin bir gerileme yaşanmaktadır.
        - **Gayri Nakdi Krediler TL**: Bölge ortalamasına <span style="color:red">**-235.259**</span> bin TL, banka ortalamasına <span style="color:red">**-265.717**</span> bin TL farkla TL cinsi gayri nakdi kredilerde şube geride kalmaktadır.
        - **Kredi Kartı Alacakları**: Bölge ortalamasına <span style="color:red">**-138.345**</span> bin TL, banka ortalamasına <span style="color:red">**-140.081**</span> bin TL farkla kart alacak hacminde beklenen büyüme sağlanamamıştır.
        - **DBS, Faktoring ve Leasing Ürünleri**: Bu ürün gruplarında şube değerleri <span style="color:red">**0**</span> bin TL seviyesinde olup, bölge ve banka ortalamalarının üzerinde herhangi bir hacim bulunmamakta, dolayısıyla bu segmentlerde tamamen pasif konumdadır.

        ---

        ## 4. Karlılık

        **4.1. Özet**
        - Banka ortalamasına göre en fazla öne çıkan gelir metriği **Kredi Kartı** geliri olup, banka ortalamasının üzerinde (<span style="color:green">**%100**</span>) performans göstermektedir.
        - **Net Komisyon Geliri** de banka ortalamasının üzerinde (<span style="color:green">**%47,09**</span>) gerçekleşerek gelir yapısına katkı sağlamıştır.
        - Gider kalemlerinde **Operasyonel Giderler** (<span style="color:red">**-19.616**</span>), **Kredi Karşılıkları** (<span style="color:red">**-20.884**</span>), **Masraf Hissesi** (<span style="color:red">**-6.077**</span>) ve **Personel Gideri** (<span style="color:red">**-5.586**</span>) gibi ana gider grupları banka ortalamasının üzerinde (farkları negatif) seyrederek gider yönetimi açısından dezavantajlı durumdadır.
        - Gelir tarafındaki güçlü performans, gider yönetimi alanındaki gerilikler ile kısmen dengelenmiş olup, genel karlılık profili banka ortalamasının gerisinde (<span style="color:red">**Vergi Öncesi Kar farkı: -29.976**</span>) seyretmektedir.

        **4.2. ✅ Güçlü Yönler**
        - **Kredi Kartı**: bölge (<span style="color:green">**+605**</span>) ve banka (<span style="color:green">**+8.193**</span>) ortalamalarının üzerinde; her iki bazda da güçlü.
        - **Net Komisyon Geliri**: bölge (<span style="color:green">**+537**</span>) ve banka (<span style="color:green">**+9.263**</span>) ortalamalarının üzerinde; her iki bazda da güçlü.
        - **Döviz Al-Sat**: bölge (<span style="color:green">**+277**</span>) ve banka (<span style="color:green">**+3.593**</span>) ortalamalarının üzerinde; her iki bazda da güçlü.
        - **TOPLAM GELİR**: bölge (<span style="color:green">**+289**</span>) ve banka (<span style="color:green">**+10.524**</span>) ortalamalarının üzerinde; her iki bazda da güçlü.
        - **Net Faiz Geliri**: banka ortalamasının üzerinde (<span style="color:green">**+1.260**</span>) ancak bölge ortalamasının gerisinde (<span style="color:red">**-247**</span>).
        - **Mal. Kaynak & Get. Aktif**: banka ortalamasının üzerinde (<span style="color:green">**+3.516**</span>) ancak bölge ortalamasının gerisinde (<span style="color:red">**-61**</span>).

        **4.3. ⚠️ Zayıf Yönler**
        - **VERGİ ÖNCESİ KAR**: bölge (<span style="color:red">**-2.358**</span>) ve banka (<span style="color:red">**-29.976**</span>) ortalamalarının gerisinde; her iki bazda da zayıf.
        - **Kredi Karşılıkları**: bölge (<span style="color:red">**-1.203**</span>) ve banka (<span style="color:red">**-20.884**</span>) ortalamalarının gerisinde; her iki bazda da zayıf.
        - **Operasyonel Giderler**: bölge (<span style="color:red">**-1.445**</span>) ve banka (<span style="color:red">**-19.616**</span>) ortalamalarının gerisinde; her iki bazda da zayıf.
        - **Net İşletme Geliri**: bölge (<span style="color:red">**-1.155**</span>) ve banka (<span style="color:red">**-9.092**</span>) ortalamalarının gerisinde; her iki bazda da zayıf.
        - **Masraf Hissesi**: bölge (<span style="color:red">**-460**</span>) ve banka (<span style="color:red">**-6.077**</span>) ortalamalarının gerisinde; her iki bazda da zayıf.
        - **Personel Gideri**: bölge (<span style="color:red">**-425**</span>) ve banka (<span style="color:red">**-5.586**</span>) ortalamalarının gerisinde; her iki bazda da zayıf.

        ---

        ## 5. İş Kolları Özeti

        ### Kurumsal
        - Bu iş kolunda şube faaliyeti bulunmamaktadır.

        ### Ticari
        - Bu iş kolunda şube faaliyeti bulunmamaktadır.

        ### İşletme
        - Müşteri tabanı: Toplam müşteri sayısı bölge ortalamasının **<span style="color:red">-1.973</span>** ⚠️, banka ortalamasının **<span style="color:red">-1.591</span>** ⚠️ altında seyretmektedir. Aktif müşteri sayısı şubede **0** olarak gerçekleşmiştir.
        - Hacim durumu: Nakdi kredi ve mevduat hacimleri bölge ve banka ortalamalarının altında kalmıştır. Kredi kartı alacakları hacmi bölge ortalamasının **<span style="color:red">-49.992</span>** ⚠️ altında gerçekleşmiştir.
        - Öne çıkan durum: Mevcut düşük hacimli portföy yapısı, iş kolunun büyüme potansiyelini sınırlandırmaktadır.

        ### Tarım
        - Müşteri tabanı: Toplam müşteri sayısı bölge ortalamasının **<span style="color:red">-5</span>** ⚠️, banka ortalamasının **<span style="color:red">-4.074</span>** ⚠️ altında seyretmektedir. Aktif müşteri sayısı **5** olarak gerçekleşmiştir.
        - Hacim durumu: Nakdi kredi ve mevduat hacimleri bölge ve banka ortalamalarının altında kalmıştır. Kredi kartı alacakları hacmi bölge ortalamasının **<span style="color:red">-157</span>** ⚠️ altında gerçekleşmiştir.
        - Öne çıkan durum: Dijital penetrasyon oranı banka ortalamasının **<span style="color:green">1.29</span>** ✅ üstünde seyretmekle birlikte, bölge ortalamasının **<span style="color:red">-16.67</span>** ⚠️ altında kalmıştır.

        ### Bireysel
        - Müşteri tabanı: Toplam müşteri sayısı bölge ortalamasının **<span style="color:red">-14.701</span>** ⚠️, banka ortalamasının **<span style="color:red">-9.407</span>** ⚠️ altında seyretmektedir. Aktif müşteri sayısı banka ortalamasının **<span style="color:red">-2.889</span>** ⚠️ altında gerçekleşmiştir.
        - Hacim durumu: Mevduat hacmi bölge ortalamasının **<span style="color:green">1.819.752</span>** ✅ üstünde, banka ortalamasının **<span style="color:green">2.177.565</span>** ✅ üstünde seyretmektedir. Çalışma büyüklüğü bölge ortalamasının **<span style="color:green">3.136.622</span>** ✅ üstünde, banka ortalamasının **<span style="color:green">3.748.304</span>** ✅ üstünde gerçekleşmiştir.
        - Öne çıkan durum: Nakdi kredi ve kredi kartı alacakları hacimleri bölge ve banka ortalamalarının altında kalmıştır. Dijital penetrasyon oranı bölge ortalamasının **<span style="color:red">-1.63</span>** ⚠️, banka ortalamasının **<span style="color:red">-1.26</span>** ⚠️ altında seyretmektedir.

        ---

        ## 6. Trend Analizi

        **6.1. Özet**
        - Bireysel iş kolunda Gayri Nakdi Krediler TL hacminde <span style="color:green">**%+385.0**</span> (sene başından) ve İşletme iş kolunda Vadeli TL hacminde <span style="color:green">**%+373.7**</span> (sene başından) ile en güçlü büyüme kaydedilmiştir.
        - Bireysel iş kolunda Vergi Öncesi Karında <span style="color:red">**%-2.4241**</span> (sene başından), Net İşletme Gelirlerinde <span style="color:red">**%-1.9286**</span> (sene başından) ve Spread verilerinde Vadeli YP'de <span style="color:red">**%-9.4580 pp**</span> (sene başından) ile en belirgin gerilemeler öne çıkmaktadır.
        - Genel trend; hacim tarafında Bireysel ve İşletme iş kollarında pozitif yönlü hareketlenirken, karlılık ve spread verilerinde maliyet baskısı ve YP vadeli ürünlerdeki getiri sıkışması dikkati çekmektedir.
        - İş kolları arasında belirgin fark olarak; Bireysel ve İşletme'de kredi ve mevduat hacimlerinde ivmelenme görülürken, Tarım ve Ticari'de hacim değişimleri sınırlı ve ağırlıklı olarak yatay seyretmektedir.

        **6.2. İş Kolu Bazlı Hacim Trendleri**
        #### Bireysel
        - Gayri Nakdi Krediler TL: <span style="color:green">**%+385.0**</span> (sene başından) ve <span style="color:green">**%+385.0**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vadesiz YP: <span style="color:green">**%+66.6**</span> (sene başından) ve <span style="color:green">**%+66.6**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Kredi Kartı Alacakları: <span style="color:green">**%+12.9**</span> (sene başından) ve <span style="color:green">**%+12.9**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vadeli TL: <span style="color:green">**%+11.3**</span> (sene başından) ve <span style="color:green">**%+11.3**</span> (aylık ort.) ile artış trendi devam etmektedir.

        #### İşletme
        - Vadeli TL: <span style="color:green">**%+373.7**</span> (sene başından) ve <span style="color:green">**%+373.7**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vadeli KM: <span style="color:green">**%+373.7**</span> (sene başından) ve <span style="color:green">**%+373.7**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Nakdi Krediler TL: <span style="color:green">**%+117.7**</span> (sene başından) ve <span style="color:green">**%+117.7**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Kredi Kartı Alacakları: <span style="color:green">**%+87.0**</span> (sene başından) ve <span style="color:green">**%+87.0**</span> (aylık ort.) ile artış trendi devam etmektedir.

        #### Tarım
        - Vadesiz TL: <span style="color:red">**%-42.8**</span> (sene başından) ve <span style="color:red">**%-42.8**</span> (aylık ort.) ile düşüş trendi devam etmektedir.
        - Vadesiz KM: <span style="color:red">**%-41.8**</span> (sene başından) ve <span style="color:red">**%-41.8**</span> (aylık ort.) ile düşüş trendi devam etmektedir.
        - Nakdi Krediler TL: <span style="color:red">**%-23.4**</span> (sene başından) ve <span style="color:red">**%-23.4**</span> (aylık ort.) ile düşüş trendi devam etmektedir.
        - Kredi Kartı Alacakları: <span style="color:green">**%+9.1**</span> (sene başından) ve <span style="color:green">**%+9.1**</span> (aylık ort.) ile artış trendi devam etmektedir.

        #### Ticari
        - Bilanço Büyüklüğü: **-** ve **-** ile düşüş trendi devam etmektedir.
        - Çalışma Büyüklüğü: **-** ve **-** ile düşüş trendi devam etmektedir.
        - Mevduatlar: **-** ve **-** ile yatay trendi devam etmektedir.
        - Nakdi Krediler: **-** ve **-** ile yatay trendi devam etmektedir.

        #### Kamu Finansmanı
        - Bilanço Büyüklüğü: **-** ve **-** ile yatay trendi devam etmektedir.
        - Çalışma Büyüklüğü: **-** ve **-** ile yatay trendi devam etmektedir.
        - Mevduatlar: **-** ve **-** ile yatay trendi devam etmektedir.
        - Nakdi Krediler: **-** ve **-** ile yatay trendi devam etmektedir.

        #### Özel Bankacılık
        - Bilanço Büyüklüğü: **-** ve **-** ile veri_yetersiz trendi devam etmektedir.
        - Çalışma Büyüklüğü: **-** ve **-** ile veri_yetersiz trendi devam etmektedir.
        - Mevduatlar: **-** ve **-** ile veri_yetersiz trendi devam etmektedir.
        - Nakdi Krediler: **-** ve **-** ile veri_yetersiz trendi devam etmektedir.

        **6.3. İş Kolu Bazlı Karlılık Trendleri**
        #### Bireysel
        - Net İşletme Gelirleri: <span style="color:green">**%+0.6986**</span> (sene başından) ve <span style="color:green">**%+0.1397**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Net Faiz Geliri: <span style="color:green">**%+0.6150**</span> (sene başından) ve <span style="color:green">**%+0.1230**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vadeli Mevduatlar: <span style="color:green">**%+0.1127**</span> (sene başından) ve <span style="color:green">**%+0.0225**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vergi Öncesi Kar: <span style="color:red">**%-2.4241**</span> (sene başından) ve <span style="color:red">**%-0.4848**</span> (aylık ort.) ile düşüş trendi devam etmektedir.

        #### İşletme
        - Net İşletme Gelirleri: <span style="color:green">**%+0.0051**</span> (sene başından) ve <span style="color:green">**%+0.0010**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vergi Öncesi Kar: <span style="color:green">**%+0.0007**</span> (sene başından) ve <span style="color:green">**%+0.0001**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Toplam Gelir: <span style="color:green">**%+0.0035**</span> (sene başından) ve <span style="color:green">**%+0.0007**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Diğer: <span style="color:red">**%-0.0009**</span> (sene başından) ve <span style="color:red">**%-0.0002**</span> (aylık ort.) ile düşüş trendi devam etmektedir.

        #### Kamu Finansmanı
        - Tüm metrikler **%0.0000** seviyesinde seyretmekte olup yatay trend izlenmektedir.
        - Ortalama aylık değişimler sıfır yakın seyretmekte, belirgin yön değişikliği gözlenmemektedir.
        - Veri seti genelinde istikrarlı bir seyir hakimdir.

        #### Kurumsal
        - Operasyonel Giderler: <span style="color:green">**%+0.0002**</span> (sene başından) ve <span style="color:green">**%+0.0000**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vergi Öncesi Kar: <span style="color:green">**%+0.0001**</span> (sene başından) ve <span style="color:green">**%+0.0000**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Net İşletme Gelirleri: <span style="color:green">**%+0.0001**</span> (sene başından) ve <span style="color:green">**%+0.0000**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Diğer: <span style="color:red">**%-0.0001**</span> (sene başından) ve <span style="color:red">**%-0.0000**</span> (aylık ort.) ile düşüş trendi devam etmektedir.

        #### Ortak
        - Mal. Kaynak & Get. Aktif: <span style="color:green">**%+0.0223**</span> (sene başından) ve <span style="color:green">**%+0.0045**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Net Faiz Geliri: <span style="color:green">**%+0.0223**</span> (sene başından) ve <span style="color:green">**%+0.0045**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Kredi Kartı: <span style="color:green">**%+0.0151**</span> (sene başından) ve <span style="color:green">**%+0.0030**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vergi Öncesi Kar: <span style="color:red">**%-0.0700**</span> (sene başından) ve <span style="color:red">**%-0.0140**</span> (aylık ort.) ile düşüş trendi devam etmektedir.

        #### Tarım
        - Vadesiz Mevduatlar: <span style="color:green">**%+0.0001**</span> (sene başından) ve <span style="color:green">**%+0.0000**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vergi Öncesi Kar: <span style="color:red">**%-0.0027**</span> (sene başından) ve <span style="color:red">**%-0.0005**</span> (aylık ort.) ile düşüş trendi devam etmektedir.
        - Net İşletme Gelirleri: <span style="color:red">**%-0.0020**</span> (sene başından) ve <span style="color:red">**%-0.0004**</span> (aylık ort.) ile düşüş trendi devam etmektedir.
        - Toplam Gelir: <span style="color:red">**%-0.0019**</span> (sene başından) ve <span style="color:red">**%-0.0004**</span> (aylık ort.) ile düşüş trendi devam etmektedir.

        #### Ticari
        - Operasyonel Giderler: <span style="color:green">**%+0.0006**</span> (sene başından) ve <span style="color:green">**%+0.0001**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Muhtelif Giderler: <span style="color:green">**%+0.0006**</span> (sene başından) ve <span style="color:green">**%+0.0001**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vergi Öncesi Kar: <span style="color:green">**%+0.0003**</span> (sene başından) ve <span style="color:green">**%+0.0001**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Toplam Gelir: <span style="color:red">**%-0.0003**</span> (sene başından) ve <span style="color:red">**%-0.0001**</span> (aylık ort.) ile düşüş trendi devam etmektedir.

        #### Özel Bankacılık
        - Tüm metrikler **%0.0000** seviyesinde seyretmekte olup veri_yetersiz trendi izlenmektedir.
        - Ortalama aylık değişimler sıfır yakın seyretmekte, belirgin yön değişikliği gözlenmemektedir.
        - Veri seti genelinde istikrarlı bir seyir hakimdir.

        **6.4. Spread Trendleri**
        - Gayri Nakdi Krediler: <span style="color:green">**%+1.2556 pp**</span> (sene başından) ve <span style="color:green">**%+0.2511 pp**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Vadeli TL: <span style="color:green">**%+0.1297 pp**</span> (sene başından) ve <span style="color:green">**%+0.0259 pp**</span> (aylık ort.) ile artış trendi devam etmektedir.
        - Nakdi Krediler: <span style="color:red">**%-0.8467 pp**</span> (sene başından) ve <span style="color:red">**%-0.1693 pp**</span> (aylık ort.) ile düşüş trendi devam etmektedir.
        - Vadeli YP: <span style="color:red">**%-9.4580 pp**</span> (sene başından) ve <span style="color:red">**%-1.8916 pp**</span> (aylık ort.) ile düşüş trendi devam etmektedir.

        ---

        ## 7. Kritik Metrikler Takibi

        **7.1. Maaş Müşterisi**
        - **Maaş Müşteri Adedi**: Şube değeri **544** | Banka farkı <span style="color:red">**-838**</span> | Trend: -43% | *Şube, banka ortalamasının altında kalmakta ve yılbaşından itibaren negatif seyir izlemektedir.*
        - **Maaş Firma Adedi**: Şube değeri **8** | Banka farkı <span style="color:red">**-27**</span> | Trend: 14% | *Sınırlı bir büyüme söz konusu olmakla birlikte hacim hedeflerine ulaşmada yetersiz kalmaktadır.*

        **7.2. Emekli Müşterisi**
        - **Emekli Müşteri Adedi**: Şube değeri **1286** | Banka farkı <span style="color:red">**-611**</span> | Trend: 48% | *Yılbaşından itibaren pozitif bir büyüme yakalanmış olsa da mevcut taban banka ortalamasının gerisindedir.*
        - **Tarım Emekli**: Şube değeri **[VERİ YOK]** | Banka farkı <span style="color:red">**-236**</span> | Trend: [VERİ YOK] | *Tarım segmentinde emekli profili bulunmamakta olup bu kitleye yönelik ürünleşme fırsatı değerlendirilebilir.*

        **7.3. Vadesiz Mevduat**
        - **Vadesiz Mevduatlar TL hacmi**: Şube değeri **94.714 bin TL** | Banka farkı <span style="color:red">**-38.064 bin TL**</span> | Trend: -11,46% | *TL cinsi vadesiz mevduatta yılsonu hedefi doğrultusunda negatif bir trend izlenmektedir.*
        - **Vadesiz TL**: Şube değeri **94.714 bin TL** | Banka farkı <span style="color:red">**-38.064 bin TL**</span> | Trend: -11,46% | *Müşteri bazında vadesiz dengenin korunması için aktif takip gerekmektedir.*

        **7.4. POS Metrikleri**
        - **Aktif POS Adedi**: Şube değeri **21** | Banka farkı <span style="color:red">**-129**</span> | Trend: yatay | *Aktif cihaz adedi çeyrek içinde sabit kalmakla birlikte bölge ve banka ortalamalarının önemli ölçüde gerisindedir.*
        - **Çeyreklik POS Ciro**: Şube değeri **30.533.400 TL** | Banka farkı <span style="color:red">**-159.633.000 TL**</span> | Trend: 9,25M TL artış | *Ciro hacminde çeyrek içi pozitif bir ivme gözlenmekle birlikte genel pazar payı hedefi karşılanmamıştır.*
        - **Genel POS aktiflik trendi**: Şube değeri **N/A** | Banka farkı <span style="color:red">**N/A**</span> | Trend: yatay | *Aktif POS sayısında durağan bir seyir izlenirken, çeyreklik ciro performansındaki toparlanma ticari ilişkilerin aktif tutulduğunu göstermektedir.*

        **7.5. DTH**
        - **DTH (Döviz Tevdiat Hesabı)**: Şube değeri **[VERİ YOK]** | Banka farkı <span style="color:red">**[VERİ YOK]**</span> | Trend: [VERİ YOK] | *DTH ile ilgili metrik sistematik veriler arasında yer almadığından detaylı değerlendirme yapılamamaktadır.*

        ---

        ## 8. Genel Değerlendirme

        **8.1. ✅ Öne Çıkan Güçlü Yönler**
        - Vadesiz mevduat hedef gerçekleşme oranı, banka ortalamasının <span style="color:green">**+493,35 pp**</span> üzerinde <span style="color:green">**%514,56**</span> seviyesinde gerçekleşerek likidite odaklı büyümede belirgin üstünlük sağladı.
        - Net komisyon geliri hedef gerçekleşme oranı, banka ortalamasının <span style="color:green">**+42,62 pp**</span> üzerinde <span style="color:green">**%231,62**</span> seviyesinde tamamlanarak gelir çeşitlendirmesindeki performansı teyit etti.
        - Kredi kartı ve POS aktiflik metrikleri, anında şifre alım oranında banka ortalamasının <span style="color:green">**+24,00 pp**</span> üstünde <span style="color:green">**%94,00**</span> ve bonus kart aktiflik oranında <span style="color:green">**+5,00 pp**</span> farkla <span style="color:green">**%53,00**</span> seviyesinde gerçekleşerek kart yönetiminde güçlü duruş sergiledi.
        - Vergi öncesi kar hedef gerçekleşme oranı, banka ortalamasının <span style="color:green">**+25,16 pp**</span> üstünde <span style="color:green">**%66,44**</span> seviyesinde gerçekleşerek karlılık yönetimindeki disiplinli yaklaşımı yansıttı.

        **8.2. ⚠️ Kritik Risk Alanları**
        - Müşteri tabanı ve aktiflik metrikleri, toplam müşteri sayısında banka ortalamasının <span style="color:red">**-14.496 adet**</span> ve aktif müşteri sayısında <span style="color:red">**-4.953 adet**</span> geride kalarak taban genişletme ve müşteri sadakatinde yapısal zafiyet gösteriyor.
        - Nakdi kredi büyüme hedefi, banka ortalamasının <span style="color:red">**-9,77 pp**</span> gerisinde <span style="color:red">**%15,21**</span> net büyüme ile gerçekleşti; kredi portföyü dinamiklerinde pazar payı koruma riski mevcut.
        - Net faiz geliri hedef gerçekleşme oranı, banka ortalamasının <span style="color:red">**-17,73 pp**</span> gerisinde <span style="color:red">**%54,27**</span> seviyesinde tamamlandı; faiz marjı yönetimi ve aktif pasif uyumunda iyileştirme ihtiyacı öne çıkıyor.
        - POS iş kolu hacmi ve müşteri kazanımı, ciro tutarında banka ortalamasının <span style="color:red">**-159.633.000.000 TL**</span> ve sahip POS müşteri adedi <span style="color:red">**-174 adet**</span> geride kalarak kurumsal ve ticari segmentlerdeki ticari ayağın yetersiz büyüdüğünü ortaya koyuyor.

        ---
        """;

    public static GetBranchAiInsightResponse GetBranchAiInsights(string regionCode, string branchCode)
        => new()
        {
            Items = new List<GetBranchAiInsightItem>
            {
                new()
                {
                    Id = 1,
                    Region = "Avrupa-2",
                    BranchName = "ATRIUM",
                    BranchCode = branchCode,
                    SummaryDate = new DateTime(2026, 8, 17),
                    Summary = BranchSummary,
                    OpenSections = new List<string>(),
                    ModelName = "qwen3-6",
                    CreatedAt = new DateTime(2026, 8, 18, 9, 10, 24, 693)
                }
            }
        };
}
