using CSharpLua;
using CSharpLua.LuaGetters;
using GameUtils.Objects.Entities;
using MoonSharp.Interpreter;
using UnitsMechanic;

namespace GameUtils.Objects
{
    [MoonSharpUserData]
    public class Research
    {
        public string ResearchName { get; private set; }
        private DynValue _onResearch = null;
        private DynValue _onUnitSpawned = null;
        private DynValue _onResearchRemove = null;
        private Table eventT;

        private bool isHasOnResearch;
        private bool isHasOnUnitSpawned;
        private bool isHasOnResearchRemove;
        private string onResearchEventName;
        private string onUnitSpawnedEventName;
        private string OnResearchRemoveEventName;

        public Research(string name)
        {
            ResearchName = name;

            onResearchEventName = name + "_OnResearch";
            onUnitSpawnedEventName = name + "_OnUnitSpawned";
            OnResearchRemoveEventName = name + "_OnResearchRemove";

            eventT = DynValue.NewTable(LuaManager.ScriptObj).Table;
        }

        public void SetOnResearch(DynValue onResearch)
        {
            _onResearch = onResearch;
            isHasOnResearch = true;
        }

        public void SetOnUnitSpawned(DynValue onUnitSpawned)
        {
            _onUnitSpawned = onUnitSpawned;
            isHasOnUnitSpawned = true;
        }

        public void SetOnResearchRemove(DynValue onResearchRemove)
        {
            _onResearchRemove = onResearchRemove;
            isHasOnResearchRemove = true;
        }

        public void SetCaster(DynValue caster)
        {
            eventT["caster"] = caster;
        }

        public void SetAbility(DynValue ability)
        {
            eventT["ability"] = ability;
        }

        public void OnResearch(Player owner)
        {
            if (!isHasOnResearch) return;
            eventT["research"] = this;
            eventT["name"] = onResearchEventName;
            if (UnitEvents.isEventNotPreloaded(eventT, onResearchEventName))
                eventT["preset"] = _onResearch;

            LuaResearchGetter.OnResearch(eventT);
        }

        public void OnUnitSpawned(GameUnit unit)
        {
            if (!isHasOnUnitSpawned) return;
            eventT["research"] = this;
            eventT["name"] = onUnitSpawnedEventName;
            eventT["target"] = unit;
            if (UnitEvents.isEventNotPreloaded(eventT, onUnitSpawnedEventName))
                eventT["preset"] = _onUnitSpawned;

            LuaResearchGetter.OnUnitSpawned(eventT);
        }

        public void OnResearchRemove(Player owner)
        {
            if (!isHasOnResearchRemove) return;
            eventT["research"] = this;
            eventT["name"] = OnResearchRemoveEventName;
            if (UnitEvents.isEventNotPreloaded(eventT, OnResearchRemoveEventName))
                eventT["preset"] = _onResearchRemove;

            LuaResearchGetter.OnResearchRemove(eventT);
        }
    }
}