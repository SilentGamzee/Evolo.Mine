using System;
using System.Collections.Generic;
using System.Linq;
using CSharpLua;
using CSharpLua.LuaGetters;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GUI_Game.InGame.PauseMenu;
using MoonSharp.Interpreter;
using UnityEngine;

namespace GameUtils.ManagersAndSystems
{
    [MoonSharpUserData]
    public class ModifiersManager : MonoBehaviour
    {
        //UnitModifiers
        private static readonly List<Modifier> modifierList = new List<Modifier>();

        private static List<Modifier> removeList = new List<Modifier>();

        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["SetupModifier"] = (Func<Table, Table, Modifier>) (SetupModifier);
            t["SetupEffect"] = (Action<GameUnit, Modifier>) (SetupEffect);
            t["RemoveEffect"] = (Action<GameUnit, Modifier>) (RemoveEffect);

            LuaManager.Vm.SetGlobal("ModifiersManager", t);
        }


        public static Modifier SetupModifier(Table eventTable, Table modifier)
        {
            var name = "";
            var isbuff = -1;
            var ishiden = -1;
            var thinkInterval = -1f;
            Table onThink = null;

            foreach (var KV in modifier.Pairs)
            {
                if (string.Equals(KV.Key.String, "name", StringComparison.CurrentCultureIgnoreCase))
                    name = KV.Value.String;

                if (string.Equals(KV.Key.String, "IsBuff", StringComparison.CurrentCultureIgnoreCase))
                    int.TryParse(KV.Value.String, out isbuff);

                if (string.Equals(KV.Key.String, "IsHidden", StringComparison.CurrentCultureIgnoreCase))
                    int.TryParse(KV.Value.String, out ishiden);

                if (string.Equals(KV.Key.String, "ThinkInterval", StringComparison.CurrentCultureIgnoreCase))
                    float.TryParse(KV.Value.CastToString(), out thinkInterval);

                if (string.Equals(KV.Key.String, "OnIntervalThink", StringComparison.CurrentCultureIgnoreCase))
                    onThink = KV.Value.Table;
            }
            
            var ability = eventTable.Get("ability").ToObject<Ability>();
            eventTable["preloaded"] = DynValue.Nil;
            var mod = new Modifier(name, ability);
            ability.AddModifierChild(mod);

            if (thinkInterval > 0.0f && onThink != null)
            {
                eventTable["preset"] = onThink;
                mod.SetupThink(thinkInterval, eventTable);
            }

            return mod;
        }


        public static bool IsHasModifierByName(GameUnit unit, Modifier modifier)
        {
            return unit.ModifierList.Any(mod =>
                string.Equals(mod.name, modifier.name, StringComparison.CurrentCultureIgnoreCase));
        }

        public static void SetupEffect(GameUnit unit, Modifier effect)
        {
            if (IsHasModifierByName(unit, effect)) return;
            effect.target = unit;
            unit.ModifierList.Add(effect);

            Entities.AddStats(unit,
                new Stats(effect.hp, effect.dmg, effect.moveSpeed));
            AttachThinking(effect);
        }

        public static void RemoveEffect(GameUnit unit, Modifier effect)
        {
            if (!IsHasModifierByName(unit, effect)) return;

            Entities.SubStats(unit,
                new Stats(effect.hp, effect.dmg, effect.moveSpeed));

            unit.ModifierList.Remove(effect);
            DetachThinking(effect);
        }

        private static void AttachThinking(Modifier modifier)
        {
            if (modifier.thinkInterval <= 0) return;
            if (!modifierList.Contains(modifier))
                modifierList.Add(modifier);
        }

        private static void DetachThinking(Modifier modifier)
        {
            if (modifierList.Contains(modifier))
                modifierList.Remove(modifier);
        }

        void FixedUpdate()
        {
            if (modifierList.Count == 0 || PauseMenu_HTML.IsPaused) return;
            foreach (var modifier in modifierList.ToArray())
                modifier.OnUpdate();
            if (removeList.Count > 0)
                foreach (var mod in removeList)
                    modifierList.Remove(mod);

            removeList.Clear();
        }

        public static void AddToRemove(Modifier mod)
        {
            removeList.Add(mod);
        }
    }
}