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

namespace PvPBow
{
    class Classname : CombatRoutine
    {
        public override sealed string Name { get { return "PvPBow a Marksmans CC v. 0"; } }

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
            Logging.Write(Color.White, "________________________________________");
            Logging.Write(Color.Crimson, "------ PvPBow Marksman Hunter CC  -------");
			Logging.Write(Color.Crimson, "----------- v. 0.1.2.1 by FallDown ------------");
			Logging.Write(Color.Crimson, "---- Credit to ZenLulz for some of the code ----");
            Logging.Write(Color.White, "________________________________________");
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

            PvPBow.PvPBowForm f1 = new  PvPBow.PvPBowForm();
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
            if (unit.HasAura("Cyclone") || unit.HasAura("Ice Block") || unit.HasAura("Deterrence") || unit.HasAura("Divine Shield") || unit.HasAura("Hand of Protection") || (unit.HasAura("Anti-Magic Shell") && unit.HasAura("Icebound Fortitude")))
			return true;

            else return false;
        } 
        #endregion
		
		#region Focus Shot Conditions
        public bool FocusShot()
        {
            if (Me.CurrentTarget.Distance >= 5 && !Me.ActiveAuras.ContainsKey("Fire!") && !SpellManager.CanCast("Chimera Shot") && !SpellManager.CanCast("Kill Shot") && (SpellManager.Spells["Kill Shot"].CooldownTimeLeft.TotalSeconds > 1 || Me.CurrentTarget.HealthPercent > 20) && (SpellManager.Spells["Chimera Shot"].CooldownTimeLeft.TotalSeconds > 1 || (SpellManager.Spells["Chimera Shot"].CooldownTimeLeft.TotalSeconds <= 1 && Me.CurrentFocus < 34))) 
			return true;          
            
            else return false;
        } 
        #endregion

