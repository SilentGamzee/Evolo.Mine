using System;
using System.IO;
using GameUtils.UsualUtils;
using MoonSharp.Interpreter;
using UnityEngine;

namespace CSharpLua
{
    public class UnityOs
    {
        private static bool _isDebugCheck = false;

        public static bool IsDebugCheck
        {
            get { return _isDebugCheck; }
            set { _isDebugCheck = value; }
        }

        public const string LuaScriptPath = "LuaScripts/";

        
        public static string GetTextFromFile(string filePath)
        {
            var textAsset = Loader.LoadScript(LuaScriptPath+filePath);
            
            return textAsset == null ? "" : textAsset.text;
        }

        public static bool IsDebug()
        {
            return IsDebugCheck;
        }

        public static void InitLuaModule(LuaVM vm)
        {
            var script = vm.GetScriptObject();



            var t = new Table(script);
            t["GetTextFromFile"] = (Func<string, string>) (GetTextFromFile);
            t["IsDebug"] = (Func<bool>) (IsDebug);
            
            vm.SetGlobal("UnityOs", t);
        }
        
       
    }
}