using GameUtils.Objects.Entities;
using UnityEngine;

namespace GameUtils.ManagersAndSystems.Events
{
    public class PlayerEvents
    {
        public static void OnKilling(AbstractGameObject from, GameUnit gameUnit)
        {
            var drop = Random.Range(gameUnit.goldDrop.Key, gameUnit.goldDrop.Value);
            from.Owner.goldCount += (int)drop;
        }
    }
}