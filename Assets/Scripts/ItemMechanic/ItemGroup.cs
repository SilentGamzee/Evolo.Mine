using System.Collections.Generic;
using System.Linq;
using CSharpLua;

namespace ItemMechanic
{
    public class ItemGroup
    {
        public static Dictionary<string, Dictionary<string, string>> stackResult =
            new Dictionary<string, Dictionary<string, string>>();
            
        public static void PreInit()
        {
            stackResult = LuaNpcGetter.GetItemsStucks();

        }

        public static bool IsCanBeStacked(string name, string name2)
        {
            return stackResult.ContainsKey(name) && stackResult[name].Any(s => s.Key.Equals(name2.ToLower()));
        }

        public static string GetStackResult(string name, string name2)
        {
            if (stackResult.ContainsKey(name) && stackResult[name].ContainsKey(name2))
            {
                return stackResult[name][name2];
            }
            return "";
        }
    }
}