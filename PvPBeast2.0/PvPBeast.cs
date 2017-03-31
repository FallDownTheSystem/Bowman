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

namespace PvPBeast
{
    internal class PvPBeast : CombatRoutine
    {
        public override WoWClass Class { get { return WoWClass.Hunter; } }

        public static readonly Version Version = new Version(2, 0, 7);

        public override string Name { get { return "PvPBeast " + Version + " MoP Edition"; } }

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
            new PvPBeastGUI().ShowDialog();
        }

        #endregion

        #region Initialize
        public override void Initialize()
        {
            tslog("Character Faction: {0}", Me.IsHorde ? "Horde" : "Allience");
            tslog("Character Level: {0}", Me.Level);
            tslog("Character Race: {0}", Me.Race);
            slog("");
            slog("You are using PvPBeast Combat Routine");
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

        #region Invulnerable
        public bool Invulnerable(WoWUnit unit)
        {
            if (unit.HasAura("Cyclone") || unit.HasAura("Dispersion") || unit.HasAura("Ice Block") || unit.HasAura("Deterrence") || unit.HasAura("Divine Shield") ||
                unit.HasAura("Hand of Protection") || (unit.HasAura("Anti-Magic Shell") && unit.HasAura("Icebound Fortitude")))
                return true;

            else return false;
        }
        #endregion

        #region Dumb Bear
        public bool DumbBear(WoWUnit unit)
        {
            if (unit.Class == WoWClass.Druid && unit.HasAura("Bear Form") && unit.HasAura("Frenzied Regeneration") && unit.HealthPercent > 5 && unit.Distance > 8)
                return true;

            else return false;
        }
        #endregion

        #region Class type
        public bool MeleeClass(WoWUnit unit)
        {
            if (Me.GotTarget && (unit.Class == WoWClass.Rogue || unit.Class == WoWClass.Monk || unit.Class == WoWClass.Warrior || unit.Class == WoWClass.DeathKnight ||
                (unit.Class == WoWClass.Paladin && unit.MaxMana < 90000) ||
                (unit.Class == WoWClass.Druid && (unit.HasAura("Cat Form") || unit.HasAura("Bear Form")))))
                return true;

            else return false;
        }
        public bool RangedClass(WoWUnit unit)
        {
            if (Me.GotTarget && (unit.Class == WoWClass.Hunter || unit.Class == WoWClass.Shaman || unit.Class == WoWClass.Priest ||
                unit.Class == WoWClass.Mage || unit.Class == WoWClass.Warlock || (unit.Class == WoWClass.Paladin && unit.MaxMana >= 90000) ||
                (unit.Class == WoWClass.Druid && !unit.HasAura("Cat Form") && !unit.HasAura("Bear Form"))))
                return true;

            else return false;
        }
        public bool HealerClass(WoWUnit unit)
        {
            if (Me.GotTarget && (unit.Class == WoWClass.Shaman || (unit.Class == WoWClass.Priest && !unit.HasAura("Shadow Form")) ||
                (unit.Class == WoWClass.Paladin && unit.MaxMana >= 90000) || (unit.Class == WoWClass.Druid && !unit.HasAura("Cat Form") && !unit.HasAura("Bear Form") && !unit.HasAura("Boomkin Form"))))
                return true;

            else return false;
        }

        #endregion

        #region SelfControl
        public bool SelfControl(WoWUnit unit)
        {
            if (Me.GotTarget && (unit.HasAura("Freezing Trap") || unit.HasAura("Wyvern Sting") || unit.HasAura("Scatter Shot") || unit.HasAura("Bad Manner")))
                return true;

            else return false;
        }
        #endregion

        #region Alive Hostile Enemy
        public bool HostilePlayer(WoWUnit unit)
        {
            if (Me.GotTarget && unit.IsPlayer && unit.IsHostile && unit.IsAlive)
                return true;

            else return false;
        }

        public bool HostileNPC(WoWUnit unit)
        {
            if (unit.IsHostile && unit.IsAlive && !unit.IsPlayer)
                return true;

            else return false;
        }
        #endregion

        #region CastSpell Method
        public static bool CastSpell(string SpellName, WoWUnit target)
        {
            if (SpellManager.HasSpell(SpellName) && SpellManager.Spells[SpellName].CooldownTimeLeft.TotalMilliseconds < 200 && Me.CurrentFocus >= SpellManager.Spells[SpellName].PowerCost)
            {
                if (!PvPBeastSettings.Instance.TL5_BRG || Me.ChanneledCastingSpellId != 120360)
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
                if (!PvPBeastSettings.Instance.TL5_BRG || Me.ChanneledCastingSpellId != 120360)
                {
                    SpellManager.Cast(SpellName);
                    return true;
                }
            }
            return false;
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

        // Big thanks and credit to ZenLulz for all the movement imparement related code.
        #region Movement Imparement
        private static List<WoWSpellMechanic> controlMechanic = new List<WoWSpellMechanic>()
        {
            WoWSpellMechanic.Charmed,
            WoWSpellMechanic.Disoriented,
            WoWSpellMechanic.Fleeing,
            WoWSpellMechanic.Frozen,
            WoWSpellMechanic.Incapacitated,
            WoWSpellMechanic.Polymorphed,
            WoWSpellMechanic.Sapped
        };
        private static List<WoWSpellMechanic> slowMechanic = new List<WoWSpellMechanic>()
        {
            WoWSpellMechanic.Dazed,
            WoWSpellMechanic.Shackled,
            WoWSpellMechanic.Slowed,
            WoWSpellMechanic.Snared
        };
        private static List<WoWSpellMechanic> rootMechanic = new List<WoWSpellMechanic>()
        {
            WoWSpellMechanic.Rooted
        };
        private static List<WoWSpellMechanic> stunMechanic = new List<WoWSpellMechanic>()
        {
            WoWSpellMechanic.Stunned,
            WoWSpellMechanic.Frozen,
            WoWSpellMechanic.Fleeing,
            WoWSpellMechanic.Horrified
        };
        private static List<WoWSpellMechanic> forsakenMechanic = new List<WoWSpellMechanic>()
        {
            WoWSpellMechanic.Asleep,
            WoWSpellMechanic.Horrified,
            WoWSpellMechanic.Fleeing,
            WoWSpellMechanic.Charmed
        };

        public static TimeSpan isForsaken(WoWUnit unit)
        {
            TimeSpan tsTimeRemaining = new TimeSpan(0, 0, 0);
            foreach (WoWAura aura in unit.Auras.Values)
            {
                if (forsakenMechanic.Contains(WoWSpell.FromId(aura.SpellId).Mechanic))
                {
                    if (tsTimeRemaining < aura.TimeLeft)
                        tsTimeRemaining = aura.TimeLeft;
                }
            }
            return tsTimeRemaining;
        }

        public static TimeSpan isStunned(WoWUnit unit)
        {
            TimeSpan tsTimeRemaining = new TimeSpan(0, 0, 0);
            foreach (WoWAura aura in unit.Auras.Values)
            {
                if (stunMechanic.Contains(WoWSpell.FromId(aura.SpellId).Mechanic))
                {
                    if (tsTimeRemaining < aura.TimeLeft)
                        tsTimeRemaining = aura.TimeLeft;
                }
            }
            return tsTimeRemaining;
        }

        public static bool isSlowed(WoWUnit unit)
        {
            if (unit.MovementInfo.RunSpeed < 4.5)
                return true;
            else
                return false;
        }

        public static TimeSpan isControlled(WoWUnit unit)
        {
            TimeSpan tsTimeRemaining = new TimeSpan(0, 0, 0);
            foreach (WoWAura aura in unit.Auras.Values)
            {
                if (controlMechanic.Contains(WoWSpell.FromId(aura.SpellId).Mechanic) || aura.SpellId == 19503)
                {
                    if (tsTimeRemaining < aura.TimeLeft)
                        tsTimeRemaining = aura.TimeLeft;
                }
            }
            return tsTimeRemaining;
        }

        public static TimeSpan isRooted(WoWUnit unit)
        {
            TimeSpan tsTimeRemaining = new TimeSpan(0, 0, 0);
            foreach (WoWAura aura in unit.Auras.Values)
            {
                if (rootMechanic.Contains(WoWSpell.FromId(aura.SpellId).Mechanic) || isStunned(Me).Milliseconds > 0)
                {
                    if (tsTimeRemaining < aura.TimeLeft)
                        tsTimeRemaining = aura.TimeLeft;
                }
            }
            return tsTimeRemaining;
        }
        #endregion

        // Following region is also from ZenLulz, if you're reading this and see him on the forums, +rep him!
        #region Snare Check

        public static bool isValid(WoWUnit unit)
        {
            return unit != null && unit.IsValid && !unit.IsDead && ((unit.IsPet && !unit.OwnedByUnit.IsPlayer) || !unit.IsPet);
        }

        public static bool NeedSnare(WoWUnit unit)
        {
            if (isValid(unit))
            {
                if (isSlowed(unit))
                    return false;
                else if (unit.HasAura("Divine Shield"))
                    return false;
                else if (unit.HasAura("Hand Of Freedom"))
                    return false;
                else if (unit.HasAura("Hand Of Protection"))
                    return false;
                else if (unit.HasAura("Master's Call"))
                    return false;
                else if (unit.HasAura("Pillar Of Frost"))
                    return false;
                else if (unit.IsWithinMeleeRange && isControlled(unit).TotalMilliseconds > 0)
                    return false;

                return true;
            }

            return false;
        }
        #endregion

        #region rest

        public override bool NeedRest
        {
            get
            {
                if (HaltFeign() && StyxWoW.IsInWorld && !Me.IsGhost && Me.IsAlive && !Me.Mounted && !Me.IsFlying && !Me.IsOnTransport)
                {
                    if (PvPBeastSettings.Instance.RP && !Me.GotAlivePet && SpellManager.HasSpell("Revive Pet"))
                    {
                        if (CastSpell("Revive Pet"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Reviving Pet <<");
                            StyxWoW.SleepForLagDuration();
                        }
                    }
                    if (PvPBeastSettings.Instance.CP && Me.Pet == null && !Me.IsCasting)
                    {
                        if (PvPBeastSettings.Instance.PET == 1 && SpellManager.HasSpell("Call Pet 1"))
                        {
                            SpellManager.Cast("Call Pet 1");
                            StyxWoW.SleepForLagDuration();
                        }
                        if (PvPBeastSettings.Instance.PET == 2 && SpellManager.HasSpell("Call Pet 2"))
                        {
                            SpellManager.Cast("Call Pet 2");
                            StyxWoW.SleepForLagDuration();
                        }
                        if (PvPBeastSettings.Instance.PET == 3 && SpellManager.HasSpell("Call Pet 3"))
                        {
                            SpellManager.Cast("Call Pet 3");
                            StyxWoW.SleepForLagDuration();
                        }
                        if (PvPBeastSettings.Instance.PET == 4 && SpellManager.HasSpell("Call Pet 4"))
                        {
                            SpellManager.Cast("Call Pet 4");
                            StyxWoW.SleepForLagDuration();
                        }
                        if (PvPBeastSettings.Instance.PET == 5 && SpellManager.HasSpell("Call Pet 5"))
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
            if (Me.GotTarget && Me.GotAlivePet && Me.Pet.CurrentTargetGuid != Me.CurrentTargetGuid && !SelfControl(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget))
            {
                Lua.DoString("PetAttack()");
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
            if (Me.GotAlivePet && IsMyAuraActive(Me.CurrentTarget, "Scatter Shot") && !DumbBear(Me.CurrentTarget))
            {
                Lua.DoString("PetAttack()");
            }
            if (PvPBeastSettings.Instance.KCO && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && IsMyAuraActive(Me.CurrentTarget, "Scatter Shot") && !DumbBear(Me.CurrentTarget))
            {
                if (CastSpell("Kill Command"))
                {
                    Logging.Write(Colors.Aquamarine, ">> Kill Command <<");
                }
            }
            if (!SelfControl(Me.CurrentTarget))
            {
                if (Me.GotTarget && Me.GotAlivePet && Me.Pet.CurrentTargetGuid != Me.CurrentTargetGuid && !SelfControl(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget))
                {
                    Lua.DoString("PetAttack()");
                }
                if (!Me.Mounted && HaltFeign() && !Me.IsDead)
                {
                    if (PvPBeastSettings.Instance.MP && Me.GotAlivePet && !Me.Pet.HasAura("Mend Pet") && (isStunned(Me.Pet).TotalSeconds > 0 || isForsaken(Me.Pet).TotalSeconds > 0 || isRooted(Me.Pet).TotalMilliseconds > 0 || isSlowed(Me.Pet) || isControlled(Me.Pet).TotalSeconds > 0 || Me.Pet.HealthPercent < PvPBeastSettings.Instance.MendHealth))
                    {
                        if (CastSpell("Mend Pet"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Mend Pet <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.RS && Me.Race == WoWRace.Draenei && Me.HealthPercent < 30 && !SpellManager.Spells["Gift of the Naaru"].Cooldown)
                    {
                        if (CastSpell("Gift of the Naaru"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Gift of the Naaru <<");
                        }
                    }

                }
                if (Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.Mounted && HaltFeign() && !Me.IsDead)
                {
                    //////////////////////////////// Trinkets ///////////////////////////////////////
                    if (PvPBeastSettings.Instance.T1MOB && Me.Inventory.Equipped.Trinket1 != null && Me.Inventory.Equipped.Trinket1.Cooldown <= 0 && (isStunned(Me).TotalSeconds > 2 || isControlled(Me).TotalSeconds > 2))
                    {
                        Lua.DoString("RunMacroText('/use 13');");
                    }
                    if (PvPBeastSettings.Instance.T1DMG && !Invulnerable(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 15 && Me.CurrentTarget.Distance > 6 && Me.Inventory.Equipped.Trinket1 != null && Me.Inventory.Equipped.Trinket1.Cooldown <= 0)
                    {
                        Lua.DoString("RunMacroText('/use 13');");
                    }
                    if (PvPBeastSettings.Instance.T1DEF && !Invulnerable(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.HealthPercent < 50 && Me.Inventory.Equipped.Trinket1 != null && Me.Inventory.Equipped.Trinket1.Cooldown <= 0 && (Me.CurrentTarget.Distance < 20 || Me.CurrentTarget.IsCasting))
                    {
                        Lua.DoString("RunMacroText('/use 13');");
                    }
                    if (PvPBeastSettings.Instance.T2MOB && Me.Inventory.Equipped.Trinket2 != null && Me.Inventory.Equipped.Trinket2.Cooldown <= 0 && (isStunned(Me).TotalSeconds > 2 || isControlled(Me).TotalSeconds > 2))
                    {
                        Lua.DoString("RunMacroText('/use 14');");
                    }
                    if (PvPBeastSettings.Instance.T2DMG && !Invulnerable(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 15 && Me.CurrentTarget.Distance > 9 && Me.Inventory.Equipped.Trinket2 != null && Me.Inventory.Equipped.Trinket2.Cooldown <= 0)
                    {
                        Lua.DoString("RunMacroText('/use 14');");
                    }
                    if (PvPBeastSettings.Instance.T2DEF && !Invulnerable(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.HealthPercent < 50 && Me.Inventory.Equipped.Trinket2 != null && Me.Inventory.Equipped.Trinket2.Cooldown <= 0 && (Me.CurrentTarget.Distance < 20 || Me.CurrentTarget.IsCasting))
                    {
                        Lua.DoString("RunMacroText('/use 14');");
                    }
                    /////////////////////// Defense stuff, to make sure we don't QQ when all the bad kids beat us. Also hi, thanks for reading my code :> ////////////////////////////////////////////
                    if (PvPBeastSettings.Instance.TL2_EXH && (Me.HealthPercent < 70
                        || (Me.Pet.HealthPercent < 15 && SpellManager.HasSpell("Heart of the Phoenix") && SpellManager.Spells["Heart of the Phoenix"].CooldownTimeLeft.TotalSeconds > 5)
                        || (Me.Pet.HealthPercent < 15 && !SpellManager.HasSpell("Heart of the Phoenix"))))
                    {
                        if (CastSpell("Exhilaration"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Exhilaration <<");
                        }
                    }
                    if (isSlowed(Me) || isRooted(Me).TotalMilliseconds > 0)
                    {
                        if (CastSpell("Master's Call"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Master's Call <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.MDPet && Me.GotAlivePet && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !Me.CurrentTarget.IsPlayer && !IsMyAuraActive(Me, "Misdirection")
                        && !WoWSpell.FromId(34477).Cooldown && !SpellManager.Spells["Misdirection"].Cooldown)
                    {
                        Lua.DoString("RunMacroText('/cast [@pet,exists] Misdirection');");
                        {
                            Logging.Write(Colors.Aquamarine, ">> Misdirection on Pet <<");
                        }
                    }
                    if (Me.GotAlivePet && IsMyAuraActive(Me.CurrentTarget, "Scatter Shot"))
                    {
                        Lua.DoString("PetAttack()");
                    }
                    if (PvPBeastSettings.Instance.KCO && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && IsMyAuraActive(Me.CurrentTarget, "Scatter Shot"))
                    {
                        if (CastSpell("Kill Command"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Kill Command <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.IntimidateBox == "1. Interrupt" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 && HostilePlayer(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast)
                    {
                        if (CastSpell("Intimidation"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Intimidation Interrupt <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.IntimidateBox == "2. Low Health" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 && HostilePlayer(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent <= PvPBeastSettings.Instance.TargetHealth)
                    {
                        if (CastSpell("Intimidation"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Intimidation Target Low on Health <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.IntimidateBox == "3. Protection" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 && HostilePlayer(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.Distance < 7 && MeleeClass(Me.CurrentTarget))
                    {
                        if (CastSpell("Intimidation"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Intimidation Stranger Danger <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.IntimidateBox == "1 + 2" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 && HostilePlayer(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && ((Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast) || Me.CurrentTarget.HealthPercent <= PvPBeastSettings.Instance.TargetHealth))
                    {
                        if (CastSpell("Intimidation"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Intimidation Interrupt <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.IntimidateBox == "1 + 3" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 && HostilePlayer(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && ((Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast) || (Me.CurrentTarget.Distance < 7 && MeleeClass(Me.CurrentTarget))))
                    {
                        if (CastSpell("Intimidation"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Intimidation Interrupt <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.IntimidateBox == "2 + 3" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 && HostilePlayer(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && (Me.CurrentTarget.HealthPercent <= PvPBeastSettings.Instance.TargetHealth || (Me.CurrentTarget.Distance < 7 && MeleeClass(Me.CurrentTarget))))
                    {
                        if (CastSpell("Intimidation"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Intimidation Interrupt <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.IntimidateBox == "1 + 2 + 3" && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 9 && HostilePlayer(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && ((Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast) || (Me.CurrentTarget.Distance < 7 && MeleeClass(Me.CurrentTarget)) || (Me.CurrentTarget.HealthPercent <= PvPBeastSettings.Instance.TargetHealth)))
                    {
                        if (CastSpell("Intimidation"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Intimidation Interrupt <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.RS && Me.Race == WoWRace.Human && !SpellManager.Spells["Every Man for Himself"].Cooldown && !PvPBeastSettings.Instance.T2MOB && !PvPBeastSettings.Instance.T1MOB && (isStunned(Me).TotalSeconds > 2 || isControlled(Me).TotalSeconds > 2))
                    {
                        if (CastSpell("Every Man for Himself"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Every Man for Himself <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.RS && Me.Race == WoWRace.Gnome && !SpellManager.Spells["Escape Artist"].Cooldown && isSlowed(Me) || isRooted(Me).TotalMilliseconds > 0)
                    {
                        if (CastSpell("Escape Artist"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Escape Artist <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.RS && Me.Race == WoWRace.Undead && !SpellManager.Spells["Will of The Forsaken"].Cooldown && isForsaken(Me).TotalMilliseconds > 0)
                    {
                        if (CastSpell("Will of The Forsaken"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Will of The Forsaken <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.RS && Me.Race == WoWRace.Dwarf && !SpellManager.Spells["Stoneform"].Cooldown && StyxWoW.Me.GetAllAuras().Any(a => a.Spell.Mechanic == WoWSpellMechanic.Bleeding || a.Spell.DispelType == WoWDispelType.Disease || a.Spell.DispelType == WoWDispelType.Poison))
                    {
                        if (CastSpell("Stoneform"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Stoneform <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.INT && Me.Race == WoWRace.Tauren && PvPBeastSettings.Instance.RS && Me.CurrentTarget.Distance < 8 && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && (SpellManager.Spells["Silencing Shot"].CooldownTimeLeft.TotalSeconds > 1 || Me.CurrentTarget.Distance < 5))
                    {
                        if (CastSpell("War Stomp"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> War Stomp, Interrupt <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.RS && Me.Race == WoWRace.Tauren && (Me.CurrentTarget.Distance < 8 || Me.CurrentTarget.Pet.Distance < 8) && (Me.CurrentTarget.CurrentTargetGuid == Me.Guid || Me.CurrentTarget.Pet.CurrentTargetGuid == Me.Guid) && (NeedSnare(Me.CurrentTarget) || NeedSnare(Me.CurrentTarget.Pet)) && (!Invulnerable(Me.CurrentTarget) || Me.CurrentTarget.Pet.Distance < 8))
                    {
                        if (CastSpell("War Stomp"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> War Stomp, Evade <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.INT && PvPBeastSettings.Instance.RS && Me.CurrentTarget.IsPlayer && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && Me.CurrentTarget.Distance >= 5)
                    {
                        if (CastSpell("Arcane Torrent"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Arcane Torrent <<");
                        }
                    }
                    if (Me.GotAlivePet && !WoWSpell.FromId(90361).Cooldown)
                    {
                        if (PvPBeastSettings.Instance.SpiritMendBox == "1. Me" && Me.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Me)
                        {
                            Lua.DoString("RunMacroText(\"/cast [@" + Me.Name + "] Spirit Mend\")");
                            {
                                Logging.Write(Colors.Aquamarine, ">> Spirit Mend on <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.SpiritMendBox == "2. Pet" && Me.Pet.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Pet)
                        {
                            Lua.DoString("RunMacroText(\"/cast [@Pet] Spirit Mend\")");
                            {
                                Logging.Write(Colors.Aquamarine, ">> Spirit Mend on <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.SpiritMendBox == "3. Focus" && Me.FocusedUnit.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Focus)
                        {
                            Lua.DoString("RunMacroText(\"/cast [@Focus] Spirit Mend\")");
                            {
                                Logging.Write(Colors.Aquamarine, ">> Spirit Mend on <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.SpiritMendBox == "1 + 2")
                        {
                            if (Me.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Me)
                            {
                                Lua.DoString("RunMacroText(\"/cast [@" + Me.Name + "] Spirit Mend\")");
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Spirit Mend on <<");
                                }
                            }
                            if (Me.Pet.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Pet)
                            {
                                Lua.DoString("RunMacroText(\"/cast [@Pet] Spirit Mend\")");
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Spirit Mend on Pet <<");
                                }
                            }
                        }
                        if (PvPBeastSettings.Instance.SpiritMendBox == "1 + 3")
                        {
                            if (Me.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Me)
                            {
                                Lua.DoString("RunMacroText(\"/cast [@" + Me.Name + "] Spirit Mend\")");
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Spirit Mend on Me <<");
                                }
                            }
                            if (Me.FocusedUnit.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Focus)
                            {
                                Lua.DoString("RunMacroText(\"/cast [@Focus] Spirit Mend\")");
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Spirit Mend on Focus <<");
                                }
                            }
                        }
                        if (PvPBeastSettings.Instance.SpiritMendBox == "2 + 3")
                        {
                            if (Me.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Pet)
                            {
                                Lua.DoString("RunMacroText(\"/cast [@Pet] Spirit Mend\")");
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Spirit Mend on Pet <<");
                                }
                            }
                            if (Me.FocusedUnit.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Focus)
                            {
                                Lua.DoString("RunMacroText(\"/cast [@Focus] Spirit Mend\")");
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Spirit Mend on Focus <<");
                                }
                            }
                        }
                        if (PvPBeastSettings.Instance.SpiritMendBox == "1 + 2 + 3")
                        {
                            if (Me.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Me)
                            {
                                Lua.DoString("RunMacroText(\"/cast [@" + Me.Name + "] Spirit Mend\")");
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Spirit Mend on Me <<");
                                }
                            }
                            if (Me.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Pet)
                            {
                                Lua.DoString("RunMacroText(\"/cast [@Pet] Spirit Mend\")");
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Spirit Mend on Pet <<");
                                }
                            }
                            if (Me.FocusedUnit.HealthPercent < PvPBeastSettings.Instance.SpiritHealth_Focus)
                            {
                                Lua.DoString("RunMacroText(\"/cast [@Focus] Spirit Mend\")");
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Spirit Mend on Focus <<");
                                }
                            }
                        }
                    }
                    if (PvPBeastSettings.Instance.WEB && NeedSnare(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !WoWSpell.FromId(54706).Cooldown && Me.CurrentTarget.Distance <= 30)
                    {
                        Lua.DoString("RunMacroText('/cast Venom Web Spray');");
                        {
                            Logging.Write(Colors.Crimson, ">> Pet: Silithid Web <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.FDCBox == "1. Pet Near" && Me.CurrentTarget.GotAlivePet && Me.CurrentTarget.Pet.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Pet.Distance <= 5)
                    {
                        if (CastSpell("Feign Death"))
                        {
                            System.Threading.Thread.Sleep(1000);
                            Logging.Write(Colors.Aquamarine, ">> Feign Death Enemy Pet Near <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.FDCBox == "2. Target Casting" && Me.CurrentTarget.IsPlayer && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && WoWSpell.FromId(Me.CurrentTarget.CastingSpellId).SpellEffect1.EffectType != WoWSpellEffectType.Heal)
                    {
                        if (CastSpell("Feign Death"))
                        {
                            System.Threading.Thread.Sleep(1000);
                            Logging.Write(Colors.Aquamarine, ">> Feign Death Target Is Casting <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.FDCBox == "1 + 2" && ((Me.CurrentTarget.GotAlivePet && Me.CurrentTarget.Pet.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Pet.Distance <= 5) || (Me.CurrentTarget.IsPlayer && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && WoWSpell.FromId(Me.CurrentTarget.CastingSpellId).SpellEffect1.EffectType != WoWSpellEffectType.Heal)))
                    {
                        if (CastSpell("Feign Death"))
                        {
                            System.Threading.Thread.Sleep(1000);
                            Logging.Write(Colors.Aquamarine, ">> Feign Death Enemy Pet Near or Target is Casting <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.FDCBox == "3. Low Health" && Me.HealthPercent < 10)
                    {
                        if (CastSpell("Feign Death"))
                        {
                            System.Threading.Thread.Sleep(1000);
                            Logging.Write(Colors.Aquamarine, ">> Feign Death: Low Health <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.FDCBox == "1 + 2 + 3" && ((Me.CurrentTarget.GotAlivePet && Me.CurrentTarget.Pet.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Pet.Distance <= 5) || (Me.CurrentTarget.IsPlayer && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && WoWSpell.FromId(Me.CurrentTarget.CastingSpellId).SpellEffect1.EffectType != WoWSpellEffectType.Heal) || Me.HealthPercent < 10))
                    {
                        if (CastSpell("Feign Death"))
                        {
                            System.Threading.Thread.Sleep(1000);
                            Logging.Write(Colors.Aquamarine, ">> Feign Death: Enemy pet is near us or Target is casting a harmful spell or We're low on health.<<");
                        }
                    }
                    if (PvPBeastSettings.Instance.FDCBox == "1 + 3" && ((Me.CurrentTarget.GotAlivePet && Me.CurrentTarget.Pet.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Pet.Distance <= 5) || Me.HealthPercent < 10))
                    {
                        if (CastSpell("Feign Death"))
                        {
                            System.Threading.Thread.Sleep(1000);
                            Logging.Write(Colors.Aquamarine, ">> Feign Death: Enemy pet is near us or We're low on health.<<");
                        }
                    }
                    if (PvPBeastSettings.Instance.FDCBox == "2 + 3" && ((Me.CurrentTarget.IsPlayer && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && WoWSpell.FromId(Me.CurrentTarget.CastingSpellId).SpellEffect1.EffectType != WoWSpellEffectType.Heal) || Me.HealthPercent < 10))
                    {
                        if (CastSpell("Feign Death"))
                        {
                            System.Threading.Thread.Sleep(1000);
                            Logging.Write(Colors.Aquamarine, ">> Feign Death: Target is casting a harmful spell or We're low on health.<<");
                        }
                    }
                    if (PvPBeastSettings.Instance.DETR && Me.HealthPercent < 15 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid)
                    {
                        if (CastSpell("Deterrence"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Deterrence <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.ScatterBox == "1. Interrupt" && Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && !Invulnerable(Me.CurrentTarget))
                    {
                        if (CastSpell("Scatter Shot"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Scatter Shot, Interrupt <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.ScatterBox == "2. Defense" && Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !Invulnerable(Me.CurrentTarget))
                    {
                        if (CastSpell("Scatter Shot"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Scatter Shot, Evade <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.ScatterBox == "1 + 2" && ((Me.CurrentTarget.Distance <= 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !Invulnerable(Me.CurrentTarget))
                        || (Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast)))
                    {
                        if (CastSpell("Scatter Shot"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Scatter Shot <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.TL1_SS && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast)
                    {
                        if (CastSpell("Silencing Shot"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Silencing Shot <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.TL1_WS && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast)
                    {
                        if (CastSpell("Wyvern Sting"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Wyvern Sting, Interrupt <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.TL1_BS && Me.CurrentTarget.Distance < 15 && MeleeClass(Me.CurrentTarget))
                    {
                        if (CastSpell("Binding Shot"))
                        {
                            SpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
                            Logging.Write(Colors.Aquamarine, ">> Binding Shot Launched! <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.CONC && NeedSnare(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && MyDebuffTime("Concussive Shot", Me.CurrentTarget) <= 1 && Me.CurrentTarget.Distance <= 40 && (Me.CurrentTarget.CurrentTargetGuid == Me.Guid || Me.CurrentTarget.IsMoving))
                    {
                        if (CastSpell("Concussive Shot"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Concussive Shot <<");
                        }
                    }
                    if (PvPBeastSettings.Instance.TL3_FV && (Me.CurrentFocus < 60 || (Me.HasAura("The Beast within") && Me.CurrentFocus < 40)))
                    {
                        if (CastSpell("Fervor"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Fervor <<");
                        }
                    }
                    /////////////////////////////////////////////////////Cooldowns/////////////////////////////////////////////////////////////////////////////////////////////////           
                    if (PvPBeastSettings.Instance.RF && (SpellManager.Spells["Bestial Wrath"].Cooldown || !PvPBeastSettings.Instance.BWR)
                        && !Me.HasAura("Rapid Fire") && !Me.HasAura("The Beast Within")
                        && !Me.HasAura("Bloodlust") && !Me.HasAura("Heroism") && !Me.HasAura("Ancient Hysteria") && !Me.HasAura("Time Warp")
                        && Me.CurrentTarget.CurrentHealth > 25000 && HostilePlayer(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget))
                    {
                        if (CastSpell("Rapid Fire"))
                        {
                            Logging.Write(Colors.Aquamarine, ">> Rapid Fire <<");
                        }
                        if (PvPBeastSettings.Instance.STAM && Me.GotTarget && !Invulnerable(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget) && Me.CurrentTarget.Distance < 30)
                        {
                            if (CastSpell("Stampede"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Stampede <<");
                            }
                        }
                        if (Me.GotAlivePet && PvPBeastSettings.Instance.BWR && (!PvPBeastSettings.Instance.TL4_LR || SpellManager.Spells["Lynx Rush"].CooldownTimeLeft.TotalSeconds > 10
                            || !SpellManager.Spells["Lynx Rush"].Cooldown) && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25 && !Me.HasAura("Rapid Fire")
                            && !Me.HasAura("The Beast Within") && !Me.HasAura("Bloodlust") && !Me.HasAura("Heroism") && !Me.HasAura("Ancient Hysteria") && !Me.HasAura("Time Warp")
                            && HostilePlayer(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && !Invulnerable(Me.Pet.CurrentTarget))
                        {
                            if (CastSpell("Bestial Wrath"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Bestial Wrath <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.RDN
                            && SpellManager.Spells["Rapid Fire"].CooldownTimeLeft.TotalSeconds > 1
                            && SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 1
                            && (!PvPBeastSettings.Instance.KCO || SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL1_SS || !PvPBeastSettings.Instance.ARN || SpellManager.Spells["Silencing Shot"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL1_WS || !PvPBeastSettings.Instance.ARN || SpellManager.Spells["Wyvern Sting"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL1_BS || !PvPBeastSettings.Instance.ARN || SpellManager.Spells["Binding Shot"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL3_DB || SpellManager.Spells["Dire Beast"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL3_FV || SpellManager.Spells["Fervor"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL4_AMOC || SpellManager.Spells["A Murder of Crows"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL4_BSTRK || SpellManager.Spells["Blink Strike"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL4_LR || SpellManager.Spells["Lynx Rush"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL5_GLV || SpellManager.Spells["Glaive Toss"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL5_PWR || SpellManager.Spells["Powershot"].CooldownTimeLeft.TotalSeconds > 1)
                            && (!PvPBeastSettings.Instance.TL5_BRG || SpellManager.Spells["Barrage"].CooldownTimeLeft.TotalSeconds > 1)
                            && (PvPBeastSettings.Instance.IntimidateBox == "Never" || !PvPBeastSettings.Instance.ARN || SpellManager.Spells["Intimidation"].CooldownTimeLeft.TotalSeconds > 1))
                        {
                            if (CastSpell("Readiness"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Readiness <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.LB && SpellManager.HasSpell("Lifeblood") && !SpellManager.Spells["Lifeblood"].Cooldown && Me.HealthPercent < 99)
                        {
                            if (CastSpell("Lifeblood"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Lifeblood <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.GE && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.Inventory.Equipped.Hands != null
                            && Me.Inventory.Equipped.Hands.Usable && Me.Inventory.Equipped.Hands.CooldownTimeLeft.TotalSeconds == 0)
                        {
                            Lua.DoString("RunMacroText('/use 10');");
                        }
                        //////////////////////////////////////////////////Racial Skills/////////////////////////////////////////////////////////////////////////////////////////
                        if (PvPBeastSettings.Instance.RS && !Invulnerable(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget) && Me.Race == WoWRace.Troll && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 15 && !SpellManager.Spells["Berserking"].Cooldown)
                        {
                            Lua.DoString("RunMacroText('/Cast Berserking');");
                        }
                        if (PvPBeastSettings.Instance.RS && !Invulnerable(Me.CurrentTarget) && Me.Race == WoWRace.Orc && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 15 && !SpellManager.Spells["Blood Fury"].Cooldown)
                        {
                            Lua.DoString("RunMacroText('/Cast Blood Fury');");
                        }
                    }
                    ////////////////////////////////// Traps and Launchers ////////////////////////////////////////////////
                    if (Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.Mounted && HaltFeign() && !Me.IsDead)
                    {
                        if (Me.CurrentTarget.Distance > 5 && Me.CurrentTarget.Distance < 40 && HostilePlayer(Me.CurrentTarget)
                            && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.InLineOfSpellSight && !Me.CurrentTarget.IsMoving)
                        {
                            if (PvPBeastSettings.Instance.TL && MeleeClass(Me.CurrentTarget) && !SpellManager.Spells["Ice Trap"].Cooldown)
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
                                    Lua.DoString("CastSpellByName('Ice Trap');");
                                    {
                                        SpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
                                        Logging.Write(Colors.Crimson, ">> Ice Trap Launched! <<");
                                    }
                                }
                            }
                            if (PvPBeastSettings.Instance.TL2 && MeleeClass(Me.CurrentTarget) && !SpellManager.Spells["Snake Trap"].Cooldown && !DumbBear(Me.CurrentTarget))
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
                                    Lua.DoString("CastSpellByName('Snake Trap');");
                                    {
                                        SpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
                                        Logging.Write(Colors.Crimson, ">> Snake Trap Launched! <<");
                                    }
                                }
                            }
                            if (PvPBeastSettings.Instance.TL3 && RangedClass(Me.CurrentTarget) && !SpellManager.Spells["Freezing Trap"].Cooldown)
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
                                    Lua.DoString("CastSpellByName('Freezing Trap');");
                                    {
                                        SpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
                                        Logging.Write(Colors.Crimson, ">> Freezing Trap Launched! <<");
                                    }
                                }
                            }
                            if (PvPBeastSettings.Instance.TL4 && !SpellManager.Spells["Explosive Trap"].Cooldown && !DumbBear(Me.CurrentTarget))
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
                        if (Me.CurrentTarget.Distance <= 5 && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid)
                        {
                            if (PvPBeastSettings.Instance.ICET)
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
                                    if (CastSpell("Ice Trap"))
                                    {
                                        Logging.Write(Colors.Crimson, ">> Dropping Ice Trap <<");
                                    }
                                }
                            }
                            if (PvPBeastSettings.Instance.SNAT)
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
                                    if (CastSpell("Snake Trap"))
                                    {
                                        Logging.Write(Colors.Crimson, ">> Dropping Snake Trap <<");
                                    }
                                }
                            }
                            if (PvPBeastSettings.Instance.FRET)
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
                                    if (CastSpell("Freezing Trap"))
                                    {
                                        Logging.Write(Colors.Crimson, ">> Dropping Freezing Trap <<");
                                    }
                                }
                            }
                            if (PvPBeastSettings.Instance.EXPT && !DumbBear(Me.CurrentTarget))
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
                    }
                    ///////////////////////////////////////////////Aspect Switching////////////////////////////////////////////////////////////////////////////////////////////
                    if (PvPBeastSettings.Instance.AspectSwitching && HaltFeign() && Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.Mounted && !Me.IsDead)
                    {
                        if (PvPBeastSettings.Instance.TL2_AOTIH)
                        {
                            if (!Me.IsMoving && !Me.Auras.ContainsKey("Aspect of the Iron Hawk"))
                            {
                                if (CastSpell("Aspect of the Hawk"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Not moving - Aspect of the Iron Hawk <<");
                                }
                            }
                            if (Me.IsMoving && Me.Auras.ContainsKey("Aspect of the Iron Hawk") && Me.CurrentFocus < 60)
                            {
                                if (CastSpell("Aspect of the Fox"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Moving - Aspect of the Fox <<");
                                }
                            }
                        }
                        if (!PvPBeastSettings.Instance.TL2_AOTIH)
                        {
                            if (!Me.IsMoving && !Me.Auras.ContainsKey("Aspect of the Hawk"))
                            {
                                if (CastSpell("Aspect of the Hawk"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Not moving - Aspect of the Hawk <<");
                                }
                            }
                            if (Me.IsMoving && Me.Auras.ContainsKey("Aspect of the Hawk") && Me.CurrentFocus < 60)
                            {
                                if (CastSpell("Aspect of the Fox"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Moving - Aspect of the Fox <<");
                                }
                            }
                        }
                    }
                    /////////////////////////////////////////////Beastmastery Rotation///////////////////////////////////////////////////////////////////////////////////////////
                    if (Me.GotTarget && HaltFeign() && Me.CurrentTarget.IsAlive && !Me.Mounted && !Me.IsDead && !Invulnerable(Me.CurrentTarget) && !DumbBear(Me.CurrentTarget))
                    {
                        if (PvPBeastSettings.Instance.HM && !Me.CurrentTarget.HasAura("Hunter's Mark"))
                        {
                            if (CastSpell("Hunter's Mark"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Hunter's Mark <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.KSH && Me.CurrentTarget.HealthPercent <= 20)
                        {
                            if (CastSpell("Kill Shot"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Kill Shot <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.SerpentBox == "Always" && (!IsMyAuraActive(Me.CurrentTarget, "Serpent Sting") || MyDebuffTime("Serpent Sting", Me.CurrentTarget) < 1))
                        {
                            if (CastSpell("Serpent Sting"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Serpent Sting <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.SerpentBox == "Sometimes" && (!IsMyAuraActive(Me.CurrentTarget, "Serpent Sting") || MyDebuffTime("Serpent Sting", Me.CurrentTarget) < 1)
                            && Me.CurrentTarget.HealthPercent > 50)
                        {
                            if (CastSpell("Serpent Sting"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Serpent Sting <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.WVE && Me.CurrentFocus > 53 && HostilePlayer(Me.CurrentTarget) && SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 1 && (!IsMyAuraActive(Me.CurrentTarget, "Widow Venom") || MyDebuffTime("Widow Venom", Me.CurrentTarget) <= 1))
                        {
                            if (CastSpell("Widow Venom"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Widow Venom <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.TL3_DB)
                        {
                            if (CastSpell("Dire Beast"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Dire Beast <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.TL4_BSTRK && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 40)
                        {
                            if (CastSpell("Blink Strike"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Blink Strike <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.KCO && Me.GotAlivePet && Me.Pet.Location.Distance(Me.CurrentTarget.Location) <= 25)
                        {
                            if (CastSpell("Kill Command"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Kill Command <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.TL4_LR && (!PvPBeastSettings.Instance.BWR || SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 10)
                            && Me.Pet.Location.Distance(Me.CurrentTarget.Location) < 25 && Me.GotAlivePet)
                        {
                            if (CastSpell("Lynx Rush"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Lynx Rush <<");
                            }
                        }
                        if (Me.Level >= 90 && PvPBeastSettings.Instance.TL5_GLV && (!SpellManager.CanCast("Kill Command") || !PvPBeastSettings.Instance.KCO))
                        {
                            if (CastSpell("Glaive Toss"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Glaive Toss <<");
                            }
                        }
                        if (Me.Level >= 90 && PvPBeastSettings.Instance.TL5_PWR && (!SpellManager.CanCast("Kill Command") || !PvPBeastSettings.Instance.KCO))
                        {
                            if (CastSpell("Powershot"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Powershot <<");
                            }
                        }
                        if (Me.Level >= 90 && PvPBeastSettings.Instance.TL5_BRG && (!SpellManager.CanCast("Kill Command") || !PvPBeastSettings.Instance.KCO))
                        {
                            if (CastSpell("Barrage"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Barrage <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.TL4_AMOC && !IsMyAuraActive(Me.CurrentTarget, "A Murder of Crows") && ((Me.CurrentTarget.MaxHealth > Me.MaxHealth * 2 && Me.CurrentTarget.HealthPercent > 20)
                            || (Me.CurrentTarget.MaxHealth > Me.MaxHealth && Me.CurrentTarget.HealthPercent <= 20) || Me.CurrentTarget.Name.Contains("Training Dummy")))
                        {
                            if (CastSpell("A Murder of Crows"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> A Murder of Crows <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.FF && Me.GotAlivePet && Me.Pet.Auras.ContainsKey("Frenzy") && !Me.HasAura("The Beast Within")
                            && ((SpellManager.Spells["Bestial Wrath"].Cooldown && SpellManager.Spells["Bestial Wrath"].CooldownTimeLeft.TotalSeconds > 9)
                            || (!SpellManager.Spells["Bestial Wrath"].Cooldown && (Me.CurrentTarget.MaxHealth <= 200000 || Me.HasAura("Rapid Fire")))))
                        {
                            if (PvPBeastSettings.Instance.FFS == 5 && Me.Pet.Auras["Frenzy"].StackCount >= 5)
                            {
                                if (CastSpell("Focus Fire"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Focus Fire: 5 Stacks <<");
                                }
                            }
                            if (PvPBeastSettings.Instance.FFS == 4 && Me.Pet.Auras["Frenzy"].StackCount >= 4)
                            {
                                if (CastSpell("Focus Fire"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Focus Fire: 4 Stacks <<");
                                }
                            }
                            if (PvPBeastSettings.Instance.FFS == 3 && Me.Pet.Auras["Frenzy"].StackCount >= 3)
                            {
                                if (CastSpell("Focus Fire"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Focus Fire: 3 Stacks <<");
                                }
                            }
                            if (PvPBeastSettings.Instance.FFS == 2 && Me.Pet.Auras["Frenzy"].StackCount >= 2)
                            {
                                if (CastSpell("Focus Fire"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Focus Fire: 2 Stacks <<");
                                }
                            }
                            if (PvPBeastSettings.Instance.FFS == 1 && Me.Pet.Auras["Frenzy"].StackCount >= 1)
                            {
                                if (CastSpell("Focus Fire"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Focus Fire: 1 Stack <<");
                                }
                            }
                        }
                        if (PvPBeastSettings.Instance.FF && Me.GotAlivePet && Me.Pet.HasAura("Frenzy") && Me.Pet.Auras["Frenzy"].StackCount >= 1 && DebuffTime("Frenzy", Me.Pet) < 2)
                        {
                            if (CastSpell("Focus Fire"))
                            {
                                Logging.Write(Colors.Aquamarine, ">> Focus Fire: Running out of time <<");
                            }
                        }
                        if (PvPBeastSettings.Instance.ARC
                            && (!PvPBeastSettings.Instance.KSH || !SpellManager.CanCast("Kill Shot"))
                            && (!PvPBeastSettings.Instance.TL5_GLV || !SpellManager.CanCast("Glaive Toss"))
                            && (!PvPBeastSettings.Instance.TL5_BRG || !SpellManager.CanCast("Barrage"))
                            && (!PvPBeastSettings.Instance.TL5_PWR || !SpellManager.CanCast("Powershot")))
                        {
                            if (PvPBeastSettings.Instance.KCO)
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
                            else if (!PvPBeastSettings.Instance.KCO)
                            {
                                if (CastSpell("Arcane Shot"))
                                {
                                    Logging.Write(Colors.Aquamarine, ">> Arcane Shot <<");
                                }
                            }
                        }
                        if ((Me.CurrentFocus > 105 || (Me.CurrentTarget.HealthPercent < 5 && SpellManager.CanCast("Kill Shot"))) && Me.IsCasting && (Me.CastingSpell.Name == "Cobra Shot" || Me.CastingSpell.Name == "Steady Shot") && Me.CurrentCastTimeLeft.TotalMilliseconds > 700)
                        {
                            SpellManager.StopCasting();
                            Logging.Write(Colors.Red, ">> Stop Cobra Shot <<");
                        }
                        if (!Me.IsCasting && (!PvPBeastSettings.Instance.KSH || !SpellManager.CanCast("Kill Shot"))
                            && (!PvPBeastSettings.Instance.TL5_GLV || !SpellManager.CanCast("Glaive Toss"))
                            && (!PvPBeastSettings.Instance.TL3_FV || !SpellManager.CanCast("Fervor"))
                            && (!PvPBeastSettings.Instance.TL5_BRG || !SpellManager.CanCast("Barrage"))
                            && (!PvPBeastSettings.Instance.TL5_PWR || !SpellManager.CanCast("Powershot"))
                            && ((!Me.HasAura("The Beast Within") && (SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 1) || Me.CurrentFocus < 39)
                            || (Me.HasAura("The Beast Within") && (SpellManager.Spells["Kill Command"].CooldownTimeLeft.TotalSeconds > 1) || Me.CurrentFocus < 19)))
                        {
                            if (Me.Level >= 81)
                            {
                                if ((Me.CurrentFocus < PvPBeastSettings.Instance.FocusShots || (Me.HasAura("The Beast Within") && Me.CurrentFocus < PvPBeastSettings.Instance.FocusShots / 2))
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
                                if (Me.CurrentFocus < PvPBeastSettings.Instance.FocusShots || (Me.HasAura("The Beast Within") && Me.CurrentFocus < PvPBeastSettings.Instance.FocusShots / 2))
                                {
                                    Lua.DoString("RunMacroText('/cast Steady Shot');");
                                    {
                                        Logging.Write(Colors.Aquamarine, ">> Steady Shot <<");
                                    }
                                }
                            }
                        }
                    }
                }

            }
        #endregion
        }
    }
}