using System.Collections.Generic;
using System.Linq;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.ManagersAndSystems.Quests;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic;
using GlobalMechanic.NonInteract;
using GlobalMechanic.NonInteract.UnitBar;
using GUI_Game.InGame.UnitBar;
using UnitsMechanic.AI_Logic;
using UnityEngine;

namespace UnitsMechanic
{
    public class UnitEvolution
    {
        public static Dictionary<string, Dictionary<string, string>> stackResult =
            new Dictionary<string, Dictionary<string, string>>();

        public static Dictionary<string, string> SoloEvolutionDict = new Dictionary<string, string>();

        public static bool IsHasSoloEvolution(string from)
        {
            return SoloEvolutionDict.ContainsKey(from);
        }

        public static void AddToStackDict(string ent1, string ent2, string result)
        {
            if (!stackResult.ContainsKey(ent1))
                stackResult[ent1] = new Dictionary<string, string>();
            stackResult[ent1][ent2] = result;
        }

        public static void AddToSoloDict(string from, string to)
        {
            SoloEvolutionDict[from] = to;
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

        


        private static readonly Dictionary<GameEntity, GameEntity> stackTarget =
            new Dictionary<GameEntity, GameEntity>();

        public static void Update(AbstractGameObject unit)
        {
            var EvolutionTime = unit.EvolutionTime;
            if (unit.State != EventManager.InProgressEvents.Stay)
            {
                ProgressUnitBar.RemoveProgressBar(unit);


                if (stackTarget.ContainsKey(unit))
                {
                    var target = stackTarget[unit];
                    var progressName = ProgressUnitBar.ProgressName.GroupEvolution;
                    ProgressUnitBar.RemoveProgressBar(target, progressName);
                    stackTarget.Remove(unit);
                    stackTarget.Remove(target);
                }

                return;
            }

            var chunk = ChunkManager.GetChunkByNum(unit.ChunkNumber);

            //Solo evolution
            if (unit.SoloEvolution)
            {
                var progressName = ProgressUnitBar.ProgressName.SoloEvolution;
                var pos = unit.CurrentPos;
                if (!SecondaryGroundLvL.IsEmptyPos(chunk.ChunkNumber, pos) &&
                    ChunkUtil.IsAnyEntity(chunk.ChunkNumber, pos))
                {
                    ProgressUnitBar.RemoveProgressBar(unit, progressName);
                    return;
                }

                ProgressUnitBar.Setup(unit, progressName, EvolutionTime);


                if (!ProgressUnitBar.IsReady(unit) ||
                    !ProgressUnitBar.IsThisProgressName(unit, progressName)) return;

                // Debug.Log("Original name = " + unit.OriginalName);


                if (SoloEvolutionDict.ContainsKey(unit.OriginalName) || unit.EvolutionNext.Length > 0)
                {
                    var evoName = "";
                    evoName = unit.EvolutionNext.Length == 0
                        ? SoloEvolutionDict[unit.OriginalName]
                        : unit.EvolutionNext;

                    if (evoName.Length > 0)
                    {
                        //  Debug.Log("Evolution name = " + evoName);

                        var prevIndex = ChunkUtil.GetIndex(chunk.ChunkNumber, pos);
                        var prevEnt = chunk.GetGameObjectByIndex(pos);
                        var check = unit.PrefabIndex == prevIndex;

                     
                        unit.KillSelf();

                        if (!SecondaryGroundLvL.IsEmptyPos(chunk.ChunkNumber, pos))
                        {
                            SecondaryGroundLvL.GetGroundEnt(chunk.ChunkNumber, pos).KillSelf();
                            SecondaryGroundLvL.RemovePos(chunk.ChunkNumber, pos);
                        }

                        var ent = chunk.PreSetupObject(evoName, pos, unit.Owner);

                        if (GroupUtil.isBuilding(ent.Group) || GroupUtil.IsItem(ent.Group))
                        {
                            chunk.SetupItem(ent, evoName, pos, unit.Owner);
                        }
                        else
                        {
                            chunk.SetupUnit(ent, evoName, pos, unit.Owner);
                        }
                    }
                }
            }
            //Group evolution
            else if (stackResult.ContainsKey(unit.OriginalName))
            {
                var progressName = ProgressUnitBar.ProgressName.GroupEvolution;
                var friends = AI_Calculation.GetNearFriendUnits(chunk.ChunkNumber, unit, unit.CurrentPos);


                GameEntity target = null;
                if (stackTarget.ContainsKey(unit))
                    target = stackTarget[unit];

                if (target != null && !friends.Contains(target as GameUnit))
                {
                    ProgressUnitBar.RemoveProgressBar(target, progressName);
                    target = null;
                    stackTarget.Remove(unit);
                }
                foreach (var obj in friends)
                {
                    if (obj.Destroyed) continue;
                    if (!stackResult[unit.OriginalName].ContainsKey(obj.OriginalName)) continue;
                    if (obj.State != EventManager.InProgressEvents.Stay) continue;

                    stackTarget[unit] = obj;
                    target = obj;
                    break;
                }

                if (target == null) return;

                ProgressUnitBar.Setup(unit, progressName, EvolutionTime);

                //EvolutionTimeList[target] = EvolutionTimeList[ent];
                // UpdateProgressBar(target);

                if (!ProgressUnitBar.IsReady(unit) ||
                    !ProgressUnitBar.IsThisProgressName(unit, progressName)) return;

                var evoName = stackResult[unit.OriginalName][target.OriginalName];
                var pos = unit.CurrentPos;
                var owner = unit.Owner;

                unit.KillSelf();
                target.KillSelf();

                var evoUnit = chunk.SetupUnit(evoName, pos, owner);
                QuestManager.OnEvolution(evoUnit);

                Coloring.RecolorObject(ChunkUtil.GetDovvner(unit.CurrentPos));
                Coloring.RecolorObject(ChunkUtil.GetDovvner(target.CurrentPos));

                UnitEvents.OnEvolution(evoUnit);

                PathCalcManager.CalculatePoint(ChunkUtil.GetDovvner(pos));
                PathCalcManager.CalculatePoint(pos);
            }
        }
    }
}