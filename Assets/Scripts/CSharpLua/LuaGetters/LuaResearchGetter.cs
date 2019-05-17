using MoonSharp.Interpreter;

namespace CSharpLua.LuaGetters
{
    public class LuaResearchGetter
    {
        private static Table _luaResearchManager;
        public static void Init()
        {
            _luaResearchManager = LuaManager.Vm.GetGlobalTable("LuaResearchManager");
        }

        public static Table GetResearchTable(string researchName)
        {
            var f = _luaResearchManager.Get("GetResearch");
            return LuaManager.Vm.Call(f, researchName).Table;
        }

        public static void OnResearch(Table eventT)
        {
            var f = _luaResearchManager.Get("OnResearch");
            LuaManager.Vm.Call(f, eventT);
        }
        
        
        public static void OnUnitSpawned(Table eventT)
        {
            var f = _luaResearchManager.Get("OnUnitSpawned");
            LuaManager.Vm.Call(f, eventT);
        }

        public static void OnResearchRemove(Table eventT)
        {
            var f = _luaResearchManager.Get("OnResearchRemove");
            LuaManager.Vm.Call(f, eventT);
        }
    }
}