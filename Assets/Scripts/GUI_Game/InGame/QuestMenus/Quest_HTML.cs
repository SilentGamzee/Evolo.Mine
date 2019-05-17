using System;
using Boo.Lang;
using CSharpLua;
using GameUtils.ManagersAndSystems;
using GameUtils.ManagersAndSystems.Quests;
using GameUtils.Objects;
using Jint.Native;
using PowerUI;
using UnityEngine;

namespace GUI_Game.InGame.QuestMenus
{
    public class Quest_HTML : MonoBehaviour
    {
        void Start()
        {
            LuaLibrariesIniter.PostInit();
            //JS_test();
           // SetupQuest(1);
           // SetupQuest(2);
           // SetupQuest(3);
            //DeleteQuest(0);
        }

        public static Quest GetQuestByNum(int n)
        {
            Debug.Log("Quest call: " + n);
            var q = QuestManager.GetQuest(n);
            return q ?? Quest.GetQuestTemplate();
        }

        public static void JS_test()
        {
            UI.document.Run("jsTest", "s", 1);
            
        }

        public static void DeleteQuest(int n)
        {
            UI.document.Run("DeleteQuest", n);
        }

        public static void SetupQuest(int n)
        {
            UI.document.Run("SetupQuest", n);
        }

        public static string GetQuestHTML(int n)
        {
            var quest = GetQuestByNum(n);
            var s = "<div class='Quest' value='" + n + @"'>
                <div class='QuestTitle'>
                <div class='QuestTitleBG'>
                <div class='QuestTitleText'>
                "
                    + quest.questTitle +
                    @"</div>
                </div>
                <div class='QuestMarkBG' onclick='ShowQuestWindow(" + n + @")'>
                <div class='QuestMarkBG_hover' 
                    onmousedown='ChangeOpacityQuestQuestion(event,this,0.6)' 
                    onmouseup='ChangeOpacityQuestQuestion(event,this,1.0)' 
                    onmouseleave='returnOpacity()'>
                <div class='QuestMark'>
                </div>
                </div>
                </div>
                </div>
                <div class='QuestDescr'>" + quest.questObjectivies + @"
                </div>
                </div>";
            return s;
        }
    }
}