using System;
using System.Collections.Generic;
using System.Linq;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using UnitsMechanic.Groups;

namespace GameUtils.ManagersAndSystems
{
    public class CreatureGroupManager
    {
        private static readonly Dictionary<string, List<GameUnit>> GroupList =
            new Dictionary<string, List<GameUnit>>();

        public static void Init(GameUnit unit)
        {
            var group = unit.Group;
            unit.GroupObj = GroupsList.GetGroupObject(group);

            if (group.Length == 0) return;
            if (GroupList.ContainsKey(group))
            {
                GroupList[group].Add(unit);
            }
            else
            {
                GroupList.Add(group, new List<GameUnit> {unit});
            }
        }

        public static void ClearGroups()
        {
            foreach (var kv in GroupList)
            {
                GroupList[kv.Key].Clear();
            }
            GroupList.Clear();
        }

        public static List<GameUnit> GetAllInGroup(string group)
        {
            return GroupList.ContainsKey(@group) ? GroupList[@group] : new List<GameUnit>();
        }

        public static List<GameUnit> GetAllByPlayer(Player player)
        {
            return (from unitsList
                        in GroupList.Values
                    from unit
                        in unitsList
                    where unit.Owner == player
                    select unit)
                .ToList();
        }

        public static List<GameUnit> GetAll()
        {
            return GroupList.Values.SelectMany(unitList => unitList).ToList();
        }
        
        public static List<GameUnit> GetAllInChunk(int chunkNumber)
        {
            return (from unitList in GroupList.Values
                from unit in unitList
                where unit.ChunkNumber == chunkNumber
                select unit).ToList();
        }

        public static void RemoveEntFromGroup(GameUnit ent)
        {
            if (GroupList.ContainsKey(ent.Group) && GroupList[ent.Group].Contains(ent))
                GroupList[ent.Group].Remove(ent);
        }
    }
}