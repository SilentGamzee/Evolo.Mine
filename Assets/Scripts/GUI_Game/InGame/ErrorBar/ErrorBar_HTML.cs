using System;
using CSharpLua;
using GameUtils.Objects.Entities;
using Json;
using MoonSharp.Interpreter;
using PowerUI;
using UnityEngine;

namespace GUI_Game.InGame.ErrorBar
{
    [MoonSharpUserData]
    public class ErrorBar_HTML
    {
        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["SetupError"] = (Action<string>) (SetupError);

            LuaManager.Vm.SetGlobal("ErrorBar", t);
        }
        public static void SetupError(string errorText)
        {
            SetupErrorText(errorText);
        }

        private static void SetupErrorText(string errorText)
        {
            UI.document.Run("SetupErrorText", errorText);
        }
    }
}