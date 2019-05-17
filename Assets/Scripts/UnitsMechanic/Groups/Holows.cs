using GlobalMechanic;
using UnityEngine;

namespace UnitsMechanic.Groups
{
    public class Holows:Group
    {
        public const string GroupName = "holow";
     
        private Vector3Int _lairPos = Vector3Int.zero;

        public Holows()
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