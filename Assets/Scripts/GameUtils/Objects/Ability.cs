using System;
using System.Collections.Generic;
using System.Linq;
using CSharpLua;
using CSharpLua.LuaGetters;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GUI_Game.InGame.AbilityBar;
using GUI_Game.InGame.PauseMenu;
using MoonSharp.Interpreter;
using UnitsMechanic;
using UnityEngine;

namespace GameUtils.Objects
{
    public class AbilityProperties
    {
        public enum AbilityBehaviorEnum
        {
            EVOLO_ABILITY_BEHAVIOR_NO_TARGET, //Doesn't need a target to be cast. 

            //Requires AbilityUnitTargetTeam and AbilityUnitTargetType
            EVOLO_ABILITY_BEHAVIOR_UNIT_TARGET, //Needs a target to be cast on.
            EVOLO_ABILITY_BEHAVIOR_POINT, //Can be cast anywhere the mouse cursor is 
            EVOLO_ABILITY_BEHAVIOR_PASSIVE, //Cannot be cast
            EVOLO_ABILITY_BEHAVIOR_CHANNELLED, //Channeled ability 
            EVOLO_ABILITY_BEHAVIOR_TOGGLE, //Can be toggled On and Off.
            EVOLO_ABILITY_BEHAVIOR_AURA, //Ability is an aura.
            EVOLO_ABILITY_BEHAVIOR_HIDDEN, //	Can't be cast and won't show up on the HUD.
            EVOLO_ABILITY_BEHAVIOR_AOE, /*Can draws a radius where the ability will have effect. 
            Like POINT but with a an area of effect display. 
            Makes use of AoERadius */

            EVOLO_ABILITY_BEHAVIOR_NOT_LEARNABLE, //	Cannot be learned by clicking on the HUD 
            EVOLO_ABILITY_BEHAVIOR_ITEM, /* Ability is tied up to an item.
             There is no need to use this, the game will internally assign
              this behavior to any "item_datadriven"*/

            EVOLO_ABILITY_BEHAVIOR_DIRECTIONAL, /* 	Has a direction from the hero. 
            Example: Mirana Arrow or Pudge Hook.*/

            EVOLO_ABILITY_BEHAVIOR_IMMEDIATE, //Can be used instantly without going into the action queue.
            EVOLO_ABILITY_BEHAVIOR_ATTACK, //Is an attack and cannot hit attack-immune targets
            EVOLO_ABILITY_BEHAVIOR_ROOT_DISABLES, //Cannot be used when rooted
            EVOLO_ABILITY_BEHAVIOR_RUNE_TARGET //Targets runes.
        }

        public enum AbilityTypeEnum
        {
            EVOLO_ABILITY_TYPE_BASIC,
            EVOLO_ABILITY_TYPE_ULTIMATE,
        }

        public enum AbilityUnitTargetTeamEnum
        {
            EVOLO_UNIT_TARGET_TEAM_BOTH,
            EVOLO_UNIT_TARGET_TEAM_FRIENDLY,
            EVOLO_UNIT_TARGET_TEAM_ENEMY,
            EVOLO_UNIT_TARGET_TEAM_NONE
        }

        public enum AbilityUnitTargetTypeEnum
        {
            EVOLO_UNIT_TARGET_NEUTRAL,
            EVOLO_UNIT_TARGET_BUILDING,
            EVOLO_UNIT_TARGET_CREEP,
            EVOLO_UNIT_TARGET_HERO,
            EVOLO_UNIT_TARGET_NONE
        }

        public enum AbilityUnitDamageTypeEnum
        {
            DAMAGE_TYPE_MAGICAL,
            DAMAGE_TYPE_PHYSICAL,
            DAMAGE_TYPE_PURE,
            DAMAGE_TYPE_NONE
        }

        public enum eventNames
        {
            OnOwnerSpawned,
            OnSpellStart
        }

        public enum eventActions
        {
        }
    }

    [MoonSharpUserData]
    public class Ability
    {
        private static int lastID = 0;

        public int abilityID { get; private set; }
        public string abilityName { get; private set; }

        public Player Owner { get; set; }

        private Dictionary<string, string> abilitySpecial;
        private Dictionary<string, string> upgradedAbilitySpecial;
        private Table eventTable;

        private BigAbility parentBigAbility;

        public float CurrentCooldown { get; set; }

        public float PostCurrent { get; set; }

        public int _currentLvl = 1; //Now Default lvl = 1 //In future lvl = 0

        private List<Modifier> childrenModifiers = new List<Modifier>();

        private AbstractGameObject abilityOwner = null;

        private string onOwnerSpawnedEventName;
        private string onSpellStartEventName;

        public Table vars;

        public Ability(Table AbilityTable,
            string abilityName,
            string texturePath,
            List<string> researchRequirements,
            int maxLevel,
            int abilityCastRange,
            float abilityCooldown,
            int abilityManaCost,
            AbilityProperties.AbilityBehaviorEnum abilityBehavior,
            AbilityProperties.AbilityTypeEnum abilityType,
            AbilityProperties.AbilityUnitTargetTeamEnum abilityUnitTargetTeam,
            AbilityProperties.AbilityUnitTargetTypeEnum abilityUnitTargetType,
            AbilityProperties.AbilityUnitDamageTypeEnum abilityUnitDamageType)
        {
            abilityID = lastID;
            lastID++;
            this.abilityName = abilityName;
            CurrentCooldown = 0f;
            PostCurrent = -1f;

            onOwnerSpawnedEventName = abilityName + "_OnOwnerSpawned";
            onSpellStartEventName = abilityName + "_OnSpellStart";

            eventTable = DynValue.NewTable(LuaManager.ScriptObj).Table;
            var specials = LuaAbilitiesGetter.GetSpecials(AbilityTable);


            abilitySpecial = specials
                .Select(c => new {c.Key, value = c.Value + " "})
                .ToDictionary(x => x.Key, x => x.value);

            upgradedAbilitySpecial = abilitySpecial.ToDictionary(x => x.Key, x => x.Value);

            vars = DynValue.NewTable(LuaManager.ScriptObj).Table;

            parentBigAbility = BigAbilityManager.SetupBigAbility(AbilityTable,
                abilityName,
                texturePath,
                researchRequirements,
                maxLevel,
                abilityCastRange,
                abilityCooldown,
                abilityManaCost,
                abilityBehavior,
                abilityType,
                abilityUnitTargetTeam,
                abilityUnitTargetType,
                abilityUnitDamageType);
        }

