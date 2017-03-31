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
using Styx.Logic.Combat;
using Styx.Logic.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;

namespace PvPBeast
{
    public partial class PvPBeastForm : Form
    {
        public PvPBeastForm()
        {
            InitializeComponent();
        }

        private void FillFDCBox()
        {
            FDBox.Items.Clear();
            FDBox.Items.Add("Never");
            FDBox.Items.Add("1. Pet Near");
            FDBox.Items.Add("2. Target Casting");
            FDBox.Items.Add("3. Low Health");
            FDBox.Items.Add("1 + 2");
            FDBox.Items.Add("1 + 2 + 3");
            FDBox.Items.Add("1 + 3");
            FDBox.Items.Add("2 + 3");

            FerBox.Items.Clear();
            FerBox.Items.Add("Never");
            FerBox.Items.Add("Low Focus");
            FerBox.Items.Add("Target Low Health");

            IntiBox.Items.Clear();
            IntiBox.Items.Add("Never");
            IntiBox.Items.Add("1. Interrupt");
            IntiBox.Items.Add("2. Low Health");
            IntiBox.Items.Add("3. Protection");
            IntiBox.Items.Add("1 + 2");
            IntiBox.Items.Add("1 + 3");
            IntiBox.Items.Add("2 + 3");
            IntiBox.Items.Add("1 + 2 + 3");
        }

        private void IntiBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((string)IntiBox.SelectedItem)
            {
                case "Never":
                    PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
                    break;
                case "1. Interrupt":
                    PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
                    break;
                case "2. Low Health":
                    PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
                    break;
                case "3. Protection":
                    PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
                    break;
                case "1 + 2":
                    PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
                    break;
                case "1 + 3":
                    PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
                    break;
                case "2 + 3":
                    PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
                    break;
                case "1 + 2 + 3":
                    PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
                    break;
            }
        }

