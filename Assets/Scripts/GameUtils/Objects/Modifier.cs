using System;
using CSharpLua;
using CSharpLua.LuaGetters;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects.Entities;
using GUI_Game.InGame.PauseMenu;
using MoonSharp.Interpreter;
using UnityEngine;

namespace GameUtils.Objects
{
    [MoonSharpUserData]
    public class Modifier
    {
        public string name { get; private set; }
        public int hp { get; set; }
        public int dmg { get; set; }
        public float moveSpeed { get; set; }

        public float thinkInterval { get; private set; }
        private Table eventTable;

        private float t = 0;

        public Table vars;

        public AbstractGameObject target = null;

        public Ability abilityOwner;

        public Modifier(string name, Ability abilityOwner)
        {
            this.name = name;
            this.abilityOwner = abilityOwner;
            thinkInterval = -1;
            vars = DynValue.NewTable(LuaManager.ScriptObj).Table;
        }

        public void SetupThink(float thinkInterval, Table eventTable)
        {
            eventTable["modifier"] = this;
            this.thinkInterval = thinkInterval;
            this.eventTable = eventTable;
            t = thinkInterval;
        }


        public void OnUpdate()
        {
            if (thinkInterval <= 0) return;

            if (target == null)
            {
                Remove();
                return;
            }
            
            t += Time.deltaTime;
            if (t < thinkInterval) return;
            t = 0;
            eventTable["name"] = name;
            eventTable["target"] = target;
            LuaModifiersGetter.OnThink(eventTable);
        }

        public void Remove()
        {
            eventTable.Clear();
            vars.Clear();
            ModifiersManager.AddToRemove(this);
        }
    }
}