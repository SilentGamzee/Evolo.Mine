using System;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using MoonSharp.Interpreter;
using UnityEngine;

namespace CSharpLua
{
    public class LuaChunkManager
    {
        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["SetupUnit"] = (Func<string, Vector3Int, Player, GameUnit>) (SetupUnit);
            t["SetupItem"] = (Func<string, Vector3Int, Player, GameItem>) (SetupItem);
            t["IsUnit"] = (Func<string, bool>) (IsUnit);
            t["IsBuilding"] = (Func<string, bool>) (IsBuilding);
            t["IsMapPos"] = (Func<Vector3Int, bool>) (IsMapPos);
            t["IsMapPos_3int"] = (Func<int, int, int, bool>) (IsMapPos);
            t["IsEmptyPos"] = (Func<Vector3Int, bool>) (IsEmptyPos);

            LuaManager.Vm.SetGlobal("Chunk", t);
        }


        public static bool IsUnit(string name)
        {
            return name.ToLower().Contains("npc");
        }

        public static bool IsBuilding(string name)
        {
            return name.ToLower().Contains("building");
        }

        public static GameItem SetupItem(string name, Vector3Int pos, Player owner)
        {
            var chunk = ChunkManager.CurrentChunk;
            return chunk.SetupItem(name, pos, owner);
        }

        public static GameUnit SetupUnit(string name, Vector3Int pos, Player owner)
        {
            var chunk = ChunkManager.CurrentChunk;
            return chunk.SetupUnit(name, pos, owner);
        }

        public static bool IsEmptyPos(Vector3Int pos)
        {
            var chunk = ChunkManager.CurrentChunk;
            var unit = chunk.GetGameObjectByIndex(pos);
            return unit == null;
        }

        public static bool IsMapPos(Vector3Int pos)
        {
            var chunk = ChunkManager.CurrentChunk;
            return chunk.IsMapPos(pos);
        }

        public static bool IsMapPos(int x, int y, int z)
        {
            var chunk = ChunkManager.CurrentChunk;
            return chunk.IsMapPos(new Vector3Int(x, y, z));
        }
    }
}