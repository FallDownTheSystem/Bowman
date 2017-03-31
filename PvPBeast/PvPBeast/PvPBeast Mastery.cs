using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.IO;
using System.Drawing;
using Styx;
using Styx.Combat.CombatRoutine;
using Styx.Helpers;
using Styx.Logic;
using Styx.Logic.Combat;
using Styx.Logic.Pathing;
using Styx.WoWInternals;
using Styx.WoWInternals.WoWObjects;
using TreeSharp;

namespace PvPBeast
{
    class Classname : CombatRoutine
    {
        public override sealed string Name { get { return "PvPBeast Mastery CC v. 1.0"; } }

        public override WoWClass Class { get { return WoWClass.Hunter; } }


        private static LocalPlayer Me { get { return ObjectManager.Me; } }


        #region Log
        private void slog(string format, params object[] args) //use for slogging
        {
            Logging.Write(format, args);
        }
        #endregion


        #region Initialize
        public override void Initialize()
        {
            Logging.Write(Colors.White, "________________________________________");
            Logging.Write(Colors.Crimson, "------ PvPBeast Mastery Hunter CC  -------");
            Logging.Write(Colors.Crimson, "----------- v. 1.0 by FallDown ------------");
            Logging.Write(Colors.Crimson, "---- Credit to ZenLulz for some of the code ----");
            Logging.Write(Colors.White, "________________________________________");
        }
        #endregion



        #region Settings

        public override bool WantButton
        {
            get
            {
                return true;
            }
        }

        public override void OnButtonPress()
        {

            PvPBeast.PvPBeastForm f1 = new PvPBeast.PvPBeastForm();
            f1.ShowDialog();
        }
        #endregion

        #region Halt on Trap Launcher
        public bool HaltTrap()
        {
            if (!Me.HasAura("Trap Launcher"))
                return true;

            else return false;
        }
        #endregion

        #region Halt on Feign Death
        public bool HaltFeign()
        {
            if (!Me.ActiveAuras.ContainsKey("Feign Death"))
                return true;

            else return false;
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
            if (unit.Class == WoWClass.Druid && unit.Auras.ContainsKey("Bear Form") && unit.Auras.ContainsKey("Frenzied Regeneration") && unit.HealthPercent > 9 && unit.Distance > 9)
                return true;

            else return false;
        }
        #endregion

        #region Class type
        public bool MeleeClass(WoWUnit unit)
        {
            if (Me.GotTarget && (unit.Class == WoWClass.Rogue || unit.Class == WoWClass.Warrior || unit.Class == WoWClass.DeathKnight ||
                (unit.Class == WoWClass.Paladin && Me.CurrentTarget.MaxMana < 90000) || 
                (unit.Class == WoWClass.Druid && (Me.CurrentTarget.Auras.ContainsKey("Cat Form") || Me.CurrentTarget.Auras.ContainsKey("Bear Form")))))
                return true;

            else return false;
        }
        public bool RangedClass(WoWUnit unit)
        {
            if (Me.GotTarget && (unit.Class == WoWClass.Hunter || unit.Class == WoWClass.Shaman || unit.Class == WoWClass.Priest ||
                unit.Class == WoWClass.Mage || unit.Class == WoWClass.Warlock || (unit.Class == WoWClass.Paladin && Me.CurrentTarget.MaxMana >= 90000) ||
                (unit.Class == WoWClass.Druid && !Me.CurrentTarget.Auras.ContainsKey("Cat Form") && !Me.CurrentTarget.Auras.ContainsKey("Bear Form"))))
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
            if (unit.IsHostile && unit.IsAlive)
                return true;

            else return false;
        }
        #endregion

        #region Add Detection
        private int addCount()
        {
            int count = 0;
            foreach (WoWUnit u in ObjectManager.GetObjectsOfType<WoWUnit>(true, true))
            {
                if (u.IsAlive
                    && u.Guid != Me.Guid
                    && u.IsHostile
                    && !u.IsCritter
                    && (u.Location.Distance(Me.CurrentTarget.Location) <= 12 || u.Location.Distance2D(Me.CurrentTarget.Location) <= 12)
                    && (u.IsTargetingMyPartyMember || u.IsTargetingMyRaidMember || u.IsTargetingMeOrPet || u.IsTargetingAnyMinion)
                    && !u.IsFriendly)
                {
                    count++;
                }
            }
            return count;
        }
        private bool IsTargetBoss()
        {
            if (Me.CurrentTarget.CreatureRank == WoWUnitClassificationType.WorldBoss ||
            (Me.CurrentTarget.Level >= 85 && Me.CurrentTarget.Elite && Me.CurrentTarget.MaxHealth > 3500000))
                return true;

            else return false;
        }
        #endregion

        #region CastSpell Method
        // Credit to Apoc for the below CastSpell code
        // Used for calling CastSpell in the Combat Rotation
        //Credit to Wulf!
        public bool CastSpell(string spellName)
        {
            if (SpellManager.CanCast(spellName))
            {
                SpellManager.Cast(spellName);
                // We managed to cast the spell, so return true, saying we were able to cast it.
                return true;
            }
            // Can't cast the spell right now, so return false.
            return false;
        }
        #endregion

        #region MyDebuffTime
        //Used for checking how the time left on "my" debuff
        private int MyDebuffTime(String spellName, WoWUnit unit)
        {
            if (unit.HasAura(spellName))
            {
                var auras = unit.GetAllAuras();
                foreach (var a in auras)
                {
                    if (a.Name == spellName && a.CreatorGuid == Me.Guid)
                    {
                        return a.TimeLeft.Seconds;
                    }
                }
            }
            return 0;
        }
        #endregion

        #region DebuffTime
        //Used for checking debuff timers
        private int DebuffTime(String spellName, WoWUnit unit)
        {
            if (unit.HasAura(spellName))
            {
                var auras = unit.GetAllAuras();
                foreach (var b in auras)
                {
                    if (b.Name == spellName)
                    {
                        return b.TimeLeft.Seconds;
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
            return Who.GetAllAuras().Where(p => p.CreatorGuid == Me.Guid && p.Name == What).FirstOrDefault() != null;
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
            return unit != null && unit.IsValid && !unit.Dead && ((unit.IsPet && !unit.OwnedByUnit.IsPlayer) || !unit.IsPet);
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

        #region Combat

        public override void Combat()
        {
        if (!SelfControl(Me.CurrentTarget))
        {
            if (PvPBeastSettings.Instance.FT && !Me.Mounted && !Me.Dead)
            {
                if (Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.CurrentTarget.IsPet && Me.FocusedUnit != Me.CurrentTarget)
                {
                    Me.SetFocus(Me.CurrentTarget);
                }
                if (!Me.GotTarget && Me.FocusedUnit != Me.CurrentTarget && Me.FocusedUnit.Distance < 40 && Me.FocusedUnit.InLineOfSight)
                {
                    Me.FocusedUnit.Target();
                }
            }
          }
        }
        #endregion
    }
}