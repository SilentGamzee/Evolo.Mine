using System.Linq;
using MoonSharp.Interpreter;
using UnityEngine;

namespace CSharpLua.LuaGetters
{
    public class LuaQuestGetter
    {
        private static Table _luaQuestManager;
        public static void Init()
        {
            _luaQuestManager = LuaManager.Vm.GetGlobalTable("LuaQuestManager");
        }
        
        public static Table GetQuestById(int id)
        {
            var f = _luaQuestManager.Get("GetQuestById");
            return LuaManager.Vm.Call(f, id).Table;
        }


        public static void PrintQuestInfo(Table quest)
        {
            foreach (var KV in quest.Pairs)
            {
                Debug.LogError(KV.Key+"   "+KV.Value );
            }
        }

        public static string GetName(Table quest)
        {
            var name = quest.Get("name");
            return name.IsNilOrNan() ? "" : name.String;
        }
        
        public static string GetTitle(Table quest)
        {
            var title = quest.Get("title");
            return title.IsNilOrNan() ? "" : title.String;
        }

        public static string GetObjectivies(Table quest)
        {
            var title = quest.Get("objectivies");
            return title.IsNilOrNan() ? "" : title.String;
        }
        
        public static string GetDescription(Table quest)
        {
            var descr = quest.Get("description");
            return descr.IsNilOrNan() ? "" : descr.String;
        }
        
        public static string GetReward(Table quest)
        {
            var reward = quest.Get("reward");
            return reward.IsNilOrNan() ? "" : reward.String;
        }

        public static int GetQuestCount()
        {
            var f = _luaQuestManager.Get("GetQuestCount");
            var castToNumber = LuaManager.Vm.Call(f).CastToNumber();
            if (castToNumber != null) return (int) castToNumber;
            return -1;
        }

        
    }
}