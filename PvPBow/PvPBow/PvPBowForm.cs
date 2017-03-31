using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Styx;
using Styx.Combat.CombatRoutine;
using Styx.Helpers;
using Styx.Logic;
using Styx.Pathing;
using Styx.Combat;
using Styx.Logic.Combat;
using Styx.Logic.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace PvPBow
{
    public partial class PvPBowForm : Form
    {
        public PvPBowForm()
        {
            InitializeComponent();
        }
        private void PvPBowForm_Load(object sender, EventArgs e)
        {
            PvPBowSettings.Instance.Load();

            Mobs.Value = new decimal(PvPBowSettings.Instance.Mobs);
            PET.Value = new decimal(PvPBowSettings.Instance.PET);
            FocusShots.Value = new decimal(PvPBowSettings.Instance.FocusShots);
            Trinket1Mob.Checked = PvPBowSettings.Instance.T1MOB;
            Trinket1Dmg.Checked = PvPBowSettings.Instance.T1DMG;
            Trinket1Def.Checked = PvPBowSettings.Instance.T1DEF;
            Trinket2Mob.Checked = PvPBowSettings.Instance.T2MOB;
			Trinket2Dmg.Checked = PvPBowSettings.Instance.T2DMG;
            Trinket2Def.Checked = PvPBowSettings.Instance.T2DEF;
            Marksman.Checked = PvPBowSettings.Instance.MMSPEC;
            CallPet.Checked = PvPBowSettings.Instance.CP;
            RevivePet.Checked = PvPBowSettings.Instance.RP;
            MendPet.Checked = PvPBowSettings.Instance.MP;
			PetWeb.Checked = PvPBowSettings.Instance.WEB;
			Recovery.Checked = PvPBowSettings.Instance.ROR;
			Sacrifice.Checked = PvPBowSettings.Instance.ROS;
            IceTrap.Checked = PvPBowSettings.Instance.ICET;
			SnakeTrap.Checked = PvPBowSettings.Instance.SNAT;
			FreezeTrap.Checked = PvPBowSettings.Instance.FRET;
			ExploTrap.Checked = PvPBowSettings.Instance.EXPT;
			LauncherFail.Checked = PvPBowSettings.Instance.TLF;
            Launcher.Checked = PvPBowSettings.Instance.TL;
			Launcher2.Checked = PvPBowSettings.Instance.TL2;
			Launcher3.Checked = PvPBowSettings.Instance.TL3;
			Launcher4.Checked = PvPBowSettings.Instance.TL4;
            Trinket1.Checked = PvPBowSettings.Instance.T1;
            Trinket2.Checked = PvPBowSettings.Instance.T2;
            Rapid.Checked = PvPBowSettings.Instance.RF;
            LifeBlood.Checked = PvPBowSettings.Instance.LB;
            Gloves.Checked = PvPBowSettings.Instance.GE;
			FocusTarget.Checked = PvPBowSettings.Instance.FT;
			PlaceHolder.Checked = PvPBowSettings.Instance.PH;
            Racial.Checked = PvPBowSettings.Instance.RS;
            AspectSwitching.Checked = PvPBowSettings.Instance.AspectSwitching;
			Melee.Checked = PvPBowSettings.Instance.MLE;
			Interrupt.Checked = PvPBowSettings.Instance.INT;
			Disengage.Checked = PvPBowSettings.Instance.DIS;
			Scatter.Checked = PvPBowSettings.Instance.SCAT;
			HMark.Checked = PvPBowSettings.Instance.HM;
			WVenom.Checked = PvPBowSettings.Instance.WVE;
			Concussive.Checked = PvPBowSettings.Instance.CONC;
			AimedShot.Checked = PvPBowSettings.Instance.AIM;
			Deterrence.Checked = PvPBowSettings.Instance.DETR;
			SerSting.Checked = PvPBowSettings.Instance.STING;
			Aggro.Checked = PvPBowSettings.Instance.AGR;

            
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.thebuddyforum.com/members/61684-falldown.html");
            linkLabel1.LinkVisited = true;
        }
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.wowhead.com/talent#cZfMGRhkMuro");
            linkLabel3.LinkVisited = true;
        }
        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.noxxic.com/wow/pvp/hunter/marksmanship/stat-priority-and-details");
            linkLabel10.LinkVisited = true;
        }
        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.thebuddyforum.com/newreply.php?t=51333&noquote=1");
            linkLabel5.LinkVisited = true;
        }
        private void Mobs_ValueChanged(object sender, EventArgs e)
        {
            PvPBowSettings.Instance.Mobs = (int)Mobs.Value;
        }
        private void PET_ValueChanged(object sender, EventArgs e)
        {
            PvPBowSettings.Instance.PET = (int)PET.Value;
        }

        private void FocusShots_ValueChanged(object sender, EventArgs e)
        {
            PvPBowSettings.Instance.FocusShots = (int)FocusShots.Value;
        }
        private void Trinket1Mob_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket1Mob.Checked == true)
            {
                PvPBowSettings.Instance.T1MOB = true;
            }
            else
            {
                PvPBowSettings.Instance.T1MOB = false;
            }
        }
        private void Trinket1Dmg_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket1Dmg.Checked == true)
            {
                PvPBowSettings.Instance.T1DMG = true;
            }
            else
            {
                PvPBowSettings.Instance.T1DMG = false;
            }
        }
        private void Trinket1Def_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket1Def.Checked == true)
            {
                PvPBowSettings.Instance.T1DEF = true;
            }
            else
            {
                PvPBowSettings.Instance.T1DEF = false;
            }
        }
        private void Trinket2Mob_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket2Mob.Checked == true)
            {
                PvPBowSettings.Instance.T2MOB = true;
            }
            else
            {
                PvPBowSettings.Instance.T2MOB = false;
            }
        }
		private void Trinket2Def_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket2Def.Checked == true)
            {
                PvPBowSettings.Instance.T2DEF = true;
            }
            else
            {
                PvPBowSettings.Instance.T2DEF = false;
            }
        }
		private void Trinket2Dmg_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket2Dmg.Checked == true)
            {
                PvPBowSettings.Instance.T2DMG = true;
            }
            else
            {
                PvPBowSettings.Instance.T2DMG = false;
            }
        }
        private void Marksman_CheckedChanged(object sender, EventArgs e)
        {
            if (Marksman.Checked == true)
            {
                PvPBowSettings.Instance.MMSPEC = true;
            }
            else
            {
                PvPBowSettings.Instance.MMSPEC = false;
            }
        }
        private void CallPet_CheckedChanged(object sender, EventArgs e)
        {
            if (CallPet.Checked == true)
            {
                PvPBowSettings.Instance.CP = true;
            }
            else
            {
                PvPBowSettings.Instance.CP = false;
            }
        }
        private void RevivePet_CheckedChanged(object sender, EventArgs e)
        {
            if (RevivePet.Checked == true)
            {
                PvPBowSettings.Instance.RP = true;
            }
            else
            {
                PvPBowSettings.Instance.RP = false;
            }
        }
        private void MendPet_CheckedChanged(object sender, EventArgs e)
        {
            if (MendPet.Checked == true)
            {
                PvPBowSettings.Instance.MP = true;
            }
            else
            {
                PvPBowSettings.Instance.MP = false;
            }
        }
		private void PetWeb_CheckedChanged(object sender, EventArgs e)
        {
            if (PetWeb.Checked == true)
            {
                PvPBowSettings.Instance.WEB = true;
            }
            else
            {
                PvPBowSettings.Instance.WEB = false;
            }
        }
		private void Recovery_CheckedChanged(object sender, EventArgs e)
        {
            if (Recovery.Checked == true)
            {
                PvPBowSettings.Instance.ROR = true;
            }
            else
            {
                PvPBowSettings.Instance.ROR = false;
            }
        }
		private void Sacrifice_CheckedChanged(object sender, EventArgs e)
        {
            if (Sacrifice.Checked == true)
            {
                PvPBowSettings.Instance.ROS = true;
            }
            else
            {
                PvPBowSettings.Instance.ROS = false;
            }
        }
		private void LauncherFail_CheckedChanged(object sender, EventArgs e)
        {
            if (LauncherFail.Checked == true)
            {
                PvPBowSettings.Instance.TLF = true;
            }
            else
            {
                PvPBowSettings.Instance.TLF = false;
            }
        }
        private void Launcher_CheckedChanged(object sender, EventArgs e)
        {
            if (Launcher.Checked == true)
            {
                PvPBowSettings.Instance.TL = true;
            }
            else
            {
                PvPBowSettings.Instance.TL = false;
            }
        }
		private void Launcher2_CheckedChanged(object sender, EventArgs e)
        {
            if (Launcher2.Checked == true)
            {
                PvPBowSettings.Instance.TL2 = true;
            }
            else
            {
                PvPBowSettings.Instance.TL2 = false;
            }
        }
		private void Launcher3_CheckedChanged(object sender, EventArgs e)
        {
            if (Launcher3.Checked == true)
            {
                PvPBowSettings.Instance.TL3 = true;
            }
            else
            {
                PvPBowSettings.Instance.TL3 = false;
            }
        }
		private void Launcher4_CheckedChanged(object sender, EventArgs e)
        {
            if (Launcher4.Checked == true)
            {
                PvPBowSettings.Instance.TL4 = true;
            }
            else
            {
                PvPBowSettings.Instance.TL4 = false;
            }
        }
        private void Trinket1_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket1.Checked == true)
            {
                PvPBowSettings.Instance.T1 = true;
            }
            else
            {
                PvPBowSettings.Instance.T1 = false;
            }
        }
        private void Trinket2_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket2.Checked == true)
            {
                PvPBowSettings.Instance.T2 = true;
            }
            else
            {
                PvPBowSettings.Instance.T2 = false;
            }
        }
        private void Rapid_CheckedChanged(object sender, EventArgs e)
        {
            if (Rapid.Checked == true)
            {
                PvPBowSettings.Instance.RF = true;
            }
            else
            {
                PvPBowSettings.Instance.RF = false;
            }
        }
        private void Gloves_CheckedChanged(object sender, EventArgs e)
        {
            if (Gloves.Checked == true)
            {
                PvPBowSettings.Instance.GE = true;
            }
            else
            {
                PvPBowSettings.Instance.GE = false;
            }
        }
        private void LifeBlood_CheckedChanged(object sender, EventArgs e)
        {
            if (LifeBlood.Checked == true)
            {
                PvPBowSettings.Instance.LB = true;
            }
            else
            {
                PvPBowSettings.Instance.LB = false;
            }
        }
		private void FocusTarget_CheckedChanged(object sender, EventArgs e)
        {
            if (FocusTarget.Checked == true)
            {
                PvPBowSettings.Instance.FT = true;
            }
            else
            {
                PvPBowSettings.Instance.FT = false;
            }
        }
		private void PlaceHolder_CheckedChanged(object sender, EventArgs e)
        {
            if (PlaceHolder.Checked == true)
            {
                PvPBowSettings.Instance.PH = true;
            }
            else
            {
                PvPBowSettings.Instance.PH = false;
            }
        }
        private void Racial_CheckedChanged(object sender, EventArgs e)
        {
            if (Racial.Checked == true)
            {
                PvPBowSettings.Instance.RS = true;
            }
            else
            {
                PvPBowSettings.Instance.RS = false;
            }
        }
        private void IceTrap_CheckedChanged(object sender, EventArgs e)
        {
            if (IceTrap.Checked == true)
            {
                PvPBowSettings.Instance.ICET = true;
            }
            else
            {
                PvPBowSettings.Instance.ICET = false;
            }
        }
		private void SnakeTrap_CheckedChanged(object sender, EventArgs e)
        {
            if (SnakeTrap.Checked == true)
            {
                PvPBowSettings.Instance.SNAT = true;
            }
            else
            {
                PvPBowSettings.Instance.SNAT = false;
            }
        }
		private void FreezeTrap_CheckedChanged(object sender, EventArgs e)
        {
            if (FreezeTrap.Checked == true)
            {
                PvPBowSettings.Instance.FRET = true;
            }
            else
            {
                PvPBowSettings.Instance.FRET = false;
            }
        }
		private void ExploTrap_CheckedChanged(object sender, EventArgs e)
        {
            if (ExploTrap.Checked == true)
            {
                PvPBowSettings.Instance.EXPT = true;
            }
            else
            {
                PvPBowSettings.Instance.EXPT = false;
            }
        }
        private void AspectSwitching_CheckedChanged(object sender, EventArgs e)
        {
            if (AspectSwitching.Checked == true)
            {
                PvPBowSettings.Instance.AspectSwitching = true;
            }
            else
            {
                PvPBowSettings.Instance.AspectSwitching = false;
            }
        }
		
		private void Melee_CheckedChanged(object sender, EventArgs e)
        {
            if (Melee.Checked == true)
            {
                PvPBowSettings.Instance.MLE = true;
            }
            else
            {
                PvPBowSettings.Instance.MLE = false;
            }
        }
		private void Interrupt_CheckedChanged(object sender, EventArgs e)
        {
            if (Interrupt.Checked == true)
            {
                PvPBowSettings.Instance.INT = true;
            }
            else
            {
                PvPBowSettings.Instance.INT = false;
            }
        }
		private void Disengage_CheckedChanged(object sender, EventArgs e)
        {
            if (Disengage.Checked == true)
            {
                PvPBowSettings.Instance.DIS = true;
            }
            else
            {
                PvPBowSettings.Instance.DIS = false;
            }
        }
		private void Scatter_CheckedChanged(object sender, EventArgs e)
        {
            if (Scatter.Checked == true)
            {
                PvPBowSettings.Instance.SCAT = true;
            }
            else
            {
                PvPBowSettings.Instance.SCAT = false;
            }
        }
		private void HMark_CheckedChanged(object sender, EventArgs e)
        {
            if (HMark.Checked == true)
            {
                PvPBowSettings.Instance.HM = true;
            }
            else
            {
                PvPBowSettings.Instance.HM = false;
            }
        }
		private void WVenom_CheckedChanged(object sender, EventArgs e)
        {
            if (WVenom.Checked == true)
            {
                PvPBowSettings.Instance.WVE = true;
            }
            else
            {
                PvPBowSettings.Instance.WVE = false;
            }
        }
		private void Concussive_CheckedChanged(object sender, EventArgs e)
        {
            if (Concussive.Checked == true)
            {
                PvPBowSettings.Instance.CONC = true;
            }
            else
            {
                PvPBowSettings.Instance.CONC = false;
            }
        }
		private void Deterrence_CheckedChanged(object sender, EventArgs e)
        {
            if (Deterrence.Checked == true)
            {
                PvPBowSettings.Instance.DETR = true;
            }
            else
            {
                PvPBowSettings.Instance.DETR = false;
            }
        }
		private void AimedShot_CheckedChanged(object sender, EventArgs e)
        {
            if (AimedShot.Checked == true)
            {
                PvPBowSettings.Instance.AIM = true;
            }
            else
            {
                PvPBowSettings.Instance.AIM = false;
            }
        }
		private void SerSting_CheckedChanged(object sender, EventArgs e)
        {
            if (SerSting.Checked == true)
            {
                PvPBowSettings.Instance.STING = true;
            }
            else
            {
                PvPBowSettings.Instance.STING = false;
            }
        }
		private void Aggro_CheckedChanged(object sender, EventArgs e)
        {
            if (Aggro.Checked == true)
            {
                PvPBowSettings.Instance.AGR = true;
            }
            else
            {
                PvPBowSettings.Instance.AGR = false;
            }
        }      
        private void SaveButton_Click_1(object sender, EventArgs e)
        {
            PvPBowSettings.Instance.Save();
            Logging.Write("Configuration Saved");
            Close();
        }

        private void SaveButton2_Click_1(object sender, EventArgs e)
        {
            PvPBowSettings.Instance.Save();
            Logging.Write("Configuration Saved");
            Close();
        }
    }
}
