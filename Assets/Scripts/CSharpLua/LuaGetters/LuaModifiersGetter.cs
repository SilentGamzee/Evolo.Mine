using GameUtils.Objects;
using MoonSharp.Interpreter;

namespace CSharpLua.LuaGetters
{
    public class LuaModifiersFunc
    {
        
        
    }
    
    public class LuaModifiersGetter:LuaModifiersFunc
    {
        private static Table _luaModifiersManager;
        private static DynValue OnIntervalThink;

        public static void Init()
        {
            _luaModifiersManager = LuaManager.Vm.GetGlobalTable("LuaModifiersManager");
            OnIntervalThink = _luaModifiersManager.Get("OnIntervalThink");
        }
        
        public static Table GetModifierByName(string name)
        {
            var f = _luaModifiersManager.Get("GetModifierByName");
            return LuaManager.Vm.Call(f, name).Table;
        }

        public static string GetModifierLuaPath(Table npc)
        {
            var dynValue = npc.Get("lua");
            return dynValue.IsNilOrNan() ? "" : dynValue.String;
        }

        
        public static void OnThink(Table eventTable)
        {
            LuaManager.Vm.Call(OnIntervalThink, eventTable);
        }
    }
}