using System;
using System.Collections.Generic;
using System.Linq;
using CSharpLua;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using MoonSharp.Interpreter;
using UnitsMechanic.AI_Logic;
using UnityEngine;

namespace GameUtils.ManagersAndSystems
{
    public class FlagManager : MonoBehaviour
    {
        public GameObject flag;

        public const int UPDATE_RADIUS = 10;

        public static GameObject staticFlag;

        public static Dictionary<Vector2Int, GameObject> FlagDict = new Dictionary<Vector2Int, GameObject>();

        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["IsFlagAtPos"] = (Func<Vector3Int, bool>) (IsFlagAtPos);
            t["GetNearestFlagPos"] = (Func<Vector3Int, Vector3Int>) (GetNearestFlagPos);
            t["GetFlagPoses"] = (Func<Vector3Int[]>) (GetFlagPoses);


            LuaManager.Vm.SetGlobal("FlagManager", t);
        }

        void Start()
        {
            staticFlag = flag;
        }

        public static void SetupFlag(Vector3Int pos)
        {
            SetupFlag(new Vector2Int(pos.x, pos.y));
        }

        public static void SetupFlag(Vector2Int pos)
        {
            if (!ChunkManager.staticFlagsEnabled || FlagDict.ContainsKey(pos)) return;

            var pos3D = Util.Get3DPosByIndex(pos.x, pos.y, 1);
            var instFlag = Instantiate(staticFlag, pos3D, Quaternion.identity);

            FlagDict[pos] = instFlag;
            UpdateFlag(pos);
        }

        public static void RemoveFlag(Vector3Int pos)
        {
            RemoveFlag(new Vector2Int(pos.x, pos.y));
        }

        public static void RemoveFlag(Vector2Int pos)
        {
            if (!FlagDict.ContainsKey(pos)) return;
            Destroy(FlagDict[pos]);
            FlagDict.Remove(pos);
        }

        public static Vector3Int[] GetFlagPoses()
        {
            var poses = new Vector3Int[FlagDict.Count];
            var i = 0;
            foreach (var pos in FlagDict.Keys)
            {
                poses[i] = new Vector3Int(pos.x, pos.y, 0);
                i++;
            }
            return poses;
        }

        public static Vector3Int GetNearestFlagPos(Vector3Int pos)
        {
            var minDist = 1000;
            var targetPos = new Vector3Int(-1, -1, -1);
            foreach (var KV in FlagDict)
            {
                var tPos = new Vector3Int(KV.Key.x, KV.Key.y, 0);
                var dist = AI_Calculation.GetDistTo(pos, tPos, false);
                if (dist < minDist)
                {
                    minDist = dist;
                    targetPos = tPos;
                }
            }
            return targetPos;
        }

        public static bool IsFlagAtPos(Vector3Int pos)
        {
            return IsFlagAtPos(new Vector2Int(pos.x, pos.y));
        }

        public static bool IsFlagAtPos(Vector2Int pos)
        {
            return FlagDict.ContainsKey(pos);
        }

        public static Vector3Int[] GetPosesInRadius(Vector3Int fromPos, int radius)
        {
            return GetFlagPoses().Where(pos =>
                Util.GetDistanceFromTo(fromPos, pos) <= radius).ToArray();
        }

        public static void UpdateFlagsInRadius(Vector3Int fromPos)
        {
            var poses = GetPosesInRadius(fromPos, UPDATE_RADIUS);
            foreach (var pos in poses)
                UpdateFlag(new Vector2Int(pos.x, pos.y));
        }

        public static void UpdateFlag(Vector2Int pos)
        {
            if (!FlagDict.ContainsKey(pos)) return;
            var z = -1;
            GameEntity prev = null;
            for (int i = 0; i < ChunkManager.MaxGroundsLvls; i++)
            {
                var ent = ChunkManager.CurrentChunk.GetGameObjectByIndex(new Vector3Int(pos.x, pos.y, i));

                z = i;
                if (prev != null
                    && (GroupUtil.isCreatureGroup(prev.Group) || !FieldOfView.IsVisible(prev))) z--;

                if (ent == null)
                    break;
                prev = ent;
            }
            if (z == -1) z = ChunkManager.MaxGroundsLvls;

            var pos3D = Util.Get3DPosByIndex(pos.x, pos.y, z);
            var flag = FlagDict[pos];
            flag.transform.position = pos3D + staticFlag.transform.position;
        }
    }
}