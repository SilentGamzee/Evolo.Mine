using System.Collections;
using System.Collections.Generic;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using UnitsMechanic;
using UnityEngine;

namespace GameUtils.ManagersAndSystems
{
    public class EventManager : MonoBehaviour
    {
        public GameObject evoEffect;
        public GameObject tpEffect;
        
        public static GameObject staticEvoEffect { get; private set; }
        public static GameObject staticTpEffect { get; private set; }

        public enum GameEvents
        {
            Pause,
            InProgress
        }

        public enum InProgressEvents
        {
            Move,
            Stay,
            Attack,
        }

        public static ArrayList Moving = new ArrayList();
    
   

        void Start()
        {
            staticEvoEffect = evoEffect;
            staticTpEffect = tpEffect;
        }
    
        public static GameUnit ReincarnateUnitName(Dictionary<string, List<string>> dictNames, GameUnit unit,
            int maxLvl)
        {
            
            var group = unit.Group;
            if (!dictNames.ContainsKey(group) || dictNames[group].Count == 0) return null;
            var rand = Random.Range(0, dictNames[group].Count);
            var nextEntName = dictNames[@group][rand];

            return ReincarnateUnitName(nextEntName, unit, maxLvl);
        }

        public static GameUnit ReincarnateUnitName(Dictionary<string, string> dictNames, GameUnit unit, int maxLvl)
        {
            var group = unit.Group;
            if (!dictNames.ContainsKey(group)) return null;

            var nextEntName = dictNames[group];

            return ReincarnateUnitName(nextEntName, unit, maxLvl);
        }

        private static GameUnit ReincarnateUnitName(string nextEntName, GameUnit unit, int maxLvl)
        {
            var pos = unit.CurrentPos;
            var origName = unit.OriginalName;
            var owner = unit.Owner;

            var lvlText = origName[origName.Length - 1].ToString();
            int lvl;
            if (!int.TryParse(lvlText, out lvl))
            {
                Debug.LogError("ReincarnateUnitName: " + origName + ". Cant parse lvl");
                return null;
            }

            if (nextEntName.Length == 0) return null;
            if (lvl > maxLvl) lvl = maxLvl;
            nextEntName = nextEntName + lvl;

            var unitChunkNumber = unit.ChunkNumber;

            unit.KillSelf();

            var chunk = ChunkManager.GetChunkByNum(unitChunkNumber);
            var nUnit = chunk.SetupUnit(nextEntName, pos, owner);
            UnitEvents.OnEvolution(nUnit);
            return nUnit;
        }
        

        public static void SetupEvolutionEffect(Vector3Int pos)
        {
            var pos3D = Util.Get3DPosByIndex(pos);
            var effect =Instantiate(staticEvoEffect, pos3D, Quaternion.identity);
            Destroy(effect, 5f);
        }
        
        public static void SetupTpEffect(Vector3Int pos)
        {
            var pos3D = Util.Get3DPosByIndex(pos);
            var effect =Instantiate(staticTpEffect, pos3D, Quaternion.identity);
            Destroy(effect, 5f);
        }
    }
}