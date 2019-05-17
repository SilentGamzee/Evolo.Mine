using System;
using System.Collections.Generic;
using System.Linq;
using CSharpLua;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic;
using GlobalMechanic.NonInteract;
using MoonSharp.Interpreter;
using UnityEngine;

namespace UnitsMechanic.AI_Logic
{
    public class AI_Calculation
    {
        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["FindFreePosNearPos"] = (Func<Vector3Int, bool, Vector3Int>) (FindFreePosNearPos);
            t["GetDistTo"] = (Func<Vector3Int, Vector3Int, bool, int>) (GetDistTo);
            t["IsAttackUnitInMemoryPos"] = (Func<GameUnit, GameUnit, bool>) (IsAttackUnitInMemoryPos);

            LuaManager.Vm.SetGlobal("AI_Calculation", t);
        }

        public static bool IsAttackUnitInMemoryPos(GameUnit unit, GameUnit attackTarget)
        {
            if (!unit.IsMoving) return false;
            var pos =unit.MovingPath.LastOrDefault();
            if (pos == null) return false;
            return ChunkUtil.GetDovvner(attackTarget.CurrentPos) == pos.Index;
        }
        
        public static bool IsTreeIndex(int index)
        {
            return ChunkManager.treeIndexes.Contains(index);
        }


        public static int GetDistTo(Vector3Int pos, Vector3Int pos1, bool includeZ)
        {
            var distVec = pos - pos1;
            if (pos == Vector3Int.zero || pos1 == Vector3Int.zero) return 1000;
            var dist = Mathf.Abs(distVec.x) + Mathf.Abs(distVec.y);
            if (includeZ) dist += Mathf.Abs(distVec.z);
            return dist;
        }

        public static Vector3Int GetNearestNeighbour(int chunkNumber, Vector3Int myPos, Vector3Int pos)
        {
            var list = GetGoodNeighbors(chunkNumber, pos);

            var minDist = -1;
            var p = new Vector3Int();
            foreach (var point in list)
            {
                var dist = GetDistTo(myPos, point, true);
                if (minDist != -1 && dist > minDist) continue;
                if ((ChunkUtil.IsAnyEntity(chunkNumber, ChunkUtil.GetUpper(point))
                     && myPos != ChunkUtil.GetUpper(point))
                    || !ChunkUtil.IsCanStayHere(chunkNumber, ChunkUtil.GetDovvner(point))
                    || !PathCalcManager.IsReaching(ChunkUtil.GetDovvner(myPos), ChunkUtil.GetDovvner(point))
                )
                    continue;

                minDist = dist;
                p = point;
            }


            return p;
        }

        public static List<Vector3Int> GetGoodNeighbors(int chunkNumber, Vector3Int pos)
        {
            var neighbourPoints = new Vector3Int[4];
            neighbourPoints[0] = new Vector3Int(pos.x + 1, pos.y, pos.z);
            neighbourPoints[1] = new Vector3Int(pos.x - 1, pos.y, pos.z);
            neighbourPoints[2] = new Vector3Int(pos.x, pos.y + 1, pos.z);
            neighbourPoints[3] = new Vector3Int(pos.x, pos.y - 1, pos.z);

            var chunk = ChunkManager.GetChunkByNum(chunkNumber);
            return neighbourPoints.Where(chunk.IsMapPos).ToList();
        }

        public static Vector3Int GetGoodNeighbour(int chunkNumber, Vector3Int pos)
        {
            return GetGoodNeighbors(chunkNumber, pos).FirstOrDefault();
        }

        public static bool IsInvalidPoint(int chunkNumber, Vector3Int point)
        {
            return (point.z + 1 < ChunkManager.MaxGroundsLvls
                    && ChunkUtil.IsAnyEntity(chunkNumber, ChunkUtil.GetUpper(point)))
                   || ChunkUtil.IsGround(chunkNumber, point)
                   || ChunkUtil.IsEntity(chunkNumber, point);
        }

        public static Vector3Int FindFreePosNearPos(Vector3Int pos, bool includeSecondLvl)
        {
            return FindFreePosNearPos(ChunkManager.CurrentChunk.ChunkNumber, pos, includeSecondLvl);
        }

