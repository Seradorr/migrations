using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Migrations {
    /// <summary>
    /// Tüm migration işlemleri için ortak altyapıyı sağlayan soyut temel sınıf.
    /// </summary>
    /// <remarks>
    /// Bu sınıf, farklı EDA araçları için migration sınıflarının temelini oluşturur.
    /// Giriş doğrulama, dizin oluşturma, loglama gibi ortak işlevleri sağlar.
    /// </remarks>
    public abstract class MigrationCommon {
        /// <summary>Eksik bilgi hatası kodu</summary>
        public const int ERRORCODE_LACKING_INFO = 0xA1;
        /// <summary>Proje dosyası bulunamadı hatası kodu</summary>
        public const int ERRORCODE_NO_PROJECT_FILE = 0xB1;
        /// <summary>Geçersiz proje dosyası hatası kodu</summary>
        public const int ERRORCODE_INVALID_PROJECT_FILE = 0xD1;
        /// <summary>Geçersiz proje dizini hatası kodu</summary>
        public const int ERRORCODE_INVALID_PROJ_DIR = 0xD2;
        /// <summary>Geçersiz kaynak dizini hatası kodu</summary>
        public const int ERRORCODE_INVALID_SOURCES_DIR = 0xD3;
        /// <summary>Geçersiz çalışma dizini hatası kodu</summary>
        public const int ERRORCODE_INVALID_WORK_DIR = 0xD4;
        /// <summary>Bilinmeyen kaynak türü hatası kodu</summary>
        public const int ERRORCODE_UNKNOWN_SOURCE_TYPE = 0xE1;

        /// <summary>İşlenecek proje dosyasının tam yolu</summary>
        public string projectFileFA { get; set; }

        /// <summary>Hedef proje dizininin yolu</summary>
        public string targetDir {
            get {
                return FolderStructure.rootDir;
            }
            set {
                FolderStructure.rootDir = value;
            }
        }

        /// <summary>Log mesajlarını yazmak için kullanılan delegate</summary>
        public delegate void WriteLog(string log);
        /// <summary>Log yazma fonksiyonu</summary>
        public WriteLog writeLog { get; set; }

        /// <summary>.gitignore dosyası oluşturulup oluşturulmayacağı</summary>
        public bool createGitIgnore = false;

        /// <summary>EDA aracına özel migration işlemlerini gerçekleştirir</summary>
        /// <returns>İşlem sonucu kodu (0 = başarı)</returns>
        protected abstract int MigrateProjectSpecific();

        /// <summary>EDA aracına özel .gitignore dosyası oluşturur</summary>
        protected abstract void CreateGitIgnoreSpecific();

        /// <summary>Migration türünü tutar (örn: "vivado")</summary>
        protected string migrationType;

        /// <summary>Log mesajları listesi</summary>
        protected List<string> logs;

        /// <summary>Uyarı mesajları listesi</summary>
        protected List<string> warnings;

        /// <summary>
        /// MigrationCommon sınıfının yeni bir örneğini oluşturur.
        /// </summary>
        public MigrationCommon() {
            logs = new List<string>();
            warnings = new List<string>();
        }

        /// <summary>
        /// Giriş parametrelerinin doğruluğunu kontrol eder.
        /// </summary>
        /// <returns>Doğrulama sonucu kodu (0 = başarı)</returns>
        /// <remarks>
        /// Debug modunda DebugInput sınıfından parametreleri okur.
        /// projectFileFA ve targetDir'in varlığını kontrol eder.
        /// </remarks>
        private int CheckInputs() {
#if DEBUG
            if (DebugInput.Available()) {
                MigrationInput mi = DebugInput.Get(migrationType);

                if (projectFileFA is null) {
                    projectFileFA = mi.projectFileFA;
                }

                if (targetDir is null) {
                    targetDir = mi.targetDir;

                    // Remove target directory when debug path is used
                    if (Directory.Exists(targetDir))
                        Directory.Delete(targetDir, true);
                }
            }
#endif

            if (projectFileFA is null) {
                if (writeLog != null) writeLog.Invoke("Project file is not given\n");
                return ERRORCODE_LACKING_INFO;
            }

            if (targetDir is null || targetDir.Trim().Length == 0) {
                if (writeLog != null) writeLog.Invoke("Target project directory is not given or empty\n");
                return ERRORCODE_LACKING_INFO;
            }

            if (!File.Exists(projectFileFA)) {
                if (writeLog != null) writeLog.Invoke("ERROR - Project file is not found.");
                return ERRORCODE_NO_PROJECT_FILE;
            }

            if (Directory.Exists(targetDir)) {
                if (writeLog != null) writeLog.Invoke("ERROR - Target directory already exists.");
                return ERRORCODE_INVALID_PROJ_DIR;
            }

            return 0;
        }

        /// <summary>
        /// Migration işlemini başlatan ana metot.
        /// </summary>
        /// <returns>İşlem sonucu kodu (0 = başarı)</returns>
        /// <remarks>
        /// İşlem akışı:
        /// 1. Giriş parametrelerini doğrular
        /// 2. Hedef dizin yapısını oluşturur
        /// 3. EDA aracına özel migration işlemlerini çalıştırır
        /// 4. İsteğe bağlı olarak .gitignore oluşturur
        /// 5. Uyarıları loglar
        /// </remarks>
        public int MigrateProject() {
            int returnVal;

            returnVal = CheckInputs();
            if (returnVal != 0) return returnVal;

            returnVal = CreateRemoteDir();
            if (returnVal != 0) return returnVal;

            returnVal = MigrateProjectSpecific();
            if (returnVal != 0) return returnVal;

            if (createGitIgnore) CreateGitIgnoreSpecific();

            LogWarnings();

            Log("Done");
            return 0;
        }

        /// <summary>
        /// Hedef dizin ve alt dizin yapısını oluşturur.
        /// </summary>
        /// <returns>Oluşturma sonucu kodu (0 = başarı)</returns>
        /// <remarks>
        /// FolderStructure.AllStructure() ile tanımlanan tüm dizinleri oluşturur.
        /// </remarks>
        private int CreateRemoteDir() {
            Directory.CreateDirectory(targetDir);

            foreach (string dir in FolderStructure.AllStructure()) {
                Directory.CreateDirectory(dir);
            }

            return 0;
        }

        /// <summary>
        /// Bir dizinin içeriğini başka bir dizine kopyalar.
        /// </summary>
        /// <param name="sourceDir">Kaynak dizin</param>
        /// <param name="destDir">Hedef dizin</param>
        /// <param name="excludeRegex">Dışlanacak dosyalar için regex pattern</param>
        /// <remarks>
        /// Alt dizinleri ve dosyaları recursive olarak kopyalar.
        /// excludeRegex ile eşleşen dosyalar kopyalanmaz (örn: checkpoint dosyaları).
        /// </remarks>
        protected void DirectoryCopy(string sourceDir, string destDir, string excludeRegex = @"^$") {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            Directory.CreateDirectory(destDir);
            DirectoryInfo[] dirsIn = dir.GetDirectories();
            FileInfo[] filesIn = dir.GetFiles();

            foreach (FileInfo file in filesIn) {
                // Do not copy checkpoint files
                if (!Regex.IsMatch(file.Name, excludeRegex)) {
                    string tempPath = Path.Combine(destDir, file.Name);
                    file.CopyTo(tempPath);
                }
            }

            foreach (DirectoryInfo dirIn in dirsIn) {
                string tempPath = Path.Combine(destDir, dirIn.Name);
                DirectoryCopy(dirIn.FullName, tempPath, excludeRegex);
            }
        }

        /// <summary>
        /// Zip dosyasını belirtilen dizine açar.
        /// </summary>
        /// <param name="zipFileFA">Zip dosyasının yolu</param>
        /// <param name="destDir">Hedef dizin</param>
        /// <param name="excludeRegex">Dışlanacak dosyalar için regex pattern</param>
        /// <remarks>
        /// ZipStorer kütüphanesini kullanarak zip dosyasını açar.
        /// excludeRegex ile eşleşen dosyalar çıkarılmaz.
        /// </remarks>
        protected void Unzip(string zipFileFA, string destDir, string excludeRegex = @"^$") {
            using (ZipStorer zip = ZipStorer.Open(zipFileFA, FileAccess.Read)) {
                List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

                foreach (ZipStorer.ZipFileEntry zipEntry in dir) {
                    string relativeFilePath = zipEntry.FilenameInZip.Replace('/', '\\');

                    if (!Regex.IsMatch(relativeFilePath, excludeRegex)) {
                        int lastSep = relativeFilePath.LastIndexOf('\\');
                        string relativeDirPath = lastSep > 0 ? relativeFilePath.Substring(0, lastSep) : "";
                        byte[] output;
                        zip.ExtractFile(zipEntry, out output);

                        if (!string.IsNullOrEmpty(relativeDirPath)) {
                            Directory.CreateDirectory(destDir + '\\' + relativeDirPath);
                        }
                        File.WriteAllBytes(destDir + '\\' + zipEntry.FilenameInZip.Replace('/', '\\'), output);
                    }
                }
            }
        }

        /// <summary>
        /// XCIX arşivinden sadece XCI dosyasını ve COE/MEM dosyalarını çıkarır.
        /// Synth, sim gibi Vivado tarafından üretilen artifactleri atlar.
        /// </summary>
        /// <param name="xcixFileFA">XCIX dosyasının yolu</param>
        /// <param name="destDir">Hedef dizin</param>
        /// <param name="xciFileName">Çıkarılacak XCI dosyasının adı</param>
        protected void UnzipXciOnly(string xcixFileFA, string destDir, string xciFileName) {
            using (ZipStorer zip = ZipStorer.Open(xcixFileFA, FileAccess.Read)) {
                List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

                // Sadece şu uzantıları çıkar: .xci, .coe, .mem
                string[] allowedExtensions = new string[] { ".xci", ".coe", ".mem" };
                
                // Hariç tutulacak klasörler
                string[] excludeDirs = new string[] { "sim", "synth", "sim_netlist" };

                foreach (ZipStorer.ZipFileEntry zipEntry in dir) {
                    string relativeFilePath = zipEntry.FilenameInZip.Replace('/', '\\');
                    string fileName = Path.GetFileName(relativeFilePath);
                    string ext = Path.GetExtension(fileName).ToLowerInvariant();

                    // cc.xml'i atla
                    if (fileName.Equals("cc.xml", StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    // Hariç tutulan klasörleri atla
                    bool skipDir = false;
                    foreach (string excludeDir in excludeDirs) {
                        if (relativeFilePath.IndexOf("\\" + excludeDir + "\\", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            relativeFilePath.StartsWith(excludeDir + "\\", StringComparison.OrdinalIgnoreCase)) {
                            skipDir = true;
                            break;
                        }
                    }
                    if (skipDir) continue;

                    // Sadece izin verilen uzantıları çıkar
                    bool allowed = false;
                    foreach (string allowedExt in allowedExtensions) {
                        if (ext.Equals(allowedExt, StringComparison.OrdinalIgnoreCase)) {
                            allowed = true;
                            break;
                        }
                    }

                    if (!allowed) continue;

                    // Dosyayı çıkar
                    int lastSep = relativeFilePath.LastIndexOf('\\');
                    string relativeDirPath = lastSep > 0 ? relativeFilePath.Substring(0, lastSep) : "";
                    byte[] output;
                    zip.ExtractFile(zipEntry, out output);

                    if (!string.IsNullOrEmpty(relativeDirPath)) {
                        Directory.CreateDirectory(destDir + '\\' + relativeDirPath);
                    }
                    
                    string targetPath = destDir + '\\' + relativeFilePath;
                    
                    // MAX_PATH kontrolü
                    if (targetPath.Length >= 260) {
                        Log("Warning - Skipping file due to MAX_PATH: " + targetPath);
                        continue;
                    }
                    
                    File.WriteAllBytes(targetPath, output);
                }
            }
        }

        /// <summary>
        /// Dosya yolundan dosya adını uzantısıyla birlikte döndürür.
        /// </summary>
        /// <param name="fileFA">Dosya yolu</param>
        /// <returns>Dosya adı ve uzantısı</returns>
        protected string GetFileNameWExt(string fileFA) {
            string fn = fileFA.Substring(fileFA.LastIndexOf('/') + 1);
            fn = fn.Substring(fn.LastIndexOf('\\') + 1);
            return fn;
        }

        /// <summary>
        /// Dosya yolundan dosya adını uzantısız döndürür.
        /// </summary>
        /// <param name="fileFA">Dosya yolu</param>
        /// <returns>Dosya adı (uzantısız)</returns>
        protected string GetFileNameWoExt(string fileFA) {
            string fnwoe = GetFileNameWExt(fileFA);

            if (fnwoe.IndexOf('.') != -1)
                fnwoe = fnwoe.Substring(0, fnwoe.LastIndexOf('.'));

            return fnwoe;
        }

        /// <summary>
        /// Göreceli yolu, belirtilen dizine göre mutlak yola dönüştürür.
        /// </summary>
        /// <param name="path">Dönüştürülecek göreceli yol</param>
        /// <param name="relativeToDir">Referans dizin</param>
        /// <returns>Mutlak yol</returns>
        /// <remarks>
        /// .. ve . pathlerini doğru şekilde işler.
        /// </remarks>
        protected string TraversePath(string path, string relativeToDir) {
            string resPath = relativeToDir;
            string[] steps = path.Split(new char[] { '\\', '/' });

            foreach (string step in steps) {
                if (step == "..") {
                    if (Directory.GetParent(resPath) is object) {
                        resPath = Directory.GetParent(resPath).FullName;
                    }

                    else {
                        return "%invalid%";
                    }
                }

                else {
                    resPath = Path.Combine(resPath, step);
                }
            }

            return resPath;
        }

        /// <summary>
        /// Mutlak yolu, belirtilen dizine göre göreceli yola dönüştürür.
        /// </summary>
        /// <param name="path">Dönüştürülecek mutlak yol</param>
        /// <param name="relativeToDir">Referans dizin</param>
        /// <returns>Göreceli yol</returns>
        protected string RelativizePath(string path, string relativeToDir) {
            string resPath = "";
            var pSteps = path.Split(new char[] { '\\', '/' }).ToList();
            var rSteps = relativeToDir.Split(new char[] { '\\', '/' }).ToList();

            for (int i = 0; i < pSteps.Count; i++) {
                string pStep = pSteps[0];
                string rStep = rSteps[0];
                pSteps.RemoveAt(0);
                rSteps.RemoveAt(0);

                if (pStep != rStep) {
                    break;
                }
            }

            foreach (string rStep in rSteps) {
                resPath = Path.Combine(resPath, "..");
            }

            foreach (string pStep in pSteps) {
                resPath = Path.Combine(resPath, pStep);
            }

            return resPath;
        }

        /// <summary>
        /// Log mesajını yazar ve log listesine ekler.
        /// </summary>
        /// <param name="msg">Log mesajı</param>
        /// <remarks>
        /// Mesajın başına UTC zaman damgası ekler.
        /// writeLog delegate'i üzerinden dışarıya gönderir.
        /// </remarks>
        protected void Log(string msg) {
            DateTime now = DateTime.UtcNow;
            string logMessage = now + " " + msg + '\n';
            if (writeLog != null) {
                writeLog.Invoke(logMessage);
            }
            logs.Add(now + " " + msg);
        }

        /// <summary>
        /// Birikmiş uyarı mesajlarını loglar.
        /// </summary>
        /// <remarks>
        /// Migration işlemi sonunda çağrılır.
        /// Tüm uyarılar "Warning: " ön eki ile loglanır.
        /// </remarks>
        protected void LogWarnings() {
            foreach (string warn in warnings) {
                Log("Warning: " + warn);
            }
        }

        /// <summary>
        /// Dosya yolunu normalize eder (küçük harf, tek ayracı kullanır).
        /// </summary>
        /// <param name="path">Normalize edilecek yol</param>
        /// <returns>Normalize edilmiş yol</returns>
        protected string NormalizePath(string path) {
            if (string.IsNullOrEmpty(path)) return path;
            return path.Replace('/', '\\').ToLowerInvariant().TrimEnd('\\');
        }

        /// <summary>
        /// İki dosya yolunun eşit olup olmadığını kontrol eder (case-insensitive).
        /// </summary>
        /// <param name="path1">Birinci yol</param>
        /// <param name="path2">İkinci yol</param>
        /// <returns>Yollar eşitse true</returns>
        protected bool PathsEqual(string path1, string path2) {
            if (path1 == null || path2 == null) return path1 == path2;
            return NormalizePath(path1).Equals(NormalizePath(path2), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Bir yolun başka bir yolun alt dizini olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="potentialChild">Potansiyel alt dizin yolu</param>
        /// <param name="potentialParent">Potansiyel üst dizin yolu</param>
        /// <returns>potentialChild, potentialParent'in altındaysa true</returns>
        protected bool IsSubPath(string potentialChild, string potentialParent) {
            if (string.IsNullOrEmpty(potentialChild) || string.IsNullOrEmpty(potentialParent))
                return false;

            string normalizedChild = NormalizePath(potentialChild);
            string normalizedParent = NormalizePath(potentialParent);

            return normalizedChild.StartsWith(normalizedParent + "\\", StringComparison.OrdinalIgnoreCase) ||
                   normalizedChild.Equals(normalizedParent, StringComparison.OrdinalIgnoreCase);
        }
    }
}

