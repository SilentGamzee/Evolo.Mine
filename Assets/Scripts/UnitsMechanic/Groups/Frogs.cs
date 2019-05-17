using GlobalMechanic;
using UnityEngine;

namespace UnitsMechanic.Groups
{
    public class Frogs:Group
    {
        public const string GroupName = "frogs";
        
        private Vector3Int _lairPos = Vector3Int.zero;


        public Frogs()
        {
            
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