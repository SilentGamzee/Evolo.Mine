using System.Collections.Generic;
using System.Linq;
using GameUtils;
using GameUtils.Objects;
using GameUtils.UsualUtils;
using MoonSharp.Interpreter;
using UnitsMechanic;
using UnityEngine;

namespace CSharpLua
{
    public class LuaNpcGetter
    {
        private static Table _luaNpcManager;

        private const bool IsGetterDebug = false;

        public static void Init()
        {
            _luaNpcManager = LuaManager.Vm.GetGlobalTable("LuaNpcManager");
        }

        public static Table GetNpcById(int id)
        {
            var f = _luaNpcManager.Get("GetNpcById");
            return LuaManager.Vm.Call(f, id).Table;
        }

        public static string GetNpcNameById(int id)
        {
            var f = _luaNpcManager.Get("GetNpcNameById");
            return LuaManager.Vm.Call(f, id).String;
        }

        public static string GetNpcModelById(int id)
        {
            var f = _luaNpcManager.Get("GetNpcModelById");
            return LuaManager.Vm.Call(f, id).String;
        }

        public static string GetEvolutionTo(Table npc)
        {
            var dynValue = npc.Get("evoTo");
            return dynValue.IsNilOrNan() ? "" : dynValue.String;
        }


        public static Table GetNpcByName(string name)
        {
            var f = _luaNpcManager.Get("GetNpcByName");
            return LuaManager.Vm.Call(f, name).Table;
        }

        public static int GetNpcId(Table npc)
        {
            var dynValue = npc.Get("id");
            var num = dynValue.CastToNumber();


            if (num != null && (!dynValue.IsNilOrNan() && (int) num != -1))
                return (int) num;
            Debug.LogError("Cannot get npc id");
            return Loader.GetErrorIndex();
        }

        public static string GetNpcModel(Table npc)
        {
            var dynValue = npc.Get("model");
            if (!dynValue.IsNilOrNan())
                return dynValue.String;
            Debug.LogError("Cannot get npc model");
            return "";
        }

        public static bool IsNpcSoloEvolution(Table npc)
        {
            var dynValue = npc.Get("soloEvolution");
            return !dynValue.IsNilOrNan() && dynValue.String.ToLower() == "true";
        }

        public static bool IsNpcSoloEvolution(int id)
        {
            var npc = GetNpcById(id);
            return IsNpcSoloEvolution(npc);
        }

        public static string GetNpcGroup(Table npc)
        {
            var dynValue = npc.Get("group");
            return dynValue.String;
        }

        public static string GetNpcGroupById(int id)
        {
            var f = _luaNpcManager.Get("GetNpcGroupById");
            return LuaManager.Vm.Call(f, id).String;
        }


        public static Stats GetStatsFromTable(Table npc)
        {
            var stats = new Stats();

            var statsTable = npc.Get("Stats");
            if (statsTable.IsNilOrNan()) return stats;

            foreach (var pair in statsTable.Table.Pairs)
            {
                if (pair.Key == null || pair.Value == null) continue;

                if (pair.Key.String.ToLower() == "hp")
                {
                    var castToNumber = pair.Value.CastToNumber();
                    if (castToNumber != null) stats.Hp = (int) castToNumber;
                }

                if (pair.Key.String.ToLower() == "attack_dmg")
                {
                    var castToNumber = pair.Value.CastToNumber();
                    if (castToNumber != null) stats.Dmg = (int) castToNumber;
                }

                if (pair.Key.String.ToLower() == "movespeed")
                {
                    var castToNumber = pair.Value.CastToNumber();
                    if (castToNumber != null) stats.MoveSpeed = (float) castToNumber;
                }
            }
            return stats;
        }

        public static Dictionary<string, string> GetNpcEvoCrossing(Table npc)
        {
            var evoCrossDict = new Dictionary<string, string>();

            var evoCross = npc["EvoCrossing"] as Table;
            if (evoCross != null)
            {
                foreach (var pair in evoCross.Pairs)
                {
                    var second = pair.Key.String;
                    var result = pair.Value.String;
                    evoCrossDict[second] = result;
                }
            }

            return evoCrossDict;
        }


        public static Stats GetNpcStatsById(int id)
        {
            var f = _luaNpcManager.Get("GetNpcStatsById");
            var ret = LuaManager.Vm.Call(f, id);

            var stats = new Stats();
            foreach (var pair in ret.Table.Pairs)
            {
                if (pair.Key == null || pair.Value == null) continue;

                if (pair.Key.String.ToLower() == "hp")
                {
                    var castToNumber = pair.Value.CastToNumber();
                    if (castToNumber != null) stats.Hp = (int) castToNumber;
                }

                if (pair.Key.String.ToLower() == "attack_dmg")
                {
                    var castToNumber = pair.Value.CastToNumber();
                    if (castToNumber != null) stats.Dmg = (int) castToNumber;
                }
            }

            return stats;
        }

