using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Styx;
using Styx.Common;
using Styx.CommonBot;
using Styx.CommonBot.POI;
using Styx.CommonBot.Routines;
using Styx.Helpers;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using System.Diagnostics;

namespace TheBeastMaster
{
    internal class BeastMaster : CombatRoutine
    {
        public override WoWClass Class { get { return WoWClass.Hunter; } }

        public static readonly Version Version = new Version(1, 7, 0);

        public override string Name { get { return "The Beast Master PvE " + Version; } }

        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        #region Log
        private static void slog(string format, params object[] args) //use for standard logging
        {
            if (format != null)
            {
                Logging.Write(LogLevel.Normal, Colors.Aquamarine, format, args);
            }
        }

        private static void tslog(string format, params object[] args) //use for troubleshoot logging
        {
            if (format != null)
            {
                Logging.Write(LogLevel.Quiet, Colors.SeaGreen, format, args);
            }
        }

        #endregion

        #region Settings

        public override bool WantButton { get { return true; } }

        public override void OnButtonPress()
        {
            slog("Configuration opened!");
            new BeastForm1().ShowDialog();
        }

        #endregion

        #region Initialize
        public override void Initialize()
        {
            tslog("Character Faction: {0}", Me.IsHorde ? "Horde" : "Allience");
            tslog("Character Level: {0}", Me.Level);
            tslog("Character Race: {0}", Me.Race);
            slog("");
            slog("You are using The Beast Master Combat Routine");
            slog("Version: " + Version);
            slog("Made by FallDown");
            slog("For LazyRaider only!");
        }
        #endregion

        #region Halt on Feign Death
        public bool HaltFeign()
        {
            {
                if (!Me.HasAura("Feign Death"))
                    return true;
            }
            return false;
        }
        #endregion

        #region SelfControl
        public bool SelfControl(WoWUnit unit)
        {
            if (Me.GotTarget && (unit.HasAura("Freezing Trap") || unit.HasAura("Wyvern Sting") || unit.HasAura("Bad Manner") || unit.HasAura("Scatter Shot")))
                return true;

            else return false;
        }
        #endregion

        #region Dragon Soul

        public bool DebuffByID(int spellId)
        {
            if (Me.HasAura(spellId) && StyxWoW.Me.GetAuraById(spellId).TimeLeft.TotalMilliseconds <= 2000)
                return true;
            else return false;
        }

        public bool Ultra()
        {
            if (BeastMasterSettings.Instance.DSNOR || BeastMasterSettings.Instance.DSLFR)
            {
                if (!Me.HasAura("Deterrence"))
                {
                    foreach (WoWUnit u in ObjectManager.GetObjectsOfType<WoWUnit>(true, true))
                    {
                        if (u.IsAlive
                            && u.Guid != Me.Guid
                            && u.IsHostile
                            && u.IsCasting
                            && (u.CastingSpell.Id == 106174 
                                || u.CastingSpell.Id == 106389 
                                || u.CastingSpell.Id == 103327 
                                || u.CastingSpell.Id == 109417
                                || u.CastingSpell.Id == 109416
                                || u.CastingSpell.Id == 109415
                                || u.CastingSpell.Id == 106371)
                            && u.CurrentCastTimeLeft.TotalMilliseconds <= 1000)
                            return true;
                    }
                }
            }
            return false;
        }

        public bool UltraFL()
        {
            if (DebuffByID(110079)
                || DebuffByID(110080)
                || DebuffByID(110070)
                || DebuffByID(110069)
                || DebuffByID(109075)
                || DebuffByID(109200) 
                || DebuffByID(110068)
                || DebuffByID(105926) 
                || DebuffByID(105925)
                || DebuffByID(110078))
                return true;

            else return false;
        }

        public bool DW()
        {
            if (DebuffByID(110139)
                || DebuffByID(110140)
                || DebuffByID(110141)
                || DebuffByID(106791)
                || DebuffByID(109599)
                || DebuffByID(106794)
                || DebuffByID(109597)
                || DebuffByID(109598))
                return true;

            else return false;
        }
        #endregion

        #region Boss Check

        private bool IsTargetBoss()
        {
            if (Me.CurrentTarget.CreatureRank == WoWUnitClassificationType.WorldBoss ||
               (Me.CurrentTarget.Level >= 90 && Me.CurrentTarget.Elite && Me.CurrentTarget.MaxHealth > 4500000))
                return true;

            else return false;
        }
        private bool IsTargetEasyBoss()
        {
            if (Me.CurrentTarget.CreatureRank == WoWUnitClassificationType.WorldBoss ||
               (Me.CurrentTarget.Level >= 90 && Me.CurrentTarget.Elite && Me.CurrentTarget.MaxHealth > 1000000))
                return true;

            else return false;
        }
        #endregion

        #region CastSpell Method
        public static bool CastSpell(string SpellName, WoWUnit target)
        {
            if (SpellManager.HasSpell(SpellName) && SpellManager.Spells[SpellName].CooldownTimeLeft.TotalMilliseconds < 200 && Me.CurrentFocus >= SpellManager.Spells[SpellName].PowerCost)
            {
                if (!BeastMasterSettings.Instance.TL5_BRG || Me.ChanneledCastingSpellId != 120360)
                {
                    SpellManager.Cast(SpellName, target);
                    return true;
                }
            }
            return false;
        }

