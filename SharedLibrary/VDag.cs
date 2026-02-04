using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

namespace Migrations {
    /// <summary>
    /// Proje içindeki her bir dosya/kaynak için bilgi tutan veri yapısı.
    /// </summary>
    /// <remarks>
    /// VDag (Vertex Directed Acyclic Graph), migration sürecindeki her bir kaynağı
    /// ve bu kaynakların arasındaki bağımlılık ilişkilerini temsil eder.
    /// Her dosya için kaynak/hedef yollar, türü, kopyalama durumu ve bağımlılıkları tutar.
    /// </remarks>
    class VDag {
        /// <summary>Kaynak dosyanın türü (RTL, IP, BD, vb.)</summary>
        public SourceType type { get; set; }

        /// <summary>Bu dosyayı etkileyen diğer dosyaların listesi</summary>
        public List<VDag> affector { get; private set; }

        /// <summary>Kopyalanmadan önce beklenmesi gereken dosyaların listesi</summary>
        public List<VDag> dagsToWaitCopied { get; private set; }

        /// <summary>Surrogate için asıl dosya (örn: .xcix)</summary>
        public VDag actual { get; set; }

        /// <summary>Bu VDag'in bir surrogate (vekil) olup olmadığı</summary>
        public bool isSurrogate { get; set; }

        /// <summary>Bu dosyanın bir surrogate'ü olup olmadığı</summary>
        public bool hasSurrogate { get; set; }

        /// <summary>Kaynak dosyanın tam yolu</summary>
        public string sourceFA { get; set; }

        /// <summary>Dosya adının私有 alanı</summary>
        public string name_ { get; private set; }

        /// <summary>.xpr dosyasındaki XML node referansı</summary>
        public XmlNode fileNode { get; set; }

        /// <summary>Dosyanın kopyalanıp kopyalanmayacağı</summary>
        public bool isCopied { get; set; }

        /// <summary>Dosyanın kayıp olup olmadığı</summary>
        public bool isLost { get; set; }

        /// <summary>Dosyanın kopyalanıp kopyalanmadığı</summary>
        public bool wasCopied { get; set; }

        /// <summary>Bu dosyanın başka bir dosya tarafından taşınıp taşınmadığı</summary>
        public bool isCarried { get; set; }

        /// <summary>Bu dosyayı taşıyan ana dosya (örn: BD içindeki IP)</summary>
        public VDag carrier { get; set; }

        /// <summary>Taşıyıcı dosyaya göre göreceli konum</summary>
        public string carrierRelativeLocation { get; set; }

        /// <summary>Aynı isimli dosyalar için çakışma çözüm sayacı</summary>
        public int knownIdentical { get; set; }

        /// <summary>VDag'in benzersiz kimliği</summary>
        public string uuid { get; }