        public static Table GetItemsTable()
        {
            var f = _luaNpcManager.Get("GetItemsTable");
            return LuaManager.Vm.Call(f).Table;
        }

        public static Dictionary<string, Dictionary<string, string>> GetItemsStucks()
        {
            var t = GetItemsTable();

            var stackResult = new Dictionary<string, Dictionary<string, string>>();

            foreach (var pair in t.Pairs)
            {
                if (pair.Key == null || pair.Value == null) continue;

                var stuckvar = pair.Value.Table["ItemStucks"] as Table;
                if (stuckvar != null)
                {
                    var firstItem = pair.Key.String;
                    if (!stackResult.ContainsKey(firstItem))
                        stackResult[firstItem] = new Dictionary<string, string>();


                    foreach (var itemStuckPair in stuckvar.Pairs)
                    {
                        var secondItem = itemStuckPair.Key.String;
                        var result = itemStuckPair.Value.String;
                        if (!stackResult[firstItem].ContainsKey(secondItem))
                            stackResult[firstItem].Add(secondItem, result);

                        if (!stackResult.ContainsKey(secondItem))
                            stackResult[secondItem] = new Dictionary<string, string>();

                        if (!stackResult[secondItem].ContainsKey(firstItem))
                            stackResult[secondItem].Add(firstItem, result);
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (IsGetterDebug)
                            Debug.LogError("Adding item stuck: " + firstItem + " + " + secondItem + " = " + result);
                    }
                }
            }
            return stackResult;
        }

        public static string GetNpcSoundPathById(int id)
        {
            var npc = GetNpcById(id);
            var soundValue = npc.Get("Sound");
            return !soundValue.IsNilOrNan() ? soundValue.String : "";
        }

        public static float GetNpcSoundVolume(Table npc)
        {
            var volume = npc.Get("SoundVolume");
            if (volume.IsNilOrNan()) return 1f;
            var castToNumber = volume.CastToNumber();
            if (castToNumber == null) return 1f;

            var fVolume = castToNumber.Value;
            return (float) fVolume;
        }

        public static string GetNpcItemDrop(Table npc)
        {
            var itemDrop = npc.Get("itemDrop");
            return itemDrop.IsNilOrNan() ? "" : itemDrop.String;
        }

        public static List<string> GetNpcModifiersNames(Table npc)
        {
            var modifiers = npc.Get("Modifiers");
            var list = new List<string>();
            if (modifiers.IsNilOrNan()) return list;

            list.AddRange(modifiers.Table.Keys.Select(key => key.String));

            return list;
        }

        public static string GetNpcName(Table npc)
        {
            var name = npc.Get("name");
            return name.IsNilOrNan() ? "" : name.String;
        }

        public static Dictionary<int, string> GetNpcAbilitiesNames(Table npc)
        {
            var abilityList = new Dictionary<int, string>();

            var abilitiesTable = npc.Get("Abilities");

            if (!abilitiesTable.IsNilOrNan())
            {
                foreach (var KV in abilitiesTable.Table.Pairs)
                {
                    int i;
                    var intParse = int.TryParse(KV.Key.String.Replace("Ability", ""), out i);
                    var skill = KV.Value.String;

                    i--;
                    if (skill.Length > 0 && intParse)
                    {
                        abilityList[i] = skill;
                    }
                }
            }

            return abilityList;
        }

        public static int GetNpcFoodGive(Table npc)
        {
            var foodGive = npc.Get("foodGive");
            return foodGive.IsNilOrNan() ? 0 : int.Parse(foodGive.String);
        }

        public static KeyValuePair<float, float> GetNpcGoldDrop(Table npc)
        {
            var minValue = npc.Get("GoldMin");
            var min = 0;
            if (minValue.IsNilOrNan()) min = 0;
            else
                int.TryParse(minValue.String, out min);

            var maxValue = npc.Get("GoldMax");
            var max = 0;
            if (maxValue.IsNilOrNan()) max = 0;
            else
                int.TryParse(maxValue.String, out max);
            return new KeyValuePair<float, float>(min, max);
        }

        public static int GetNpcFoodCost(Table npc)
        {
            var foodCost = npc.Get("foodCost");
            return foodCost.IsNilOrNan() ? 0 : int.Parse(foodCost.String);
        }

        public static string GetNpcResearch(Table npc)
        {
            var research = npc.Get("research");
            return research.IsNilOrNan() ? "" : research.String;
        }
    }
}