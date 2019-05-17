using System;
using System.Collections.Generic;
using System.Linq;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using UnityEngine;

namespace GameUtils.UsualUtils
{
    public class TilePosition
    {
        public TilePosition(Vector3Int index)
        {
            Pos3D = Util.Get3DPosByIndex(index);
            Index = index;
        }

        public TilePosition(Vector3 pos3D, Vector3Int index)
        {
            Pos3D = pos3D;
            Index = index;
        }

        public Vector3 Pos3D { get; set; }
        public Vector3Int Index { get; set; }
    }

    public class Util
    {
        private const bool Debug = true;

        public static bool IsDebug()
        {
            return Debug;
        }

        private const float DistanceX = 3.0f;
        private const float DistanceY = 0.4f;
        private const float NLine = 1.7f;
        private const float ZXvalue = 0.12f, ZYvalue = 0.001f;

        private const float extraSpaceY = 0.15f;


        public static T ToEnum<T>(string value, T defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            T result;
            return TryParseToEnum<T>(value, true, out result) ? result : defaultValue;
        }

        private static bool TryParseToEnum<T>(string probablyEnumAsString_, bool ignoreCase, out T enumValue_)
            where T : struct
        {
            enumValue_ = (T) Enum.GetValues(typeof(T)).GetValue(0);
            if (!Enum.IsDefined(typeof(T), probablyEnumAsString_))
                return false;

            enumValue_ = (T) Enum.Parse(typeof(T), probablyEnumAsString_, ignoreCase);
            return true;
        }

        

        public static Vector2 Get2DPosByIndex(int i, int j, int groundLvl)
        {
            var x = j * DistanceX + NLine * i - ChunkManager.StaticMapSize / 2f;
            var y = (groundLvl + j * DistanceY - NLine * i + ChunkManager.StaticMapSize / 2f) + extraSpaceY * groundLvl;
            return new Vector2(x, y);
        }

        public static Vector2 Get2DPosByIndex(Vector2Int vec, int groundLvl)
        {
            return Get2DPosByIndex(vec.x, vec.y, groundLvl);
        }

        public static float GetZPosByIndex(int i, int j, int groundLvl)
        {
            return -i / 10f * ZXvalue + j * ZYvalue - groundLvl / 100f;
        }

        public static Vector3 Get3DPosByIndex(int i, int j, int groundLvl)
        {
            var vec2D = Get2DPosByIndex(i, j, groundLvl);
            return new Vector3(vec2D.x, vec2D.y, GetZPosByIndex(i, j, groundLvl));
        }

        public static Vector3 Get3DPosByIndex(Vector3Int pos)
        {
            return Get3DPosByIndex(pos.x, pos.y, pos.z);
        }

        public static int GetCountAfterPoint(float number)
        {
            int decimals = 0;
            while ((int) number % 10 == 0)
            {
                number *= 10;
                decimals++;
            }
            return decimals;
        }

        public static int GetDistanceFromTo(Vector3Int from, Vector3Int to)
        {
            return Mathf.Abs(Mathf.Abs(from.x) - Mathf.Abs(to.x))
                   + Mathf.Abs(Mathf.Abs(from.y) - Mathf.Abs(to.y));
        }
        
        public static float GetDistanceFromTo(Vector3 from, Vector3 to)
        {
            return Mathf.Abs(Mathf.Abs(from.x) - Mathf.Abs(to.x))
                   + Mathf.Abs(Mathf.Abs(from.y) - Mathf.Abs(to.y));
        }
        
        
    }
}