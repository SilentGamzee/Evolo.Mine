using GameUtils;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using MoonSharp.Interpreter;

namespace UnitsMechanic.Orders
{
    [MoonSharpUserData]
    public class UnitOrder
    {    
        private readonly TilePosition _moveTo;
        private readonly AbstractGameObject _obj;
        private readonly OrderTypes _orderType;
        private readonly Ability castAbility = null;
          

        public static int IdOrder = 0;
        public UnitOrder(AbstractGameObject unit, TilePosition to, OrderTypes orderType)
        {
            _moveTo = to;
            _obj = unit;
            _orderType = orderType;
        }

        public UnitOrder(AbstractGameObject unit, TilePosition to, OrderTypes orderType, Ability ability)
            : this(unit, to, orderType)
        {
            castAbility = ability;
        }
        
        public TilePosition GetTo()
        {
            return _moveTo;
        }

        public GameUnit GetUnit()
        {
            return _obj as GameUnit;
        }

        public OrderTypes GetOrderType()
        {
            return _orderType;
        }

        public Ability GetAbilityToCast()
        {
            return castAbility;
        }
    }

    public enum OrderTypes
    {
        MoveOrder,
        AttackOrder,
        CastAbilityOrder,
        Nothing
    }
}