		#region Alive Hostile Enemy
        public bool HostilePlayer(WoWUnit unit)
        {
            if (unit.IsPlayer && unit.IsHostile && unit.IsAlive)
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
		
		#region rest
        public override bool NeedRest
        {
            get
            {
				if (StyxWoW.IsInWorld && !Me.IsGhost && !Me.GotAlivePet && !Me.Dead && !Me.Mounted) 
				{
					if (PvPBowSettings.Instance.RP && !Me.GotAlivePet && SpellManager.HasSpell("Revive Pet"))
                    {
                        if (CastSpell("Revive Pet")) 
                        StyxWoW.SleepForLagDuration();
                    }
				}
				if (PvPBowSettings.Instance.CP && Me.Pet == null && !Me.IsCasting)
				{
					if (PvPBowSettings.Instance.PET == 1 && SpellManager.HasSpell("Call Pet 1"))
					{
						SpellManager.Cast("Call Pet 1");
						StyxWoW.SleepForLagDuration();
					}
					if (PvPBowSettings.Instance.PET == 2 && SpellManager.HasSpell("Call Pet 2"))
					{
						SpellManager.Cast("Call Pet 2");
						StyxWoW.SleepForLagDuration();
					}
					if (PvPBowSettings.Instance.PET == 3 && SpellManager.HasSpell("Call Pet 3"))
					{
						SpellManager.Cast("Call Pet 3");
						StyxWoW.SleepForLagDuration();
					}
					if ( PvPBowSettings.Instance.PET == 4 && SpellManager.HasSpell("Call Pet 4"))
					{
						SpellManager.Cast("Call Pet 4");
						StyxWoW.SleepForLagDuration();
					}
					if (PvPBowSettings.Instance.PET == 5 && SpellManager.HasSpell("Call Pet 5"))
					{
						SpellManager.Cast("Call Pet 5");
						StyxWoW.SleepForLagDuration();
					}
                    StyxWoW.SleepForLagDuration();
                }		
                return true;
            }    
        }
        #endregion
		
        #region CombatStart

        private void AutoAttack()
        {
            if (!Me.IsAutoAttacking && HaltTrap() && HaltFeign() && Me.GotAlivePet)
            {
				Lua.DoString("PetAttack()");
                Lua.DoString("StartAttack()");
            }
			else if (!Me.IsAutoAttacking && HaltTrap() && HaltFeign())
            {
                Lua.DoString("StartAttack()");
            }

        }
        #endregion

        #region Combat

        public override void Combat()
        {
			if (PvPBowSettings.Instance.TLF && Me.HasAura("Trap Launcher") && MyDebuffTime("Trap Launcher", Me) < 13 && (Me.CurrentTarget == null || !Me.CurrentTarget.InLineOfSight ||  Me.CurrentTarget.Distance > 40 || Me.CurrentTarget.Distance < 8))
			{
				if (PvPBowSettings.Instance.TL && SpellManager.Spells["Ice Trap"].CooldownTimeLeft.TotalSeconds > 1)
				{
					Lua.DoString("RunMacroText('/cancelaura Trap Launcher');");
				}
				else if (PvPBowSettings.Instance.TL2 && SpellManager.Spells["Snake Trap"].CooldownTimeLeft.TotalSeconds > 1)
				{
					Lua.DoString("RunMacroText('/cancelaura Trap Launcher');");
				}
				else if (PvPBowSettings.Instance.TL3 && SpellManager.Spells["Freezing Trap"].CooldownTimeLeft.TotalSeconds > 1)
				{
					Lua.DoString("RunMacroText('/cancelaura Trap Launcher');");
				}
				else if (PvPBowSettings.Instance.TL4 && SpellManager.Spells["Explosive Trap"].CooldownTimeLeft.TotalSeconds > 1)
				{
					Lua.DoString("RunMacroText('/cancelaura Trap Launcher');");
				}
			}
			if (PvPBowSettings.Instance.FT && !Me.Mounted && !Me.Dead)
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
            if (Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.Mounted && !Me.Dead && HaltTrap() && HaltFeign())
            {  
				if (PvPBowSettings.Instance.MP && Me.GotAlivePet && Me.Pet.HealthPercent < 75 && !Me.Pet.ActiveAuras.ContainsKey("Mend Pet"))
				{
				if (CastSpell("Mend Pet"))
					{
					Logging.Write(Color.Aqua, ">> Mend Pet <<");
					}
				}
				if (PvPBowSettings.Instance.RS && Me.Race == WoWRace.Draenei && Me.HealthPercent < 50 && !SpellManager.Spells["Gift of the Naaru"].Cooldown)
				{
					if (CastSpell("Gift of the Naaru"))
					{
					Logging.Write(Color.Aqua, ">> Gift of the Naaru <<");
					}
				}
			//////////////////////////////// Trinkets ///////////////////////////////////////
				if (PvPBowSettings.Instance.T1MOB && Me.Inventory.Equipped.Trinket1 != null && Me.Inventory.Equipped.Trinket1.Cooldown <= 0 && (isStunned(Me).TotalSeconds > 2 || isControlled(Me).TotalSeconds > 2))
				{
					Lua.DoString("RunMacroText('/use 13');");
				}
				if (PvPBowSettings.Instance.T1DMG && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 15 && Me.CurrentTarget.Distance > 6 && Me.Inventory.Equipped.Trinket1 != null && Me.Inventory.Equipped.Trinket1.Cooldown <= 0)
				{
					Lua.DoString("RunMacroText('/use 13');");
				}
				if (PvPBowSettings.Instance.T1DEF && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.HealthPercent < 50 && Me.Inventory.Equipped.Trinket1 != null && Me.Inventory.Equipped.Trinket1.Cooldown <= 0 && (Me.CurrentTarget.Distance < 20 || Me.CurrentTarget.IsCasting))
				{
					Lua.DoString("RunMacroText('/use 13');");
				}
				if (PvPBowSettings.Instance.T2MOB && Me.Inventory.Equipped.Trinket2 != null && Me.Inventory.Equipped.Trinket2.Cooldown <= 0 && (isStunned(Me).TotalSeconds > 2 || isControlled(Me).TotalSeconds > 2))
				{
					Lua.DoString("RunMacroText('/use 14');");
				}
				if (PvPBowSettings.Instance.T2DMG && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 15 && Me.CurrentTarget.Distance > 9 && Me.Inventory.Equipped.Trinket2 != null && Me.Inventory.Equipped.Trinket2.Cooldown <= 0)
				{
					Lua.DoString("RunMacroText('/use 14');");
				}
				if (PvPBowSettings.Instance.T2DEF && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.HealthPercent < 50 && Me.Inventory.Equipped.Trinket2 != null && Me.Inventory.Equipped.Trinket2.Cooldown <= 0 && (Me.CurrentTarget.Distance < 20 || Me.CurrentTarget.IsCasting))
				{
					Lua.DoString("RunMacroText('/use 14');");
				}
			/////////////////////////////Close Combat and Defense Mechanisms////////////////////////////////
				if (isSlowed(Me) || isRooted(Me).TotalMilliseconds > 0) 
				{
					if (CastSpell("Master's Call"))
					{
						Logging.Write(Color.Aqua, ">> Master's Call <<");
					}
				}
				if (PvPBowSettings.Instance.RS && Me.Race == WoWRace.Human && !SpellManager.Spells["Every Man for Himself"].Cooldown && !PvPBowSettings.Instance.T2MOB && !PvPBowSettings.Instance.T1MOB && (isStunned(Me).TotalSeconds > 2 || isControlled(Me).TotalSeconds > 2))
				{
					if (CastSpell("Every Man for Himself"))
					{
						Logging.Write(Color.Aqua, ">> Every Man for Himself <<");
					}
				}
				if (PvPBowSettings.Instance.RS && Me.Race == WoWRace.Gnome && !SpellManager.Spells["Escape Artist"].Cooldown && isSlowed(Me) || isRooted(Me).TotalMilliseconds > 0) 
				{
					if (CastSpell("Escape Artist"))
					{
						Logging.Write(Color.Aqua, ">> Escape Artist <<");
					}
				}
				if (PvPBowSettings.Instance.RS && Me.Race == WoWRace.Undead && !SpellManager.Spells["Will of The Forsaken"].Cooldown && isForsaken(Me).TotalMilliseconds > 0) 
				{
					if (CastSpell("Will of The Forsaken"))
					{
						Logging.Write(Color.Aqua, ">> Will of The Forsaken <<");
					}
				}
				if (PvPBowSettings.Instance.RS && Me.Race == WoWRace.Dwarf && !SpellManager.Spells["Stoneform"].Cooldown && StyxWoW.Me.GetAllAuras().Any(a => a.Spell.Mechanic == WoWSpellMechanic.Bleeding || a.Spell.DispelType == WoWDispelType.Disease || a.Spell.DispelType == WoWDispelType.Poison)) 
				{
					if (CastSpell("Stoneform"))
					{
						Logging.Write(Color.Aqua, ">> Stoneform <<");
					}
				}						
				if (PvPBowSettings.Instance.INT && Me.CurrentTarget.IsPlayer && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && Me.CurrentTarget.Distance >= 5)
				{
					if (CastSpell("Silencing Shot"))
					{
					Logging.Write(Color.Aqua, ">> Silencing Shot <<");
					}
				}
                if (PvPBowSettings.Instance.INT && Me.Race == WoWRace.BloodElf && SpellManager.Spells["Silencing Shot"].CooldownTimeLeft.TotalSeconds > 0 && Me.CurrentTarget.IsPlayer && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && Me.CurrentTarget.Distance >= 5)
				{
					if (CastSpell("Arcane Torrent"))
					{
					Logging.Write(Color.Aqua, ">> Arcane Torrent <<");
					}
				}
				if (PvPBowSettings.Instance.INT && Me.CurrentTarget.Distance <= 20 && !Invulnerable(Me.CurrentTarget) &&  Me.CurrentTarget.IsCasting &&Me.CanInterruptCurrentSpellCast && (SpellManager.Spells["Silencing Shot"].CooldownTimeLeft.TotalSeconds > 1 || Me.CurrentTarget.Distance < 5))
				{
					if (CastSpell("Scatter Shot"))
					{
					Logging.Write(Color.Aqua, ">> Scatter Shot, Interrupt <<");
					}
				}
				if (PvPBowSettings.Instance.INT && Me.Race == WoWRace.Tauren && PvPBowSettings.Instance.RS && Me.CurrentTarget.Distance < 8 && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && (SpellManager.Spells["Silencing Shot"].CooldownTimeLeft.TotalSeconds > 1 || Me.CurrentTarget.Distance < 5))
				{
					if (CastSpell("War Stomp"))
					{
					Logging.Write(Color.Aqua, ">> War Stomp, Interrupt <<");
					}
				}
				if (PvPBowSettings.Instance.SCAT && Me.CurrentTarget.Distance < 10 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !Invulnerable(Me.CurrentTarget))
				{
					if (CastSpell("Scatter Shot"))
					{
					Logging.Write(Color.Aqua, ">> Scatter Shot, Evade <<");
					}
				}
				if (PvPBowSettings.Instance.RS && Me.Race == WoWRace.Tauren && (Me.CurrentTarget.Distance < 8 || Me.CurrentTarget.Pet.Distance < 8) && (Me.CurrentTarget.CurrentTargetGuid == Me.Guid || Me.CurrentTarget.Pet.CurrentTargetGuid == Me.Guid) && (NeedSnare(Me.CurrentTarget) || NeedSnare(Me.CurrentTarget.Pet)) && (!Invulnerable(Me.CurrentTarget) || Me.CurrentTarget.Pet.Distance < 8))
				{
					if (CastSpell("War Stomp"))
					{
					Logging.Write(Color.Aqua, ">> War Stomp, Evade <<");
					}
				}
				if (PvPBowSettings.Instance.WEB && NeedSnare(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !WoWSpell.FromId(4167).Cooldown && Me.CurrentTarget.Distance <= 30)
				{
					Lua.DoString("RunMacroText('/cast Web');");	
					{
					Logging.Write(Color.Crimson, ">> Pet: Spider Web <<");
					}
				}
				if (PvPBowSettings.Instance.CONC && NeedSnare(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && DebuffTime("Wing Clip", Me.CurrentTarget) <= 1 && DebuffTime("Concussive Shot", Me.CurrentTarget) <= 1 && DebuffTime("Concussive Barrage", Me.CurrentTarget) <= 1 && Me.CurrentTarget.Distance <= 40 && Me.CurrentTarget.Distance >= 5)
				{
					if (CastSpell("Concussive Shot"))
					{
					Logging.Write(Color.Aqua, ">> Concussive Shot <<");
					}
				}
				if (PvPBowSettings.Instance.CONC && NeedSnare(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && DebuffTime("Wing Clip", Me.CurrentTarget) <= 1 && DebuffTime("Concussive Shot", Me.CurrentTarget) <= 1 && DebuffTime("Concussive Barrage", Me.CurrentTarget) <= 1 && Me.CurrentTarget.Distance <= 40 && Me.CurrentTarget.Distance >= 5 && Me.CurrentTarget.IsMoving)
				{
					if (CastSpell("Concussive Shot"))
					{
					Logging.Write(Color.Aqua, ">> Concussive Shot <<");
					}
				}
				if (PvPBowSettings.Instance.MLE && NeedSnare(Me.CurrentTarget) && !Invulnerable(Me.CurrentTarget) && !Me.IsCasting && DebuffTime("Wing Clip", Me.CurrentTarget) < 2 && Me.CurrentTarget.Distance < 5)
				{
					if (CastSpell("Wing Clip"))
					{
					Logging.Write(Color.Aqua, ">> Wing Clip<<");
					}
				}
				if (PvPBowSettings.Instance.MLE && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.Distance < 5 && !Me.IsCasting)
				{
					if (CastSpell("Raptor Strike"))
					{
					Logging.Write(Color.Aqua, ">> Raptor Strike <<");
					}
				}
				if (PvPBowSettings.Instance.AGR && ((Me.CurrentTarget.GotAlivePet && Me.CurrentTarget.Pet.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Pet.Distance <= 5) || (Me.CurrentTarget.IsPlayer && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.IsCasting && Me.CanInterruptCurrentSpellCast && WoWSpell.FromId(Me.CurrentTarget.CastingSpellId).SpellEffect1.EffectType != WoWSpellEffectType.Heal)))
				{
					if (CastSpell("Feign Death"))
					{
						System.Threading.Thread.Sleep(1000);
						Logging.Write(Color.Aqua, ">> Feign Death <<");
					}
				}
				if (PvPBowSettings.Instance.DIS && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Distance <= 5  && Me.HealthPercent < 60)
				{
					if (CastSpell("Disengage"))
					{
					Logging.Write(Color.Aqua, ">> Disengage <<");
					}
				}
				if (PvPBowSettings.Instance.DETR &&	Me.HealthPercent < 20 && Me.CurrentTarget.CurrentTargetGuid == Me.Guid)
				{
					if (CastSpell("Deterrence"))
					{
					Logging.Write(Color.Aqua, ">> Deterrence <<");
					}
				}
				if (PvPBowSettings.Instance.ICET && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Distance < 5)
				{
					if (CastSpell("Ice Trap"))
					{
					Logging.Write(Color.Aqua, ">> Ice Trap<<");
					}
				}
				if (PvPBowSettings.Instance.SNAT && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Distance < 5)
				{
					if (CastSpell("Snake Trap"))
					{
					Logging.Write(Color.Aqua, ">> Snake Trap<<");
					}
				}
				if (PvPBowSettings.Instance.EXPT && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Distance < 5)
				{
					if (CastSpell("Explosive Trap"))
					{
					Logging.Write(Color.Aqua, ">> Explosive Trap<<");
					}
				}
				if (PvPBowSettings.Instance.FRET && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && Me.CurrentTarget.Distance < 5)
				{
					if (CastSpell("Freezing Trap"))
					{
					Logging.Write(Color.Aqua, ">> Freezing Trap<<");
					}
				}
/////////////////////////////////////////////////////Cooldowns are here/////////////////////////////////////////////////////////////////////////////////////////////////           
				if (PvPBowSettings.Instance.RF && !Me.ActiveAuras.ContainsKey("Rapid Fire") && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 20 && Me.CurrentTarget.Distance > 9 && !Invulnerable(Me.CurrentTarget))
				{
					if (CastSpell("Rapid Fire"))
					{
						Logging.Write(Color.Aqua, ">> Rapid Fire <<");
					}
				}
				if (PvPBowSettings.Instance.RF && SpellManager.Spells["Rapid Fire"].CooldownTimeLeft.TotalSeconds > 100 && (SpellManager.Spells["Silencing Shot"].Cooldown || SpellManager.Spells["Scatter Shot"].Cooldown || SpellManager.Spells["Disengage"].Cooldown))
				{
					if (CastSpell("Readiness"))
					{
						Logging.Write(Color.Aqua, ">> Readiness <<");
					}
				}
				if (PvPBowSettings.Instance.ROR && !Invulnerable(Me.CurrentTarget) && Me.CurrentTarget.CurrentTargetGuid == Me.Guid && !WoWSpell.FromId(53517).Cooldown && Me.CurrentTarget.Distance <= 30)
				{
					Lua.DoString("RunMacroText('/cast Roar Of Recovery');");	
					{
					Logging.Write(Color.Crimson, ">> Pet: Roar Of Recovery <<");
					}
				}
				if (PvPBowSettings.Instance.ROS && !WoWSpell.FromId(53480).Cooldown && (Me.HealthPercent < 60 || ((isStunned(Me).TotalSeconds > 2 || isControlled(Me).TotalSeconds > 2) && ((!PvPBowSettings.Instance.T1MOB && !PvPBowSettings.Instance.T2MOB) || ((PvPBowSettings.Instance.T1MOB && Me.Inventory.Equipped.Trinket1.Cooldown > 1) || (PvPBowSettings.Instance.T2MOB && Me.Inventory.Equipped.Trinket2.Cooldown > 1))))))
				{
					Lua.DoString("RunMacroText(\"/cast [@" + Me.Name + "] Roar Of Sacrifice\")");
					{
					Logging.Write(Color.Crimson, ">> Pet: Roar Of Sacrifice <<");
					}
				}
				if (PvPBowSettings.Instance.LB && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 10 && !Invulnerable(Me.CurrentTarget))
				{
					if (CastSpell("Lifeblood"))
					{
						Logging.Write(Color.Aqua, ">> Lifeblood <<");
					}
				}
				if (PvPBowSettings.Instance.GE && !Invulnerable(Me.CurrentTarget) && HostilePlayer(Me.CurrentTarget) && Me.Inventory.Equipped.Hands != null && Me.Inventory.Equipped.Hands.Usable && Me.Inventory.Equipped.Hands.CooldownTimeLeft.TotalSeconds == 0 && Me.CurrentTarget.HealthPercent > 10)
				{
					Lua.DoString("RunMacroText('/use 10');");
				}
//////////////////////////////////////////////////Racial Skills here/////////////////////////////////////////////////////////////////////////////////////////
				if (PvPBowSettings.Instance.RS && !Invulnerable(Me.CurrentTarget) && Me.Race == WoWRace.Troll && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 20 && !SpellManager.Spells["Berserking"].Cooldown)
				{
					Lua.DoString("RunMacroText('/Cast Berserking');");
				}
				if (PvPBowSettings.Instance.RS && !Invulnerable(Me.CurrentTarget) && Me.Race == WoWRace.Orc && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.HealthPercent > 20 && !SpellManager.Spells["Blood Fury"].Cooldown)
				{
					Lua.DoString("RunMacroText('/Cast Blood Fury');");
				}
			}
////////////////////////////////////////////////// Trap Launchers /////////////////////////////////////////////////////////////////////////
				if (PvPBowSettings.Instance.TL4 && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.InLineOfSight && !Me.CurrentTarget.IsMoving && !SpellManager.Spells["Explosive Trap"].Cooldown && !Me.HasAura("Trap Launcher") && Me.CurrentTarget.Distance >= 20 && Me.CurrentTarget.Distance < 40 && Me.CurrentTarget.HealthPercent > 20)
				{
					if (CastSpell("Trap Launcher"))
					{
						Logging.Write(Color.Red, ">> Trap Launcher Activated! <<");
					}
				}
				if (Me.HasAura("Trap Launcher") && PvPBowSettings.Instance.TL4)
				{
					Lua.DoString("CastSpellByName('Explosive Trap');");
					{
						LegacySpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
					}
				}
				if (PvPBowSettings.Instance.TL && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.InLineOfSight && !Me.CurrentTarget.IsMoving && !SpellManager.Spells["Ice Trap"].Cooldown && !Me.HasAura("Trap Launcher") && Me.CurrentTarget.Distance >= 20 && Me.CurrentTarget.Distance < 40 && Me.CurrentTarget.HealthPercent > 20)
				{
					if (CastSpell("Trap Launcher"))
					{
						Logging.Write(Color.Red, ">> Trap Launcher Activated! <<");
					}
				}
				if (Me.HasAura("Trap Launcher") && PvPBowSettings.Instance.TL)
				{
					Lua.DoString("CastSpellByName('Ice Trap');");
					{
						LegacySpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
					}
				}
				if (PvPBowSettings.Instance.TL2 && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.InLineOfSight && !Me.CurrentTarget.IsMoving && !SpellManager.Spells["Snake Trap"].Cooldown && !Me.HasAura("Trap Launcher") && Me.CurrentTarget.Distance >= 20 && Me.CurrentTarget.Distance < 40 && Me.CurrentTarget.HealthPercent > 20)
				{
					if (CastSpell("Trap Launcher"))
					{
						Logging.Write(Color.Red, ">> Trap Launcher Activated! <<");
					}
				}
				if (Me.HasAura("Trap Launcher") && PvPBowSettings.Instance.TL2)
				{
					Lua.DoString("CastSpellByName('Snake Trap');");
					{
						LegacySpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
					}
				}
				if (PvPBowSettings.Instance.TL3 && HostilePlayer(Me.CurrentTarget) && Me.CurrentTarget.InLineOfSight && !Me.CurrentTarget.IsMoving && !SpellManager.Spells["Freezing Trap"].Cooldown && !Me.HasAura("Trap Launcher") && Me.CurrentTarget.Distance >= 20 && Me.CurrentTarget.Distance < 40 && Me.CurrentTarget.HealthPercent > 20)
				{
					if (CastSpell("Trap Launcher"))
					{
						Logging.Write(Color.Red, ">> Trap Launcher Activated! <<");
					}
				}
				if (Me.HasAura("Trap Launcher") && PvPBowSettings.Instance.TL3)
				{
					Lua.DoString("CastSpellByName('Freezing Trap');");
					{
						LegacySpellManager.ClickRemoteLocation(StyxWoW.Me.CurrentTarget.Location);
					}
				}
////////////////////////////////////////////////// MM Spec Rotation /////////////////////////////////////////////////////////////////////////////////////////
			if (Me.CurrentTarget.Distance >= 5 && HaltTrap() && HaltFeign() && !Invulnerable(Me.CurrentTarget) && Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.Mounted && !Me.Dead)
			{
				if (PvPBowSettings.Instance.STING && !IsMyAuraActive(Me.CurrentTarget, "Serpent Sting"))
				{
					if (CastSpell("Serpent Sting"))
					{
						Logging.Write(Color.Aqua, ">> Serpent Sting <<");
					}
				}
				if (Me.CurrentTarget.HealthPercent <= 6 && Me.IsCasting && Me.CastingSpell.Name == "Steady Shot" && Me.CurrentCastTimeLeft.TotalMilliseconds > 500 && !SpellManager.Spells["Kill Shot"].Cooldown)
				{
					SpellManager.StopCasting();
					{
						Logging.Write(Color.Aqua, ">> Kill Shot Time, Stop Casting <<");
					}
				}
				if (Me.CurrentTarget.HealthPercent <= 20)
				{
					if (CastSpell("Kill Shot"))
					{
						Logging.Write(Color.Aqua, ">> Kill Shot <<");
					}
				}
				if (PvPBowSettings.Instance.WVE && Me.CurrentFocus > 58 && Me.CurrentTarget.IsPlayer && SpellManager.Spells["Chimera Shot"].CooldownTimeLeft.TotalSeconds > 1 && (!IsMyAuraActive(Me.CurrentTarget, "Widow Venom") || MyDebuffTime("Widow Venom", Me.CurrentTarget) <= 1))
				{
					if (CastSpell("Widow Venom"))
					{
						Logging.Write(Color.Aqua, ">> Widow Venom <<");
					}
				}
				if ((PvPBowSettings.Instance.STING && MyDebuffTime("Serpent Sting", Me.CurrentTarget) >= 1) || !PvPBowSettings.Instance.STING)
				{
					if (CastSpell("Chimera Shot"))
					{
						Logging.Write(Color.Aqua, ">> Chimera Shot <<");
					}
				}
				if (PvPBowSettings.Instance.HM && !Me.CurrentTarget.HasAura("Hunter's Mark") && !Me.CurrentTarget.HasAura("Marked for Death"))
				{
					if (CastSpell("Hunter's Mark"))
					{
						Logging.Write(Color.Aqua, ">> Hunter's Mark <<");
					}
				}
				if (SpellManager.Spells["Chimera Shot"].CooldownTimeLeft.TotalMilliseconds > 500 && Me.Auras.ContainsKey("Fire!"))
				{
					Lua.DoString(String.Format("RunMacroText(\"/use Aimed Shot!\")"));
					SpellManager.StopCasting();
					{
						Logging.Write(Color.Aqua, ">> Free Aimed Shot <<");
					}
				}
				if ((Me.CurrentFocus >= 60 && SpellManager.Spells["Chimera Shot"].CooldownTimeLeft.TotalSeconds > 1 && SpellManager.Spells["Chimera Shot"].CooldownTimeLeft.TotalSeconds < 4) || (Me.CurrentFocus >= 26 && SpellManager.Spells["Chimera Shot"].CooldownTimeLeft.TotalSeconds >= 4))
				{
					if (CastSpell("Arcane Shot"))
					{
						Logging.Write(Color.Aqua, ">> Arcane Shot <<");
					}
				}
				if (PvPBowSettings.Instance.AIM && Me.CurrentTarget.HealthPercent > 90 && Me.CurrentFocus > 90 && Me.CurrentTarget.Distance >= 35 && Me.CurrentTarget.CurrentTargetGuid != Me.Guid && !Me.CurrentTarget.IsMoving) 
				{
					if (CastSpell("Aimed Shot"))
					{
						Logging.Write(Color.Aqua, ">> Aimed Shot <<");
					}
				}
				if (!Me.IsMoving && !Me.IsCasting && Me.CurrentFocus <= PvPBowSettings.Instance.FocusShots && FocusShot())
				{
					if (CastSpell("Steady Shot"))
					{
						Logging.Write(Color.Aqua, ">> Steady Shot <<");
					}
				}	   
			}
/////////////////////////////////////////////// Moving Rotation here ////////////////////////////////////////////////////////////////////////////////////////////
			if (HaltTrap() && HaltFeign() && Me.GotTarget && Me.CurrentTarget.IsAlive && !Me.Mounted)
            {
				if (!Me.Auras.ContainsKey("Aspect of the Hawk") && PvPBowSettings.Instance.AspectSwitching && (!Me.IsMoving || Me.CurrentFocus >= 60))
				{
					if (CastSpell("Aspect of the Hawk"))
					{
						Logging.Write(Color.Aqua, ">> Not moving - Aspect of the Hawk <<");
					}
				}
				if (PvPBowSettings.Instance.AspectSwitching && Me.IsMoving && !Me.Auras.ContainsKey("Aspect of the Fox") && Me.CurrentFocus < 50)
				{
					if (CastSpell("Aspect of the Fox"))
					{
						Logging.Write(Color.Aqua, ">> Moving Below 60 Focus - Asp. of the Fox <<");
					}
				}
				if (Me.IsMoving && !Me.IsCasting && Me.Auras.ContainsKey("Aspect of the Fox") && Me.CurrentFocus <= PvPBowSettings.Instance.FocusShots && FocusShot())
				{
					Lua.DoString("RunMacroText('/cast Steady Shot');");
					{
						Logging.Write(Color.Red, ">> Moving - Steady Shot <<");
					}
				}
			}
        }
		#endregion
    }
}