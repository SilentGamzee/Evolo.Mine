using GlobalMechanic;
using UnityEngine;

namespace UnitsMechanic.Groups
{
    public class Green_eye : Group
    {
        public const string GroupName = "green_eye";

        private Vector3Int _lairPos = Vector3Int.zero;

        public Green_eye()
        {
        }


        public override void SetUpLairPos(Vector3Int pos)
        {
            _lairPos = pos;
        }

        public override Vector3Int GetLairPos()
        {
            return _lairPos;
        }
    }
}