using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Migrations {
    /// <summary>
    /// Migration sürecinde desteklenen kaynak dosya türlerini tanımlar.
    /// </summary>
    public enum SourceType {
        /// <summary>Vivado proje dosyası</summary>
        XPR,
        /// <summary>RTL kaynak kodu (VHDL/Verilog)</summary>
        RTL,
        /// <summary>Simülasyon dosyası</summary>
        SIM,
        /// <summary>IP modülü (.xci)</summary>
        IP,
        /// <summary>Managed IP arşivi (.xcix)</summary>
        IP_XCIX,
        /// <summary>IP XML dosyası</summary>
        IP_XML,
        /// <summary>Block Design dosyası</summary>
        BD,
        /// <summary>Block Design XML dosyası</summary>
        BD_XML,
        /// <summary>Constraint dosyası (.xdc)</summary>
        CONST,
        /// <summary>Coefficient dosyası (.coe)</summary>
        COE,
        /// <summary>Çıktı dosyası (.bit, .ltx)</summary>
        OUT,
        /// <summary>Diğer dosyalar</summary>
        OTHER
    }
}