        /// <summary>
        /// Bu VDag'in bir klasörü temsil edip etmediğini belirtir.
        /// </summary>
        /// <remarks>
        /// XPR, IP ve BD türleri tüm klasörü temsil eder.
        /// Bu dosyalar kopyalanırken tüm klasör içeriği kopyalanır.
        /// </remarks>
        public bool isRepresentingItsFolder {
            get {
                switch (type) {
                    case SourceType.XPR:
                        return true;
                    case SourceType.IP:
                        return true;
                    case SourceType.BD:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Dosya adını döndürür.
        /// </summary>
        /// <remarks>
        /// Aynı isimli dosyalar için _N soneki ekler (örn: file_1.v).
        /// </remarks>
        public string name {
            get {
                if (!isRepresentingItsFolder && knownIdentical != 1) {
                    int dotIndex = name_.LastIndexOf('.');
                    // Uzantısız dosyalarda sona ekle, uzantılı dosyalarda uzantıdan önce ekle
                    return name_.Insert((dotIndex > 0) ? dotIndex : name_.Length, "_" + knownIdentical);
                }

                else {
                    return name_;
                }
            }
            set {
                name_ = value;
            }
        }

        /// <summary>
        /// Kopyalanacak kaynak dizinin veya dosyanın yolunu döndürür.
        /// </summary>
        /// <remarks>
        /// Klasör temsil ediyorsa üst dizini, değilse kendisini döndürür.
        /// </remarks>
        public string copiedFA {
            get {
                if (isRepresentingItsFolder) {
                    return Directory.GetParent(sourceFA).FullName;
                }

                else {
                    return sourceFA;
                }
            }
        }

        /// <summary>
        /// Hedef dizindeki göreceli konumu döndürür.
        /// </summary>
        /// <remarks>
        /// Klasör temsil ediyorsa klasör adını, değilse dosya adını döndürür.
        /// </remarks>
        public string targetRelativeLocation {
            get {
                string rel;

                if (isRepresentingItsFolder) {
                    rel = Directory.GetParent(sourceFA).Name;
                    rel = rel + ((knownIdentical != 1) ? "_" + knownIdentical : "");
                }

                else {
                    rel = name;
                }

                return rel;
            }
        }

        /// <summary>
        /// Hedef konumun tam yolunu döndürür.
        /// </summary>
        /// <remarks>
        /// Taşınan dosya ise taşıyıcının konumunu kullanır.
        /// Normal dosyalar için: klasör yolu + dosya adı (targetFA ile aynı)
        /// Klasör temsil eden dosyalar için: hedef klasör yolu (targetFA = targetLocation + name)
        /// </remarks>
        public string targetLocation {
            get {
                if (isCarried) {
                    // Taşınan dosyalar için: carrier'ın hedef konumu + göreceli klasör yolu
                    // NOT: Bu sadece klasör yolu verir, dosya adı targetFA'da eklenir
                    if (isRepresentingItsFolder) {
                        return carrier.targetLocation + carrierRelativeLocation;
                    } else {
                        // Normal dosyalar için targetLocation = klasör yolu (dosya adı hariç)
                        // carrierRelativeLocation zaten sadece klasör yolunu içeriyor
                        string basePath = carrier.targetLocation;
                        if (!string.IsNullOrEmpty(carrierRelativeLocation)) {
                            basePath = basePath + carrierRelativeLocation;
                        }
                        return basePath;
                    }
                }

                else {
                    return FolderStructure.AbsoluteDir(type) + '\\' + targetRelativeLocation;
                }
            }
        }

        /// <summary>
        /// Hedef dosyanın tam yolunu döndürür.
        /// </summary>
        /// <remarks>
        /// Klasör temsil ediyorsa içindeki dosya adını ekler.
        /// Taşınan normal dosyalar için de dosya adını ekler.
        /// </remarks>
        public string targetFA {
            get {
                if (isRepresentingItsFolder) {
                    return targetLocation + '\\' + name;
                }
                else if (isCarried) {
                    // Taşınan normal dosyalar için: klasör yolu + dosya adı
                    return targetLocation + '\\' + name;
                }
                else {
                    return targetLocation;
                }
            }
        }

        /// <summary>
        /// .xpr dosyasına göre göreceli konumu döndürür.
        /// </summary>
        /// <remarks>
        /// $PPRDIR gibi Vivado değişkenlerini kullanır.
        /// </remarks>
        public string PPRRelativeLocation {
            get {
                if (isCarried) {
                    if (isRepresentingItsFolder) {
                        return carrier.PPRRelativeLocation + carrierRelativeLocation.Replace('\\', '/');
                    }

                    else {
                        if (string.IsNullOrEmpty(carrierRelativeLocation)) {
                            return carrier.PPRRelativeLocation;
                        }
                        string rel = carrierRelativeLocation.Replace('\\', '/');
                        if (rel.StartsWith("/")) {
                            rel = rel.Substring(1);
                        }
                        if (string.IsNullOrEmpty(rel)) {
                            return carrier.PPRRelativeLocation;
                        }
                        return carrier.PPRRelativeLocation + "/" + rel;
                    }
                }

                else if (isRepresentingItsFolder) {
                    return FolderStructure.RelativeToPPRDir(type) + '/' + targetRelativeLocation;
                }
                else {
                    return FolderStructure.RelativeToPPRDir(type);
                }
            }
        }

        /// <summary>
        /// .xpr dosyasına göre göreceli dosya adını döndürür.
        /// </summary>
        public string PPRRelativeFA
        {
            get {
                return PPRRelativeLocation + "/" + name;
            }
        }

        /// <summary>
        /// VDag sınıfının yeni bir örneğini oluşturur.
        /// </summary>
        public VDag() {
            affector = new List<VDag>();
            dagsToWaitCopied = new List<VDag>();
            knownIdentical = 1;
            isSurrogate = false;
            hasSurrogate = false;
            isCarried = false;
            isLost = false;
            isCopied = true;
            wasCopied = false;
            uuid = Guid.NewGuid().ToString();
        }

    }
}
