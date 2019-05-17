using System.Collections.Generic;
using System.Linq;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using UnityEngine;

namespace UnitsMechanic.AI_Logic
{
    public class AI_Main
    {
        private static readonly Dictionary<GameEntity, float> timeDict = new Dictionary<GameEntity, float>();
        private const float TIME_AI = 1f;

        private static readonly Dictionary<GameEntity, Vector3Int> attackTarget =
            new Dictionary<GameEntity, Vector3Int>();

        private const int enemyRadiusCheck = 3;

        private const bool AI_DEBUG = false;

        public static void Update(GameUnit unit)
        {
            if (unit == null || unit.gameObject == null) return;
            if (!timeDict.ContainsKey(unit)) timeDict[unit] = 0;

            timeDict[unit] += Time.deltaTime;

            if (timeDict[unit] < TIME_AI) return;
            timeDict[unit] = 0;

            var _ai_state = "";

            if (AI_Calculation.IsNearEnemy(unit, enemyRadiusCheck))
            {
                var p = new Vector3Int(-1, -1, -1);

                var path = unit.MovingPath.ToArray().LastOrDefault();
                if (path != null)
                    p = path.Index;


                var enemy = AI_Calculation.GetNearEnemy(unit, enemyRadiusCheck);
                if (!unit.IsMoving
                    || (p != new Vector3Int(-1, -1, -1) &&
                        p != ChunkUtil.GetDovvner(enemy.CurrentPos)))
                    SimpleOrderManager.AttackTarget(unit, enemy as GameUnit);

                _ai_state = "AttackEnemy";
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (AI_DEBUG && _ai_state != "")
                Debug.LogError(unit + "  State: " + _ai_state);
        }
    }
}