        public static bool CastSpell(string SpellName)
        {
            if (SpellManager.HasSpell(SpellName) && SpellManager.Spells[SpellName].CooldownTimeLeft.TotalMilliseconds < 200 && Me.CurrentFocus >= SpellManager.Spells[SpellName].PowerCost)
            {
                if (!BeastMasterSettings.Instance.TL5_BRG || Me.ChanneledCastingSpellId != 120360)
                {
                    SpellManager.Cast(SpellName);
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Controlled

         private static List<WoWSpellMechanic> controlMechanic = new List<WoWSpellMechanic>()
        {
            WoWSpellMechanic.Charmed,
            WoWSpellMechanic.Disoriented,
            WoWSpellMechanic.Fleeing,
            WoWSpellMechanic.Frozen,
            WoWSpellMechanic.Asleep,
            WoWSpellMechanic.Incapacitated,
            WoWSpellMechanic.Polymorphed,
            WoWSpellMechanic.Sapped
        };

        public bool isControlled(WoWUnit u)
        {
            foreach (WoWAura aura in u.Auras.Values)
            {
                if (controlMechanic.Contains(WoWSpell.FromId(aura.SpellId).Mechanic))
                    return true;
                else
                    return false;
            }
            return true;
        }
        #endregion

        #region Add Detection

        private int addCount()
        {
            int count = 0;
            foreach (WoWUnit u in ObjectManager.GetObjectsOfType<WoWUnit>(true, true))
            {
                if (Me.GotTarget
                    && u.IsAlive
                    && u.Guid != Me.Guid
                    && !u.IsFriendly
                    && u.IsHostile
                    && u.Attackable
                    && !u.IsTotem
                    && !u.IsCritter
                    && !u.IsNonCombatPet
                    && (u.Location.Distance(Me.CurrentTarget.Location) <= 10 || u.Location.Distance2D(Me.CurrentTarget.Location) <= 10))
                {
                    count++;
                }
            }
            return count;
        }
        #endregion

        #region MyDebuffTime
        //Used for checking how the time left on "my" debuff
        private int MyDebuffTime(String SpellName, WoWUnit unit)
        {
            {
                if (unit.HasAura(SpellName))
                {
                    var auras = unit.GetAllAuras();
                    foreach (var a in auras)
                    {
                        if (a.Name == SpellName && a.CreatorGuid == Me.Guid)
                        {
                            return a.TimeLeft.Seconds;
                        }
                    }
                }
            }
            return 0;
        }
        #endregion

        #region DebuffTime
        //Used for checking debuff timers
        private int DebuffTime(String SpellName, WoWUnit unit)
        {
            {
                if (unit.HasAura(SpellName))
                {
                    var auras = unit.GetAllAuras();
                    foreach (var b in auras)
                    {
                        if (b.Name == SpellName)
                        {
                            return b.TimeLeft.Seconds;
                        }
                    }
                }
            }
            return 0;
        }
        #endregion

        #region IsMyAuraActive
        //Used for checking auras that has no time
        private bool IsMyAuraActive(WoWUnit Who, String What)
        {
            {
                return Who.GetAllAuras().Where(p => p.CreatorGuid == Me.Guid && p.Name == What).FirstOrDefault() != null;
            }
        }
        #endregion

        #region Utilities

        /* Time to death code was made by HandNavi for the Junglebook CC, all credit goes to him! */

        private static uint _firstLife;
        private static uint _firstLifeMax;
        public static Stopwatch deathTimer = new Stopwatch();
        private static uint _currentLife;
        private static ulong _guid;

        public static long CalculateTimeToDeath(WoWUnit target)
        {
            if (target.Name.Contains("Training Dummy"))
            {
                return 111;
            }
            if (target.CurrentHealth == 0 || target.IsDead || !target.IsValid || !target.IsAlive)
            {
                return 0;
            }
            //Fill variables on new target or on target switch, this will lose all calculations from last target
            if (_guid != target.Guid || (_guid == target.Guid && target.CurrentHealth == _firstLifeMax))
            {
                _guid = target.Guid;
                _firstLife = target.CurrentHealth;
                _firstLifeMax = target.MaxHealth;
                deathTimer.Restart();
            }
            _currentLife = target.CurrentHealth;
            int timeDiff = deathTimer.Elapsed.Seconds;
            uint hpDiff = _firstLife - _currentLife;
            if (hpDiff > 0)
            {
                long fullTime = timeDiff * _firstLifeMax / hpDiff;
                long pastFirstTime = (_firstLifeMax - _firstLife) * timeDiff / hpDiff;
                long timeToDie = fullTime - pastFirstTime - timeDiff;

                if (timeToDie < 1) timeToDie = 1;
                return timeToDie;
            }
            if (hpDiff <= 0)
            {
                // Target was healed, reset to initial values
                _guid = target.Guid;
                _firstLife = target.CurrentHealth;
                _firstLifeMax = target.MaxHealth;
                deathTimer.Restart();
                return -1;
            }
            // Target is at full health
            if (_currentLife == _firstLifeMax)
            {
                return -1;
            }
            // No damage done, nothing to calculate
            return -1;
        }

        #endregion

        #region rest

        public override bool NeedRest
        {
            get
            {
                if (HaltFeign()  && StyxWoW.IsInWorld && !Me.IsGhost && Me.IsAlive && !Me.Mounted && !Me.IsFlying && !Me.IsOnTransport)
                {
                    if (BeastMasterSettings.Instance.RP && !Me.GotAlivePet && SpellManager.HasSpell("Revive Pet"))
                    {
                        if (CastSpell("Revive Pet"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Reviving Pet <<");
                        }
                        StyxWoW.SleepForLagDuration();
                    }
                    if (BeastMasterSettings.Instance.CP && Me.Pet == null && !Me.IsCasting)
                    {
                        if (BeastMasterSettings.Instance.PET == 1 && SpellManager.HasSpell("Call Pet 1"))
                        {
                            SpellManager.Cast("Call Pet 1");
                            StyxWoW.SleepForLagDuration();
                        }
                        if (BeastMasterSettings.Instance.PET == 2 && SpellManager.HasSpell("Call Pet 2"))
                        {
                            SpellManager.Cast("Call Pet 2");
                            StyxWoW.SleepForLagDuration();
                        }
                        if (BeastMasterSettings.Instance.PET == 3 && SpellManager.HasSpell("Call Pet 3"))
                        {
                            SpellManager.Cast("Call Pet 3");
                            StyxWoW.SleepForLagDuration();
                        }
                        if (BeastMasterSettings.Instance.PET == 4 && SpellManager.HasSpell("Call Pet 4"))
                        {
                            SpellManager.Cast("Call Pet 4");
                            StyxWoW.SleepForLagDuration();
                        }
                        if (BeastMasterSettings.Instance.PET == 5 && SpellManager.HasSpell("Call Pet 5"))
                        {
                            SpellManager.Cast("Call Pet 5");
                            StyxWoW.SleepForLagDuration();
                        }
                        StyxWoW.SleepForLagDuration();
                    }
                }
                return true;
            }
        }
        #endregion

        #region CombatStart

        private void AutoAttack()
        {
            if (!Me.IsAutoAttacking && Me.GotTarget && HaltFeign() && !SelfControl(Me.CurrentTarget))
            {
                Lua.DoString("StartAttack()");  
            }

        }
        #endregion

        #region Combat

        public override void Combat()
        {
            if (SelfControl(Me.CurrentTarget))
            {
                Lua.DoString("StopAttack()");
                {
                    Logging.Write(Colors.Aquamarine, ">> Stop Everything! <<");
                }
                SpellManager.StopCasting();
                {
                    Logging.Write(Colors.Aquamarine, ">> Stop Everything! <<");
                }
            }
            if (Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.Mounted && HaltFeign())
            {
                if (Ultra())
                {
                    Lua.DoString("RunMacroText('/click ExtraActionButton1');");
                    SpellManager.StopCasting();
                    {
                        Logging.Write(Colors.Aquamarine, ">> Heroic Will! <<");
                    }
                } 
                if (UltraFL())
                {
                    Lua.DoString("RunMacroText('/click ExtraActionButton1');");
                    SpellManager.StopCasting();
                    {
                        Logging.Write(Colors.Aquamarine, ">> Heroic Will! <<");
                    }
                }
                if (DW())
                {
                    Lua.DoString("RunMacroText('/click ExtraActionButton1');");
                    SpellManager.StopCasting();
                    {
                        Logging.Write(Colors.Aquamarine, ">> Enter the dream! <<");
                    }
                }
                if (BeastMasterSettings.Instance.MP && Me.GotAlivePet && Me.Pet.HealthPercent <= BeastMasterSettings.Instance.MendHealth && !IsMyAuraActive(Me.Pet, "Mend Pet"))
                {
                    if (CastSpell("Mend Pet"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Mend Pet <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL2_EXH && (Me.HealthPercent < 70 
                    || (Me.Pet.HealthPercent < 15 && SpellManager.HasSpell("Heart of the Phoenix") && SpellManager.Spells["Heart of the Phoenix"].CooldownTimeLeft.TotalSeconds > 5)
                    || (Me.Pet.HealthPercent < 15 && !SpellManager.HasSpell("Heart of the Phoenix"))))
                {
                    if (CastSpell("Exhilaration"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Exhilaration <<");
                    }
                }
                if (BeastMasterSettings.Instance.MDPet && Me.GotAlivePet && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !IsMyAuraActive(Me, "Misdirection")
                    && !WoWSpell.FromId(34477).Cooldown && !SpellManager.Spells["Misdirection"].Cooldown)
                {
                    Lua.DoString("RunMacroText('/cast [@pet,exists] Misdirection');");
                    {
                        Logging.Write(Colors.Aquamarine, ">> Misdirection on Pet <<");
                    }
                }
                if (BeastMasterSettings.Instance.DTS && Me.GotAlivePet && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !IsMyAuraActive(Me.CurrentTarget, "Distracting Shot"))
                {
                    if (CastSpell("Distracting Shot"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Distracting Shot! <<");
                    }
                }
                if (Me.GotAlivePet && IsMyAuraActive(Me.CurrentTarget, "Scatter Shot"))
                {
                    Lua.DoString("PetAttack()");
                }
                if (BeastMasterSettings.Instance.KCO && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && IsMyAuraActive(Me.CurrentTarget, "Scatter Shot"))
                {
                    if (CastSpell("Kill Command"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Kill Command <<");
                    }
                }
                if (BeastMasterSettings.Instance.MDF && Me.FocusedUnit != null && Me.FocusedUnit != Me.Pet && !Me.HasAura("Misdirection") 
                    && !WoWSpell.FromId(34477).Cooldown && !SpellManager.Spells["Misdirection"].Cooldown)
                {
                    Lua.DoString("RunMacroText('/cast [@focus,exists] Misdirection');");
                    {
                        Logging.Write(Colors.Aquamarine, ">> Misdirection on Focus <<");
                    }
                }
                ///////////////////////////////////////////Close Combat and Defense Mechanisms//////////////////////////////////////////////////////////////////////////////////////
                if (BeastMasterSettings.Instance.SMend && Me.CurrentHealth < Me.MaxHealth - 30000 && !WoWSpell.FromId(90361).Cooldown)
                {
                    Lua.DoString("RunMacroText(\"/cast [@" + Me.Name + "] Spirit Mend\")");
                    {
                        Logging.Write(Colors.Aquamarine, ">> Pet: Spirit Mend <<");
                    }
                }
                if (BeastMasterSettings.Instance.FDCBox == "1. High Threat" && Me.CurrentTarget.ThreatInfo.RawPercent > 90)
                {
                    if (CastSpell("Feign Death"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> High Aggro, Feign Death <<");
                    }
                }
                if (BeastMasterSettings.Instance.FDCBox == "2. On Aggro" && Me.CurrentTarget.ThreatInfo.RawPercent > 90 
                    && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.Distance < 10))
                {
                    if (CastSpell("Feign Death"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Aggro'ed, Feign Death <<");
                    }
                }
                if (BeastMasterSettings.Instance.FDCBox == "3. Low Health" && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.HealthPercent < 15)
                {
                    if (CastSpell("Feign Death"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Low Health, Feign Death <<");
                    }
                }
                if (BeastMasterSettings.Instance.FDCBox == "1 + 3" && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && Me.HealthPercent < 15)
                {
                    if (CastSpell("Feign Death"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Thread + Low HP, Feign Death <<");
                    }
                }
                if (BeastMasterSettings.Instance.FDCBox == "2 + 3" && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && Me.HealthPercent < 15 
                    && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.Distance < 10))
                {
                    if (CastSpell("Feign Death"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Aggro + Low HP, Feign Death <<");
                    }
                }
                if (BeastMasterSettings.Instance.DETR && Me.HealthPercent < 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && (Me.CurrentTarget.Distance < 10 || Me.CurrentTarget.IsCasting))
                {
                    if (CastSpell("Deterrence"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Deterrence <<");
                    }
                }
            }
            if (Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.Mounted && HaltFeign() && !SelfControl(Me.CurrentTarget))
            {
                if (BeastMasterSettings.Instance.ScatterBox == "1. Interrupt" && Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast)
                {
                    if (CastSpell("Scatter Shot"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Scatter Shot, Interrupt <<");
                    }
                }
                if (BeastMasterSettings.Instance.ScatterBox == "2. Defense" && Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid)
                {
                    if (CastSpell("Scatter Shot"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Scatter Shot, Evade <<");
                    }
                }
                if (BeastMasterSettings.Instance.ScatterBox == "1 + 2" && ((Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid)
                    || (Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast)))
                {
                    if (CastSpell("Scatter Shot"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Scatter Shot <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL1_SS && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast)
                {
                    if (CastSpell("Silencing Shot"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Silencing Shot <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL1_WS && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast)
                {
                    if (CastSpell("Wyvern Sting"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Wyvern Sting, Interrupt <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL1_BS && addCount() >= BeastMasterSettings.Instance.Mobs)
                {
                    if (CastSpell("Binding Shot"))
                    {
                        SpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
                        Logging.Write(Colors.Aquamarine, ">> Binding Shot Launched! <<");
                    }
                }
                if (BeastMasterSettings.Instance.CONC && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !Me.CurrentTarget.HasAura("Concussive Shot") && Me.CurrentTarget.Distance < 25)
                {
                    if (CastSpell("Concussive Shot"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Concussive Shot <<");
                    }
                }
                if (BeastMasterSettings.Instance.IntimidateBox == "1. Interrupt" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 
                    && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && !SpellManager.Spells["Intimidation"].Cooldown)
                {
                    if (CastSpell("Intimidation"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Intimidation, Interrupt <<");
                    }
                }
                if (BeastMasterSettings.Instance.IntimidateBox == "2. Defense" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 
                    && Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid)
                {
                    if (CastSpell("Intimidation"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Intimidation, Defense <<");
                    }
                }
                if (BeastMasterSettings.Instance.IntimidateBox == "1 + 2" && ((Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid) 
                    || (Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && !SpellManager.Spells["Intimidation"].Cooldown)))
                {
                    if (CastSpell("Intimidation"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Intimidation Stranger Danger! <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL3_FV && (Me.CurrentFocus < 60 || (Me.HasAura("The Beast within") && Me.CurrentFocus < 40)))
                {
                    if (CastSpell("Fervor"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Fervor <<");
                    }
                }
                /////////////////////////////////////////////////////Cooldowns/////////////////////////////////////////////////////////////////////////////////////////////////           
                if (BeastMasterSettings.Instance.RF && (SpellManager.Spells["Bestial Wrath"].Cooldown || !BeastMasterSettings.Instance.BWR) 
                    && !Me.HasAura("Rapid Fire") && !Me.HasAura("The Beast Within") 
                    && !Me.HasAura("Bloodlust") && !Me.HasAura("Heroism") 
                    && !Me.HasAura("Ancient Hysteria") && !Me.HasAura("Time Warp") 
                    && (Me.CurrentTarget.CurrentHealth > 400000 || CalculateTimeToDeath(Me.CurrentTarget) > 14) && (IsTargetBoss() || Me.CurrentTarget.Name.Contains("Training Dummy")))
                {
                    if (CastSpell("Rapid Fire"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Rapid Fire <<");
                    }
                }
                if (BeastMasterSettings.Instance.CW && IsTargetBoss() && (Me.CurrentTarget.CurrentHealth > 500000 || CalculateTimeToDeath(Me.CurrentTarget) > 20))
                {
                    if (CastSpell("Stampede"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Stampede <<");
                    }
                }
                if (Me.GotAlivePet && BeastMasterSettings.Instance.BWR && (!BeastMasterSettings.Instance.TL4_LR || SpellManager.Spells["Lynx Rush"].CooldownTimeLeft.TotalSeconds > 10 
                    || !SpellManager.Spells["Lynx Rush"].Cooldown) && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && !Me.HasAura("Rapid Fire") 
                    && !Me.HasAura("The Beast Within") && !Me.HasAura("Bloodlust") && !Me.HasAura("Heroism")
                    && !Me.HasAura("Ancient Hysteria") && !Me.HasAura("Time Warp") && (Me.CurrentTarget.CurrentHealth > 100000 || CalculateTimeToDeath(Me.CurrentTarget) > 8) && (Me.CurrentTarget.MaxHealth > 400000 || Me.CurrentTarget.Name.Contains("Training Dummy")))
                {
                    if (CastSpell("Bestial Wrath"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Bestial Wrath <<");
                    }
                }
                if (BeastMasterSettings.Instance.RDN 
                    && SpellManager.Spells["Rapid Fire"].CooldownTimeLeft.TotalSeconds > 1 
                    && SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 1
                    && (SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 1 || !BeastMasterSettings.Instance.KCO)
                    && (SpellManager.HasSpell("Lynx Rush") && SpellManager.Spells["Lynx Rush"].CooldownTimeLeft.TotalSeconds > 1 || !BeastMasterSettings.Instance.TL4_LR)
                    && (SpellManager.HasSpell("Glaive Toss") && SpellManager.Spells["Glaive Toss"].CooldownTimeLeft.TotalSeconds > 1 || !BeastMasterSettings.Instance.TL5_GLV)
                    && (SpellManager.HasSpell("Powershot") && SpellManager.Spells["Powershot"].CooldownTimeLeft.TotalSeconds > 1 || !BeastMasterSettings.Instance.TL5_PWR)
                    && (SpellManager.HasSpell("Barrage") && SpellManager.Spells["Barrage"].CooldownTimeLeft.TotalSeconds > 1 || !BeastMasterSettings.Instance.TL5_BRG)
                    && (SpellManager.HasSpell("Fervor") && SpellManager.Spells["Fervor"].CooldownTimeLeft.TotalSeconds > 1 || !BeastMasterSettings.Instance.TL3_FV)
                    && (SpellManager.HasSpell("A Murder of Crows") && SpellManager.Spells["A Murder of Crows"].CooldownTimeLeft.TotalSeconds > 1 || !BeastMasterSettings.Instance.TL4_AMOC)
                    && (SpellManager.HasSpell("Blink Strike") && SpellManager.Spells["Blink Strike"].CooldownTimeLeft.TotalSeconds > 1 || !BeastMasterSettings.Instance.TL4_BSTRK)
                    && (SpellManager.HasSpell("Dire Beast") && SpellManager.Spells["Dire Beast"].CooldownTimeLeft.TotalSeconds > 1 || !BeastMasterSettings.Instance.TL3_DB))
                {
                    if (CastSpell("Readiness"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Readiness <<");
                    }
                }
                if (BeastMasterSettings.Instance.LB && IsTargetBoss())
                {
                    if (CastSpell("Lifeblood"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Lifeblood <<");
                    }
                }
                if (BeastMasterSettings.Instance.GE && IsTargetBoss() && StyxWoW.Me.Inventory.Equipped.Hands != null && StyxWoW.Me.Inventory.Equipped.Hands.Cooldown <= 0)
                {
                    Lua.DoString("RunMacroText('/use 10');");
                }
                if (BeastMasterSettings.Instance.T1 && IsTargetBoss() && StyxWoW.Me.Inventory.Equipped.Trinket1 != null && StyxWoW.Me.Inventory.Equipped.Trinket1.Cooldown <= 0)
                {
                    Lua.DoString("RunMacroText('/use 13');");
                }
                if (BeastMasterSettings.Instance.T2 && IsTargetBoss() && StyxWoW.Me.Inventory.Equipped.Trinket2 != null && StyxWoW.Me.Inventory.Equipped.Trinket2.Cooldown <= 0)
                {
                    Lua.DoString("RunMacroText('/use 14');");
                }
                //////////////////////////////////////////////////Racial Skills/////////////////////////////////////////////////////////////////////////////////////////
                if (BeastMasterSettings.Instance.RS && Me.Race == WoWRace.Troll && IsTargetBoss() && !SpellManager.Spells["Berserking"].Cooldown)
                {
                    Lua.DoString("RunMacroText('/Cast Berserking');");
                }
                if (BeastMasterSettings.Instance.RS && Me.Race == WoWRace.Orc && IsTargetBoss() && !SpellManager.Spells["Blood Fury"].Cooldown)
                {
                    Lua.DoString("RunMacroText('/Cast Blood Fury');");
                }
            }
            ///////////////////////////////////////////////Aspect Switching////////////////////////////////////////////////////////////////////////////////////////////
            if (BeastMasterSettings.Instance.AspectSwitching && HaltFeign() && Me.CurrentTarget != null && Me.CurrentTarget.IsAlive && !Me.Mounted)
            {
                if (BeastMasterSettings.Instance.TL2_AOTIH)
                {
                    if (!Me.IsMoving && !Me.HasAura("Aspect of the Iron Hawk"))
                    {
                        if (CastSpell("Aspect of the Hawk"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Not moving - Aspect of the Iron Hawk <<");
                        }
                    }
                    if (Me.IsMoving && Me.HasAura("Aspect of the Iron Hawk") && Me.CurrentFocus < 60)
                    {
                        if (CastSpell("Aspect of the Fox"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Moving - Aspect of the Fox <<");
                        }
                    }
                }
                if (!BeastMasterSettings.Instance.TL2_AOTIH)
                {
                    if (!Me.IsMoving && !Me.HasAura("Aspect of the Hawk"))
                    {
                        if (CastSpell("Aspect of the Hawk"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Not moving - Aspect of the Hawk <<");
                        }
                    }
                    if (Me.IsMoving && Me.HasAura("Aspect of the Hawk") && Me.CurrentFocus < 60)
                    {
                        if (CastSpell("Aspect of the Fox"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Moving - Aspect of the Fox <<");
                        }
                    }
                }
            }
            /////////////////////////////////////////////Beastmastery Rotation///////////////////////////////////////////////////////////////////////////////////////////
            if (Me.GotTarget && (addCount() < BeastMasterSettings.Instance.Mobs || (!BeastMasterSettings.Instance.MS && !BeastMasterSettings.Instance.TL))
                && HaltFeign() && Me.CurrentTarget.IsAlive && !Me.Mounted && !SelfControl(Me.CurrentTarget))
            {
                if (BeastMasterSettings.Instance.HM && Me.CurrentTarget.HealthPercent > 25 && !Me.CurrentTarget.HasAura("Hunter's Mark") && IsTargetEasyBoss())
                {
                    if (CastSpell("Hunter's Mark"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Hunter's Mark <<");
                    }
                }
                if (BeastMasterSettings.Instance.KSH && Me.CurrentTarget.HealthPercent <= 20)
                {
                    if (CastSpell("Kill Shot"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Kill Shot <<");
                    }
                }
                if (BeastMasterSettings.Instance.SerpentBox == "Always" && (!IsMyAuraActive(Me.CurrentTarget, "Serpent Sting") || MyDebuffTime("Serpent Sting", Me.CurrentTarget) < 1))
                {
                    if (CastSpell("Serpent Sting"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Serpent Sting <<");
                    }
                }
                if (BeastMasterSettings.Instance.SerpentBox == "Sometimes" && (!IsMyAuraActive(Me.CurrentTarget, "Serpent Sting") || MyDebuffTime("Serpent Sting", Me.CurrentTarget) < 1)
                    && (Me.CurrentTarget.CurrentHealth > 600000 || CalculateTimeToDeath(Me.CurrentTarget) > 15))
                {
                    if (CastSpell("Serpent Sting"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Serpent Sting <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL3_DB && ((Me.CurrentTarget.Level >= Me.Level && (Me.CurrentTarget.CurrentHealth > 90000 || CalculateTimeToDeath(Me.CurrentTarget) > 10))
                    || Me.CurrentFocus < 20) || Me.CurrentTarget.Name.Contains("Training Dummy"))
                {
                    if (CastSpell("Dire Beast"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Dire Beast <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL4_BSTRK && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 40)
                {
                    if (CastSpell("Blink Strike"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Blink Strike <<");
                    }
                }
                if (BeastMasterSettings.Instance.KCO && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25)
                {
                    if (CastSpell("Kill Command"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Kill Command <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL4_LR && (!BeastMasterSettings.Instance.BWR || SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 10)
                    && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 25 && Me.GotAlivePet 
                    && ((Me.CurrentTarget.MaxHealth > 500000 && (Me.CurrentTarget.CurrentHealth > 90000 || CalculateTimeToDeath(Me.CurrentTarget) > 4)) || Me.CurrentTarget.Name.Contains("Training Dummy")))
                {
                    if (CastSpell("Lynx Rush"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Lynx Rush <<");
                    }
                }
                if (Me.Level >= 90 && BeastMasterSettings.Instance.TL5_GLV && (!SpellManager.CanCast("Kill Command") || !BeastMasterSettings.Instance.KCO))
                {
                    if (CastSpell("Glaive Toss"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Glaive Toss <<");
                    }
                }
                if (Me.Level >= 90 && BeastMasterSettings.Instance.TL5_PWR && (!SpellManager.CanCast("Kill Command") || !BeastMasterSettings.Instance.KCO))
                {
                    if (CastSpell("Powershot"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Powershot <<");
                    }
                }
                if (Me.Level >= 90 && BeastMasterSettings.Instance.TL5_BRG && (!SpellManager.CanCast("Kill Command") || !BeastMasterSettings.Instance.KCO))
                {
                    if (CastSpell("Barrage"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Barrage <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL4_AMOC && !IsMyAuraActive(Me.CurrentTarget, "A Murder of Crows") && ((Me.CurrentTarget.MaxHealth > Me.MaxHealth * 2 && Me.CurrentTarget.HealthPercent > 20) 
                    || (Me.CurrentTarget.MaxHealth > Me.MaxHealth && Me.CurrentTarget.HealthPercent <= 20) || Me.CurrentTarget.Name.Contains("Training Dummy")))
                {
                    if (CastSpell("A Murder of Crows"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> A Murder of Crows <<");
                    }
                }
                if (BeastMasterSettings.Instance.FF && Me.GotAlivePet && Me.Pet.HasAura("Frenzy") && !Me.HasAura("The Beast Within") 
                    && ((SpellManager.Spells["Bestial Wrath"].Cooldown && SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 9) 
                    || (!SpellManager.Spells["Bestial Wrath"].Cooldown && (Me.CurrentTarget.MaxHealth <= 200000 || Me.HasAura("Rapid Fire")))))
                {
                    if (BeastMasterSettings.Instance.FFS == 5 && Me.Pet.Auras["Frenzy"].StackCount >= 5)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Focus Fire: 5 Stacks <<");
                        }
                    }
                    if (BeastMasterSettings.Instance.FFS == 4 && Me.Pet.Auras["Frenzy"].StackCount >= 4)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Focus Fire: 4 Stacks <<");
                        }
                    }
                    if (BeastMasterSettings.Instance.FFS == 3 && Me.Pet.Auras["Frenzy"].StackCount >= 3)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Focus Fire: 3 Stacks <<");
                        }
                    }
                    if (BeastMasterSettings.Instance.FFS == 2 && Me.Pet.Auras["Frenzy"].StackCount >= 2)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Focus Fire: 2 Stacks <<");
                        }
                    }
                    if (BeastMasterSettings.Instance.FFS == 1 && Me.Pet.Auras["Frenzy"].StackCount >= 1)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Focus Fire: 1 Stack <<");
                        }
                    }
                }
                if (BeastMasterSettings.Instance.FF && Me.Pet.HasAura("Frenzy") && Me.Pet.Auras["Frenzy"].StackCount >= 1 && DebuffTime("Frenzy", Me.Pet) < 2)
                {
                    if (CastSpell("Focus Fire"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Focus Fire: Running out of time <<");
                    }
                }
                if (BeastMasterSettings.Instance.ARC
                    && (!BeastMasterSettings.Instance.KSH || !SpellManager.CanCast("Kill Shot")) 
                    && (!BeastMasterSettings.Instance.TL5_GLV ||!SpellManager.CanCast("Glaive Toss"))
                    && (!BeastMasterSettings.Instance.TL5_BRG || !SpellManager.CanCast("Barrage"))
                    && (!BeastMasterSettings.Instance.TL5_PWR || !SpellManager.CanCast("Powershot")))
                {
                    if (BeastMasterSettings.Instance.KCO)
                    {
                        if (!Me.HasAura("Thrill of the Hunt"))
                        {
                            if (Me.GotAlivePet && SpellManager.Spells["Kill Command"].Cooldown && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalMilliseconds > 250)
                            {
                                if (!Me.HasAura("The Beast Within"))
                                {
                                    if ((Me.CurrentFocus > 60 && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds <= 2) 
                                        || (Me.CurrentFocus > 40 && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 2))
                                    {
                                        if (CastSpell("Arcane Shot"))
                                        {
                                            Logging.Write(Colors.Aquamarine, ">> Arcane Shot <<");
                                        }
                                    }
                                }
                                else if (Me.HasAura("The Beast Within"))
                                {
                                    if ((Me.CurrentFocus > 50 && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds <= 2)
                                        || (Me.CurrentFocus > 30 && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 2))
                                    {
                                        if (CastSpell("Arcane Shot"))
                                        {
                                            Logging.Write(Colors.Aquamarine, ">> Arcane Shot <<");
                                        }
                                    }
                                }
                            }
                            else if (!Me.GotAlivePet)
                            {
                                if (CastSpell("Arcane Shot"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Arcane Shot <<");
                                }
                            }
                        }
                        else if (Me.HasAura("Thrill of the Hunt"))
                        {
                            if (Me.GotAlivePet && SpellManager.Spells["Kill Command"].Cooldown && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalMilliseconds > 250)
                            { 
                                if (CastSpell("Arcane Shot"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Arcane Shot <<");
                                } 
                            }
                            else if (!Me.GotAlivePet)
                            {
                                if (CastSpell("Arcane Shot"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Arcane Shot <<");
                                }
                            }
                        }
                    }
                    else if (!BeastMasterSettings.Instance.KCO)
                    {
                        if (CastSpell("Arcane Shot"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Arcane Shot <<");
                        }
                    }
                }
                if (Me.CurrentFocus > 105 && Me.IsCasting && (Me.CastingSpell.Name == "Cobra Shot" || Me.CastingSpell.Name == "Steady Shot") && Me.CurrentCastTimeLeft.TotalMilliseconds > 700)
                {
                    SpellManager.StopCasting();
                    Logging.Write(Colors.Red, ">> Stop Cobra Shot <<");
                }
                if (!Me.IsCasting && (!BeastMasterSettings.Instance.KSH || !SpellManager.CanCast("Kill Shot")) 
                    && (!BeastMasterSettings.Instance.TL5_GLV ||!SpellManager.CanCast("Glaive Toss"))
                    && (!BeastMasterSettings.Instance.TL3_FV || !SpellManager.CanCast("Fervor"))
                    && (!BeastMasterSettings.Instance.TL5_BRG || !SpellManager.CanCast("Barrage"))
                    && (!BeastMasterSettings.Instance.TL5_PWR || !SpellManager.CanCast("Powershot")) 
                    && ((!Me.HasAura("The Beast Within") && (SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 1) || Me.CurrentFocus < 39)
                    || (Me.HasAura("The Beast Within") && (SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 1) || Me.CurrentFocus < 19)))
                {
                    if (Me.Level >= 81)
                    {
                        if ((Me.CurrentFocus < BeastMasterSettings.Instance.FocusShots || (Me.HasAura("The Beast Within") && Me.CurrentFocus < BeastMasterSettings.Instance.FocusShots / 2))
                            || (MyDebuffTime("Serpent Sting", Me.CurrentTarget) < 9 && Me.CurrentFocus < 60))
                        {
                            Lua.DoString("RunMacroText('/cast Cobra Shot');");
                            {
                                Logging.Write(Colors.Aquamarine, ">> Cobra Shot <<");
                            }
                        }
                    }
                    else if (Me.Level < 81)
                    {
                        if (Me.CurrentFocus < BeastMasterSettings.Instance.FocusShots || (Me.HasAura("The Beast Within") && Me.CurrentFocus < BeastMasterSettings.Instance.FocusShots / 2))
                        {
                            Lua.DoString("RunMacroText('/cast Steady Shot');");
                            {
                                Logging.Write(Colors.Aquamarine, ">> Steady Shot <<");
                            }
                        }
                    }
                }       
            }
            //////////////////////////////////////////////AoE Rotation here/////////////////////////////////////////////////////////////////////////////////////////////////
            if (addCount() >= BeastMasterSettings.Instance.Mobs && HaltFeign() && Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.Mounted && !SelfControl(Me.CurrentTarget) 
                && (BeastMasterSettings.Instance.MS || BeastMasterSettings.Instance.TL))
            {
                if (Me.CurrentTarget.Distance >= 5)
                {
                    if (BeastMasterSettings.Instance.TL && SpellManager.Spells["Explosive Trap"].CooldownTimeLeft.TotalSeconds < 1 && Me.CurrentTarget.InLineOfSight)
                    {
                        if (!Me.HasAura("Trap Launcher"))
                        {
                            if (CastSpell("Trap Launcher"))
                            {
                            Logging.Write(Colors.Crimson, ">> Trap Launcher Activated! <<");                    
                            }
                        }
                        else if (Me.HasAura("Trap Launcher"))
                        {
                            Lua.DoString("CastSpellByName('Explosive Trap');");
                            {
                                SpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
                                Logging.Write(Colors.Crimson, ">> Explosive Trap Launched! <<");
                            }
                        }
                    }
                }
                else if (Me.CurrentTarget.Distance < 5)
                {
                    if (BeastMasterSettings.Instance.TL && SpellManager.Spells["Explosive Trap"].CooldownTimeLeft.TotalSeconds < 1)
                    {
                        if (Me.HasAura("Trap Launcher"))
                        {
                            if (CastSpell("Trap Launcher"))
                            {
                                Logging.Write(Colors.Crimson, ">> Trap Launcher Deactivated! <<");
                            }
                        }

                        else if (!Me.HasAura("Trap Launcher"))
                        {
                            if (CastSpell("Explosive Trap"))
                            {
                                Logging.Write(Colors.Crimson, ">> Dropping Explosive Trap <<");
                            }
                        }
                    }              
                }
                if (BeastMasterSettings.Instance.FF && Me.GotAlivePet && Me.Pet.HasAura("Frenzy") && !Me.HasAura("The Beast Within")
                    && ((SpellManager.Spells["Bestial Wrath"].Cooldown && SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 9)
                    || (!SpellManager.Spells["Bestial Wrath"].Cooldown && (Me.CurrentTarget.MaxHealth <= 200000 || Me.HasAura("Rapid Fire")))))
                {
                    if (BeastMasterSettings.Instance.FFS == 5 && Me.Pet.Auras["Frenzy"].StackCount >= 5)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Crimson, ">> Focus Fire: 5 Stacks <<");
                        }
                    }
                    if (BeastMasterSettings.Instance.FFS == 4 && Me.Pet.Auras["Frenzy"].StackCount >= 4)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Crimson, ">> Focus Fire: 4 Stacks <<");
                        }
                    }
                    if (BeastMasterSettings.Instance.FFS == 3 && Me.Pet.Auras["Frenzy"].StackCount >= 3)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Crimson, ">> Focus Fire: 3 Stacks <<");
                        }
                    }
                    if (BeastMasterSettings.Instance.FFS == 2 && Me.Pet.Auras["Frenzy"].StackCount >= 2)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Crimson, ">> Focus Fire: 2 Stacks <<");
                        }
                    }
                    if (BeastMasterSettings.Instance.FFS == 1 && Me.Pet.Auras["Frenzy"].StackCount >= 1)
                    {
                        if (CastSpell("Focus Fire"))
                        {
                            Logging.Write(Colors.Crimson, ">> Focus Fire: 1 Stack <<");
                        }
                    }
                }
                if (BeastMasterSettings.Instance.FF && Me.Pet.HasAura("Frenzy") && Me.Pet.Auras["Frenzy"].StackCount >= 1 && DebuffTime("Frenzy", Me.Pet) < 2)
                {
                    if (CastSpell("Focus Fire"))
                    {
                        Logging.Write(Colors.Crimson, ">> Focus Fire: Running out of time <<");
                    }
                }
                if (Me.GotAlivePet && BeastMasterSettings.Instance.BWR && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && !Me.HasAura("The Beast Within"))
                {
                    if (CastSpell("Bestial Wrath"))
                    {
                        Logging.Write(Colors.Crimson, ">> Bestial Wrath, AoE <<");
                    }
                }
                if (BeastMasterSettings.Instance.KSH && Me.CurrentTarget.HealthPercent < 20 && (Me.CurrentFocus < 40 || (Me.HasAura("The Beast Within") && Me.CurrentFocus < 20)))
                {
                    if (CastSpell("Kill Shot"))
                    {
                        Logging.Write(Colors.Crimson, ">> Kill Shot <<");
                    }
                }
                if (Me.Level >= 90 && BeastMasterSettings.Instance.TL5_GLV)
                {
                    if (CastSpell("Glaive Toss"))
                    {
                        Logging.Write(Colors.Aquamarine, ">> Glaive Toss <<");
                    }
                }
                if (Me.CurrentPendingCursorSpell == null && BeastMasterSettings.Instance.MS && (!Me.Pet.HasAura("Beast Cleave") || DebuffTime("Beast Cleave", Me.Pet) < 1 
                    || Me.CurrentFocus > 70 || Me.HasAura("The Beast Within") || Me.HasAura("Thrill of the Hunt")))
                {
                    if (CastSpell("Multi-Shot"))
                    {
                        Logging.Write(Colors.Crimson, ">> Multi-Shot <<");
                    }
                }
                if (BeastMasterSettings.Instance.TL3_DB && BeastMasterSettings.Instance.AOEDB && Me.CurrentFocus < 80)
                {
                    if (CastSpell("Dire Beast"))
                    {
                        Logging.Write(Colors.Crimson, ">> Dire Beast, AoE <<");
                    }
                }
                if (BeastMasterSettings.Instance.AOELR && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && Me.CurrentFocus < 40 && !Me.HasAura("The Beast Within")
                    && ((Me.CurrentTarget.MaxHealth > 30000 && (Me.CurrentTarget.CurrentHealth > 90000 || CalculateTimeToDeath(Me.CurrentTarget) > 4)) || Me.CurrentTarget.Name.Contains("Training Dummy")))
                {
                    if (CastSpell("Lynx Rush"))
                    {
                        Logging.Write(Colors.Crimson, ">> Lynx Rush, AoE <<");
                    }
                }
                if (BeastMasterSettings.Instance.FSB && !WoWSpell.FromId(92380).Cooldown && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 10 && !Me.Pet.HasAura("Froststorm Breath"))
                {
                    Lua.DoString("RunMacroText('/cast Froststorm Breath');");
                    {
                        Logging.Write(Colors.Crimson, ">> Pet AoE: Froststorm Breath <<");
                    }
                }
                if (BeastMasterSettings.Instance.BRA && !WoWSpell.FromId(93433).Cooldown && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 5)
                {
                    Lua.DoString("RunMacroText('/cast Burrow Attack');");
                    {
                        Logging.Write(Colors.Crimson, ">> Pet AoE: Burrow Attack <<");
                    }
                }
                if (!SpellManager.CanCast("Kill Shot") && (Me.CurrentFocus < 40 || ((Me.HasAura("The Beast Within") || Me.HasAura("Thrill of the Hunt")) && Me.CurrentFocus < 20)
                    || (!Me.HasAura("The Beast Within") && !Me.HasAura("Thrill of the Hunt") && Me.CurrentFocus <= 68 && Me.Pet.HasAura("Beast Cleave") 
                    && DebuffTime("Beast Cleave", Me.Pet) > 1)))
                {
                    if (Me.Level >= 81)
                    {
                        Lua.DoString("RunMacroText('/cast Cobra Shot');");
                        {
                            Logging.Write(Colors.Crimson, ">> AoE Cobra Shot <<");
                        }
                    }
                    else if (Me.Level < 81)
                    {
                        Lua.DoString("RunMacroText('/cast Steady Shot');");
                        {
                            Logging.Write(Colors.Crimson, ">> AoE Steady Shot <<");
                        }
                    }
                }
            }
        }
        #endregion
    }
}