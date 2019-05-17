using GameUtils.Objects.Entities;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

namespace CSharpLua
{
    public class LuaManager : MonoBehaviour
    {
        public static LuaVM Vm;
        public static Script ScriptObj;

        void Awake()
        {
            Script.DefaultOptions.ScriptLoader = new UnityAssetsScriptLoader();


            Vm = new LuaVM(CoreModules.Preset_Complete, new string[]
            {
                "Assets/Resources/Scripts/LuaScripts/?",
                "Assets/Resources/Scripts/LuaScripts/?.txt",
                "Resources/Scripts/LuaScripts/?",
                "Resources/Scripts/LuaScripts/?.txt",
                "Scripts/LuaScripts/?",
                "Scripts/LuaScripts/?.txt",
                
                "Assets/Resources/Scripts/LuaScripts/?/?",
                "Assets/Resources/Scripts/LuaScripts/?/?.txt",
                "Resources/Scripts/LuaScripts/?/?",
                "Resources/Scripts/LuaScripts/?/?.txt",
                "Scripts/LuaScripts/?/?",
                "Scripts/LuaScripts/?/?.txt",
                
                "Assets/Resources/Scripts/LuaScripts/?/?/?",
                "Assets/Resources/Scripts/LuaScripts/?/?/?.txt",
                "Resources/Scripts/LuaScripts/?/?/?",
                "Resources/Scripts/LuaScripts/?/?/?.txt",
                "Scripts/LuaScripts/?/?/?",
                "Scripts/LuaScripts/?/?/?.txt",
                
            });
            ScriptObj = Vm.GetScriptObject();

            LuaLibrariesIniter.InitLibraries(Vm);

            //UnityOs.IsDebugCheck = true;

            var fileText = UnityOs.GetTextFromFile("main");
            var EvoEngine = Vm.ExecuteString(fileText);
            var main = EvoEngine.Table.Get("Main");

            InitForLua();
            
            Vm.Call(main);
        }

        private static void InitForLua()
        {
            UserData.RegisterAssembly();
            UserData.RegisterType<Vector3Int>();
        }
    }
}