        public void SetAbilityOwner(AbstractGameObject obj)
        {
            abilityOwner = obj;
        }

        public List<KeyValuePair<string, string>> GetSpecialsList()
        {
            return abilitySpecial.ToList();
        }

        public bool IsAllRequirementsReady()
        {
            return parentBigAbility.ResearchRequirements.All(req => Owner.HasResearch(req));
        }

        public bool IsHasSpecialValue(string specialKey)
        {
            return specialKey != null && abilitySpecial.ContainsKey(specialKey);
        }

        public DynValue GetAbilitySpecial(string key, string varType)
        {
            return GetAbilitySpecial(key, _currentLvl, varType);
        }

        public DynValue GetAbilitySpecial(string key, int lvl, string varType)
        {
            if (!IsHasSpecialValue(key)) return DynValue.Nil;
            if (lvl <= 0)
                lvl = 1;
            var value = abilitySpecial[key];
            var prevIndex = 0;
            var index = 0;
            int n = -1;
            for (int i = 0; i < lvl; i++)
            {
                if (i == 0)
                    index = value.IndexOf(' ');

                if (i == lvl - 1)
                    break;
                n = value.IndexOf(' ', index + 1);
                if (n == -1)
                    break;
                prevIndex = index;
                index = n;
            }
            if (index == -1)
            {
                prevIndex = 0;
                index = value.Length;
            }
            var specialValue = value.Substring(prevIndex, index - prevIndex);
            // Debug.Log("Special lvl - " + lvl + " : " + key + " = " + specialValue);
            DynValue dyn = null;
            if (varType == "number")
            {
                float num;
                if (float.TryParse(specialValue, out num))
                    dyn = DynValue.NewNumber(num);
            }
            else if (varType == "string")
                dyn = DynValue.NewString(specialValue.Replace(" ", ""));
            else
                dyn = DynValue.Nil;

            return dyn;
        }

        public void SetSpecialValue(string key, int value)
        {
            SetSpecialValue(key, value.ToString());
        }

        public void SetSpecialValue(string key, string value)
        {
            if (!IsHasSpecialValue(key)) return;
            abilitySpecial[key] = value + " ";
        }


        public float GetCooldown()
        {
            return parentBigAbility.AbilityCooldown;
        }

        public AbilityProperties.AbilityBehaviorEnum GetAbilityBehavior()
        {
            return parentBigAbility.AbilityBehavior;
        }

        public List<string> GetResearchRequirements()
        {
            return parentBigAbility.ResearchRequirements;
        }

        public string GetTexturePath()
        {
            return parentBigAbility.texturePath;
        }

        public Table GetModifierTable()
        {
            return parentBigAbility.modifierTable;
        }

        public void AddModifierChild(Modifier mod)
        {
            childrenModifiers.Add(mod);
        }

        public int GetMaxLevel()
        {
            return parentBigAbility.MaxLevel;
        }

        public int GetCurrentLevel()
        {
            return _currentLvl;
        }

        public void SetCurrentLvl(int lvl)
        {
            var max = GetMaxLevel();
            _currentLvl = max <= lvl ? lvl : max;
        }

        public void AddLvl(int lvl)
        {
            var max = GetMaxLevel();
            var p = _currentLvl + lvl;
            _currentLvl = p <= max ? p : max;
        }

        public void OnOwnerSpawned(AbstractGameObject caster)
        {
            if (!parentBigAbility.eventsPreset.ContainsKey("OnOwnerSpawned")) return;
            eventTable["caster"] = caster;
            eventTable["target"] = DynValue.Nil;
            eventTable["ability"] = this;
            eventTable["name"] = onOwnerSpawnedEventName;
            if (UnitEvents.isEventNotPreloaded(eventTable, onOwnerSpawnedEventName))
                eventTable["preset"] = parentBigAbility.eventsPreset["OnOwnerSpawned"];

            LuaAbilitiesGetter.OnOwnerSpawned(eventTable);
        }


        public bool OnSpellStart(AbstractGameObject caster, GameUnit target)
        {
            if (!parentBigAbility.eventsPreset.ContainsKey("OnSpellStart")) return true;
            eventTable["caster"] = caster;
            eventTable["target"] = target;
            eventTable["ability"] = this;
            eventTable["name"] = onSpellStartEventName;

            if (UnitEvents.isEventNotPreloaded(eventTable, onSpellStartEventName))
                eventTable["preset"] = parentBigAbility.eventsPreset["OnSpellStart"];

            return LuaAbilitiesGetter.OnSpellStart(eventTable);
        }

        private float time;

        public void Update()
        {
            if (PauseMenu_HTML.IsPaused || CurrentCooldown <= 0f) return;
            
            time = Time.deltaTime;

            if (CurrentCooldown - time < 0) CurrentCooldown = 0f;
            else CurrentCooldown -= time;

            AbilityBar_HTML.Update(this);
        }
    }
}