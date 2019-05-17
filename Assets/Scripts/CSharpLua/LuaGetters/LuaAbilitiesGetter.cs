using System;
using System.Collections.Generic;
using System.Linq;
using GameUtils.Objects;
using GameUtils.UsualUtils;
using MoonSharp.Interpreter;
using UnityEngine;

namespace CSharpLua.LuaGetters
{
    public class LuaAbilitiesGetter
    {
        private static Table _luaAbilitiesManager;
        private static Table _luaAbilitiesEvents;

        private static DynValue _OnOwnerSpawned;
        private static DynValue _OnSpellStart;
        private static DynValue _GetAbilityEventsList;

        public static void Init()
        {
            _luaAbilitiesManager = LuaManager.Vm.GetGlobalTable("LuaAbilityManager");
            _luaAbilitiesEvents = LuaManager.Vm.GetGlobalTable("LuaAbilityEvents");

            _GetAbilityEventsList = _luaAbilitiesManager.Get("GetAbilityEventsList");

            _OnOwnerSpawned = _luaAbilitiesEvents.Get("OnOwnerSpawned");
            _OnSpellStart = _luaAbilitiesEvents.Get("OnSpellStart");


            _luaAbilitiesManager["GetAbility"] = (Func<string, Ability>) (GetAbility);
        }

        public static Table GetAbilityTableByName(string name)
        {
            var f = _luaAbilitiesManager.Get("GetAbilityByName");
            return LuaManager.Vm.Call(f, name).Table;
        }

        public static Ability GetAbility(string name)
        {
            var t = GetAbilityTableByName(name);
            return GetAbility(t);
        }

        public static Ability GetAbility(Table abilityTable)
        {
            var abilityName = "";
            var AbilityTextureName = "";
            var AbilityRequirements = new List<string>();
            var MaxLevel = 1;
            var CoolDovvn = 0f;
            var AbilityCastRange = -1;
            var AbilityCooldown = 0.0f;
            var AbilityManaCost = 0;
            var abilityBehavior =
                AbilityProperties.AbilityBehaviorEnum.EVOLO_ABILITY_BEHAVIOR_HIDDEN;
            var AbilityType = AbilityProperties.AbilityTypeEnum.EVOLO_ABILITY_TYPE_BASIC;
            var AbilityUnitTargetTeam = AbilityProperties.AbilityUnitTargetTeamEnum.EVOLO_UNIT_TARGET_TEAM_NONE;
            var AbilityUnitTargetType = AbilityProperties.AbilityUnitTargetTypeEnum.EVOLO_UNIT_TARGET_NONE;
            var AbilityUnitDamageType = AbilityProperties.AbilityUnitDamageTypeEnum.DAMAGE_TYPE_NONE;

            foreach (var KV in abilityTable.Pairs)
            {
                if (string.Equals(KV.Key.String, "name",
                    StringComparison.CurrentCultureIgnoreCase))
                    abilityName = KV.Value.String;

                if (string.Equals(KV.Key.String, "AbilityTextureName",
                    StringComparison.CurrentCultureIgnoreCase))
                    AbilityTextureName = KV.Value.String;

                if (string.Equals(KV.Key.String, "ResearchRequirement",
                    StringComparison.CurrentCultureIgnoreCase))
                    AbilityRequirements.AddRange(GetAbilityRequirements(KV.Value.Table));

                if (string.Equals(KV.Key.String, "MaxLevel",
                    StringComparison.CurrentCultureIgnoreCase))
                    int.TryParse(KV.Value.String, out MaxLevel);

                if (string.Equals(KV.Key.String, "AbilityBehavior",
                    StringComparison.CurrentCultureIgnoreCase))
                    abilityBehavior = Util.ToEnum(KV.Value.String,
                        AbilityProperties.AbilityBehaviorEnum.EVOLO_ABILITY_BEHAVIOR_HIDDEN);

                if (string.Equals(KV.Key.String, "AbilityUnitTargetTeam",
                    StringComparison.CurrentCultureIgnoreCase))
                    AbilityUnitTargetTeam = Util.ToEnum(KV.Value.String,
                        AbilityProperties.AbilityUnitTargetTeamEnum.EVOLO_UNIT_TARGET_TEAM_NONE);

                if (string.Equals(KV.Key.String, "AbilityUnitTargetType",
                    StringComparison.CurrentCultureIgnoreCase))
                    AbilityUnitTargetType = Util.ToEnum(KV.Value.String,
                        AbilityProperties.AbilityUnitTargetTypeEnum.EVOLO_UNIT_TARGET_NONE);

                if (string.Equals(KV.Key.String, "AbilityCooldown",
                    StringComparison.CurrentCultureIgnoreCase))
                    float.TryParse(KV.Value.String, out AbilityCooldown);

                if (string.Equals(KV.Key.String, "AbilityManaCost",
                    StringComparison.CurrentCultureIgnoreCase))
                    int.TryParse(KV.Value.String, out AbilityManaCost);

                if (string.Equals(KV.Key.String, "AbilityType",
                    StringComparison.CurrentCultureIgnoreCase))
                    AbilityType = Util.ToEnum(KV.Value.String,
                        AbilityProperties.AbilityTypeEnum.EVOLO_ABILITY_TYPE_BASIC);

                if (string.Equals(KV.Key.String, "AbilityCastRange",
                    StringComparison.CurrentCultureIgnoreCase))
                    AbilityCastRange = (int) KV.Value.Number;

                if (string.Equals(KV.Key.String, "AbilityUnitDamageType",
                    StringComparison.CurrentCultureIgnoreCase))
                    AbilityUnitDamageType =
                        Util.ToEnum(KV.Value.String, AbilityProperties.AbilityUnitDamageTypeEnum.DAMAGE_TYPE_NONE);
            }

            return new Ability(abilityTable,
                abilityName,
                AbilityTextureName,
                AbilityRequirements,
                MaxLevel,
                AbilityCastRange,
                AbilityCooldown,
                AbilityManaCost,
                abilityBehavior,
                AbilityType,
                AbilityUnitTargetTeam,
                AbilityUnitTargetType,
                AbilityUnitDamageType
            );
        }

