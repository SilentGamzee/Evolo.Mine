using System;
using System.Collections.Generic;
using System.Linq;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using UnityEngine;

namespace GameUtils.ManagersAndSystems
{
    public class AbilitiesManager : MonoBehaviour
    {
        private static List<Ability> abilitiesList = new List<Ability>();

        public static bool IsUnitHasAbility(GameUnit unit, string abilityName)
        {
            return unit.AbilityList.Any(ability =>
                string.Equals(ability.abilityName, abilityName, StringComparison.CurrentCultureIgnoreCase));
        }

        public static Ability GetUnitAbility(GameUnit unit, string abilityName)
        {
            return unit.AbilityList.FirstOrDefault(ability =>
                string.Equals(ability.abilityName, abilityName, StringComparison.CurrentCultureIgnoreCase));
        }

        //Only CD in curr time
        public static void UpdateUnitAbilities(AbstractGameObject unit)
        {
            foreach (var ab in unit.AbilityList.ToArray())
                ab.Update();
        }

        public static void AttachAbility(Ability ability)
        {
            if(!abilitiesList.Contains(ability))
                abilitiesList.Add(ability);
        }
        
        public static void DetachAbility(Ability ability)
        {
            if(abilitiesList.Contains(ability))
                abilitiesList.Remove(ability);
        }

        void Update()
        {
            if (abilitiesList.Count <= 0) return;
            foreach (var ab in abilitiesList.ToArray())
                ab.Update();
        }
    }
}