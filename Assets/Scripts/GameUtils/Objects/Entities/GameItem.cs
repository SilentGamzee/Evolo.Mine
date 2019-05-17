using System.Collections.Generic;
using GameUtils.ManagersAndSystems;
using GameUtils.UsualUtils;
using GlobalMechanic.Interact;
using GlobalMechanic.NonInteract;
using GlobalMechanic.NonInteract.UnitBar;
using GUI_Game.InGame.AbilityBar;
using GUI_Game.InGame.PauseMenu;
using GUI_Game.InGame.UnitBar;
using ItemMechanic;
using Menus;
using MoonSharp.Interpreter;
using UnitsMechanic;
using UnitsMechanic.AI_Logic;
using UnitsMechanic.Groups;
using UnityEngine;

namespace GameUtils.Objects.Entities
{
    [MoonSharpUserData]
    public class GameItem : AbstractGameObject
    {
        
        public bool CanBePickuped { get; private set; } //Is can be pickuped by GameUnit

        public Group GroupObj { get; set; } //AI group

        


        public override Vector3Int CurrentPos
        {
            get { return _currentPos; }
            set
            {
                if (_currentPos == value) return;

                Current3Dpos = Util.Get3DPosByIndex(value);
                if (CurrentTilePosition == null)
                    CurrentTilePosition = new TilePosition(Current3Dpos, value);
                else
                {
                    CurrentTilePosition.Index = value;
                    CurrentTilePosition.Pos3D = Current3Dpos;
                }

                var prev = _currentPos;
                _currentPos = value;

                FieldOfView.UpdateFog();
                if (FlagManager.IsFlagAtPos(value))
                    FlagManager.RemoveFlag(value);

                Coloring.RecolorObject(ChunkUtil.GetDovvner(prev));
                Coloring.RecolorObject(ChunkUtil.GetDovvner(value));
            }
        }


        public bool IsMyItem()
        {
            return Owner == PlayersManager.GetMyPlayer();
        }

        void Start()
        {
            Destroyed = false;
            EntityIndex = LastEntIndex;
            LastEntIndex++;

            CanBePickuped = true; // ONLY FOR DEFAULT
        }

        void Update()
        {
            if (PauseMenu_HTML.IsPaused) return;

            if (SoloEvolution)
                UnitEvolution.Update(this);

            if (GroupUtil.IsNeutral(Group)) return;
            AbilitiesManager.UpdateUnitAbilities(this);
        }


        public void Init(int chunkNumber, Vector3Int pos, bool isGround, int prefIndex, Stats stats)
        {
            CurrentPos = pos;
            Owner = PlayersManager.Empty;
            PrefabIndex = prefIndex;
            EntStats = stats;
            UpgradedStats = new Stats(stats.Hp, stats.Dmg, stats.MoveSpeed);
            ChunkNumber = chunkNumber;
        }

        public override void KillSelf()
        {
            if (Destroyed) return;


            if (SecondaryGroundLvL.isSecondaryGroup(Group))
                SecondaryGroundLvL.RemovePos(ChunkNumber, CurrentPos);
            else
            {
                var chunk = ChunkManager.GetChunkByNum(ChunkNumber);
                chunk.SetIndex(CurrentPos, -1);
                chunk.RemoveObject(CurrentPos);
            }

            ProgressUnitBar.RemoveProgressBar(this);
            

            

            if (gameObject != null)
            {
                ClickManager.RemoveChosing(gameObject);
                Destroy(gameObject);
            }
            Destroyed = true;
        }
    }
}