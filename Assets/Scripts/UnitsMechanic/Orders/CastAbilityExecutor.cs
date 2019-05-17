using GameUtils.ManagersAndSystems;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GUI_Game.InGame.AbilityBar;
using UnityEngine;

namespace UnitsMechanic.Orders
{
    public class CastAbilityExecutor
    {
        public static bool Execute(UnitOrder order)
        {
            var ability = order.GetAbilityToCast();
            if (ability == null) return true;
            var unit = order.GetUnit();
            var target = ChunkManager.GetChunkByNum(unit.ChunkNumber).GetGameObjectByIndex(order.GetTo().Index);
            ability.CurrentCooldown = ability.GetCooldown();

            var succ = ability.OnSpellStart(unit, target as GameUnit);

            if (ability.PostCurrent >= 0)
            {
                ability.CurrentCooldown = ability.PostCurrent;
                ability.PostCurrent = -1;
            }


            return succ;
        }
    }
}