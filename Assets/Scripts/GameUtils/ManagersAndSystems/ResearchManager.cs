using System;
using CSharpLua;
using GameUtils.Objects;
using MoonSharp.Interpreter;

namespace GameUtils.ManagersAndSystems
{
    public class ResearchManager
    {
        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["SetupResearch"] = (Func<string, Research>) (SetupResearch);
           
            LuaManager.Vm.SetGlobal("ResearchManager", t);
        }

        public static Research SetupResearch(string researchName)
        {
            return new Research(researchName);
        }
    }
}