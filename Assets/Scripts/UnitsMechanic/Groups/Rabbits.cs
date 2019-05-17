﻿using GlobalMechanic;
using UnityEngine;

namespace UnitsMechanic.Groups
{
    public class Rabbits:Group
    {
        public const string GroupName = "rabbit";
        
        private Vector3Int _lairPos = Vector3Int.zero;

        public Rabbits()
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