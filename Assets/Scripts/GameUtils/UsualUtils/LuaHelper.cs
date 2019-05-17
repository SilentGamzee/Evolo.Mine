using System;
using CSharpLua;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using MoonSharp.Interpreter;
using UnityEngine;

namespace GameUtils.UsualUtils
{
    [MoonSharpUserData]
    public class LuaHelper
    {
        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["Vector3Int"] = (Func<int, int, int, Vector3Int>) (CreateVector3Int);

            LuaManager.Vm.SetGlobal("LuaHelper", t);
        }

        private static Vector3Int CreateVector3Int(int x, int y, int z)
        {
            return new Vector3Int(x, y, z);
        }
        
        
    }
}