using System;
using System.Collections.Generic;
using System.Linq;
using CSharpLua;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic.NonInteract;
using GUI_Game.InGame.PauseMenu;
using Menus;
using MoonSharp.Interpreter;
using UnitsMechanic.Orders;
using UnityEngine;

namespace UnitsMechanic
{
    public class GameMoveManager : MonoBehaviour
    {
        [Header("Move speed on map")] [SerializeField] private float _entsMoveSpeed = 1f;

        public static float StaticMoveSpeed { get; set; }

        // List<UnitOrder> orderList = new List<UnitOrder>();
        private static readonly Dictionary<GameEntity, List<UnitOrder>> OrderList =
            new Dictionary<GameEntity, List<UnitOrder>>();

        private static readonly Dictionary<GameEntity, List<UnitOrder>> CanceledOrderList =
            new Dictionary<GameEntity, List<UnitOrder>>();


        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["LastOrderType"] = (Func<GameUnit, OrderTypes>) (LastOrderType);

            LuaManager.Vm.SetGlobal("GameMoveManager", t);
        }

        void Start()
        {
            StaticMoveSpeed = _entsMoveSpeed;
        }

        public static Dictionary<TKey, TValue> CloneDictionaryCloningValues<TKey, TValue>
            (Dictionary<TKey, TValue> original)
        {
            var ret = new Dictionary<TKey, TValue>(original.Count,
                original.Comparer);
            foreach (var entry in original)
            {
                ret.Add(entry.Key, entry.Value);
            }
            return ret;
        }


        public static void OnMoveUpdate(GameUnit unit)
        {
            if (PauseMenu_HTML.IsPaused) return;

            if (unit.MovingPath.Count == 0
                && unit.transform.position != unit.Current3Dpos)
            {
                var dist = Util.GetDistanceFromTo(unit.transform.position, unit.Current3Dpos);
                if (dist > 0.5f)
                {
                    var tilePos = unit.CurrentTilePosition;
                    tilePos.Index = ChunkUtil.GetDovvner(tilePos.Index);
                    AddOrder(unit,
                        new List<UnitOrder> {new UnitOrder(unit, tilePos, OrderTypes.MoveOrder)});
                }
                else
                    unit.IsMoving = false;
                
            }

            if (unit.MovingPath.Count > 0 && !OrderList.ContainsKey(unit))
                GameOrderManager.DoOrder(unit, unit.MovingPath, OrderTypes.AttackOrder);


            if (!OrderList.ContainsKey(unit)) return;

            var orders = OrderList[unit].ToArray();
            if (orders.Length == 0) return;


            var order = orders[0];

            var succ = CallOrderByType(order);

            if (succ)
            {
                var cancList = CanceledOrderList.ContainsKey(unit)
                    ? CanceledOrderList[unit]
                    : new List<UnitOrder>();
                cancList.Add(order);
                CanceledOrderList[unit] = cancList;
            }
            if (CanceledOrderList.ContainsKey(unit) && OrderList[unit].Count == CanceledOrderList[unit].Count)
            {
                unit.State = EventManager.InProgressEvents.Stay;
                SimpleOrderManager.CancelOrders(unit);
            }
        }

        void Update()
        {
            foreach (var kv in CanceledOrderList)
            {
                if (!OrderList.ContainsKey(kv.Key)) continue;
                var unitOrders = OrderList[kv.Key];
                var canceledUnitOrders = CanceledOrderList[kv.Key];

                if (unitOrders.Count == canceledUnitOrders.Count)
                {
                    OrderList[kv.Key].Clear();
                    OrderList.Remove(kv.Key);
                }
                else
                {
                    foreach (var order in canceledUnitOrders)
                    {
                        if (unitOrders.Contains(order))
                        {
                            unitOrders.Remove(order);
                        }
                        var unit = order.GetUnit();
                        if (unit != null)
                            unit.MovingPath.Remove(order.GetTo());
                    }
                    OrderList[kv.Key] = unitOrders;
                }
            }


            CanceledOrderList.Clear();
        }

        private static bool CallOrderByType(UnitOrder order)
        {
            var orderType = order.GetOrderType();
            switch (orderType)
            {
                case OrderTypes.MoveOrder: return Move(order);
                case OrderTypes.AttackOrder: return Attack(order);
                case OrderTypes.CastAbilityOrder: return CastAbility(order);
                default:
                    Debug.LogError("Not right order type: " + orderType);
                    return true;
            }
        }

        private static bool Attack(UnitOrder order)
        {
            return AttackExecutor.Execute(order);
        }


        private static bool Move(UnitOrder order)
        {
            return MoveExecutor.Execute(order);
        }

        private static bool CastAbility(UnitOrder order)
        {
            return CastAbilityExecutor.Execute(order);
        }


        public static void AddOrder(GameUnit unit, List<UnitOrder> list)
        {
            unit.CanceledOrders = false;
            if (!OrderList.ContainsKey(unit))
                OrderList.Add(unit, list);
            else
            {
                OrderList[unit].Clear();
                OrderList[unit] = list;
            }
        }

        public static void AddOrderAtEnd(AbstractGameObject unit, List<UnitOrder> list)
        {
            unit.CanceledOrders = false;
            if (!OrderList.ContainsKey(unit))
                OrderList.Add(unit, list);
            else
                OrderList[unit].AddRange(list);
        }

        public static void CancelOrders(GameUnit unit)
        {
            if (OrderList.ContainsKey(unit))
            {
                //OrderList[ent].Clear();

                var unitOrders = OrderList[unit];
                var cancList = CanceledOrderList.ContainsKey(unit)
                    ? CanceledOrderList[unit]
                    : new List<UnitOrder>();
                cancList.AddRange(unitOrders);
                CanceledOrderList[unit] = cancList;
            }
            if (!unit.IsMoving) return;

            var tilePos = unit.CurrentTilePosition;
            tilePos.Index = ChunkUtil.GetDovvner(tilePos.Index);

            AddOrder(unit,
                new List<UnitOrder> {new UnitOrder(unit, tilePos, OrderTypes.MoveOrder)});
            unit.CanceledOrders = true;
        }

        public static void CancelAllOrders(GameUnit unit)
        {
            if (OrderList.ContainsKey(unit))
            {
                OrderList[unit].Clear();
            }
            unit.CanceledOrders = true;
        }

        private void CancelOrder(GameUnit unit, UnitOrder order)
        {
            var list = CanceledOrderList.ContainsKey(unit) ? CanceledOrderList[unit] : new List<UnitOrder>();

            list.Add(order);
            CanceledOrderList.Add(unit, list);
        }

        public static bool CancelPathVVay(GameUnit unit)
        {
            if (unit == null) return false;
            CancelOrders(unit);

            //if (ClickManager.IsChoosed(ent.gameObject))
            GameOrderManager.RemoveMarks(unit);
            return false;
        }

        public static OrderTypes LastOrderType(GameUnit unit)
        {
            if (!OrderList.ContainsKey(unit)) return OrderTypes.Nothing;
            var lastOrder = OrderList[unit].LastOrDefault();
            return lastOrder == null ? OrderTypes.Nothing : lastOrder.GetOrderType();
        }
    }
}