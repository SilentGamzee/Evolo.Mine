using System;
using System.Collections.Generic;
using System.Linq;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GlobalMechanic.NonInteract;
using MoonSharp.Interpreter;
using Sound;
using UnityEngine;

namespace UnitsMechanic
{
    public class UnitEvents
    {
        public static void OnUnitSpawned(GameUnit unit)
        {
            if (unit == null || unit.AbilityList == null) return;
            foreach (var ability in unit.AbilityList)
                ability.OnOwnerSpawned(unit);
            if (unit.Owner != null)
                unit.Owner.OnUnitSpawned(unit); //Research event
        }

        public static void OnUnitNear(GameUnit unit, List<GameUnit> nearList)
        {
            foreach (var nearUnit in nearList)
            {
                if (!unit.Destroyed)
                    OnUnitNear(unit, nearUnit);
            }
        }

        private static void OnUnitNear(GameUnit unit, GameUnit nearUnit)
        {
        }


        public static void OnKilling(AbstractGameObject from, GameUnit target)
        {
            if (target.itemDrop.Length > 0)
            {
                Player owner = null;
                if (from != null)
                    owner = from.Owner;
                SpecialActions.SpawnItem(target.itemDrop, target.CurrentPos, owner);
            }

            if (target.IsMy())
            {
               // var myUnits = CreatureGroupManager.GetAllByPlayer(PlayersManager.GetMyPlayer());
                // var myBuildings = BuildingsGroupManager.GetAllByPlayer(PlayersManager.GetMyPlayer());

               
            }


            target.GroupObj.OnDeath(target, from);
        }


        public static void OnEvolution(GameEntity ent)
        {
            EventManager.SetupEvolutionEffect(ent.CurrentPos);
            GameSoundManager.PlayOnEvolution();
        }
        
        public static bool isEventNotPreloaded(Table eventT, string eventName)
        {
            return eventT.Get("preloaded").IsNil() || eventT.Get("preloaded").Table.Get(eventName).IsNil();
        }
    }
}