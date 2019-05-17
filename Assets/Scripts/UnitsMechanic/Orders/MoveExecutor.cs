using System.Collections.Generic;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.UsualUtils;
using GlobalMechanic;
using GlobalMechanic.NonInteract;
using UnitsMechanic.Groups.SpecialGroups;
using UnityEngine;

namespace UnitsMechanic.Orders
{
    public class MoveExecutor
    {
        private const float MinDist = 0.05f; //0.05f

        //False if countinue moving
        private static bool DoMove(UnitOrder order)
        {
            var ent = order.GetUnit();
            var moveTo = order.GetTo();

            if (ent == null) return true;
            var vec = ((moveTo.Pos3D+ent.ExtraPos) - ent.transform.position);
            //var idealVec = (moveTo.Pos3D - Util.Get3DPosByIndex(ent.CurrentPos)).normalized;


            if (Mathf.Abs(vec.x) <= MinDist && Mathf.Abs(vec.y) <= MinDist)
            {
                ent.transform.position = moveTo.Pos3D+ent.ExtraPos;
                ent.MovingTo = null;

                var upperIndexFrom = ent.CurrentPos;
                var upperIndexTo = ChunkUtil.GetUpper(moveTo.Index);

                var chunk = ChunkManager.GetChunkByNum(ent.ChunkNumber);

                chunk.MoveFromTo(upperIndexFrom, upperIndexTo);

                ent.CurrentPos = upperIndexTo;
                


                if (upperIndexFrom != upperIndexTo)
                    PathCalcManager.CalculatePoint(upperIndexFrom);


                GameOrderManager.RemoveMark(ent, moveTo.Index);

                return true;
            }
            if (ChunkUtil.GetDovvner(ent.CurrentPos) != order.GetTo().Index &&
                PathFind.IsInvalidPath(order.GetTo().Index))
                SimpleOrderManager.CancelOrders(ent);

            ent.State = EventManager.InProgressEvents.Move;
            ent.MovingTo = moveTo;
            var speedMod = GameMoveManager.StaticMoveSpeed;
            var moveSpeed = ent.EntStats.MoveSpeed;

            var modVec = MultVector(vec, new Vector3(speedMod, speedMod, 1f));

            var speedVec =
                MultVector(modVec, new Vector3(moveSpeed, moveSpeed, 1f))
               // vec * speedMod
                * Time.deltaTime;

            if (Mathf.Abs(vec.x) <= MinDist * 3 && Mathf.Abs(vec.y) <= MinDist * 3)
            {
                speedVec *= 3;
            }


            ent.transform.Translate(speedVec);
            return false;
        }

        private static Vector3 MultVector(Vector3 vec1, Vector3 vec2)
        {
            var vec = new Vector3
            {
                x = vec1.x * vec2.x,
                y = vec1.y * vec2.y,
                z = vec1.z * vec2.z
            };
            return vec;
        }


        public static bool Execute(UnitOrder order)
        {
            return DoMove(order);
        }
    }
}