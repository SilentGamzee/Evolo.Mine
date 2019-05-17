using System;
using MoonSharp.Interpreter;

namespace CSharpLua.LuaGetters
{
    public class LuaLanguageGetter
    {
        private static Table _luaLanguageManager;

        public static void Init()
        {
            _luaLanguageManager = LuaManager.Vm.GetGlobalTable("LuaLangManager");
        }

        public static Table GetLangTable(string langName)
        {
            var f = _luaLanguageManager.Get("GetLangTable");
            return LuaManager.Vm.Call(f, langName).Table;
        }


        public static string GetTextByName(Table langTable, string labelName)
        {
            var text = labelName;
            int _;
            if (!int.TryParse(labelName, out _))
                text = "#" + labelName;
            else
                return text;

            foreach (var KV in langTable.Pairs)
            {
                if (string.Equals(KV.Key.String, labelName, StringComparison.CurrentCultureIgnoreCase))
                    return KV.Value.String;
            }

            return text;
        }
    }
}