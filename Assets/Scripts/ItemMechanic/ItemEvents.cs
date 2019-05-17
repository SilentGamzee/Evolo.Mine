using System;
using System.Collections.Generic;
using GameUtils.ManagersAndSystems;
using GameUtils.ManagersAndSystems.Quests;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GlobalMechanic.NonInteract.UnitBar;
using GUI_Game.InGame.UnitBar;
using UnityEngine;

namespace ItemMechanic
{
    public class ItemEvents
    {
        public static void OnPickup(GameItem item, GameUnit unit)
        {
            /*
            foreach (var mod in item.ModifierNamesList)
            {
                var modifier = ModifiersManager.GetModifier(mod);
                ModifiersManager.SetupEffect(unit, modifier);
            }
            if (UnitBar_HTML.IsUnitBarOpened(unit))
                UnitBar_HTML.UpdateInfo(unit);
                */
        }

        public static void OnDropItem(GameItem item, GameUnit unit)
        {
            /*
            foreach (var mod in item.ModifierNamesList)
            {
                var modifier = ModifiersManager.GetModifier(mod);
                ModifiersManager.RemoveEffect(unit, modifier);
            }
            if (UnitBar_HTML.IsUnitBarOpened(unit))
                UnitBar_HTML.UpdateInfo(unit);
                */
        }

        public static void OnDeathDropItem(GameItem item, GameUnit unit)
        {
        }

        public static void OnEnter(GameItem item, GameUnit unit)
        {
           

            QuestManager.OnItemEnter(item, unit);
        }

        public static void OnExit(GameItem item, GameUnit unit)
        {
           

            QuestManager.OnItemExit(item, unit);
        }

        public static void OnCreateItem(GameItem item, bool firstCreate)
        {
           
        }

       
    }
}