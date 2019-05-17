using System.Collections.Generic;
using System.Linq;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic;
using GlobalMechanic.NonInteract;
using UnitsMechanic.Orders;
using UnityEngine;

namespace UnitsMechanic
{
    public class GameOrderManager : MonoBehaviour
    {
        [Header("Move marker")] [SerializeField] private GameObject _marker;

        private static readonly Dictionary<GameEntity, List<MarkObject>> MarkList =
            new Dictionary<GameEntity, List<MarkObject>>();

        class MarkObject
        {
            public Vector3Int PosIndex { get; private set; }
            public GameObject Obj { get; private set;}

            public MarkObject(Vector3Int posIndex, GameObject obj)
            {
                PosIndex = posIndex;
                Obj = obj;
            }
        }

        private static GameObject _staticMarker;


        void Start()
        {
            _staticMarker = _marker;
        }


        static void InitMark(GameUnit unit, Vector3Int index)
        {
            var pos3D = Util.Get3DPosByIndex(index.x, index.y, index.z);

            pos3D = pos3D + _staticMarker.transform.position;

            var obj = Instantiate(_staticMarker, pos3D, _staticMarker.transform.rotation);

            var markObject = new MarkObject(index, obj);
            if (MarkList.ContainsKey(unit))
            {
                var list = MarkList[unit];
                list.Add(markObject);
                MarkList[unit] = list;
            }
            else
            {
                MarkList.Add(unit, new List<MarkObject> {markObject});
            }
        }

        private static List<UnitOrder> PreInitOrder(GameUnit unit, List<TilePosition> pathIndexes,
            OrderTypes orderType)
        {
            var orderList = new List<UnitOrder>();
            foreach (var tilePos in pathIndexes)
            {
                if (unit.Owner == PlayersManager.GetMyPlayer())
                    InitMark(unit, tilePos.Index);
                var unitOrder = new UnitOrder(
                    unit,
                    tilePos,
                    orderType);
                orderList.Add(unitOrder);
            }
            return orderList;
        }

        public static void DoOrder(GameUnit unit, List<TilePosition> pathIndexes,
            OrderTypes orderType)
        {
            if (pathIndexes.Count == 0) return;

            var orderList = PreInitOrder(unit, pathIndexes, orderType);

            GameMoveManager.AddOrder(unit, orderList);
        }

        public static void AddOrder(GameUnit unit, List<TilePosition> pathIndexes,
            OrderTypes orderType)
        {
            if (pathIndexes.Count == 0) return;

            var orderList = PreInitOrder(unit, pathIndexes, orderType);

            GameMoveManager.AddOrderAtEnd(unit, orderList);
        }

        //One order at end for cast ability
        public static void AddCastAbilityOrder(AbstractGameObject unit, TilePosition tilePos, Ability ability)
        {
            if (tilePos == null) return;
            var orderType = OrderTypes.CastAbilityOrder;

            var unitOrder = new UnitOrder(
                unit,
                tilePos,
                orderType,
                ability);

            GameMoveManager.AddOrderAtEnd(unit, new List<UnitOrder>() {unitOrder});
        }


        public static void RemoveMarks(GameUnit unit)
        {
            if (!MarkList.ContainsKey(unit)) return;
            foreach (var mark in MarkList[unit])
            {
                Destroy(mark.Obj);
            }
            MarkList[unit].Clear();
        }

        public static void RemoveMark(GameUnit unit, Vector3Int posIndex)
        {
            if (!MarkList.ContainsKey(unit)) return;
            for (var index = 0; index < MarkList[unit].Count; index++)
            {
                var mark = MarkList[unit][index];

                if (mark.PosIndex == posIndex)
                {
                    Destroy(mark.Obj);
                    MarkList[unit].RemoveAt(index);
                }
            }
        }
    }
}