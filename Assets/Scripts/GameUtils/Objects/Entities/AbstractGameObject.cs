using System.Collections.Generic;
using System.Linq;
using GameUtils.ManagersAndSystems;
using GlobalMechanic.Interact;
using GlobalMechanic.NonInteract;
using GUI_Game.InGame.AbilityBar;
using UnitsMechanic.Groups;
using UnityEngine;

namespace GameUtils.Objects.Entities
{
    public abstract class AbstractGameObject : GameEntity
    {
        [SerializeField] private bool _isMoving; //if state moving = true


        public Group GroupObj { get; set; }
        public bool SoloEvolution; //Is can solo evolve
        protected string _evolutionNext = ""; //Solo evolution next Entity

        protected EventManager.InProgressEvents _state = EventManager.InProgressEvents.Stay; //State of unit

        public virtual Stats EntStats { get; set; } //Stats
        public virtual Stats UpgradedStats { get; set; } //Stats updated by modifiers

        public List<string> ModifierNamesList = new List<string>(); //Items modifiers
        public List<Modifier> ModifierList = new List<Modifier>(); //List of modifiers on unit

        public List<Ability> AbilityList = new List<Ability>();
        protected float _evolutionTime;
        private bool _canceledOrders;

        public bool CanceledOrders
        {
            get { return _canceledOrders; }
            set { _canceledOrders = value; }
        }

        public float EvolutionTime
        {
            get { return _evolutionTime; }
            set { _evolutionTime = value; }
        }

        public bool IsMoving
        {
            get { return _isMoving; }
            set { _isMoving = value; }
        }

        public string EvolutionNext
        {
            get { return _evolutionNext; }
            set
            {
                _evolutionNext = value;

                SoloEvolution = value.Length > 0;
            }
        }


        public EventManager.InProgressEvents State
        {
            get { return _state; }
            set
            {
                if (value == EventManager.InProgressEvents.Stay) IsMoving = false;
                _state = value;
            }
        }

        public bool IsEnemy(GameEntity anotherEnt)
        {
            return Owner != anotherEnt.Owner;
        }

        public bool IsEnemy(Player anotherPlayer)
        {
            return Owner != anotherPlayer;
        }

        public bool IsHisName(string unitName)
        {
            if (OriginalName == null || unitName == null) return false;
            return OriginalName.ToLower().Equals(unitName.ToLower());
        }

        public bool IsMy()
        {
            return Owner == PlayersManager.GetMyPlayer();
        }

        public bool IsOwner(Player player)
        {
            return Owner == player;
        }

        public void AddAbility(Ability ab)
        {
            if (!isHasAbility(ab))
            {
                ab.SetAbilityOwner(this);
                AbilityList.Add(ab);
            }
        }

        public void RemoveAbility(Ability ab)
        {
            if (!isHasAbility(ab)) return;

            AbilityList.Remove(ab);
            AbilityBar_HTML.SetupAbilityBar(this);
        }

        public void RemoveAbility(string abilityName)
        {
            var ability = AbilityList.FirstOrDefault(x => x.abilityName == abilityName);
            if (ability == null) return;

            RemoveAbility(ability);
        }

        public bool isHasAbility(Ability ability)
        {
            return AbilityList.Contains(ability);
        }

        public bool isHasAbility(string abilityName)
        {
            foreach (var ab in AbilityList)
            {
                if (ab.abilityName == abilityName)
                    return true;
            }
            return false;
        }

        public bool IsHasAbilitySpecialValue(string specialKey)
        {
            foreach (var ab in AbilityList)
            {
                if (ab.IsHasSpecialValue(specialKey))
                    return true;
            }
            return false;
        }

        public Ability[] GetAbilitiesWithSpecialValue(string specialKey)
        {
            var t = new Ability[AbilityList.Count];
            var c = 0;
            foreach (var ab in AbilityList)
            {
                if (!ab.IsHasSpecialValue(specialKey)) continue;
                t[c] = ab;
                c++;
            }
            return t;
        }

        public Ability GetAbility(string abilityName)
        {
            foreach (var ability in AbilityList)
            {
                if (ability.abilityName == abilityName)
                    return ability;
            }
            return null;
        }

        public Modifier GetModifier(string name)
        {
            foreach (var mod in ModifierList)
                if (mod.name == name)
                    return mod;
            return null;
        }

        public void RemoveAbilities()
        {
            AbilityList.Clear();
            AbilityBar_HTML.SetupAbilityBar(this);
        }

        public int GetCurrentHp()
        {
            return UpgradedStats.Hp;
        }

        public int GetMaxHp()
        {
            return EntStats.Hp;
        }

        public bool isChoosed()
        {
            return ClickManager.IsChoosed(this);
        }

        public void ChooseUnit()
        {
            ClickManager.ChooseUnit(this);
        }
    }
}