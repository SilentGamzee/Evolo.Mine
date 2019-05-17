using GlobalMechanic;
using UnityEngine;

namespace UnitsMechanic.Groups
{
    public class Spiders:Group
    {
        public const string GroupName = "spider";
        
        private Vector3Int _lairPos = Vector3Int.zero;

        public Spiders()
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