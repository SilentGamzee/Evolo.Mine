using System.Collections.Generic;
using System.Linq;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using PowerUI;
using UnityEngine;

namespace GUI_Game.InGame.AbilityBar
{
    public class AbilityBar_HTML
    {
        public static void SetupAbilityBar(AbstractGameObject unit)
        {
            if (unit == null || !unit.isChoosed()) return;
            if (IsHided())
                UI.document.Run("AbilityBarUnHide");
            UI.document.Run("ClearAbilities");
            
            foreach (var ability in unit.AbilityList.ToArray())
                UI.document.Run("SetupAbility", ability);
            UpdateHP_MP_bar(unit);
        }

        public static void HideAbilityBar()
        {
            if (!IsHided())
                UI.document.Run("AbilityBarHide");
        }

        public static void UpdateHP_MP_bar(AbstractGameObject unit)
        {
            if (IsHided()) return;
            UI.document.Run("UpdateHP_count", unit.UpgradedStats.Hp);
            UI.document.Run("UpdateHP_max", unit.EntStats.Hp);
        }

        //CD
        public static void Update(Ability ab)
        {
            if (IsHided()) return;
            UI.document.Run("UpdateAbilityBar", ab);
        }

        //Researches
        public static void Update()
        {
            if (IsHided()) return;
            UI.document.Run("UpdateAbilityBar");
        }

        public static bool IsHided()
        {
            return UI.document.Run("IsAbilityBarHided").ToString().ToLower() == "true";
        }
    }
}