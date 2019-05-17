using GameUtils.Objects.Entities;
using UnityEngine;

namespace UnitsMechanic.Groups
{
    public class DefaultGroup : Group
    {
        private Vector3Int _lairPos = Vector3Int.zero;

        public override bool IsTimeForTree(GameUnit unit)
        {
            return IsSetUpedLairPos(unit);
        }


        public override bool IsTimeForAngry(GameUnit unit)
        {
            return false;
        }
        
        public override Vector3Int GetLairPos()
        {
            return _lairPos;
        }


        public override void SetUpLairPos(Vector3Int pos)
        {
            _lairPos = pos;
        }
    }
}