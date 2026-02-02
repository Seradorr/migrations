using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Migrations {
    /// <summary>
    /// Migration sonrası oluşturulacak hedef dizin yapısını tanımlar.
    /// </summary>
    /// <remarks>
    /// Bu statik sınıf, kaynak türlerine göre hangi dizinlerin kullanılacağını
    /// ve bu dizinlerin mutlak/göreceli yollarını belirler.
    /// </remarks>
    public static class FolderStructure {
        
        /// <summary>Projenin kök dizini</summary>
        public static string rootDir { get; set; }

        /// <summary>Çalışma dizini (work)</summary>
        /// <remarks>
        /// Güncellenmiş .xpr dosyasının bulunduğu dizin.
        /// </remarks>
        public static string workDir {
            get {
                return rootDir + @"\work";
            }
        }

        /// <summary>Kaynak dosyaların bulunduğu dizin</summary>
        public static string sourcesDir {
            get {
                return rootDir;
            }
        }

        /// <summary>
        /// Belirtilen kaynak türü için mutlak dizin yolunu döndürür.
        /// </summary>
        /// <param name="type">Kaynak türü</param>
        /// <returns>Mutlak dizin yolu</returns>
        /// <example>
        /// FolderStructure.AbsoluteDir(SourceType.RTL) -> "C:\project\hdl"
        /// </example>
        public static string AbsoluteDir(SourceType type) {
            return sourcesDir + "\\" + DirName(type).Replace('/', '\\');
        }

        /// <summary>
        /// Belirtilen kaynak türü için .xpr dosyasına göre göreceli dizin yolunu döndürür.
        /// </summary>
        /// <param name="type">Kaynak türü</param>
        /// <returns>Göreceli dizin yolu</returns>
        /// <example>
        /// FolderStructure.RelativeToPPRDir(SourceType.RTL) -> "$PPRDIR/../hdl"
        /// </example>
        public static string RelativeToPPRDir(SourceType type) {
            return "$PPRDIR/../" + DirName(type);
        }

        /// <summary>
        /// Kaynak türüne göre dizin adını döndürür.
        /// </summary>
        /// <param name="type">Kaynak türü</param>
        /// <returns>Dizin adı</returns>
        /// <remarks>
        /// Dizin yapısı:
        /// - RTL -> hdl
        /// - IP -> ip
        /// - BD -> bd
        /// - CONST -> const
        /// - SIM -> sim
        /// - COE -> other/coe
        /// - OUT -> out
        /// - Diğerleri -> other
        /// </remarks>
        public static string DirName(SourceType type) {
            switch (type) {
                case SourceType.XPR:
                    return "other";
                case SourceType.RTL:
                    return "hdl";
                case SourceType.SIM:
                    return "sim";
                case SourceType.IP:
                    return "ip";
                case SourceType.BD:
                    return "bd";
                case SourceType.CONST:
                    return "const";
                case SourceType.COE:
                    return "other/coe";
                case SourceType.OUT:
                    return "out";
                case SourceType.ELF:
                    return "elf";
                case SourceType.HW_EXPORT:
                    return "hw_export";
                case SourceType.VITIS:
                    return "vitis";
                default:
                    return "other";
            }
        }

        /// <summary>
        /// Oluşturulacak tüm dizinlerin listesini döndürür.
        /// </summary>
        /// <returns>Dizin yolları dizisi</returns>
        /// <remarks>
        /// Bu metot, migration başlangıcında tüm dizinleri oluşturmak için kullanılır.
        /// </remarks>
        public static string[] AllStructure() {
            return new string[] {
                sourcesDir,
                workDir,
                AbsoluteDir(SourceType.BD),
                AbsoluteDir(SourceType.CONST),
                AbsoluteDir(SourceType.OTHER),
                AbsoluteDir(SourceType.COE),
                AbsoluteDir(SourceType.IP),
                AbsoluteDir(SourceType.RTL),
                AbsoluteDir(SourceType.SIM),
                AbsoluteDir(SourceType.OUT),
                AbsoluteDir(SourceType.ELF),
                AbsoluteDir(SourceType.HW_EXPORT),
                AbsoluteDir(SourceType.VITIS)
            };
        }
    }
}

