using System.Collections.Generic;
using System.Linq;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects.Entities;
using GlobalMechanic.NonInteract;
using GUI_Game.InGame.ResourceBar;
using MoonSharp.Interpreter;
using UnityEngine;

namespace GameUtils.Objects
{
    [MoonSharpUserData]
    public class Player
    {
        public const int MAX_FOOD_LIMIT = 100;
        public int Id { get; private set; }
        private static int last = 11;
        private int _foodCount;
        private int _foodMax;
        private float _goldCount;
        private readonly List<Research> researchList = new List<Research>();

        public float goldCount
        {
            get { return _goldCount; }
            set
            {
                _goldCount = value;
                if (PlayersManager.GetMyPlayer() == this)
                    ResourceBar_HTML.SetGoldCount((int) value);
            }
        }

        public int foodCount
        {
            get { return _foodCount; }
            set
            {
                _foodCount = value;
                if (PlayersManager.GetMyPlayer() == this)
                    ResourceBar_HTML.SetFoodCount(value);
            }
        }

        public int foodMax
        {
            get { return _foodMax; }
            set
            {
                if (value <= MAX_FOOD_LIMIT)
                    _foodMax = value;
                if (PlayersManager.GetMyPlayer() == this)
                    ResourceBar_HTML.SetFoodMax(_foodMax);
            }
        }

        public Player()
        {
            Id = last;
            last++;

            PlayersManager.AddPlayer(Id, this);

            foodCount = 0;
            foodMax = 0;
        }

        public Player(int id)
        {
            Id = id;
            PlayersManager.AddPlayer(Id, this);

            foodCount = 0;
            foodMax = 0;
        }

        public void OnUnitSpawned(GameUnit unit)
        {
            foreach (var research in researchList)
                research.OnUnitSpawned(unit);
        }

        public void AddResearch(Research research)
        {
            if (HasResearch(research)) return;
            if (HasResearchByName(research.ResearchName))
                RemoveResearch(research.ResearchName);

            researchList.Add(research);
            research.OnResearch(this);
        }

        public void RemoveResearch(Research research)
        {
            if (HasResearch(research))
                researchList.Remove(research);
        }

        public Research GetResearchByName(string name)
        {
            foreach (var res in researchList)
            {
                if (res.ResearchName == name)
                    return res;
            }
            return null;
        }

        public void RemoveResearch(string research)
        {
            if (HasResearch(research))
            {
                var res = researchList.First(x => x.ResearchName == research);
                res.OnResearchRemove(this);
                researchList.Remove(res);
            }
        }

        public bool HasResearch(Research research)
        {
            return researchList.Contains(research);
        }

        public bool HasResearchByName(string name)
        {
            foreach (var res in researchList)
            {
                if (res.ResearchName == name)
                    return true;
            }
            return false;
        }

        public bool HasResearch(string research)
        {
            return researchList.Any(x => x.ResearchName == research);
        }

        public override string ToString()
        {
            return "Player: " + Id;
        }
    }
}