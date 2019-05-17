using GameUtils.Objects.Entities;
using GlobalMechanic;
using UnityEngine;

namespace UnitsMechanic.Groups.SpecialGroups
{
    public class TreeGroup
    {
       

        public static bool IsTreeGroup(string group)
        {
            return group.ToLower() == "tree";
        }

        public static bool IsPickableTreeLog(GameEntity item)
        {
            return item.OriginalName=="item_log_1" || item.OriginalName=="item_log_2";
        }
    }
}