using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic;
using GlobalMechanic.NonInteract;
using GlobalMechanic.NonInteract.UnitBar;
using GUI_Game.InGame.UnitBar;
using UnityEngine;

namespace UnitsMechanic.Orders
{
    public class AttackExecutor
    {
        private const float MinDist = 0.4f;

        public static bool MoveTo(GameUnit unit)
        {
            return MoveTo(unit, unit.CurrentPos);
        }

        public static bool MoveTo(GameUnit unit, Vector3Int vecTo)
        {
            var indexPos = ChunkUtil.GetDovvner(vecTo);
            var pos3D = Util.Get3DPosByIndex(vecTo);
            return MoveExecutor.Execute(new UnitOrder
            (
                unit,
                new TilePosition(pos3D, indexPos),
                OrderTypes.MoveOrder
            ));
        }

        public static bool AttackTo(UnitOrder order)
        {
            var ent = order.GetUnit();
            var moveTo = order.GetTo();

            if (ent == null) return true;
            var vec = (moveTo.Pos3D+ent.ExtraPos) - ent.transform.position;

            var chunk = ChunkManager.GetChunkByNum(ent.ChunkNumber);
            var attackedObj = chunk.GetGameObjectByIndex(ChunkUtil.GetUpper(moveTo.Index));
            if (attackedObj == null) return MoveTo(ent);

            var attackedEnt = attackedObj.GetComponent<GameUnit>();
            if (attackedEnt == null) return true;
            if (attackedEnt.Owner == ent.Owner) return MoveTo(ent);

            UnitBar_HTML.SetupUnitBar(attackedEnt);
            ent.State = EventManager.InProgressEvents.Attack;

            if (!ent.AlreadyAttacked && Mathf.Abs(vec.x) <= MinDist && Mathf.Abs(vec.y) <= MinDist)
            {
                // ent.transform.position = ent.Current3Dpos;
                ent.AlreadyAttacked = true;


                attackedEnt.DealDamage(ent.UpgradedStats.Dmg, ent);

                return false;
            }
            if (ent.AlreadyAttacked)
            {
                var checkMove = MoveTo(ent);
                if (checkMove)
                {
                    ent.AlreadyAttacked = false;
                }

                return false;
            }


            ent.MovingTo = moveTo;
            var moveSpeed = GameMoveManager.StaticMoveSpeed;

            var speedVec = vec * moveSpeed / 2f * Time.deltaTime;


            ent.transform.Translate(speedVec);
            return false;
        }

        public static bool Execute(UnitOrder order)
        {
            //Debug.Log("Executing to pos - " + order.GetTo().Index);
            var index = ChunkUtil.GetUpper(order.GetTo().Index);
            var chunkNumber = order.GetUnit().ChunkNumber;
            var chunk = ChunkManager.GetChunkByNum(chunkNumber);
            
            if (order.GetUnit().CurrentPos != index 
                && ChunkUtil.IsAnyEntity(chunkNumber, index))
            {
                var obj = chunk.GetGameObjectByIndex(index);
                if (obj == null)
                    return GameMoveManager.CancelPathVVay(order.GetUnit()); //return false;

                var underEnt = obj.GetComponent<GameEntity>();
                if (underEnt.Owner != order.GetUnit().Owner)
                {
                    return AttackTo(order);
                }
                //Debug.Log("Cancel attacking");
                return GameMoveManager.CancelPathVVay(order.GetUnit()); //return false;
            }
            //Debug.Log("Moving to: " + order.GetTo().Index);
            return MoveExecutor.Execute(order);
        }
    }
}