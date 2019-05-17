using CSharpLua.LuaGetters;
using MoonSharp.Interpreter;
using UnityEngine;

namespace GameUtils.ManagersAndSystems
{
    public class LanguageTextManager : MonoBehaviour
    {
        public const string DefaultLanguage = "English";
        private static Table LangTable;

        public static void Init()
        {
            LangTable = LuaLanguageGetter.GetLangTable(DefaultLanguage);
        }

        public static string GetTextByName(string labelName)
        {
            int _;
            labelName = labelName.Replace("\"", "");
            if (labelName.Length > 0 && !int.TryParse(labelName[0].ToString(), out _))
                return LuaLanguageGetter.GetTextByName(LangTable,
                    labelName.Replace(" ", "")).Replace(" ", "  ");

            return labelName;
        }
    }
}