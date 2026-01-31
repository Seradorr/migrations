# Migrations Projesi Mimari Dokümantasyonu

## @brief
Xilinx Vivado projelerini versiyon kontrolüne uygun yapıya dönüştüren C# kütüphanesinin sistem mimarisi ve veri akışı.

## @details
Bu doküman, Migrations kütüphanesinin temel bileşenlerini, veri yapılarını ve Vivado .xpr proje dosyalarını nasıl işlediğini açıklamaktadır. Kütüphane, monolitik Vivado projesini kaynak kodlarına, IP'lere, constraint'lere ve diğer dosyalara ayırarak standart bir dizin yapısında organize eder.

## Sistem Bileşenleri

### 1. MigrationCommon (Soyut Temel Sınıf)
- **Görev**: Tüm migration işlemleri için ortak altyapıyı sağlar
- **Sorumluluklar**:
  - Giriş parametrelerinin doğrulanması
  - Hedef dizin yapısının oluşturulması
  - Loglama ve uyarı yönetimi
  - Dosya ve dizin kopyalama yardımcı fonksiyonları
  - Yol dönüştürme (relative/absolute) işlemleri

### 2. MigrationVivado (Vivado Özel Sınıf)
- **Görev**: Vivado .xpr proje dosyalarını işler
- **Sorumluluklar**:
  - .xpr dosyasının XML olarak parse edilmesi
  - Kaynak dosyalarının tespiti ve sınıflandırılması
  - IP bağımlılıklarının çözümlenmesi
  - Block Design içinden IP'lerin çıkarılması
  - Proje dosyasının yollarının güncellenmesi

### 3. VDag (Veri Yapısı)
- **Görev**: Proje içindeki her bir dosya/kaynak için bilgi tutar
- **Özellikler**:
  - Dosya türü (RTL, IP, BD, CONST, vb.)
  - Kaynak ve hedef yollar
  - Bağımlılık ilişkileri (affector, dagsToWaitCopied)
  - Kopyalama durumu ve meta veriler

### 4. FolderStructure (Yapılandırma)
- **Görev**: Hedef dizin yapısını tanımlar
- **Dizinler**:
  - `/hdl`: RTL kaynak kodları
  - `/ip`: IP modülleri
  - `/bd`: Block Design dosyaları
  - `/const`: Constraint dosyaları
  - `/sim`: Simülasyon dosyaları
  - `/out`: Çıktı dosyaları (.bit, .ltx)
  - `/other`: Diğer dosyalar

### 5. SourceType (Enum)
- **Görev**: Desteklenen dosya türlerini tanımlar
- **Değerler**: XPR, RTL, SIM, IP, IP_XCIX, BD, CONST, COE, OUT, OTHER

## Veri Akışı

### 1. Başlatma
```
MigrationVivado nesnesi oluşturulur
→ projectFileFA ve targetDir ayarlanır
→ MigrateProject() çağrılır
```

### 2. Ön İşlemler (MigrationCommon)
```
CheckInputs(): Giriş parametreleri doğrulanır
→ CreateRemoteDir(): Hedef dizin yapısı oluşturulur
→ MigrateProjectSpecific(): Vivado özel işlemler başlatılır
```

### 3. Vivado İşlem Akışı (MigrationVivado)
```
ParseXPR(): .xpr dosyası okunur, kaynaklar listelenir
→ RemoveRefXcis': .xcix içindeki .xci referansları temizlenir
→ CreateSurrogates(): .xcix için sanal .xci'ler oluşturulur
→ FindIpsFromBds(): Block Design içindeki IP'ler bulunur
→ FindCoesForIps(): IP'lerin kullandığı .coe dosyaları bulunur
→ ResolveCopyOrder(): Dosyaların kopyalama sırası belirlenir
→ ResolveMatchingNames(): Aynı isimli dosyalar için çakışma çözülür
→ DoMigration(): Dosyalar hedef dizine kopyalanır
→ WalkDags(): Yollar ve referanslar güncellenir
→ SaveProjectDoc(): Yeni .xpr dosyası kaydedilir
→ CopyBitFile(): .bit ve .ltx dosyaları kopyalanır
```

### 4. Veri Yapısı İlişkileri
- Her kaynak dosya için bir VDag nesnesi oluşturulur
- VDag nesneleri birbirine bağımlılık grafiği oluşturur
- `dagsToWaitCopied`: Kopyalanmadan önce beklenmesi gereken dosyalar
- `affector`: Bu dosyayı etkileyen diğer dosyalar
- `carrier`: İçinde başka dosyaları taşıyan ana dosya (örn: BD içindeki IP)

## Özel Durumlar

### 1. Managed IP (.xcix) İşlemi
- .xcix dosyaları zip arşivi olarak açılır
- İçindeki .xci dosyası için surrogate (vekil) VDag oluşturulur
- Asıl .xcix kopyalanmaz, sadece .xci içeriği işlenir

### 2. Block Design İşlemi
- .bd dosyaları parse edilir
- İçindeki `xci_name` alanlarından IP'ler çıkarılır
- Bulunan IP'ler için VDag nesneleri oluşturulur
- IP'ler `isCarried=true` olarak işaretlenir

### 3. Çakışan İsimler
- Aynı isimde farklı dosyalar için `knownIdentical` sayacı kullanılır
- Hedef dosya adına `_N` soneki eklenir (örn: `file_1.v`, `file_2.v`)

## Hata Yönetimi

### Hata Kodları
- `0xA1`: Eksik bilgi (projectFileFA veya targetDir)
- `0xB1`: Proje dosyası bulunamadı
- `0xD1`: Geçersiz proje dosyası (duplicate references)
- `0xD2`: Hedef dizin zaten mevcut
- `0xE1`: Bilinmeyen kaynak türü

### Loglama
- Tüm işlemler zaman damgası ile loglanır
- Kayıp dosyalar ve uyarılar ayrı listelerde tutulur
- `writeLog` delegate'i üzerinden dışarıya aktarılır

## Performans ve Sınırlamalar

### Performans
- Büyük projelerde dosya sayısına bağlı olarak işlem süresi artar
- .xcix dosyalarının açılması ek zaman gerektirir
- Bağımlılık çözümlemesi O(n²) karmaşıklığında çalışabilir

### Sınırlamalar
- Sadece Vivado "project-mode" .xpr projelerini destekler
- Non-project mode veya script-based projeler için uygun değildir
- Kayıp dosyalar işlemi durdurmaz, sadece uyarı verir
- Core Container IP'leri devre dışı bırakılır