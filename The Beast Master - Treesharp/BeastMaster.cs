using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Styx.TreeSharp;
using Action = Styx.TreeSharp.Action;

namespace TheBeastMasterTree
{
    internal class BeastMasterTree : CombatRoutine
    {
        public override WoWClass Class { get { return WoWClass.Hunter; } }

        public static readonly Version Version = new Version(2, 1, 8);

        public override string Name { get { return "The Beast Master PvE " + Version; } }

        private static LocalPlayer Me { get { return StyxWoW.Me; } }

        public delegate WoWUnit UnitSelection(object unit);

        public static string safeName(WoWUnit unit)
        {
            if (unit != null)
            {
                return (unit.Name == Me.Name) ? "Myself" : unit.Name;
            }
            return "No Target";
        }

        #region Log

        public static void standardLog(string msg, params object[] args)
        {
            if (msg != null)
            {
                Logging.Write(LogLevel.Normal, Colors.Aquamarine, msg, args);
            }
        }

        private static void tslog(string msg, params object[] args) //use for troubleshoot logging
        {
            if (msg != null)
            {
                Logging.Write(LogLevel.Quiet, Colors.SeaGreen, msg, args);
            }
        }

        #endregion

        #region Settings

        public override bool WantButton { get { return true; } }

        public override void OnButtonPress()
        {
            standardLog("Configuration Opened");
            new BeastGUI().ShowDialog();
        }

        #endregion

