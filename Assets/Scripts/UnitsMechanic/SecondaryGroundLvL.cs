using System.Collections.Generic;
using GameUtils.Objects.Entities;
using UnityEngine;

namespace UnitsMechanic
{
    public class SecondaryGroundLvL
    {
        private static readonly Dictionary<int, Dictionary<Vector3Int, GameItem>> ListObj
            = new Dictionary<int, Dictionary<Vector3Int, GameItem>>();

        public static bool isSecondaryGroup(string groupName)
        {
            var lower = groupName.ToLower();
            return lower == "building" || lower == "item";
        }

        public static bool IsEmptyPos(int chunkNumber, Vector3Int pos)
        {
            return !ListObj.ContainsKey(chunkNumber) || !ListObj[chunkNumber].ContainsKey(pos);
        }

        public static GameItem GetGroundEnt(int chunkNumber, Vector3Int pos)
        {
            return IsEmptyPos(chunkNumber, pos) ? null : ListObj[chunkNumber][pos];
        }

        public static void SetGroundEnt(int chunkNumber, Vector3Int pos, GameItem ent)
        {
            if (!ListObj.ContainsKey(chunkNumber)) ListObj[chunkNumber] = new Dictionary<Vector3Int, GameItem>();
            ListObj[chunkNumber][pos] = ent;
        }

        public static void RemovePos(int chunkNumber, Vector3Int pos)
        {
            if (!ListObj.ContainsKey(chunkNumber)) return;
            ListObj[chunkNumber].Remove(pos);
        }

        public static void RemoveChunk(int chunkNumber)
        {
            if (!ListObj.ContainsKey(chunkNumber)) return;
            foreach (var item in ListObj[chunkNumber].Values)
                item.gameObject.SetActive(false);
        }

        public static void FullRemoveChunk(int chunkNumber)
        {
            if (!ListObj.ContainsKey(chunkNumber)) return;
            foreach (var item in ListObj[chunkNumber].Values)
                item.KillSelf();


            ListObj.Remove(chunkNumber);
        }

        public static Dictionary<Vector3Int, GameItem> GetChunkGround(int chunkNumber)
        {
            return !ListObj.ContainsKey(chunkNumber)
                ? new Dictionary<Vector3Int, GameItem>()
                : ListObj[chunkNumber];
        }
    }
}