using System;
using System.Collections.Generic;
using CSharpLua;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GlobalMechanic.NonInteract;
using MoonSharp.Interpreter;
using UnitsMechanic;
using UnitsMechanic.AI_Logic;
using UnityEngine;

namespace GameUtils.UsualUtils
{
    [MoonSharpUserData]
    public class ChunkFinder
    {
        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["FindInRadius"] = (Func<Vector3Int, int, Table>) (FindInRadius);
            t["FindNearestGroup"] = (Func<Vector3Int, Table, string, DynValue>) (FindNearestGroup);
            t["FindNpcByName"] = (Func<string, Player, Table>) (FindNpcByName);
            t["FindInRadius"] = (Func<GameUnit, int, Table>) (FindInRadius);
            t["FindEnemiesInRadius"] = (Func<GameUnit, int, Table>) (FindEnemiesInRadius);
            t["FindNearestInTable"] = (Func<GameUnit, Table, DynValue>) (FindNearestInTable);
            t["FindNearestReacheble"] = (Func<GameUnit, Table, DynValue>) (FindNearestReacheble);
            t["GetNpcByPlayer"] = (Func<Player, Table>) (GetNpcByPlayer);

            LuaManager.Vm.SetGlobal("ChunkFinder", t);
        }

        public static DynValue FindNearestReacheble(GameUnit unit, Table t)
        {
            var dist = -1;
            GameEntity nearest = null;
            foreach (var v in t.Values)
            {
                var ent = v.ToObject<GameEntity>();
                if (ent == null) continue;

                if (!PathCalcManager.IsReaching(ChunkUtil.GetDovvner(unit.CurrentPos),
                    ChunkUtil.GetDovvner(ent.CurrentPos))) continue;

                var d = AI_Calculation.GetDistTo(unit.CurrentPos, ent.CurrentPos, false);
                if (dist == -1 || d < dist)
                {
                    dist = d;
                    nearest = ent;
                }
            }
            return nearest == null ? DynValue.Nil : DynValue.FromObject(LuaManager.ScriptObj, nearest);
        }

        public static DynValue FindNearestInTable(GameUnit unit, Table t)
        {
            var dist = -1;
            GameEntity nearest = null;
            foreach (var v in t.Values)
            {
                var ent = v.ToObject<GameEntity>();
                if (ent == null) continue;

                var d = AI_Calculation.GetDistTo(unit.CurrentPos, ent.CurrentPos, false);
                if (dist == -1 || d < dist)
                {
                    dist = d;
                    nearest = ent;
                }
            }
            return nearest == null ? DynValue.Nil : DynValue.FromObject(LuaManager.ScriptObj, nearest);
        }

        public static Table FindInRadius(GameUnit unit, int radius)
        {
            var t = DynValue.NewTable(LuaManager.ScriptObj).Table;
            var point = unit.CurrentPos;

            var n = 0;
            for (var i = point.x - radius; i <= point.x + radius; i++)
            {
                for (var j = point.y - radius; j <= point.y + radius; j++)
                {
                    if (!ChunkManager.CurrentChunk.IsMapPos(new Vector3Int(i, j, point.z))) continue;
                    var ent = ChunkManager.CurrentChunk.Ground[point.z][i][j];
                    if (ent == null || unit.EntityIndex == ent.EntityIndex) continue;
                    t.Set(DynValue.NewNumber(n), DynValue.FromObject(LuaManager.ScriptObj, ent));
                    n++;
                }
            }
            return t;
        }

        public static Table FindEnemiesInRadius(GameUnit unit, int radius)
        {
            var t = DynValue.NewTable(LuaManager.ScriptObj).Table;
            var point = unit.CurrentPos;

            var n = 0;
            for (var i = point.x - radius; i <= point.x + radius; i++)
            {
                for (var j = point.y - radius; j <= point.y + radius; j++)
                {
                    if (!ChunkManager.CurrentChunk.IsMapPos(new Vector3Int(i, j, point.z))) continue;
                    var ent = ChunkManager.CurrentChunk.Ground[point.z][i][j];
                    if (ent == null
                        || unit.EntityIndex == ent.EntityIndex
                        || unit.PlayerID == ent.PlayerID
                        || GroupUtil.IsNeutral(ent.Group)) continue;
                    t.Set(DynValue.NewNumber(n), DynValue.FromObject(LuaManager.ScriptObj, ent));
                    n++;
                }
            }
            return t;
        }

        public static Table FindInRadius(Vector3Int point, int radius)
        {
            var t = DynValue.NewTable(LuaManager.ScriptObj).Table;

            var n = 0;
            for (var i = point.x - radius; i <= point.x + radius; i++)
            {
                for (var j = point.y - radius; j <= point.y + radius; j++)
                {
                    if (!ChunkManager.CurrentChunk.IsMapPos(new Vector3Int(i, j, point.z))) continue;
                    var ent = ChunkManager.CurrentChunk.Ground[point.z][i][j];
                    if (ent == null) continue;
                    t.Set(DynValue.NewNumber(n), DynValue.FromObject(LuaManager.ScriptObj, ent));
                    n++;
                }
            }
            return t;
        }

        public static DynValue FindNearestGroup(Vector3Int point, Table t, string group)
        {
            if (t.Length == 0) return DynValue.Nil;

            var dist = -1;
            DynValue nearestEnt = null;

            foreach (var entLua in t.Values)
            {
                var ent = entLua.ToObject<GameEntity>();
                if (group.Length > 0 &&
                    !string.Equals(ent.Group, @group, StringComparison.CurrentCultureIgnoreCase)) continue;
                var d = Util.GetDistanceFromTo(ent.CurrentPos, point);
                if (dist == -1)
                {
                    nearestEnt = entLua;
                    dist = d;
                    continue;
                }

                if (d >= dist) continue;
                nearestEnt = entLua;
                dist = d;
            }
            return nearestEnt ?? t.Get(0);
        }

        public static Table FindNpcByName(string name, Player player)
        {
            var t = DynValue.NewTable(LuaManager.ScriptObj).Table;

            var list = player != null
                ? CreatureGroupManager.GetAllByPlayer(player)
                : CreatureGroupManager.GetAll();

            var n = 0;
            foreach (var ent in list)
            {
                if (!ent.IsHisName(name)) continue;
                t[DynValue.NewNumber(n)] = ent;
                n++;
            }

            return t;
        }

        public static Table GetNpcByPlayer(Player player)
        {
            var playerAll = CreatureGroupManager.GetAllByPlayer(player);
            var script = LuaManager.ScriptObj;
            var t = DynValue.NewTable(script).Table;

            foreach (var v in playerAll)
                t.Append(DynValue.FromObject(script, v));

            return t;
        }
    }
}