        #region Initialize
        public override void Initialize()
        {
            tslog("Character Faction: {0}", Me.IsHorde ? "Horde" : "Allience");
            tslog("Character Level: {0}", Me.Level);
            tslog("Character Race: {0}", Me.Race);
            standardLog("");
            standardLog("You are using The Beast Master Combat Routine");
            standardLog("Version: " + Version);
            standardLog("Made by FallDown");
            standardLog("For LazyRaider only!");
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

        #region Target Checks

        private bool IsTargetBoss()
        {
            if (Me.CurrentTarget.Name.Contains("Dummy") || Me.CurrentTarget.CreatureRank == WoWUnitClassificationType.WorldBoss ||
               (Me.CurrentTarget.CreatureRank == WoWUnitClassificationType.Elite && Me.CurrentTarget.MaxHealth > Me.MaxHealth * 10))
                return true;

            else return false;
        }
        private bool IsTargetEasyBoss()
        {
            if (Me.CurrentTarget.Name.Contains("Dummy") || Me.CurrentTarget.CreatureRank == WoWUnitClassificationType.WorldBoss ||
               (Me.CurrentTarget.CreatureRank == WoWUnitClassificationType.Elite && Me.CurrentTarget.MaxHealth > Me.MaxHealth * 4))
                return true;

            else return false;
        }

        private bool validTarget(WoWUnit unit)
        {
            if (Me.GotTarget && unit.IsAlive && unit.Attackable && unit.CanSelect && !unit.IsFriendly)
                return true;
            else return false;
        }
        #endregion

        #region CastSpell Method

        public static bool canCast(string spellName, WoWUnit target)
        {
            if (SpellManager.HasSpell(spellName) && SpellManager.Spells[spellName].CooldownTimeLeft.TotalMilliseconds < 200 && Me.CurrentFocus >= SpellManager.Spells[spellName].PowerCost)
            {
                if (!BeastMasterSettings.Instance.TL5_BRG || Me.ChanneledCastingSpellId != 120360)
                {
                    SpellManager.Cast(spellName, target);
                    return true;
                }
            }
            return false;
        }

        public static Composite castSpell(string spellName, UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return (
                new Decorator(delegate(object a)
                {
                    if (!cond(a))
                        return false;
                    if (!canCast(spellName, onUnit(a)))
                        return false;
                    return onUnit(a) != null;
                },
                    new Sequence(
                        new Action(a => standardLog("[Casting] {0} on {1}", label, safeName(onUnit(a)))),
                        new Action(a => SpellManager.Cast(spellName, onUnit(a))))));
        }

        public static Composite castSpell(string spellName, CanRunDecoratorDelegate cond, string label)
        {
            return castSpell(spellName, ret => Me.CurrentTarget, cond, label);
        }

        public static Composite castTargetSpell(string spellName, WoWUnit target, CanRunDecoratorDelegate cond, string label)
        {
            return castSpell(spellName, ret => target, cond, label);
        }

        public static Composite castSelfSpell(string spellName, CanRunDecoratorDelegate cond, string label)
        {
            return castSpell(spellName, ret => Me, cond, label);
        }

        public static Composite castOnUnitLocation(string name, UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return (
                new Decorator(delegate(object a)
                {
                    if (!cond(a))
                        return false;
                    return onUnit != null && canCast(name, onUnit(a));
                },
                    new Sequence(
                    new Action(a => standardLog("[Casting at Location] {0} ", label)),
                    new Action(a => SpellManager.Cast(name)),
                    new Action(ret => SpellManager.ClickRemoteLocation(onUnit(ret).Location)))));
        }

        public static TimeSpan spellCooldown(string spell)
        {
            return SpellManager.HasSpell(spell) ? SpellManager.Spells[spell].CooldownTimeLeft : TimeSpan.MaxValue;
        }

        #endregion

        #region Use Items

        public static Composite UseEquippedItem(uint slot)
        {
            return new PrioritySelector(
                ctx => StyxWoW.Me.Inventory.GetItemBySlot(slot),
                new Decorator(
                    ctx => ctx != null && CanUseEquippedItem((WoWItem)ctx),
                    new Action(ctx => UseItem((WoWItem)ctx))));

        }

        private static bool CanUseEquippedItem(WoWItem item)
        {
            // Check for engineering tinkers!
            string itemSpell = Lua.GetReturnVal<string>("return GetItemSpell(" + item.Entry + ")", 0);
            if (string.IsNullOrEmpty(itemSpell))
                return false;

            return item.Usable && item.Cooldown <= 0;
        }

        private static void UseItem(WoWItem item)
        {
            standardLog("Using item: " + item.Name);
            item.Use();
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
                    && u.GotTarget
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

        #region Pet Management

        public static Stopwatch reviveTimer = new Stopwatch();

        public static Composite revivePet(UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return (
                new Decorator(delegate(object a)
                {
                    if (!cond(a))
                        return false;
                    if (!canCast("Revive Pet", onUnit(a)))
                        return false;
                    return onUnit(a) != null;
                },
                    new Sequence(
                        new Action(a => reviveTimer.Start()),
                        new Action(a => standardLog("[Casting] {0} on {1}", label, safeName(onUnit(a)))),
                        new Action(a => SpellManager.Cast("Revive Pet", onUnit(a))))));
        }

        public static Composite revivePet(CanRunDecoratorDelegate cond, string label)
        {
            return revivePet(ret => Me, cond, label);
        }

        public static Composite callPet(string spellName, UnitSelection onUnit, CanRunDecoratorDelegate cond, string label)
        {
            return (
                new Decorator(delegate(object a)
                {
                    if (!cond(a))
                        return false;
                    if (!canCast(spellName, onUnit(a)))
                        return false;
                    return onUnit(a) != null;
                },
                    new Sequence(
                        new Action(a => reviveTimer.Reset()),
                        new Action(a => standardLog("[Casting] {0} on {1}", label, safeName(onUnit(a)))),
                        new Action(a => SpellManager.Cast(spellName, onUnit(a))))));
        }

        public static Composite callPet(string spellName, CanRunDecoratorDelegate cond, string label)
        {
            return callPet(spellName, ret => Me, cond, label);
        }


        #endregion

        #region rest

        public override Composite RestBehavior
        {
            get
            {
                return (
                    new Decorator(ret => HaltFeign() && StyxWoW.IsInWorld && !Me.IsGhost && Me.IsAlive && !Me.Mounted && !Me.IsFlying && !Me.IsOnTransport,
                        new PrioritySelector(
                            new Decorator(ret => reviveTimer.ElapsedMilliseconds < 100,
                                revivePet(ret => BeastMasterSettings.Instance.RP && !Me.GotAlivePet && SpellManager.HasSpell("Revive Pet"), "Reviving Pet")),
                            new Decorator(ret => BeastMasterSettings.Instance.CP && Me.Pet == null && !Me.IsCasting,
                                new PrioritySelector(
                                    callPet("Call Pet 1", ret => BeastMasterSettings.Instance.PET == 1 && SpellManager.HasSpell("Call Pet 1"), "Calling Pet 1"),
                                    callPet("Call Pet 2", ret => BeastMasterSettings.Instance.PET == 2 && SpellManager.HasSpell("Call Pet 2"), "Calling Pet 2"),
                                    callPet("Call Pet 3", ret => BeastMasterSettings.Instance.PET == 3 && SpellManager.HasSpell("Call Pet 3"), "Calling Pet 3"),
                                    callPet("Call Pet 4", ret => BeastMasterSettings.Instance.PET == 4 && SpellManager.HasSpell("Call Pet 4"), "Calling Pet 4"),
                                    callPet("Call Pet 5", ret => BeastMasterSettings.Instance.PET == 5 && SpellManager.HasSpell("Call Pet 5"), "Calling Pet 5")
                                    )))));
            }
        }

        #endregion

        /*
        #region CombatStart

        private void AutoAttack()
        {
            if (!Me.IsAutoAttacking && Me.GotTarget && HaltFeign() && !SelfControl(Me.CurrentTarget))
            {
                Lua.DoString("StartAttack()");  
            }

        }
        #endregion
        */

        #region Combat

        public override Composite CombatBehavior
        {
            get
            {
                return (
                    new PrioritySelector(
                        new Decorator(ret => SelfControl(Me.CurrentTarget),
                                new Action(delegate
                                    {
                                        Lua.DoString("StopAttack()");
                                        SpellManager.StopCasting();
                                        standardLog("Stop everything!");
                                        return RunStatus.Failure;
                                    }
                                    )),
                        new Decorator(ret => validTarget(Me.CurrentTarget) && !Me.Mounted && HaltFeign(),
                            new Action(delegate
                            {
                                if (Ultra())
                                {
                                    Lua.DoString("RunMacroText('/click ExtraActionButton1');");
                                    SpellManager.StopCasting();
                                    {
                                        Logging.Write(Colors.Aquamarine, "Heroic Will!");
                                    }
                                }
                                if (UltraFL())
                                {
                                    Lua.DoString("RunMacroText('/click ExtraActionButton1');");
                                    SpellManager.StopCasting();
                                    {
                                        Logging.Write(Colors.Aquamarine, "Heroic Will!");
                                    }
                                }
                                if (DW())
                                {
                                    Lua.DoString("RunMacroText('/click ExtraActionButton1');");
                                    SpellManager.StopCasting();
                                    {
                                        Logging.Write(Colors.Aquamarine, "Enter the dream!");
                                    }
                                }
                                return RunStatus.Failure;
                            }
                                    )),
                        new Decorator(ret => validTarget(Me.CurrentTarget) && !Me.Mounted && HaltFeign(),
                            new PrioritySelector(
                                castSelfSpell("Mend Pet", ret => BeastMasterSettings.Instance.MP && Me.GotAlivePet && Me.Pet.HealthPercent <= BeastMasterSettings.Instance.MendHealth 
                                    && !IsMyAuraActive(Me.Pet, "Mend Pet"), "Mend Pet"),

                                castSelfSpell("Exhilaration", ret => BeastMasterSettings.Instance.TL2_EXH && (Me.HealthPercent < 70 || (Me.Pet.HealthPercent < 15 
                                && SpellManager.HasSpell("Heart of the Phoenix") && SpellManager.Spells["Heart of the Phoenix"].CooldownTimeLeft.TotalSeconds > 5)
                                || (Me.Pet.HealthPercent < 15 && !SpellManager.HasSpell("Heart of the Phoenix"))), "Exhilaration"),

                                new Decorator(ret => BeastMasterSettings.Instance.MDPet && Me.GotAlivePet && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !IsMyAuraActive(Me, "Misdirection")
                                                     && !WoWSpell.FromId(34477).Cooldown && !SpellManager.Spells["Misdirection"].Cooldown,
                                new Action(delegate
                                    {
                                        Lua.DoString("RunMacroText('/cast [@pet,exists] Misdirection');");
                                        Logging.Write(Colors.Aquamarine, "Misdirection on Pet");
                                        return RunStatus.Failure;
                                    }
                                    )),

                                new Decorator(ret => BeastMasterSettings.Instance.MDF && Me.FocusedUnit != null && Me.FocusedUnit.IsPlayer && Me.FocusedUnit != Me.Pet && !Me.HasAura("Misdirection")
                                                     && !WoWSpell.FromId(34477).Cooldown && !SpellManager.Spells["Misdirection"].Cooldown,
                                new Action(delegate
                                    {
                                    Lua.DoString("RunMacroText('/cast [@focus,exists] Misdirection');");
                                     Logging.Write(Colors.Aquamarine, "Misdirection on Focus");
                                    return RunStatus.Failure;
                                    }
                                    )),

                                castSpell("Distracting Shot", ret => BeastMasterSettings.Instance.DTS && Me.GotAlivePet && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !IsMyAuraActive(Me.CurrentTarget, "Distracting Shot"), "Distracting Shot"),

                                new Decorator(ret => Me.GotAlivePet && IsMyAuraActive(Me.CurrentTarget, "Scatter Shot"),
                                new Action(delegate
                                    {
                                        Lua.DoString("PetAttack()");
                                        Logging.Write(Colors.Aquamarine, "Attacking with pet");
                                        return RunStatus.Failure;
                                    }
                                    )),

                                castSpell("Kill Command", ret => BeastMasterSettings.Instance.KCO && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && IsMyAuraActive(Me.CurrentTarget, "Scatter Shot"), "Kill Command"),

                                new Decorator(ret => BeastMasterSettings.Instance.SMend && Me.CurrentHealth < Me.MaxHealth - 50000 && !WoWSpell.FromId(90361).Cooldown,
                                new Action(delegate
                                {
                                    Lua.DoString("RunMacroText(\"/cast [@" + Me.Name + "] Spirit Mend\")");
                                    Logging.Write(Colors.Aquamarine, "Pet: Spirit Mend");
                                    return RunStatus.Failure;
                                }
                                )),

                                castSelfSpell("Feign Death", ret => BeastMasterSettings.Instance.FDCBox == "1. High Threat" && Me.CurrentTarget.ThreatInfo.RawPercent > 90, "Feign Death"),

                                castSelfSpell("Feign Death", ret => BeastMasterSettings.Instance.FDCBox == "2. On Aggro" && Me.CurrentTarget.ThreatInfo.RawPercent > 90
                                && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.Distance < 10), "Feign Death"),

                                castSelfSpell("Feign Death", ret => BeastMasterSettings.Instance.FDCBox == "3. Low Health" && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.HealthPercent < 15, "Feign Death"),

                                castSelfSpell("Feign Death", ret => BeastMasterSettings.Instance.FDCBox == "1 + 3" && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && Me.HealthPercent < 15, "Feign Death"),

                                castSelfSpell("Feign Death", ret => BeastMasterSettings.Instance.FDCBox == "2 + 3" && Me.CurrentTarget.ThreatInfo.RawPercent > 90 && Me.HealthPercent < 15
                                && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && (Me.CurrentTarget.IsCasting || Me.CurrentTarget.Distance < 10), "Feign Death"),

                                castSelfSpell("Deterrence", ret => BeastMasterSettings.Instance.DETR && Me.HealthPercent < BeastMasterSettings.Instance.DetHealth && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && (Me.CurrentTarget.Distance < 10 || Me.CurrentTarget.IsCasting), "Deterrence")

                                )),
                        new Decorator(ret => validTarget(Me.CurrentTarget) && !Me.Mounted && HaltFeign() && !SelfControl(Me.CurrentTarget),
                            new PrioritySelector(
                                castSpell("Scatter Shot", ret => BeastMasterSettings.Instance.ScatterBox == "1. Interrupt" && Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast, "Scatter Shot"),

                                castSpell("Scatter Shot", ret => BeastMasterSettings.Instance.ScatterBox == "2. Defense" && Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid, "Scatter Shot"),

                                castSpell("Scatter Shot", ret => BeastMasterSettings.Instance.ScatterBox == "1 + 2" && (Me.CurrentTarget.Distance <= 20 && (Me.CurrentTarget.CurrentTargetGuid == Me.Guid || (Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast))), "Scatter Shot"),

                                castSpell("Silencing Shot", ret => BeastMasterSettings.Instance.TL1_SS && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast, "Silencing Shot"),

                                castSpell("Wyvern Sting", ret => BeastMasterSettings.Instance.TL1_WS && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast, "Wyvern Sting"),

                                castOnUnitLocation("Binding Shot", ret => Me.CurrentTarget, ret => BeastMasterSettings.Instance.TL1_BS && addCount() >= BeastMasterSettings.Instance.Mobs, "Binding Shot"),

                                castSpell("Concussive Shot", ret => BeastMasterSettings.Instance.CONC && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !Me.CurrentTarget.HasAura("Concussive Shot") && Me.CurrentTarget.Distance < 25, "Concussive Shot"),

                                castSpell("Intimidation", ret => BeastMasterSettings.Instance.IntimidateBox == "1. Interrupt" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 
                                && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && !SpellManager.Spells["Intimidation"].Cooldown, "Intimidation"),

                                castSpell("Intimidation", ret => BeastMasterSettings.Instance.IntimidateBox == "2. Defense" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 
                                && Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid, "Intimidation"),

                                castSpell("Intimidation", ret => BeastMasterSettings.Instance.IntimidateBox == "1 + 2" && ((Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid) 
                                || (Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && !SpellManager.Spells["Intimidation"].Cooldown)), "Intimidation"),

                                castSpell("Fervor", ret => BeastMasterSettings.Instance.TL3_FV && (Me.CurrentFocus < 60 || (Me.HasAura("The Beast within") && Me.CurrentFocus < 40)), "Fervor"),

                                castSpell("Rapid Fire", ret => BeastMasterSettings.Instance.RF && (SpellManager.Spells["Bestial Wrath"].Cooldown || !BeastMasterSettings.Instance.BWR) 
                                && !Me.HasAura("Rapid Fire") && !Me.HasAura("The Beast Within") 
                                && !Me.HasAura("Bloodlust") && !Me.HasAura("Heroism") 
                                && !Me.HasAura("Ancient Hysteria") && !Me.HasAura("Time Warp") 
                                && (Me.CurrentTarget.CurrentHealth > 400000 || CalculateTimeToDeath(Me.CurrentTarget) > 14) && (IsTargetBoss() || Me.CurrentTarget.Name.Contains("Training Dummy")), "Rapid Fire"),

                                castSpell("Stampede", ret => BeastMasterSettings.Instance.CW && IsTargetBoss() && (Me.CurrentTarget.CurrentHealth > 1000000 || CalculateTimeToDeath(Me.CurrentTarget) > 20), "Stampede"),

                                castSelfSpell("Bestial Wrath", ret => Me.GotAlivePet && BeastMasterSettings.Instance.BWR && (!BeastMasterSettings.Instance.TL4_LR || SpellManager.Spells["Lynx Rush"].CooldownTimeLeft.TotalSeconds > 10 
                                || !SpellManager.Spells["Lynx Rush"].Cooldown) && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && !Me.HasAura("Rapid Fire") 
                                && !Me.HasAura("The Beast Within") && !Me.HasAura("Bloodlust") && !Me.HasAura("Heroism")
                                && !Me.HasAura("Ancient Hysteria") && !Me.HasAura("Time Warp") && (Me.CurrentTarget.CurrentHealth > 100000 
                                || CalculateTimeToDeath(Me.CurrentTarget) > 8) && (Me.CurrentTarget.MaxHealth > 300000 || Me.CurrentTarget.Name.Contains("Training Dummy")), "Bestial Wrath"),

                                castSelfSpell("Readiness", ret => BeastMasterSettings.Instance.RDN 
                                && SpellManager.Spells["Rapid Fire"].CooldownTimeLeft.TotalSeconds > 2 
                                && SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 2
                                && (SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 2 || !BeastMasterSettings.Instance.KCO)
                                && (SpellManager.HasSpell("Lynx Rush") && SpellManager.Spells["Lynx Rush"].CooldownTimeLeft.TotalSeconds > 2 || !BeastMasterSettings.Instance.TL4_LR)
                                && (SpellManager.HasSpell("Glaive Toss") && SpellManager.Spells["Glaive Toss"].CooldownTimeLeft.TotalSeconds > 2 || !BeastMasterSettings.Instance.TL5_GLV)
                                && (SpellManager.HasSpell("Powershot") && SpellManager.Spells["Powershot"].CooldownTimeLeft.TotalSeconds > 2 || !BeastMasterSettings.Instance.TL5_PWR)
                                && (SpellManager.HasSpell("Barrage") && SpellManager.Spells["Barrage"].CooldownTimeLeft.TotalSeconds > 2 || !BeastMasterSettings.Instance.TL5_BRG)
                                && (SpellManager.HasSpell("Fervor") && SpellManager.Spells["Fervor"].CooldownTimeLeft.TotalSeconds > 2 || !BeastMasterSettings.Instance.TL3_FV)
                                && (SpellManager.HasSpell("A Murder of Crows") && SpellManager.Spells["A Murder of Crows"].CooldownTimeLeft.TotalSeconds > 2 || !BeastMasterSettings.Instance.TL4_AMOC)
                                && (SpellManager.HasSpell("Blink Strike") && SpellManager.Spells["Blink Strike"].CooldownTimeLeft.TotalSeconds > 2 || !BeastMasterSettings.Instance.TL4_BSTRK)
                                && (SpellManager.HasSpell("Dire Beast") && SpellManager.Spells["Dire Beast"].CooldownTimeLeft.TotalSeconds > 2 || !BeastMasterSettings.Instance.TL3_DB), "Readiness"),

                                castSelfSpell("Lifeblood", ret => BeastMasterSettings.Instance.LB && IsTargetEasyBoss(), "Lifeblood"),
                                

                                new Decorator(ret => BeastMasterSettings.Instance.GE && IsTargetBoss(),
                                UseEquippedItem(9)),

                                new Decorator(ret => BeastMasterSettings.Instance.T1 && IsTargetBoss(),
                                UseEquippedItem(12)),

                                new Decorator(ret => BeastMasterSettings.Instance.T2 && IsTargetBoss(),
                                UseEquippedItem(13)),

                                new Decorator(ret => BeastMasterSettings.Instance.RS && Me.Race == WoWRace.Troll && IsTargetBoss() && !SpellManager.Spells["Berserking"].Cooldown,
                                new Action(delegate
                                {
                                    Lua.DoString("RunMacroText('/Cast Berserking');");
                                    return RunStatus.Failure;
                                }
                                )),

                                new Decorator(ret => BeastMasterSettings.Instance.RS && Me.Race == WoWRace.Orc && IsTargetBoss() && !SpellManager.Spells["Blood Fury"].Cooldown,
                                new Action(delegate
                                {
                                    Lua.DoString("RunMacroText('/Cast Blood Fury');");
                                    return RunStatus.Failure;
                                }
                                ))
                            )
                        ),
                        ////////////////////////////// ASPECT SWITCHING /////////////////////////////////
                        new Decorator(ret => BeastMasterSettings.Instance.AspectSwitching && HaltFeign() && Me.CurrentTarget != null && Me.CurrentTarget.IsAlive && !Me.Mounted,
                            new PrioritySelector(
                                castSelfSpell("Aspect of the Hawk", ret => !Me.IsMoving && !Me.HasAura("Aspect of the Iron Hawk") && !Me.HasAura("Aspect of the Hawk"), "Aspect of the Hawk"),
                                castSelfSpell("Aspect of the Fox", ret => Me.IsMoving && !Me.HasAura("Aspect of the Fox") && Me.CurrentFocus < BeastMasterSettings.Instance.FocusShots, "Aspect of the Fox")   
                            )
                        ),
                        //////////////////////////////// SINGLE TARGET ROTATION ////////////////////////////////////
                        new Decorator(ret => validTarget(Me.CurrentTarget) && (addCount() < BeastMasterSettings.Instance.Mobs || (!BeastMasterSettings.Instance.MS && !BeastMasterSettings.Instance.TL))
                                             && HaltFeign() && !Me.Mounted && !SelfControl(Me.CurrentTarget),
                            new PrioritySelector(
                                castSpell("Hunter's Mark", ret => BeastMasterSettings.Instance.HM && Me.CurrentTarget.HealthPercent > 25 && !Me.CurrentTarget.HasAura("Hunter's Mark") && IsTargetEasyBoss(), "Hunter's Mark"),

                                castSpell("Kill Shot", ret => BeastMasterSettings.Instance.KSH && Me.CurrentTarget.HealthPercent <= 20, "Kill Shot"),

                                castSpell("Serpent Sting", ret => BeastMasterSettings.Instance.SerpentBox == "Always" && (!IsMyAuraActive(Me.CurrentTarget, "Serpent Sting") || MyDebuffTime("Serpent Sting", Me.CurrentTarget) < 1), "Serpent Sting"),

                                castSpell("Serpent Sting", ret => BeastMasterSettings.Instance.SerpentBox == "Sometimes" && (!IsMyAuraActive(Me.CurrentTarget, "Serpent Sting") || MyDebuffTime("Serpent Sting", Me.CurrentTarget) < 1)
                                                                  && (Me.CurrentTarget.CurrentHealth > 600000 || CalculateTimeToDeath(Me.CurrentTarget) >= 15), "Serpent Sting"),

                                castSpell("Dire Beast", ret => BeastMasterSettings.Instance.TL3_DB && ((Me.CurrentTarget.Level >= Me.Level && (Me.CurrentTarget.CurrentHealth > 90000 || CalculateTimeToDeath(Me.CurrentTarget) > 10))
                                                               || Me.CurrentFocus < 20) || Me.CurrentTarget.Name.Contains("Training Dummy"), "Dire Beast"),

                                castSpell("Blink Strike", ret => BeastMasterSettings.Instance.TL4_BSTRK && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 40, "Blink Strike"),

                                castSpell("Kill Command", ret => BeastMasterSettings.Instance.KCO && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25, "Kill Command"),

                                castSpell("Lynx Rush", ret => BeastMasterSettings.Instance.TL4_LR && Me.GotAlivePet && (!BeastMasterSettings.Instance.BWR || SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 10)
                                && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 25 && ((Me.CurrentTarget.MaxHealth > 400000 
                                && (Me.CurrentTarget.CurrentHealth > 90000 || CalculateTimeToDeath(Me.CurrentTarget) > 4)) || Me.CurrentTarget.Name.Contains("Training Dummy")), "Lynx Rush"),

                                castSpell("Glaive Toss", ret => BeastMasterSettings.Instance.TL5_GLV, "Glaive Toss"),

                                castSpell("Powershot", ret => BeastMasterSettings.Instance.TL5_PWR, "Powershot"),

                                castSpell("Barrage", ret => BeastMasterSettings.Instance.TL5_BRG, "Barrage"),

                                castSpell("A Murder of Crows", ret => BeastMasterSettings.Instance.TL4_AMOC && !IsMyAuraActive(Me.CurrentTarget, "A Murder of Crows") && ((Me.CurrentTarget.MaxHealth > Me.MaxHealth * 2 && Me.CurrentTarget.HealthPercent > 20) 
                                || (Me.CurrentTarget.MaxHealth > Me.MaxHealth && Me.CurrentTarget.HealthPercent <= 20) || Me.CurrentTarget.Name.Contains("Training Dummy")), "A Murder of Crows"),

                                new Decorator(ret => BeastMasterSettings.Instance.FF && Me.GotAlivePet && Me.Pet.HasAura("Frenzy") && !Me.HasAura("The Beast Within")
                                              && ((SpellManager.Spells["Bestial Wrath"].Cooldown && SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 9)
                                              || (!SpellManager.Spells["Bestial Wrath"].Cooldown && (Me.CurrentTarget.MaxHealth <= 200000 || Me.HasAura("Rapid Fire")))),
                                    new PrioritySelector(
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 1 && Me.Pet.Auras["Frenzy"].StackCount >= 1, "Focus Fire: 1 Stack"),
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 2 && Me.Pet.Auras["Frenzy"].StackCount >= 2, "Focus Fire: 2 Stacks"),
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 3 && Me.Pet.Auras["Frenzy"].StackCount >= 3, "Focus Fire: 3 Stacks"),
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 4 && Me.Pet.Auras["Frenzy"].StackCount >= 4, "Focus Fire: 4 Stacks"),
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 5 && Me.Pet.Auras["Frenzy"].StackCount >= 5, "Focus Fire: 5 Stacks")
                                    )
                                ),

                                castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FF && Me.GotAlivePet && Me.Pet.HasAura("Frenzy") && Me.Pet.Auras["Frenzy"].StackCount >= 1 && DebuffTime("Frenzy", Me.Pet) < 2, " Focus Fire: Running out of time"),

                                new Decorator(ret => Me.CurrentTarget.Distance >= 5 && BeastMasterSettings.Instance.ST_ET && !Me.CurrentTarget.IsMoving && Me.CurrentTarget.CurrentTargetGuid != Me.Guid && SpellManager.Spells["Explosive Trap"].CooldownTimeLeft.TotalSeconds < 1 && Me.CurrentTarget.InLineOfSight,
                                    new PrioritySelector(
                                        castSelfSpell("Trap Launcher", ret => !Me.HasAura("Trap Launcher"), "Trap Launcher Activated"),
                                        castOnUnitLocation("Explosive Trap", ret => Me.CurrentTarget, ret => Me.HasAura("Trap Launcher"), "Explosive Trap Launched")
                                    )
                                ),
                                new Decorator(ret => Me.CurrentTarget.Distance < 5 && BeastMasterSettings.Instance.ST_ET && !Me.CurrentTarget.IsMoving && Me.CurrentTarget.CurrentTargetGuid != Me.Guid && !SpellManager.Spells["Explosive Trap"].Cooldown,
                                    new PrioritySelector(
                                        castSelfSpell("Trap Launcher", ret => Me.HasAura("Trap Launcher"), "Trap Launcher Deactivated"),
                                        castSpell("Explosive Trap", ret => !Me.HasAura("Trap Launcher"), "Explosive Trap Dropped")
                                    )
                                ),

                                new Decorator(ret => BeastMasterSettings.Instance.ARC && (!BeastMasterSettings.Instance.TL5_GLV || SpellManager.Spells["Glaive Toss"].Cooldown),
                                    new PrioritySelector(
                                        new Decorator(ret => BeastMasterSettings.Instance.KCO,
                                            new PrioritySelector(
                                                new Decorator(ret => !Me.HasAura("Thrill of the Hunt"),
                                                    new PrioritySelector(
                                                        new Decorator(ret => Me.GotAlivePet && SpellManager.Spells["Kill Command"].Cooldown && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalMilliseconds > 300,
                                                            new PrioritySelector(
                                                                new Decorator(ret => !Me.HasAura("The Beast Within"),
                                                                    new PrioritySelector(
                                                                        castSpell("Arcane Shot", ret => (Me.CurrentFocus > 60 && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds <= 2) 
                                                                        || (Me.CurrentFocus > 40 && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 2), "Arcane Shot")
                                                                    )
                                                                ),
                                                                new Decorator(ret => Me.HasAura("The Beast Within"),
                                                                    new PrioritySelector(
                                                                        castSpell("Arcane Shot", ret => (Me.CurrentFocus > 50 && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds <= 2) 
                                                                        || (Me.CurrentFocus > 30 && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 2), "Arcane Shot")
                                                                    )
                                                                )
                                                            )
                                                        )
                                                    )
                                                ),
                                                new Decorator(ret => Me.HasAura("Thrill of the Hunt"),
                                                    new PrioritySelector(
                                                        castSpell("Arcane Shot", ret => Me.GotAlivePet && SpellManager.Spells["Kill Command"].Cooldown && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalMilliseconds > 300, "Arcane Shot")
                                                    )
                                                )
                                            )
                                        ),
                                        castSpell("Arcane Shot", ret => !BeastMasterSettings.Instance.KCO || !Me.GotAlivePet, "Arcane Shot")
                                    )
                                ),

                                new Decorator(ret => Me.CurrentFocus > 105 && Me.IsCasting && (Me.CastingSpell.Name == "Cobra Shot" || Me.CastingSpell.Name == "Steady Shot") && Me.CurrentCastTimeLeft.TotalMilliseconds > 700,
                                new Action(delegate
                                {
                                    SpellManager.StopCasting();
                                    Logging.Write(Colors.Red, "Stopping Cobra Shot");
                                    return RunStatus.Failure;
                                }
                                )),

                                new Decorator(ret => !Me.IsCasting && (!BeastMasterSettings.Instance.TL3_FV || SpellManager.Spells["Fervor"].Cooldown) && ((!Me.HasAura("The Beast Within") && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 1) || Me.CurrentFocus < 40)
                                || ((Me.HasAura("The Beast Within") && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 1) || Me.CurrentFocus < 20),
                                    new PrioritySelector(
                                        castSpell("Steady Shot", ret => Me.CurrentFocus < BeastMasterSettings.Instance.FocusShots || (Me.HasAura("The Beast Within") && Me.CurrentFocus < BeastMasterSettings.Instance.FocusShots / 2), "Cobra Shot")
                                    )
                                ) 
                            )                     
                        ),
                        /////////////////////////////////// AOE ROTATION ///////////////////////////////
                        new Decorator(ret => addCount() >= BeastMasterSettings.Instance.Mobs && HaltFeign() && validTarget(Me.CurrentTarget) && !Me.Mounted && !SelfControl(Me.CurrentTarget) 
                                      && (BeastMasterSettings.Instance.MS || BeastMasterSettings.Instance.TL),
                            new PrioritySelector(
                                new Decorator(ret => Me.CurrentTarget.Distance >= 5 && BeastMasterSettings.Instance.TL && SpellManager.Spells["Explosive Trap"].CooldownTimeLeft.TotalSeconds < 1 && Me.CurrentTarget.InLineOfSight,
                                    new PrioritySelector(
                                        castSelfSpell("Trap Launcher", ret => !Me.HasAura("Trap Launcher"), "Trap Launcher Activated"),
                                        castOnUnitLocation("Explosive Trap", ret => Me.CurrentTarget, ret => Me.HasAura("Trap Launcher"), "Explosive Trap Launched")
                                    )
                                ),
                                new Decorator(ret => Me.CurrentTarget.Distance < 5 && BeastMasterSettings.Instance.TL && !SpellManager.Spells["Explosive Trap"].Cooldown,
                                    new PrioritySelector(
                                        castSelfSpell("Trap Launcher", ret => Me.HasAura("Trap Launcher"), "Trap Launcher Deactivated"),
                                        castSpell("Explosive Trap", ret => !Me.HasAura("Trap Launcher"), "Explosive Trap Dropped")
                                    )
                                ),

                                new Decorator(ret => BeastMasterSettings.Instance.FF && Me.GotAlivePet && Me.Pet.HasAura("Frenzy") && !Me.HasAura("The Beast Within")
                                              && ((SpellManager.Spells["Bestial Wrath"].Cooldown && SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 9)
                                              || (!SpellManager.Spells["Bestial Wrath"].Cooldown && (Me.CurrentTarget.MaxHealth <= 200000 || Me.HasAura("Rapid Fire")))),
                                    new PrioritySelector(
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 1 && Me.Pet.Auras["Frenzy"].StackCount >= 1, "Focus Fire: 1 Stack"),
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 2 && Me.Pet.Auras["Frenzy"].StackCount >= 2, "Focus Fire: 2 Stacks"),
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 3 && Me.Pet.Auras["Frenzy"].StackCount >= 3, "Focus Fire: 3 Stacks"),
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 4 && Me.Pet.Auras["Frenzy"].StackCount >= 4, "Focus Fire: 4 Stacks"),
                                        castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FFS == 5 && Me.Pet.Auras["Frenzy"].StackCount >= 5, "Focus Fire: 5 Stacks")
                                    )
                                ),

                                castSpell("Focus Fire", ret => BeastMasterSettings.Instance.FF && Me.Pet.HasAura("Frenzy") && Me.Pet.Auras["Frenzy"].StackCount >= 1 && DebuffTime("Frenzy", Me.Pet) < 2, "Focus Fire: Running out of time"),

                                castSpell("Bestial Wrath", ret => Me.GotAlivePet && BeastMasterSettings.Instance.BWR && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && !Me.HasAura("The Beast Within"), "Bestial Wrath, AoE"),
                                
                                castSpell("Kill Shot", ret => BeastMasterSettings.Instance.KSH && Me.CurrentTarget.HealthPercent < 20 && (Me.CurrentFocus < 40 || (Me.HasAura("The Beast Within") && Me.CurrentFocus < 20)), "Kill Shot"),

                                castSpell("Glaive Toss", ret => BeastMasterSettings.Instance.TL5_GLV, "Glaive Toss"),

                                castSpell("Multi-Shot", ret => BeastMasterSettings.Instance.MS && (!Me.Pet.HasAura("Beast Cleave") || MyDebuffTime("Beast Cleave", Me.Pet) < 1 
                                || Me.CurrentFocus >= 70 || Me.HasAura("The Beast Within") || Me.HasAura("Thrill of the Hunt") || (BeastMasterSettings.Instance.TL3_FV && !SpellManager.Spells["Fervor"].Cooldown)), "Multi-Shot"),

                                castSpell("Dire Beast", ret => BeastMasterSettings.Instance.AOEDB && Me.CurrentFocus < 80, "Dire Beast, AoE"),

                                 castSpell("Blink Strike", ret => BeastMasterSettings.Instance.TL4_BSTRK && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 40 
                                                           && (Me.CurrentFocus < 20 || (!Me.HasAura("The Beast Within") && Me.CurrentFocus < 40)), "Blink Strike"),
                                
                                castSpell("Lynx Rush", ret => BeastMasterSettings.Instance.AOELR && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && Me.CurrentFocus < 40 && !Me.HasAura("The Beast Within")
                                && ((Me.CurrentTarget.MaxHealth > 30000 && (Me.CurrentTarget.CurrentHealth > 90000 || CalculateTimeToDeath(Me.CurrentTarget) > 4)) || Me.CurrentTarget.Name.Contains("Training Dummy")), "Lynx Rush, AoE"),

                                new Decorator(ret => BeastMasterSettings.Instance.FSB && !WoWSpell.FromId(92380).Cooldown && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 10 && !Me.Pet.HasAura("Froststorm Breath"),
                                new Action(delegate
                                {
                                    Lua.DoString("RunMacroText('/cast Froststorm Breath');");
                                    Logging.Write(Colors.Crimson, "Pet AoE: Froststorm Breath");
                                }
                                )),

                                new Decorator(ret => BeastMasterSettings.Instance.BRA && !WoWSpell.FromId(93433).Cooldown && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 5,
                                new Action(delegate
                                {
                                    Lua.DoString("RunMacroText('/cast Burrow Attack');");
                                    Logging.Write(Colors.Crimson, "Pet AoE: Burrow Attack");
                                }
                                )),

                                new Decorator(ret => ((!Me.HasAura("The Beast Within") && !Me.HasAura("Thrill of the Hunt")) && Me.CurrentFocus < 40) || Me.CurrentFocus < 20 || (!Me.HasAura("The Beast Within")
                                    && !Me.HasAura("Thrill of the Hunt") && Me.CurrentFocus < 70 && Me.Pet.HasAura("Beast Cleave") && MyDebuffTime("Beast Cleave", Me.Pet) > 1),
                                    new PrioritySelector(
                                        castSpell("Steady Shot", ret => !BeastMasterSettings.Instance.TL3_FV || SpellManager.Spells["Fervor"].Cooldown, "Cobra Shot")
                                    )
                                )
                            )
                        )
                    )
                );
            }
        }
        #endregion
    }
}