        private void FDBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((string)FDBox.SelectedItem)
            {
                case "Never":
                    PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
                    break;
                case "1. Pet Near":
                    PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
                    break;
                case "2. Target Casting":
                    PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
                    break;
                case "3. Low Health":
                    PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
                    break;
                case "1 + 2":
                    PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
                    break;
                case "1 + 2 + 3":
                    PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
                    break;
                case "1 + 3":
                    PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
                    break;
                case "2 + 3":
                    PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
                    break;
            }
        }

        private void FerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((string)FerBox.SelectedItem)
            {
                case "Never":
                    PvPBeastSettings.Instance.FervorBox = (string)FerBox.SelectedItem;
                    break;
                case "Low Focus":
                    PvPBeastSettings.Instance.FervorBox = (string)FerBox.SelectedItem;
                    break;
                case "Target Low Health":
                    PvPBeastSettings.Instance.FervorBox = (string)FerBox.SelectedItem;
                    break;
            }
        }
        private void PvPBeastForm_Load(object sender, EventArgs e)
        {
            FillFDCBox();
            FDBox.SelectedItem = (string)PvPBeastSettings.Instance.FDCBox;
            IntiBox.SelectedItem = (string)PvPBeastSettings.Instance.IntimidateBox;
            FerBox.SelectedItem = (string)PvPBeastSettings.Instance.FervorBox;

            PvPBeastSettings.Instance.Load();

            Mobs.Value = new decimal(PvPBeastSettings.Instance.Mobs);
            PET.Value = new decimal(PvPBeastSettings.Instance.PET);
            FocusShots.Value = new decimal(PvPBeastSettings.Instance.FocusShots);
            MendHealth.Value = new decimal(PvPBeastSettings.Instance.MendHealth);
            TargetHealth.Value = new decimal(PvPBeastSettings.Instance.TargetHealth);
            Trinket1Mob.Checked = PvPBeastSettings.Instance.T1MOB;
            Trinket1Dmg.Checked = PvPBeastSettings.Instance.T1DMG;
            Trinket1Def.Checked = PvPBeastSettings.Instance.T1DEF;
            Trinket2Mob.Checked = PvPBeastSettings.Instance.T2MOB;
			Trinket2Dmg.Checked = PvPBeastSettings.Instance.T2DMG;
            Trinket2Def.Checked = PvPBeastSettings.Instance.T2DEF;
            CallPet.Checked = PvPBeastSettings.Instance.CP;
            RevivePet.Checked = PvPBeastSettings.Instance.RP;
            MendPet.Checked = PvPBeastSettings.Instance.MP;
			SpiritMend.Checked = PvPBeastSettings.Instance.SMend;
            PetWeb.Checked = PvPBeastSettings.Instance.WEB;
			Sacrifice.Checked = PvPBeastSettings.Instance.ROS;
            Recovery.Checked = PvPBeastSettings.Instance.ROR;
            CallWild.Checked = PvPBeastSettings.Instance.CW;
            IceTrap.Checked = PvPBeastSettings.Instance.ICET;
			SnakeTrap.Checked = PvPBeastSettings.Instance.SNAT;
			FreezeTrap.Checked = PvPBeastSettings.Instance.FRET;
			ExploTrap.Checked = PvPBeastSettings.Instance.EXPT;
			LauncherFail.Checked = PvPBeastSettings.Instance.TLF;
            Launcher.Checked = PvPBeastSettings.Instance.TL;
			Launcher2.Checked = PvPBeastSettings.Instance.TL2;
			Launcher3.Checked = PvPBeastSettings.Instance.TL3;
			Launcher4.Checked = PvPBeastSettings.Instance.TL4;
            Trinket1.Checked = PvPBeastSettings.Instance.T1;
            Trinket2.Checked = PvPBeastSettings.Instance.T2;
            Rapid.Checked = PvPBeastSettings.Instance.RF;
            LifeBlood.Checked = PvPBeastSettings.Instance.LB;
            BestialWrath.Checked = PvPBeastSettings.Instance.BW;
            Gloves.Checked = PvPBeastSettings.Instance.GE;
			FocusTarget.Checked = PvPBeastSettings.Instance.FT;
            Racial.Checked = PvPBeastSettings.Instance.RS;
            AspectSwitching.Checked = PvPBeastSettings.Instance.AspectSwitching;
			Melee.Checked = PvPBeastSettings.Instance.MLE;
			Interrupt.Checked = PvPBeastSettings.Instance.INT;
			Disengage.Checked = PvPBeastSettings.Instance.DIS;
			Scatter.Checked = PvPBeastSettings.Instance.SCAT;
			HMark.Checked = PvPBeastSettings.Instance.HM;
			WVenom.Checked = PvPBeastSettings.Instance.WVE;
			Concussive.Checked = PvPBeastSettings.Instance.CONC;
			Deterrence.Checked = PvPBeastSettings.Instance.DETR;
			SerSting.Checked = PvPBeastSettings.Instance.STING;      
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.thebuddyforum.com/members/61684-falldown.html");
            linkLabel1.LinkVisited = true;
        }
        private void linkLabel6_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.thebuddyforum.com/members/79446-zenlulz.html");
            linkLabel6.LinkVisited = true;
        }
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.wowhead.com/talent#cfhffkhdoRozZGMM");
            linkLabel3.LinkVisited = true;
        }
        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.wowhead.com/talent#cfhffkhdoRofMZh");
            linkLabel4.LinkVisited = true;
        }
        private void linkLabel10_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.noxxic.com/wow/pvp/hunter/beast-mastery/stat-priority-and-details");
            linkLabel10.LinkVisited = true;
        }
        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.thebuddyforum.com/newreply.php?t=51333&noquote=1");
            linkLabel5.LinkVisited = true;
        }
        private void Mobs_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.Mobs = (int)Mobs.Value;
        }
        private void PET_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.PET = (int)PET.Value;
        }

        private void FocusShots_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.FocusShots = (int)FocusShots.Value;
        }
        private void Trinket1Mob_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket1Mob.Checked == true)
            {
                PvPBeastSettings.Instance.T1MOB = true;
            }
            else
            {
                PvPBeastSettings.Instance.T1MOB = false;
            }
        }
        private void Trinket1Dmg_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket1Dmg.Checked == true)
            {
                PvPBeastSettings.Instance.T1DMG = true;
            }
            else
            {
                PvPBeastSettings.Instance.T1DMG = false;
            }
        }
        private void Trinket1Def_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket1Def.Checked == true)
            {
                PvPBeastSettings.Instance.T1DEF = true;
            }
            else
            {
                PvPBeastSettings.Instance.T1DEF = false;
            }
        }
        private void Trinket2Mob_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket2Mob.Checked == true)
            {
                PvPBeastSettings.Instance.T2MOB = true;
            }
            else
            {
                PvPBeastSettings.Instance.T2MOB = false;
            }
        }
		private void Trinket2Def_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket2Def.Checked == true)
            {
                PvPBeastSettings.Instance.T2DEF = true;
            }
            else
            {
                PvPBeastSettings.Instance.T2DEF = false;
            }
        }
		private void Trinket2Dmg_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket2Dmg.Checked == true)
            {
                PvPBeastSettings.Instance.T2DMG = true;
            }
            else
            {
                PvPBeastSettings.Instance.T2DMG = false;
            }
        }
        private void CallPet_CheckedChanged(object sender, EventArgs e)
        {
            if (CallPet.Checked == true)
            {
                PvPBeastSettings.Instance.CP = true;
            }
            else
            {
                PvPBeastSettings.Instance.CP = false;
            }
        }
        private void RevivePet_CheckedChanged(object sender, EventArgs e)
        {
            if (RevivePet.Checked == true)
            {
                PvPBeastSettings.Instance.RP = true;
            }
            else
            {
                PvPBeastSettings.Instance.RP = false;
            }
        }
        private void MendPet_CheckedChanged(object sender, EventArgs e)
        {
            if (MendPet.Checked == true)
            {
                PvPBeastSettings.Instance.MP = true;
            }
            else
            {
                PvPBeastSettings.Instance.MP = false;
            }
        }
		private void PetWeb_CheckedChanged(object sender, EventArgs e)
        {
            if (PetWeb.Checked == true)
            {
                PvPBeastSettings.Instance.WEB = true;
            }
            else
            {
                PvPBeastSettings.Instance.WEB = false;
            }
        }
        private void SpiritMend_CheckedChanged(object sender, EventArgs e)
        {
            if (SpiritMend.Checked == true)
            {
                PvPBeastSettings.Instance.SMend = true;
            }
            else
            {
                PvPBeastSettings.Instance.SMend = false;
            }
        }
        private void Recovery_CheckedChanged(object sender, EventArgs e)
        {
            if (Recovery.Checked == true)
            {
                PvPBeastSettings.Instance.ROR = true;
            }
            else
            {
                PvPBeastSettings.Instance.ROR = false;
            }
        }
        private void CallWild_CheckedChanged(object sender, EventArgs e)
        {
            if (CallWild.Checked == true)
            {
                PvPBeastSettings.Instance.CW = true;
            }
            else
            {
                PvPBeastSettings.Instance.CW = false;
            }
        }
		private void Sacrifice_CheckedChanged(object sender, EventArgs e)
        {
            if (Sacrifice.Checked == true)
            {
                PvPBeastSettings.Instance.ROS = true;
            }
            else
            {
                PvPBeastSettings.Instance.ROS = false;
            }
        }
		private void LauncherFail_CheckedChanged(object sender, EventArgs e)
        {
            if (LauncherFail.Checked == true)
            {
                PvPBeastSettings.Instance.TLF = true;
            }
            else
            {
                PvPBeastSettings.Instance.TLF = false;
            }
        }
        private void Launcher_CheckedChanged(object sender, EventArgs e)
        {
            if (Launcher.Checked == true)
            {
                PvPBeastSettings.Instance.TL = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL = false;
            }
        }
		private void Launcher2_CheckedChanged(object sender, EventArgs e)
        {
            if (Launcher2.Checked == true)
            {
                PvPBeastSettings.Instance.TL2 = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL2 = false;
            }
        }
		private void Launcher3_CheckedChanged(object sender, EventArgs e)
        {
            if (Launcher3.Checked == true)
            {
                PvPBeastSettings.Instance.TL3 = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL3 = false;
            }
        }
		private void Launcher4_CheckedChanged(object sender, EventArgs e)
        {
            if (Launcher4.Checked == true)
            {
                PvPBeastSettings.Instance.TL4 = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL4 = false;
            }
        }
        private void Trinket1_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket1.Checked == true)
            {
                PvPBeastSettings.Instance.T1 = true;
            }
            else
            {
                PvPBeastSettings.Instance.T1 = false;
            }
        }
        private void Trinket2_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket2.Checked == true)
            {
                PvPBeastSettings.Instance.T2 = true;
            }
            else
            {
                PvPBeastSettings.Instance.T2 = false;
            }
        }
        private void Rapid_CheckedChanged(object sender, EventArgs e)
        {
            if (Rapid.Checked == true)
            {
                PvPBeastSettings.Instance.RF = true;
            }
            else
            {
                PvPBeastSettings.Instance.RF = false;
            }
        }
        private void LifeBlood_CheckedChanged(object sender, EventArgs e)
        {
            if (LifeBlood.Checked == true)
            {
                PvPBeastSettings.Instance.LB = true;
            }
            else
            {
                PvPBeastSettings.Instance.LB = false;
            }
        }
        private void BestialWrath_CheckedChanged(object sender, EventArgs e)
        {
            if (BestialWrath.Checked == true)
            {
                PvPBeastSettings.Instance.BW = true;
            }
            else
            {
                PvPBeastSettings.Instance.BW = false;
            }
        }
        private void Gloves_CheckedChanged(object sender, EventArgs e)
        {
            if (Gloves.Checked == true)
            {
                PvPBeastSettings.Instance.GE = true;
            }
            else
            {
                PvPBeastSettings.Instance.GE = false;
            }
        }
		private void FocusTarget_CheckedChanged(object sender, EventArgs e)
        {
            if (FocusTarget.Checked == true)
            {
                PvPBeastSettings.Instance.FT = true;
            }
            else
            {
                PvPBeastSettings.Instance.FT = false;
            }
        }
        private void Racial_CheckedChanged(object sender, EventArgs e)
        {
            if (Racial.Checked == true)
            {
                PvPBeastSettings.Instance.RS = true;
            }
            else
            {
                PvPBeastSettings.Instance.RS = false;
            }
        }
        private void IceTrap_CheckedChanged(object sender, EventArgs e)
        {
            if (IceTrap.Checked == true)
            {
                PvPBeastSettings.Instance.ICET = true;
            }
            else
            {
                PvPBeastSettings.Instance.ICET = false;
            }
        }
		private void SnakeTrap_CheckedChanged(object sender, EventArgs e)
        {
            if (SnakeTrap.Checked == true)
            {
                PvPBeastSettings.Instance.SNAT = true;
            }
            else
            {
                PvPBeastSettings.Instance.SNAT = false;
            }
        }
		private void FreezeTrap_CheckedChanged(object sender, EventArgs e)
        {
            if (FreezeTrap.Checked == true)
            {
                PvPBeastSettings.Instance.FRET = true;
            }
            else
            {
                PvPBeastSettings.Instance.FRET = false;
            }
        }
		private void ExploTrap_CheckedChanged(object sender, EventArgs e)
        {
            if (ExploTrap.Checked == true)
            {
                PvPBeastSettings.Instance.EXPT = true;
            }
            else
            {
                PvPBeastSettings.Instance.EXPT = false;
            }
        }
        private void AspectSwitching_CheckedChanged(object sender, EventArgs e)
        {
            if (AspectSwitching.Checked == true)
            {
                PvPBeastSettings.Instance.AspectSwitching = true;
            }
            else
            {
                PvPBeastSettings.Instance.AspectSwitching = false;
            }
        }
		
		private void Melee_CheckedChanged(object sender, EventArgs e)
        {
            if (Melee.Checked == true)
            {
                PvPBeastSettings.Instance.MLE = true;
            }
            else
            {
                PvPBeastSettings.Instance.MLE = false;
            }
        }
		private void Interrupt_CheckedChanged(object sender, EventArgs e)
        {
            if (Interrupt.Checked == true)
            {
                PvPBeastSettings.Instance.INT = true;
            }
            else
            {
                PvPBeastSettings.Instance.INT = false;
            }
        }
		private void Disengage_CheckedChanged(object sender, EventArgs e)
        {
            if (Disengage.Checked == true)
            {
                PvPBeastSettings.Instance.DIS = true;
            }
            else
            {
                PvPBeastSettings.Instance.DIS = false;
            }
        }
		private void Scatter_CheckedChanged(object sender, EventArgs e)
        {
            if (Scatter.Checked == true)
            {
                PvPBeastSettings.Instance.SCAT = true;
            }
            else
            {
                PvPBeastSettings.Instance.SCAT = false;
            }
        }
		private void HMark_CheckedChanged(object sender, EventArgs e)
        {
            if (HMark.Checked == true)
            {
                PvPBeastSettings.Instance.HM = true;
            }
            else
            {
                PvPBeastSettings.Instance.HM = false;
            }
        }
		private void WVenom_CheckedChanged(object sender, EventArgs e)
        {
            if (WVenom.Checked == true)
            {
                PvPBeastSettings.Instance.WVE = true;
            }
            else
            {
                PvPBeastSettings.Instance.WVE = false;
            }
        }
		private void Concussive_CheckedChanged(object sender, EventArgs e)
        {
            if (Concussive.Checked == true)
            {
                PvPBeastSettings.Instance.CONC = true;
            }
            else
            {
                PvPBeastSettings.Instance.CONC = false;
            }
        }
		private void Deterrence_CheckedChanged(object sender, EventArgs e)
        {
            if (Deterrence.Checked == true)
            {
                PvPBeastSettings.Instance.DETR = true;
            }
            else
            {
                PvPBeastSettings.Instance.DETR = false;
            }
        }
		private void SerSting_CheckedChanged(object sender, EventArgs e)
        {
            if (SerSting.Checked == true)
            {
                PvPBeastSettings.Instance.STING = true;
            }
            else
            {
                PvPBeastSettings.Instance.STING = false;
            }
        }
        private void MendHealth_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.MendHealth = (int)MendHealth.Value;
        }
        private void TargetHealth_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.TargetHealth = (int)TargetHealth.Value;
        }
        private void SaveButton2_Click_1(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
            PvPBeastSettings.Instance.FervorBox = (string)FerBox.SelectedItem;
            PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
            PvPBeastSettings.Instance.Save();
            Logging.Write("Configuration Saved");
            Close();
        }
        private void SaveButton_Click_1(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
            PvPBeastSettings.Instance.FervorBox = (string)FerBox.SelectedItem;
            PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
            PvPBeastSettings.Instance.Save();
            Logging.Write("Configuration Saved");
            Close();
        }
    }
}