        public static Vector3Int FindFreePosNearPos(int chunkNumber, Vector3Int pos, bool includeSecondLvl)
        {
            for (var i = 1; i < 100; i++)
            {
                var neighbourPoints = new Vector3Int[8];
                neighbourPoints[0] = new Vector3Int(pos.x + i, pos.y, pos.z);
                neighbourPoints[1] = new Vector3Int(pos.x - i, pos.y, pos.z);
                neighbourPoints[2] = new Vector3Int(pos.x, pos.y + i, pos.z);
                neighbourPoints[3] = new Vector3Int(pos.x, pos.y - i, pos.z);

                neighbourPoints[4] = new Vector3Int(pos.x + i, pos.y + i, pos.z);
                neighbourPoints[5] = new Vector3Int(pos.x - i, pos.y - i, pos.z);
                neighbourPoints[6] = new Vector3Int(pos.x - i, pos.y + i, pos.z);
                neighbourPoints[7] = new Vector3Int(pos.x + i, pos.y - i, pos.z);

                var chunk = ChunkManager.GetChunkByNum(chunkNumber);
                foreach (var point in neighbourPoints)
                {
                    if (chunk.IsMapPos(point) && !IsInvalidPoint(chunkNumber, point))
                    {
                        if (includeSecondLvl)
                        {
                            if (SecondaryGroundLvL.IsEmptyPos(chunkNumber, point))
                            {
                                return point;
                            }
                        }
                        else
                            return point;
                    }
                }
            }
            return Vector3Int.zero;
        }

        public static bool IsNearEnt(int chunkNumber, Vector3Int pos)
        {
            var neighbourPoints = new Vector3Int[8];
            neighbourPoints[0] = new Vector3Int(pos.x + 1, pos.y, pos.z);
            neighbourPoints[1] = new Vector3Int(pos.x - 1, pos.y, pos.z);
            neighbourPoints[2] = new Vector3Int(pos.x, pos.y + 1, pos.z);
            neighbourPoints[3] = new Vector3Int(pos.x, pos.y - 1, pos.z);

            var chunk = ChunkManager.GetChunkByNum(chunkNumber);
            return (from point in neighbourPoints
                where chunk.IsMapPos(point) && ChunkUtil.IsEntity(chunkNumber, point)
                select ChunkUtil.GetIndex(chunkNumber, point)).Any(
                index => index > 0);
        }

        public static bool IsNearEnemy(GameEntity ent, int radius)
        {
            var enemy = GetNearEnemy(ent, radius);
            return enemy != null;
        }

        public static GameEntity GetNearEnemy(GameEntity ent, int radius)
        {
            var chunkNumber = ent.ChunkNumber;
            var chunk = ChunkManager.GetChunkByNum(chunkNumber);
            var pos = ent.CurrentPos;
            var neighbourPoints = new Vector3Int[8];
            for (var i = 0; i < radius; i++)
            {
                neighbourPoints[0] = new Vector3Int(pos.x + i, pos.y, pos.z);
                neighbourPoints[1] = new Vector3Int(pos.x - i, pos.y, pos.z);
                neighbourPoints[2] = new Vector3Int(pos.x, pos.y + i, pos.z);
                neighbourPoints[3] = new Vector3Int(pos.x, pos.y - i, pos.z);

                neighbourPoints[4] = new Vector3Int(pos.x + i, pos.y + i, pos.z);
                neighbourPoints[5] = new Vector3Int(pos.x - i, pos.y - i, pos.z);
                neighbourPoints[6] = new Vector3Int(pos.x - i, pos.y + i, pos.z);
                neighbourPoints[7] = new Vector3Int(pos.x + i, pos.y - i, pos.z);

                
                var enemy = (from point in neighbourPoints
                    where chunk.IsMapPos(point)
                    let index = ChunkUtil.GetIndex(chunkNumber, point)
                    where index > 0 && ChunkUtil.IsEntity(chunkNumber, point)
                    select chunk.GetGameObjectByIndex(point)
                    into obj
                    where obj != null
                    select obj.GetComponent<GameEntity>()).FirstOrDefault(entEnemy =>
                    !Equals(entEnemy.Owner, ent.Owner));

                if (enemy != null) return enemy;
            }
            return null;
        }

