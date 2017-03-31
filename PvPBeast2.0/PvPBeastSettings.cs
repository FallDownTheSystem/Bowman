using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Styx.Helpers;
using System.IO;
using Styx;
using System.Windows.Media;
using Styx.Common;

namespace PvPBeast
{
    public class PvPBeastSettings : Settings
    {
        public static readonly PvPBeastSettings Instance = new PvPBeastSettings();

        public PvPBeastSettings()
            : base(Path.Combine(Utilities.AssemblyDirectory, string.Format(@"Settings/PvPBeast-Settings-{0}.xml", StyxWoW.Me.Name)))
        {
        }

        [Setting, DefaultValue(true)]
        public bool T1NO { get; set; }

        [Setting, DefaultValue(true)]
        public bool T2NO { get; set; }

        [Setting, DefaultValue(false)]
        public bool ARN { get; set; }

        [Setting, DefaultValue(false)]
        public bool WEB { get; set; }

        [Setting, DefaultValue(true)]
        public bool BGS { get; set; }

        [Setting, DefaultValue(false)]
        public bool PVP { get; set; }

        [Setting, DefaultValue(false)]
        public bool INT { get; set; }

        [Setting, DefaultValue(true)]
        public bool T1MOB { get; set; }

        [Setting, DefaultValue(false)]
        public bool T1DMG { get; set; }

        [Setting, DefaultValue(false)]
        public bool T1DEF { get; set; }

        [Setting, DefaultValue(false)]
        public bool T2DMG { get; set; }

        [Setting, DefaultValue(false)]
        public bool T2DEF { get; set; }

        [Setting, DefaultValue(true)]
        public bool T2MOB { get; set; }

        [Setting, DefaultValue(true)]
        public bool TL1_NO { get; set; }

        [Setting, DefaultValue(true)]
        public bool TL3_NO { get; set; }

        [Setting, DefaultValue(true)]
        public bool TL2_NO { get; set; }

        [Setting, DefaultValue(true)]
        public bool TL5_NO { get; set; }

        [Setting, DefaultValue(true)]
        public bool TL4_NO { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL1_SS { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL1_WS { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL1_BS { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL3_FV { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL3_DB { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL3_TOTH { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL4_AMOC { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL4_BSTRK { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL4_LR { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL2_EXH { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL2_AOTIH { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL5_GLV { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL5_PWR { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL5_BRG { get; set; }
        
        [Setting, DefaultValue(false)]
        public bool TL2_SB { get; set; }

        [Setting, DefaultValue(false)]
        public bool CP { get; set; }

        [Setting, DefaultValue(false)]
        public bool STAM { get; set; }

        [Setting, DefaultValue(false)]
        public bool RP { get; set; }

        [Setting, DefaultValue(false)]
        public bool MP { get; set; }

        [Setting, DefaultValue(false)]
        public bool ICET { get; set; }

        [Setting, DefaultValue(false)]
        public bool SNAT { get; set; }

        [Setting, DefaultValue(false)]
        public bool FRET { get; set; }

        [Setting, DefaultValue(false)]
        public bool EXPT { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL2 { get; set; }

        [Setting, DefaultValue(45)]
        public int TargetHealth { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL3 { get; set; }

        [Setting, DefaultValue(false)]
        public bool TL4 { get; set; }
		
        [Setting, DefaultValue(false)]
        public bool RF { get; set; }

        [Setting, DefaultValue(false)]
        public bool WVE { get; set; }

        [Setting, DefaultValue(false)]
        public bool LB { get; set; }

        [Setting, DefaultValue(false)]
        public bool GE { get; set; }

        [Setting, DefaultValue(false)]
        public bool RS { get; set; }

        [Setting, DefaultValue(1)]
        public int PET { get; set; }

        [Setting, DefaultValue(5)]
        public int FFS { get; set; }

        [Setting, DefaultValue(75)]
        public int FocusShots { get; set; }

        [Setting, DefaultValue(75)]
        public int MendHealth { get; set; }

        [Setting, DefaultValue(93)]
        public int SpiritHealth_Me { get; set; }

        [Setting, DefaultValue(30)]
        public int SpiritHealth_Pet { get; set; }

        [Setting, DefaultValue(70)]
        public int SpiritHealth_Focus { get; set; }

        [Setting, DefaultValue(true)]
        public bool AspectSwitching { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool ARC { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool RDN { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool KCO { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool HM { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool CONC { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool KSH { get; set; }

        [Setting, DefaultValue(true)]
        public bool FF { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool DETR { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool BWR { get; set; }

        [Setting, DefaultValue(false)]
        public bool MDPet { get; set; }

        [Setting, DefaultValue("Never")]
        public string FDCBox { get; set; }

        [Setting, DefaultValue("Never")]
        public string SerpentBox { get; set; }

        [Setting, DefaultValue("2. Defense")]
        public string ScatterBox { get; set; }

        [Setting, DefaultValue("2. Defense")]
        public string IntimidateBox { get; set; }

        [Setting, DefaultValue("Never")]
        public string SpiritMendBox { get; set; }
		
    }
}
