using System;
using System.Collections.Generic;
using System.Linq;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using UnitsMechanic.Groups;

namespace GameUtils.ManagersAndSystems
{
    public class BuildingsGroupManager
    {
        private static readonly List<GameUnit> GroupList =
            new List<GameUnit>();

        public static void Init(GameUnit building)
        {
            GroupList.Add(building);
        }

        public static void ClearGroups()
        {
            GroupList.Clear();
        }


        public static List<GameUnit> GetAllByPlayer(Player player)
        {
            return (from unit
                        in GroupList
                    where unit.Owner == player
                    select unit)
                .ToList();
        }

        public static List<GameUnit> GetAllInChunk(int chunkNumber)
        {
            return GroupList.Where(building => building.ChunkNumber == chunkNumber).ToList();
        }

        public static List<GameUnit> GetAllInChunkWithName(int chunkNumber, string npcName)
        {
            return GroupList.Where(building =>
                building.ChunkNumber == chunkNumber
                && string.Equals(building.OriginalName,
                    npcName,
                    StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        public static List<GameUnit> GetAll()
        {
            return GroupList;
        }

        public static void RemoveBuildingFromGroup(GameUnit building)
        {
            GroupList.Remove(building);
        }
    }
}