        public static List<GameUnit> GetNearEnemydUnits(int chunkNumber, GameEntity ent, Vector3Int pos)
        {
            var neighbourPoints = new Vector3Int[4];
            neighbourPoints[0] = new Vector3Int(pos.x + 1, pos.y, pos.z);
            neighbourPoints[1] = new Vector3Int(pos.x - 1, pos.y, pos.z);
            neighbourPoints[2] = new Vector3Int(pos.x, pos.y + 1, pos.z);
            neighbourPoints[3] = new Vector3Int(pos.x, pos.y - 1, pos.z);

            var chunk = ChunkManager.GetChunkByNum(chunkNumber);
            return (from point in neighbourPoints
                where chunk.IsMapPos(point)
                let index = ChunkUtil.GetIndex(chunkNumber, point)
                where index > 0 && ChunkUtil.IsEntity(chunkNumber, point)
                select chunk.GetGameObjectByIndex(point) as GameUnit
                into obj
                where obj != null
                let entEnemy = obj.GetComponent<GameUnit>()
                where !Equals(entEnemy.Owner, ent.Owner)
                select obj).ToList();
        }

        public static GameEntity GetNearFriendUnit(int chunkNumber, GameEntity ent, Vector3Int pos)
        {
            var neighbourPoints = new Vector3Int[4];
            neighbourPoints[0] = new Vector3Int(pos.x + 1, pos.y, pos.z);
            neighbourPoints[1] = new Vector3Int(pos.x - 1, pos.y, pos.z);
            neighbourPoints[2] = new Vector3Int(pos.x, pos.y + 1, pos.z);
            neighbourPoints[3] = new Vector3Int(pos.x, pos.y - 1, pos.z);

            var chunk = ChunkManager.GetChunkByNum(chunkNumber);
            return (from point in neighbourPoints
                where chunk.IsMapPos(point)
                let index = ChunkUtil.GetIndex(chunkNumber, point)
                where index > 0 && ChunkUtil.IsEntity(chunkNumber, point)
                select chunk.GetGameObjectByIndex(point)
                into obj
                where obj != null
                select obj.GetComponent<GameEntity>()).FirstOrDefault(entEnemy =>
                Equals(entEnemy.Owner, ent.Owner));
        }

        public static List<GameUnit> GetNearFriendUnits(int chunkNumber, GameEntity ent, Vector3Int pos)
        {
            var neighbourPoints = new Vector3Int[4];
            neighbourPoints[0] = new Vector3Int(pos.x + 1, pos.y, pos.z);
            neighbourPoints[1] = new Vector3Int(pos.x - 1, pos.y, pos.z);
            neighbourPoints[2] = new Vector3Int(pos.x, pos.y + 1, pos.z);
            neighbourPoints[3] = new Vector3Int(pos.x, pos.y - 1, pos.z);

            var chunk = ChunkManager.GetChunkByNum(chunkNumber);
            return (from point in neighbourPoints
                where chunk.IsMapPos(point)
                let index = ChunkUtil.GetIndex(chunkNumber, point)
                where index > 0 && ChunkUtil.IsEntity(chunkNumber, point)
                select chunk.GetGameObjectByIndex(point) as GameUnit
                into obj
                where obj != null
                let entEnemy = obj.GetComponent<GameUnit>()
                where Equals(entEnemy.Owner, ent.Owner)
                select obj).ToList();
        }

        public static List<GameUnit> GetNearUnits(int chunkNumber, GameEntity ent, Vector3Int pos)
        {
            var neighbourPoints = new Vector3Int[4];
            neighbourPoints[0] = new Vector3Int(pos.x + 1, pos.y, pos.z);
            neighbourPoints[1] = new Vector3Int(pos.x - 1, pos.y, pos.z);
            neighbourPoints[2] = new Vector3Int(pos.x, pos.y + 1, pos.z);
            neighbourPoints[3] = new Vector3Int(pos.x, pos.y - 1, pos.z);

            var chunk = ChunkManager.GetChunkByNum(chunkNumber);
            return (from point in neighbourPoints
                where chunk.IsMapPos(point)
                let index = ChunkUtil.GetIndex(chunkNumber, point)
                where index > 0 && ChunkUtil.IsEntity(chunkNumber, point)
                select chunk.GetGameObjectByIndex(point) as GameUnit
                into obj
                where obj != null
                select obj).ToList();
        }
    }
}