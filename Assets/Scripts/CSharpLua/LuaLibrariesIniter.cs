using System;
using CSharpLua.LuaGetters;
using GameUtils.ManagersAndSystems;
using GameUtils.UsualUtils;
using GlobalMechanic.NonInteract;
using GUI_Game.InGame.ErrorBar;
using GUI_Game.InGame.UnitBar;
using MoonSharp.Interpreter;
using UnitsMechanic;
using UnitsMechanic.AI_Logic;

namespace CSharpLua
{
    public class LuaLibrariesIniter
    {

        public static void InitLibraries(LuaVM vm)
        {
            UnityOs.InitLuaModule(vm);
            ModifiersManager.InitLuaModule();
            ChunkFinder.InitLuaModule();
            SimpleOrderManager.InitLuaModule();
            GameMoveManager.InitLuaModule();
            AI_Calculation.InitLuaModule();
            PathCalcManager.InitLuaModule();
            FlagManager.InitLuaModule();
            ProgressUnitBar.InitLuaModule();
            LuaChunkManager.InitLuaModule();
            ErrorBar_HTML.InitLuaModule();
            LuaHelper.InitLuaModule();
            ResearchManager.InitLuaModule();

        }

        public static void PostInit()
        {
            LuaNpcGetter.Init(); 
            LuaQuestGetter.Init();
            LuaModifiersGetter.Init();
            LuaLanguageGetter.Init();
            LuaAbilitiesGetter.Init();
            LuaResearchGetter.Init();
        }
        
        
    }
}