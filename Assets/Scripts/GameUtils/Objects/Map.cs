using System;
using System.Collections.Generic;
using GameUtils.UsualUtils;

namespace GameUtils.Objects
{
    public abstract class Map
    {
        public int LeftStairs { get; set; }
        public int RightStairs { get; set; }
        public int BackStairs { get; set; }
        

        public List<KeyValuePair<float, string>> ZeroGround;
        public int FirstGround { get; set; }
        public int Tree { get; set; }
        public string BorderName { get; set; }

        public List<string[]> enemyList;

        public string TreeName;
        public List<string> additionalNeutrals;

        public enum MapTypes
        {
            DefaultMap,
        }

        public static Map GetMapByType(MapTypes type)
        {
            switch (type)
            {
                case MapTypes.DefaultMap:
                    return new DefaultMap();
                default:
                    return null;
            }
        }
    }

    public class DefaultMap : Map
    {
        public DefaultMap()
        {
            LeftStairs = Loader.GetIndexByName("object_LeftStairs");
            RightStairs = Loader.GetIndexByName("object_RightStairs");
            BackStairs = Loader.GetIndexByName("object_BackStairs");

            ZeroGround = new List<KeyValuePair<float, string>>
            {
                new KeyValuePair<float, string>(0.50f, "building_Ground_0"),
                new KeyValuePair<float, string>(0.10f, "building_Ground_1"),
                new KeyValuePair<float, string>(0.10f, "building_Ground_2"),
                new KeyValuePair<float, string>(0.10f, "building_Ground_3"),
            };
            FirstGround = Loader.GetIndexByName("building_stone");
            BorderName = "building_grass";

            TreeName = "";
            Tree = Loader.GetIndexByName(TreeName);


            enemyList = new List<string[]>
            {
                new[]
                {
                    "npc_frog_small",
                    "npc_frog_big"
                },
                new[]
                {
                    "npc_hollow_small",
                    "npc_hollow_big"
                }
            };

            additionalNeutrals = new List<string>
            {
            };
        }
    }
}