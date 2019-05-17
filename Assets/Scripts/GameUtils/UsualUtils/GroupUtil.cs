namespace GameUtils
{
    public class GroupUtil
    {
        public static bool isCreatureGroup(string group)
        {
            return group != "ground" 
                   && group != "tree" 
                   && group != "item" 
                   && group != "building"
                   && group != "rock"
                   && group != "neutral";
        }

        public static bool isTree(string group)
        {
            return group == "tree";
        }

        public static bool isBuilding(string group)
        {
            return group == "building";
        }

        public static bool IsGround(string group)
        {
            return group == "ground";
        }

        public static bool IsItem(string group)
        {
            return group == "item";
        }

        public static bool IsNeutral(string group)
        {
            return group == "neutral";
        }
    }
}