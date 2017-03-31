using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Styx.Helpers;
using System.IO;
using Styx;

namespace PvPBeast
{
    public class PvPBeastSettings : Settings
    {
        public static readonly PvPBeastSettings Instance = new PvPBeastSettings();

        public PvPBeastSettings()
            : base(Path.Combine(Logging.ApplicationPath, string.Format(@"CustomClasses/Config/PvPBeast-Settings-{0}.xml", StyxWoW.Me.Name)))
        {
        }

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

        [Setting, DefaultValue(85)]
        public int MendHealth { get; set; }

        [Setting, DefaultValue(45)]
        public int TargetHealth { get; set; }

        [Setting, DefaultValue(false)]
        public bool CP { get; set; }

        [Setting, DefaultValue(false)]
        public bool RP { get; set; }

        [Setting, DefaultValue(false)]
        public bool MP { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool WEB { get; set; }

        [Setting, DefaultValue(false)]
        public bool SMend { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool ROS { get; set; }

        [Setting, DefaultValue(false)]
        public bool ROR { get; set; }

        [Setting, DefaultValue(false)]
        public bool CW { get; set; }

        [Setting, DefaultValue(true)]
        public bool ICET { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool SNAT { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool FRET { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool EXPT { get; set; }

		[Setting, DefaultValue(false)]
        public bool TLF { get; set; }
		
        [Setting, DefaultValue(false)]
        public bool TL { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool TL2 { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool TL3 { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool TL4 { get; set; }

        [Setting, DefaultValue(false)]
        public bool T1 { get; set; }

        [Setting, DefaultValue(false)]
        public bool T2 { get; set; }

        [Setting, DefaultValue(false)]
        public bool RF { get; set; }

        [Setting, DefaultValue(false)]
        public bool LB { get; set; }

        [Setting, DefaultValue(false)]
        public bool RS { get; set; }

        [Setting, DefaultValue(4)]
        public int Mobs { get; set; }

        [Setting, DefaultValue(1)]
        public int PET { get; set; }

        [Setting, DefaultValue(66)]
        public int FocusShots { get; set; }

        [Setting, DefaultValue(true)]
        public bool AspectSwitching { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool MLE { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool INT { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool DIS { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool SCAT { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool HM { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool WVE { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool CONC { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool AIM { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool DETR { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool STING { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool FT { get; set; }
		
		[Setting, DefaultValue(false)]
        public bool PH { get; set; }

        [Setting, DefaultValue(false)]
        public bool GE { get; set; }

        [Setting, DefaultValue(false)]
        public bool BW { get; set; }
		
		[Setting, DefaultValue(true)]
        public bool AGR { get; set; }

        [Setting, DefaultValue("1 + 2")]
        public string FDCBox { get; set; }

        [Setting, DefaultValue("Target Low Health")]
        public string FervorBox { get; set; }

        [Setting, DefaultValue("2. Low Health")]
        public string IntimidateBox { get; set; }
		
    }
}
