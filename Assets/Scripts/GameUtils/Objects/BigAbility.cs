using System.Collections.Generic;
using CSharpLua.LuaGetters;
using MoonSharp.Interpreter;

namespace GameUtils.Objects
{

    public class BigAbility
    {
        public string texturePath { get; private set; }
        public List<string> ResearchRequirements { get; private set; }
        public int MaxLevel { get; private set; }
        
        public int AbilityCastRange { get; private set; }
        public float AbilityCooldown { get; private set; }
        public int AbilityManaCost { get; private set; }
        public Table modifierTable { get; private set; }
        
        public Dictionary<string, Table> eventsPreset;
        
        public AbilityProperties.AbilityBehaviorEnum AbilityBehavior { get; private set; }
        public AbilityProperties.AbilityTypeEnum AbilityType { get; private set; }
        public AbilityProperties.AbilityUnitTargetTeamEnum AbilityUnitTargetTeam { get; private set; }
        public AbilityProperties.AbilityUnitTargetTypeEnum AbilityUnitTargetType { get; private set; }
        public AbilityProperties.AbilityUnitDamageTypeEnum AbilityUnitDamageType { get; private set; }
        
        public BigAbility(
            Table AbilityTable,
            string texturePath,
            List<string> researchRequirements,
            int maxLevel,
            int abilityCastRange,
            float abilityCooldown,
            int abilityManaCost,
            AbilityProperties.AbilityBehaviorEnum abilityBehavior,
            AbilityProperties.AbilityTypeEnum abilityType,
            AbilityProperties.AbilityUnitTargetTeamEnum abilityUnitTargetTeam,
            AbilityProperties.AbilityUnitTargetTypeEnum abilityUnitTargetType,
            AbilityProperties.AbilityUnitDamageTypeEnum abilityUnitDamageType)
        {
            this.texturePath = texturePath;
            ResearchRequirements = researchRequirements;
            MaxLevel = maxLevel;
            
            AbilityCastRange = abilityCastRange;
            AbilityCooldown = abilityCooldown;
            AbilityManaCost = abilityManaCost;
            AbilityBehavior = abilityBehavior;
            AbilityType = abilityType;
            AbilityUnitTargetTeam = abilityUnitTargetTeam;
            AbilityUnitTargetType = abilityUnitTargetType;
            AbilityUnitDamageType = abilityUnitDamageType;
            
            eventsPreset = LuaAbilitiesGetter.GetEventsPreset(AbilityTable);
            modifierTable = LuaAbilitiesGetter.GetAbilityModifiersTable(AbilityTable);
        }
        
        
    }
    
    public class BigAbilityManager
    {
        
        //<AbilityName, BigAbility>
        private static Dictionary<string, BigAbility> bigAbilities = new Dictionary<string, BigAbility>();
      

        
        
        public static BigAbility SetupBigAbility(Table AbilityTable,
            string abilityName,
            string texturePath,
            List<string> researchRequirements,
            int maxLevel,
            int abilityCastRange,
            float abilityCooldown,
            int abilityManaCost,
            AbilityProperties.AbilityBehaviorEnum abilityBehavior,
            AbilityProperties.AbilityTypeEnum abilityType,
            AbilityProperties.AbilityUnitTargetTeamEnum abilityUnitTargetTeam,
            AbilityProperties.AbilityUnitTargetTypeEnum abilityUnitTargetType,
            AbilityProperties.AbilityUnitDamageTypeEnum abilityUnitDamageType)
        {
            if (bigAbilities.ContainsKey(abilityName))
                return bigAbilities[abilityName];
            
            var bigAbil = new BigAbility(AbilityTable,
                texturePath,
                researchRequirements,
                maxLevel,
                abilityCastRange,
                abilityCooldown,
                abilityManaCost,
                abilityBehavior,
                abilityType,
                abilityUnitTargetTeam,
                abilityUnitTargetType,
                abilityUnitDamageType);

            return bigAbil;
        }
       
    }
}