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
using Styx.Helpers;
using Styx.CommonBot;
using Styx.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System.Windows.Media;
using Styx.Common;

namespace PvPBeast
{
    public partial class PvPBeastGUI : Form
    {
        public PvPBeastGUI()
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

            SerBox.Items.Clear();
            SerBox.Items.Add("Never");
            SerBox.Items.Add("Always");
            SerBox.Items.Add("Sometimes");

            ScatBox.Items.Clear();
            ScatBox.Items.Add("Never");
            ScatBox.Items.Add("1. Interrupt");
            ScatBox.Items.Add("2. Defense");
            ScatBox.Items.Add("1 + 2");

            IntiBox.Items.Clear();
            IntiBox.Items.Add("Never");
            IntiBox.Items.Add("1. Interrupt");
            IntiBox.Items.Add("2. Low Health");
            IntiBox.Items.Add("3. Protection");
            IntiBox.Items.Add("1 + 2");
            IntiBox.Items.Add("1 + 3");
            IntiBox.Items.Add("2 + 3");
            IntiBox.Items.Add("1 + 2 + 3");

            SMendBox.Items.Clear();
            SMendBox.Items.Add("Never");
            SMendBox.Items.Add("1. Me");
            SMendBox.Items.Add("2. Pet");
            SMendBox.Items.Add("3. Focus");
            SMendBox.Items.Add("1 + 2");
            SMendBox.Items.Add("1 + 3");
            SMendBox.Items.Add("2 + 3");
            SMendBox.Items.Add("1 + 2 + 3");
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
        private void SMendBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((string)SMendBox.SelectedItem)
            {
                case "Never":
                    PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
                    break;
                case "1. Me":
                    PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
                    break;
                case "2. Pet":
                    PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
                    break;
                case "3. Focus":
                    PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
                    break;
                case "1 + 2":
                    PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
                    break;
                case "1 + 3":
                    PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
                    break;
                case "2 + 3":
                    PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
                    break;
                case "1 + 2 + 3":
                    PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
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
        private void SerBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((string)SerBox.SelectedItem)
            {
                case "Never":
                    PvPBeastSettings.Instance.SerpentBox = (string)SerBox.SelectedItem;
                    break;
                case "Always":
                    PvPBeastSettings.Instance.SerpentBox = (string)SerBox.SelectedItem;
                    break;
                case "Sometimes":
                    PvPBeastSettings.Instance.SerpentBox = (string)SerBox.SelectedItem;
                    break;
            }
        }
        private void ScatBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch ((string)ScatBox.SelectedItem)
            {
                case "Never":
                    PvPBeastSettings.Instance.ScatterBox = (string)ScatBox.SelectedItem;
                    break;
                case "1. Interrupt":
                    PvPBeastSettings.Instance.ScatterBox = (string)ScatBox.SelectedItem;
                    break;
                case "2. Defense":
                    PvPBeastSettings.Instance.ScatterBox = (string)ScatBox.SelectedItem;
                    break;
                case "1 + 2":
                    PvPBeastSettings.Instance.ScatterBox = (string)ScatBox.SelectedItem;
                    break;
            }
        }
        private void BeastForm1_Load(object sender, EventArgs e)
        {
            FillFDCBox();
            FDBox.SelectedItem = (string)PvPBeastSettings.Instance.FDCBox;
            IntiBox.SelectedItem = (string)PvPBeastSettings.Instance.IntimidateBox;
            SMendBox.SelectedItem = (string)PvPBeastSettings.Instance.SpiritMendBox;
            SerBox.SelectedItem = (string)PvPBeastSettings.Instance.SerpentBox;
            ScatBox.SelectedItem = (string)PvPBeastSettings.Instance.ScatterBox;

            PvPBeastSettings.Instance.Load();

            PET.Value = new decimal(PvPBeastSettings.Instance.PET);
            FFS.Value = new decimal(PvPBeastSettings.Instance.FFS);
            MendHealth.Value = new decimal(PvPBeastSettings.Instance.MendHealth);
            SpiritHealth_Me.Value = new decimal(PvPBeastSettings.Instance.SpiritHealth_Me);
            SpiritHealth_Pet.Value = new decimal(PvPBeastSettings.Instance.SpiritHealth_Pet);
            SpiritHealth_Focus.Value = new decimal(PvPBeastSettings.Instance.SpiritHealth_Focus);
            TargetHealth.Value = new decimal(PvPBeastSettings.Instance.TargetHealth);
            FocusShots.Value = new decimal(PvPBeastSettings.Instance.FocusShots);
            Trinket1_None.Checked = PvPBeastSettings.Instance.T1NO;
            Trinket2_None.Checked = PvPBeastSettings.Instance.T2NO;
            Trinket1Mob.Checked = PvPBeastSettings.Instance.T1MOB;
            Trinket1Dmg.Checked = PvPBeastSettings.Instance.T1DMG;
            Trinket1Def.Checked = PvPBeastSettings.Instance.T1DEF;
            Trinket2Mob.Checked = PvPBeastSettings.Instance.T2MOB;
            Trinket2Dmg.Checked = PvPBeastSettings.Instance.T2DMG;
            Trinket2Def.Checked = PvPBeastSettings.Instance.T2DEF;
            TL1_None.Checked = PvPBeastSettings.Instance.TL1_NO;
            TL3_None.Checked = PvPBeastSettings.Instance.TL3_NO;
            TL4_None.Checked = PvPBeastSettings.Instance.TL4_NO;
            TL2_None.Checked = PvPBeastSettings.Instance.TL2_NO;
            TL5_None.Checked = PvPBeastSettings.Instance.TL5_NO;
            TL1_Silence.Checked = PvPBeastSettings.Instance.TL1_SS;
            TL1_Wyvern.Checked = PvPBeastSettings.Instance.TL1_WS;
            TL1_Binding.Checked = PvPBeastSettings.Instance.TL1_BS;
            TL3_Fervor.Checked = PvPBeastSettings.Instance.TL3_FV;
            TL3_Dire.Checked = PvPBeastSettings.Instance.TL3_DB;
            TL3_Thrill.Checked = PvPBeastSettings.Instance.TL3_TOTH;
            TL4_Crows.Checked = PvPBeastSettings.Instance.TL4_AMOC;
            TL4_Blink.Checked = PvPBeastSettings.Instance.TL4_BSTRK;
            TL4_Lynx.Checked = PvPBeastSettings.Instance.TL4_LR;
            TL2_Exhilaration.Checked = PvPBeastSettings.Instance.TL2_EXH;
            TL2_IronHawk.Checked = PvPBeastSettings.Instance.TL2_AOTIH;
            TL2_SpiritBond.Checked = PvPBeastSettings.Instance.TL2_SB;
            TL5_Glaive.Checked = PvPBeastSettings.Instance.TL5_GLV;
            TL5_Power.Checked = PvPBeastSettings.Instance.TL5_PWR;
            TL5_Barrage.Checked = PvPBeastSettings.Instance.TL5_BRG;
            CallPet.Checked = PvPBeastSettings.Instance.CP;
            FocusFire.Checked = PvPBeastSettings.Instance.FF;
            Stampede.Checked = PvPBeastSettings.Instance.STAM;
            RevivePet.Checked = PvPBeastSettings.Instance.RP;
            PetWeb.Checked = PvPBeastSettings.Instance.WEB;
            MendPet.Checked = PvPBeastSettings.Instance.MP;
            Worldpvp.Checked = PvPBeastSettings.Instance.PVP;
            Battleground.Checked = PvPBeastSettings.Instance.BGS;
            Arena.Checked = PvPBeastSettings.Instance.ARN;
            Rapid.Checked = PvPBeastSettings.Instance.RF;
            LifeBlood.Checked = PvPBeastSettings.Instance.LB;
            Gloves.Checked = PvPBeastSettings.Instance.GE;
            Racial.Checked = PvPBeastSettings.Instance.RS;
            Interrupt.Checked = PvPBeastSettings.Instance.INT;
            AspectSwitching.Checked = PvPBeastSettings.Instance.AspectSwitching;
			Arcane.Checked = PvPBeastSettings.Instance.ARC;
			Readiness.Checked = PvPBeastSettings.Instance.RDN;
			KillCom.Checked = PvPBeastSettings.Instance.KCO;
			HMark.Checked = PvPBeastSettings.Instance.HM;
            WVenom.Checked = PvPBeastSettings.Instance.WVE;
			Concussive.Checked = PvPBeastSettings.Instance.CONC;
			KillShot.Checked = PvPBeastSettings.Instance.KSH;
			Deterrence.Checked = PvPBeastSettings.Instance.DETR;
			BWrath.Checked = PvPBeastSettings.Instance.BWR;
            Misdirect.Checked = PvPBeastSettings.Instance.MDPet;
            IceTrap.Checked = PvPBeastSettings.Instance.ICET;
            SnakeTrap.Checked = PvPBeastSettings.Instance.SNAT;
            FreezeTrap.Checked = PvPBeastSettings.Instance.FRET;
            ExploTrap.Checked = PvPBeastSettings.Instance.EXPT;
            Launcher.Checked = PvPBeastSettings.Instance.TL;
            Launcher2.Checked = PvPBeastSettings.Instance.TL2;
            Launcher3.Checked = PvPBeastSettings.Instance.TL3;
            Launcher4.Checked = PvPBeastSettings.Instance.TL4;
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.thebuddyforum.com/members/61684-falldown.html");
            linkLabel1.LinkVisited = true;
        }
        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.thebuddyforum.com/honorbuddy-forum/classes/hunter/68054-beast-master-lazyraider.html");
            linkLabel5.LinkVisited = true;
        }
        private void PET_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.PET = (int)PET.Value;
        }
        private void FFS_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.FFS = (int)FFS.Value;
        }
        private void FocusShots_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.FocusShots = (int)FocusShots.Value;
        }
        private void MendHealth_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.MendHealth = (int)MendHealth.Value;
        }
        private void SpiritHealth_Me_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.SpiritHealth_Me = (int)SpiritHealth_Me.Value;
        }
        private void SpiritHealth_Pet_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.SpiritHealth_Pet = (int)SpiritHealth_Pet.Value;
        }
        private void SpiritHealth_Focus_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.SpiritHealth_Focus = (int)SpiritHealth_Focus.Value;
        }
        private void TargetHealth_ValueChanged(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.TargetHealth = (int)TargetHealth.Value;
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
        private void TL1_None_CheckedChanged(object sender, EventArgs e)
        {
            if (TL1_None.Checked == true)
            {
                PvPBeastSettings.Instance.TL1_NO = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL1_NO = false;
            }
        }
        private void TL3_None_CheckedChanged(object sender, EventArgs e)
        {
            if (TL3_None.Checked == true)
            {
                PvPBeastSettings.Instance.TL3_NO = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL3_NO = false;
            }
        }
        private void TL4_None_CheckedChanged(object sender, EventArgs e)
        {
            if (TL4_None.Checked == true)
            {
                PvPBeastSettings.Instance.TL4_NO = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL4_NO = false;
            }
        }
        private void TL2_None_CheckedChanged(object sender, EventArgs e)
        {
            if (TL2_None.Checked == true)
            {
                PvPBeastSettings.Instance.TL2_NO = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL2_NO = false;
            }
        }
        private void TL5_None_CheckedChanged(object sender, EventArgs e)
        {
            if (TL5_None.Checked == true)
            {
                PvPBeastSettings.Instance.TL5_NO = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL5_NO = false;
            }
        }
        private void TL1_Silence_CheckedChanged(object sender, EventArgs e)
        {
            if (TL1_Silence.Checked == true)
            {
                PvPBeastSettings.Instance.TL1_SS = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL1_SS = false;
            }
        }
        private void TL1_Wyvern_CheckedChanged(object sender, EventArgs e)
        {
            if (TL1_Wyvern.Checked == true)
            {
                PvPBeastSettings.Instance.TL1_WS = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL1_WS = false;
            }
        }
        private void TL1_Binding_CheckedChanged(object sender, EventArgs e)
        {
            if (TL1_Binding.Checked == true)
            {
                PvPBeastSettings.Instance.TL1_BS = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL1_BS = false;
            }
        }
        private void TL3_Fervor_CheckedChanged(object sender, EventArgs e)
        {
            if (TL3_Fervor.Checked == true)
            {
                PvPBeastSettings.Instance.TL3_FV = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL3_FV = false;
            }
        }
        private void TL3_Dire_CheckedChanged(object sender, EventArgs e)
        {
            if (TL3_Dire.Checked == true)
            {
                PvPBeastSettings.Instance.TL3_DB = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL3_DB = false;
            }
        }
        private void TL3_Thrill_CheckedChanged(object sender, EventArgs e)
        {
            if (TL3_Thrill.Checked == true)
            {
                PvPBeastSettings.Instance.TL3_TOTH = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL3_TOTH = false;
            }
        }
        private void TL4_Crows_CheckedChanged(object sender, EventArgs e)
        {
            if (TL4_Crows.Checked == true)
            {
                PvPBeastSettings.Instance.TL4_AMOC = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL4_AMOC = false;
            }
        }
        private void TL4_Blink_CheckedChanged(object sender, EventArgs e)
        {
            if (TL4_Blink.Checked == true)
            {
                PvPBeastSettings.Instance.TL4_BSTRK = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL4_BSTRK = false;
            }
        }
        private void TL4_Lynx_CheckedChanged(object sender, EventArgs e)
        {
            if (TL4_Lynx.Checked == true)
            {
                PvPBeastSettings.Instance.TL4_LR = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL4_LR = false;
            }
        }
        private void TL2_Exhilaration_CheckedChanged(object sender, EventArgs e)
        {
            if (TL2_Exhilaration.Checked == true)
            {
                PvPBeastSettings.Instance.TL2_EXH = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL2_EXH = false;
            }
        }
        private void TL2_IronHawk_CheckedChanged(object sender, EventArgs e)
        {
            if (TL2_IronHawk.Checked == true)
            {
                PvPBeastSettings.Instance.TL2_AOTIH = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL2_AOTIH = false;
            }
        }
        private void TL2_SpiritBond_CheckedChanged(object sender, EventArgs e)
        {
            if (TL2_SpiritBond.Checked == true)
            {
                PvPBeastSettings.Instance.TL2_SB = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL2_SB = false;
            }
        }
        private void TL5_Glaive_CheckedChanged(object sender, EventArgs e)
        {
            if (TL5_Glaive.Checked == true)
            {
                PvPBeastSettings.Instance.TL5_GLV = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL5_GLV = false;
            }
        }
        private void TL5_Power_CheckedChanged(object sender, EventArgs e)
        {
            if (TL5_Power.Checked == true)
            {
                PvPBeastSettings.Instance.TL5_PWR = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL5_PWR = false;
            }
        }
        private void TL5_Barrage_CheckedChanged(object sender, EventArgs e)
        {
            if (TL5_Barrage.Checked == true)
            {
                PvPBeastSettings.Instance.TL5_BRG = true;
            }
            else
            {
                PvPBeastSettings.Instance.TL5_BRG = false;
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
        private void FocusFire_CheckedChanged(object sender, EventArgs e)
        {
            if (FocusFire.Checked == true)
            {
                PvPBeastSettings.Instance.FF = true;
            }
            else
            {
                PvPBeastSettings.Instance.FF = false;
            }
        }
        private void Stampede_CheckedChanged(object sender, EventArgs e)
        {
            if (Stampede.Checked == true)
            {
                PvPBeastSettings.Instance.STAM = true;
            }
            else
            {
                PvPBeastSettings.Instance.STAM = false;
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
        private void Trinket1_None_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket1_None.Checked == true)
            {
                PvPBeastSettings.Instance.T1NO = true;
            }
            else
            {
                PvPBeastSettings.Instance.T1NO = false;
            }
        }
        private void Trinket2_None_CheckedChanged(object sender, EventArgs e)
        {
            if (Trinket2_None.Checked == true)
            {
                PvPBeastSettings.Instance.T2NO = true;
            }
            else
            {
                PvPBeastSettings.Instance.T2NO = false;
            }
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
		
		private void Arcane_CheckedChanged(object sender, EventArgs e)
        {
            if (Arcane.Checked == true)
            {
                PvPBeastSettings.Instance.ARC = true;
            }
            else
            {
                PvPBeastSettings.Instance.ARC = false;
            }
        }
		private void Readiness_CheckedChanged(object sender, EventArgs e)
        {
            if (Readiness.Checked == true)
            {
                PvPBeastSettings.Instance.RDN = true;
            }
            else
            {
                PvPBeastSettings.Instance.RDN = false;
            }
        }
		private void KillCom_CheckedChanged(object sender, EventArgs e)
        {
            if (KillCom.Checked == true)
            {
                PvPBeastSettings.Instance.KCO = true;
            }
            else
            {
                PvPBeastSettings.Instance.KCO = false;
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
        private void Misdirect_CheckedChanged(object sender, EventArgs e)
        {
            if (Misdirect.Checked == true)
            {
                PvPBeastSettings.Instance.MDPet = true;
            }
            else
            {
                PvPBeastSettings.Instance.MDPet = false;
            }
        }
		private void KillShot_CheckedChanged(object sender, EventArgs e)
        {
            if (KillShot.Checked == true)
            {
                PvPBeastSettings.Instance.KSH = true;
            }
            else
            {
                PvPBeastSettings.Instance.KSH = false;
            }
        }
		private void BWrath_CheckedChanged(object sender, EventArgs e)
        {
            if (BWrath.Checked == true)
            {
                PvPBeastSettings.Instance.BWR = true;
            }
            else
            {
                PvPBeastSettings.Instance.BWR = false;
            }
        }
        private void Arena_CheckedChanged(object sender, EventArgs e)
        {
            if (Arena.Checked == true)
            {
                PvPBeastSettings.Instance.ARN = true;
            }
            else
            {
                PvPBeastSettings.Instance.ARN = false;
            }
        }
        private void Battleground_CheckedChanged(object sender, EventArgs e)
        {
            if (Battleground.Checked == true)
            {
                PvPBeastSettings.Instance.BGS = true;
            }
            else
            {
                PvPBeastSettings.Instance.BGS = false;
            }
        }
        private void Worldpvp_CheckedChanged(object sender, EventArgs e)
        {
            if (Worldpvp.Checked == true)
            {
                PvPBeastSettings.Instance.PVP = true;
            }
            else
            {
                PvPBeastSettings.Instance.PVP = false;
            }
        }
        private void SaveButton2_Click_1(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
            PvPBeastSettings.Instance.SerpentBox = (string)SerBox.SelectedItem;
            PvPBeastSettings.Instance.ScatterBox = (string)ScatBox.SelectedItem;
            PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
            PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
            PvPBeastSettings.Instance.Save();
            Logging.Write(Colors.Aquamarine, "Configuration Saved");
            Close();
        }
        private void SaveButton_Click_1(object sender, EventArgs e)
        {
            PvPBeastSettings.Instance.FDCBox = (string)FDBox.SelectedItem;
            PvPBeastSettings.Instance.SerpentBox = (string)SerBox.SelectedItem;
            PvPBeastSettings.Instance.ScatterBox = (string)ScatBox.SelectedItem;
            PvPBeastSettings.Instance.IntimidateBox = (string)IntiBox.SelectedItem;
            PvPBeastSettings.Instance.SpiritMendBox = (string)SMendBox.SelectedItem;
            PvPBeastSettings.Instance.Save();
            Logging.Write(Colors.Aquamarine, "Configuration Saved");
            Close();
        }
        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void label22_Click(object sender, EventArgs e)
        {

        }
    }
}
