using System;
using System.Collections.Generic;

namespace UnitsMechanic.Groups
{
    public class GroupsList
    {
        private static readonly Dictionary<string, Group> GList = new Dictionary<string, Group>();

        public static void PreInit()
        {
            AddGroup("Default",  new DefaultGroup());
            AddGroup(Green_eye.GroupName, new Green_eye());
            AddGroup(Spiders.GroupName, new Spiders());
            AddGroup(Holows.GroupName, new Holows());
            AddGroup(Frogs.GroupName, new Frogs());
            AddGroup(Rabbits.GroupName, new Rabbits());
        }

        private static void AddGroup(string group, Group obj)
        {
            GList[group] = obj;
        }

        public static Group GetGroupObject(string group)
        {
            return GList.ContainsKey(group) ? GList[group] : GList["Default"];
        }
    }
}