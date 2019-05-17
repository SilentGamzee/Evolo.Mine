using System.Collections.Generic;
using System.Linq;
using CSharpLua.LuaGetters;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using UnitsMechanic;
using UnitsMechanic.AI_Logic;
using UnityEngine;

namespace GameUtils.ManagersAndSystems.Quests
{
    public class QuestManager
    {
        public static Dictionary<int, Quest> questDict = new Dictionary<int, Quest>();
        public static Quest GetQuest(int n)
        {
            if (questDict.ContainsKey(n)) return questDict[n];
            
            var quest = LuaQuestGetter.GetQuestById(n);
            var q = new Quest(
                n,
                LuaQuestGetter.GetName(quest),
                LuaQuestGetter.GetTitle(quest),
                LuaQuestGetter.GetObjectivies(quest),
                LuaQuestGetter.GetDescription(quest),
                LuaQuestGetter.GetReward(quest)
            );
            questDict[n] = q;
            return q;
        }

        public static void OnQuestCompleted(int n)
        {
            Debug.Log("On quest compelted: " + n);
        }

        public static void OnEvolution(GameUnit unit)
        {
           
        }

        public static void OnItemStuck(GameItem itemResult)
        {
         
        }


        public static void OnItemEnter(GameItem item, GameUnit unit)
        {
           
        }

        public static void OnItemExit(GameItem item, GameUnit unit)
        {
           
        }

        public static void OnKilling(AbstractGameObject from, GameUnit target)
        {
            
        }

        public static void Reward(Quest quest, Player player)
        {
            var unit = CreatureGroupManager.GetAllByPlayer(player).FirstOrDefault();
            // Debug.LogError(unit);
            if (unit == null) return;
            var pos = AI_Calculation.FindFreePosNearPos(unit.ChunkNumber, unit.CurrentPos, true);
            var questReward = quest.reward;
            //Debug.LogError(questReward);
            if (questReward.Contains("npc"))
            {
                var unitChunkNumber = unit.ChunkNumber;
                var chunk = ChunkManager.GetChunkByNum(unitChunkNumber);
                chunk.SetupUnit(questReward, pos, unit.Owner);
            }
            else if (questReward.Contains("item") || questReward.Contains("building"))
            {
                // Debug.LogError("item/building");
                SpecialActions.SpawnItem(questReward, pos, unit.Owner);
            }
        }
    }
}