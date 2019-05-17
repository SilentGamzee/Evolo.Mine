using System;
using System.Collections.Generic;
using System.Linq;
using CSharpLua;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic;
using GlobalMechanic.NonInteract;
using MoonSharp.Interpreter;
using UnitsMechanic.AI_Logic;
using UnitsMechanic.Orders;
using UnityEngine;

namespace UnitsMechanic
{
    public class SimpleOrderManager
    {
        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["AttackToIndex"] = (Func<GameUnit, Vector3Int, bool>) (AttackToIndex);
            t["AttackTarget"] = (Func<GameUnit, GameUnit, bool>) (AttackTarget);
            t["MoveToIndex"] = (Action<GameUnit, Vector3Int>) (MoveToIndex);
            t["CastAbility"] = (Action<GameUnit, Vector3Int, Ability>) (CastAbility);
            t["CancelOrders"] = (Action<GameUnit>) (CancelOrders);


            LuaManager.Vm.SetGlobal("SimpleOrderManager", t);
        }

        public static bool IsGoodPath(GameUnit unit, Vector3Int posTarget)
        {
            return false;
        }

        public static bool AttackTarget(GameUnit unit, GameUnit target)
        {
            return AttackToIndex(unit, ChunkUtil.GetDovvner(target.CurrentPos));
        }

        public static bool AttackToIndex(GameUnit unit, Vector3Int posTarget)
        {
            var goodNeighbour = AI_Calculation.GetNearestNeighbour(unit.ChunkNumber, unit.CurrentPos, posTarget);

            if (goodNeighbour == Vector3Int.zero
                || !PathCalcManager.IsReaching(ChunkUtil.GetDovvner(unit.CurrentPos), goodNeighbour)) return false;

            var path = PresetPath(unit, goodNeighbour);
            if (path == null) return false;

            path.Add(new TilePosition(
                Util.Get3DPosByIndex(posTarget.x, posTarget.y, posTarget.z + 1)
                , posTarget));

            // Debug.Log("Attack order from: " + ent);
            GameOrderManager.DoOrder(unit, path, OrderTypes.AttackOrder);
            return true;
        }

        public static void MoveToIndex(GameUnit unit, Vector3Int posTarget)
        {
            if (posTarget == Vector3Int.zero) return;
            var ent = ChunkManager.CurrentChunk.GetGameObjectByIndex(posTarget);
            var pos = posTarget;
            if (ent == null || !GroupUtil.IsGround(ent.Group)) pos = ChunkUtil.GetDovvner(pos);
            var path = PresetPath(unit, pos);
            if (path == null) return;

            GameOrderManager.DoOrder(unit, path, OrderTypes.MoveOrder);
        }


        public static void CastNonTargetAbility(GameEntity ent, Ability ability)
        {
            CastNonTargetAbility(ent as AbstractGameObject, ability);
        }
        
        public static void CastNonTargetAbility(AbstractGameObject unit, Ability ability)
        {
            if (unit == null || unit.Destroyed) return;
            GameOrderManager.AddCastAbilityOrder(unit, new TilePosition(unit.CurrentPos), ability);
        }
        
        public static void CastAbility(GameUnit unit, Vector3Int posTarget, Ability ability)
        {
            if (unit == null) return;
            if (posTarget == Vector3Int.zero) return;
            var goodNeighbour =
                AI_Calculation.FindFreePosNearPos(unit.ChunkNumber, ChunkUtil.GetUpper(posTarget), false);
            if (goodNeighbour == Vector3Int.zero)
            {
                Debug.Log("Neighbour is zero");
                return;
            }
            var neighDist = Util.GetDistanceFromTo(goodNeighbour, posTarget);
            var dist = Util.GetDistanceFromTo(unit.CurrentPos, posTarget);


            if (dist > 1)
            {
                if (neighDist > 1)
                {
                    Debug.Log("Error. Cant reach target");
                    return;
                }
                MoveToIndex(unit, ChunkUtil.GetDovvner(goodNeighbour));
            }

            GameOrderManager.AddCastAbilityOrder(unit, new TilePosition(ChunkUtil.GetUpper(posTarget)), ability);
        }

        static List<TilePosition> PresetPath(GameUnit unit, Vector3Int posTarget)
        {
            var indexes = unit.PathFindObj.FindPath(ChunkUtil.GetDovvner(unit.CurrentPos), posTarget);

            // Debug.Log("Path = " + indexes);
            if (indexes == null)
            {
                var chunk = ChunkManager.GetChunkByNum(unit.ChunkNumber);
                chunk.ReindexTiles();
                return null;
            }

            var lastOrDefault = unit.MovingPath.LastOrDefault();
            if (lastOrDefault != null && lastOrDefault.Index == posTarget)
                return null;


            //Removing first point in some case
            if (unit.MovingPath.Count > 0
                && unit.MovingPath[0].Index != unit.CurrentPos
                && indexes.Contains(unit.MovingPath[0].Index))
                indexes = indexes.Where(t => t != ChunkUtil.GetDovvner(unit.CurrentPos)).ToList();

            CancelOrders(unit);

            var path = (from ind in indexes
                let pos3D = Util.Get3DPosByIndex(ind.x, ind.y, ind.z + 1)
                select new TilePosition(pos3D, ind)).ToList();
            unit.IsMoving = true;
            unit.MovingPath = path;

            return path;
        }

        public static void CancelOrders(GameUnit unit)
        {
            unit.MovingPath.RemoveAll(x => x != null);
            GameOrderManager.RemoveMarks(unit);
            GameMoveManager.CancelOrders(unit);
        }
    }
}