        private static List<string> GetAbilityRequirements(Table t)
        {
            return t.Keys.Select(v => v.String).ToList();
        }

        private static Dictionary<string, Table>
            GetAbilityEventsList(Table ab_T, Table eventNames)
        {
            var dict = new Dictionary<string, Table>();
            var eventsList = LuaManager.Vm.Call(_GetAbilityEventsList, ab_T, eventNames).Table;

            foreach (var KV in eventsList.Pairs)
            {
                if (KV.Value.IsNilOrNan()) continue;

                dict.Add(KV.Key.String, KV.Value.Table);
            }

            return dict;
        }

        public static Dictionary<string, Table> GetEventsPreset(Table ab_T)
        {
            var t = DynValue.NewTable(LuaManager.ScriptObj).Table;
            var i = 0;
            foreach (var events in Enum.GetValues(typeof(AbilityProperties.eventNames)))
                t.Append(DynValue.NewString(events.ToString()));

            return LuaAbilitiesGetter.GetAbilityEventsList(ab_T, t);
        }

        public static Dictionary<string, string> GetSpecials(Table abilityTable)
        {
            var KVlist = new Dictionary<string, string>();
            var AbilitySpecial = abilityTable.Get("AbilitySpecial");
            if (AbilitySpecial.IsNilOrNan()) return KVlist;
            foreach (var KV in AbilitySpecial.Table.Pairs)
                KVlist.Add(KV.Key.String, KV.Value.String);

            return KVlist;
        }

        public static Table GetAbilityModifiersTable(Table abilityTable)
        {
            var t = abilityTable.Get("Modifiers");
            return t.IsNilOrNan() ? null : t.Table;
        }

        public static void OnOwnerSpawned(Table eventTable)
        {
            LuaManager.Vm.Call(_OnOwnerSpawned, eventTable);
        }

        public static bool OnSpellStart(Table eventTable)
        {
            var ev = LuaManager.Vm.Call(_OnSpellStart, eventTable);
            return ev.Type != DataType.Boolean || ev.Boolean;
        }
    }
}