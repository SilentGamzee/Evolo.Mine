using System;
using System.Collections.Generic;
using CSharpLua;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic.Interact;
using GlobalMechanic.NonInteract;
using GlobalMechanic.NonInteract.UnitBar;
using MoonSharp.Interpreter;
using UnityEngine;

namespace GUI_Game.InGame.UnitBar
{
    public class ProgressBar
    {
        public float time { get; set; }
        public ProgressUnitBar.ProgressName name { get; private set; }
        public float needTime { get; private set; }
        public GameEntity ent;

        public ProgressBar(GameEntity ent, ProgressUnitBar.ProgressName name, float needTime)
        {
            this.name = name;
            this.needTime = needTime;
            this.ent = ent;
            this.time = 0;
        }

        public void UpdateInfo(ProgressUnitBar.ProgressName name, float needTime)
        {
            time = 0;
            this.name = name;
            this.needTime = needTime;
        }
    }

    public class ProgressUnitBar
    {
        public static readonly Dictionary<GameEntity, ProgressBar> ProgressBars =
            new Dictionary<GameEntity, ProgressBar>();

        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["Setup"] = (Action<GameEntity, string, float>) (Setup);
            t["IsHasProgressBar"] = (Func<GameEntity, bool>) (IsHasProgressBar);
            t["GetProgress"] = (Func<GameEntity, float>) (GetProgress);
            t["GetMaxProgress"] = (Func<GameEntity, float>) (GetMaxProgress);
            t["IsReady"] = (Func<GameEntity, bool>) (IsReady);
            t["RemoveProgressBar"] = (Action<GameEntity, string>) (RemoveProgressBar);

            LuaManager.Vm.SetGlobal("ProgressUnitBar", t);
        }


        public enum ProgressName
        {
            CastAbility,
            Stucking,
            Pickuping,
            Dropping,
            SoloEvolution,
            GroupEvolution,
            None
        }

        public static void Setup(GameEntity ent, string name, float needTime)
        {
            var progressName = Util.ToEnum(name, ProgressName.None);
            if (progressName == ProgressName.None) return;

            if (IsHasProgressBar(ent))
            {
                if (ProgressBars[ent].name == ProgressName.None)
                    ProgressBars[ent].UpdateInfo(progressName, needTime);
                return;
            }


            ProgressBars[ent] = new ProgressBar(ent, progressName, needTime);
        }

        public static void Setup(GameEntity ent, ProgressName name, float needTime)
        {
            if (IsHasProgressBar(ent)) return;

            ProgressBars[ent] = new ProgressBar(ent, name, needTime);
        }

        public static bool IsHasProgressBar(GameEntity ent)
        {
            return ProgressBars.ContainsKey(ent);
        }

        public static float GetProgress(GameEntity ent)
        {
            return !ProgressBars.ContainsKey(ent) ? 0 : ProgressBars[ent].time;
        }

        public static float GetMaxProgress(GameEntity ent)
        {
            return !ProgressBars.ContainsKey(ent) ? 0 : ProgressBars[ent].needTime;
        }

        public static bool IsReady(GameEntity ent)
        {
            if (!ProgressBars.ContainsKey(ent)) return false;
            return ProgressBars[ent].time >= ProgressBars[ent].needTime;
        }


        public static void RemoveProgressBar(GameEntity ent)
        {
            if (!ProgressBars.ContainsKey(ent)) return;
            ProgressBars.Remove(ent);
        }

        public static void RemoveProgressBar(GameEntity ent, string name)
        {
            var progressName = Util.ToEnum(name, ProgressName.None);
            if (progressName == ProgressName.None) return;
            RemoveProgressBar(ent, progressName);
        }

        public static void RemoveProgressBar(GameEntity ent, ProgressName name)
        {
            if (!ProgressBars.ContainsKey(ent) || !IsThisProgressName(ent, name))
                return;

            ProgressBars[ent].UpdateInfo(ProgressName.None, 1f);
        }

        public static bool IsThisProgressName(GameEntity ent, ProgressName name)
        {
            if (!ProgressBars.ContainsKey(ent)) return false;
            return ProgressBars[ent].name == name;
        }

        public static string GetTextForProgressName(ProgressName name)
        {
            switch (name)
            {
                case ProgressName.Dropping: return "Progress_Dropping";
                case ProgressName.GroupEvolution: return "Progress_GroupEvolution";
                case ProgressName.SoloEvolution: return "Progress_SoloEvolution";
                case ProgressName.Pickuping: return "Progress_Pickuping";
                case ProgressName.Stucking: return "Progress_Stucking";
                default: return "#ProgressBar";
            }
        }
    }
}