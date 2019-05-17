using System.Collections.Generic;
using GameUtils.ManagersAndSystems;
using GameUtils.ManagersAndSystems.Events;
using GameUtils.ManagersAndSystems.Quests;
using GameUtils.UsualUtils;
using GlobalMechanic.Interact;
using GlobalMechanic.NonInteract;
using GlobalMechanic.NonInteract.UnitBar;
using GUI_Game.InGame.PauseMenu;
using GUI_Game.InGame.UnitBar;
using ItemMechanic;
using Menus;
using MoonSharp.Interpreter;
using Sound;
using UnitsMechanic;
using UnitsMechanic.AI_Logic;
using UnitsMechanic.Groups;
using UnityEngine;

namespace GameUtils.Objects.Entities
{
    [MoonSharpUserData]
    public class GameUnit : AbstractGameObject
    {
        public PathFind PathFindObj; //Calculation pathVVay obj


        public List<TilePosition> MovingPath
        {
            get { return _movingPath; }
            set { _movingPath = value; }
        } //PathVVay

        public Research research { get; set; } //One research per building

        public bool StayOnBuilding { get; set; } // for item pickup logic only

        public string itemDrop = ""; //ItemDrop
        public AudioClip GameAudio { get; set; } //Audio on attack
        public GameItem pickUped = null; //Pickuped item

        public bool Disabled { get; private set; }


        private float t; // time counter
        private List<TilePosition> _movingPath = new List<TilePosition>();


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
                FlagManager.UpdateFlagsInRadius(value);

                Coloring.RecolorObject(ChunkUtil.GetDovvner(prev));
                Coloring.RecolorObject(ChunkUtil.GetDovvner(value));

                if (FlagManager.IsFlagAtPos(value))
                    FlagManager.RemoveFlag(value);

                if (!SecondaryGroundLvL.IsEmptyPos(ChunkNumber, prev))
                {
                    ItemEvents.OnExit(SecondaryGroundLvL.GetGroundEnt(ChunkNumber, prev), this);
                }
                if (!SecondaryGroundLvL.IsEmptyPos(ChunkNumber, value))
                {
                    ItemEvents.OnEnter(SecondaryGroundLvL.GetGroundEnt(ChunkNumber, value), this);
                }
                if (!Destroyed)
                {
                    var nearUnits = AI_Calculation.GetNearUnits(ChunkNumber, this, CurrentPos);
                    UnitEvents.OnUnitNear(this, nearUnits);
                }
            }
        }


        void Start()
        {
            Destroyed = false;
            EntityIndex = LastEntIndex;
            LastEntIndex++;
            PathFindObj = new PathFind();
        }


        void Update()
        {
            if (PauseMenu_HTML.IsPaused) return;

            if (GroupUtil.IsNeutral(Group)) return;

            if (SoloEvolution)
                UnitEvolution.Update(this);

            if (!GroupUtil.isCreatureGroup(Group)) return;

            ItemMain.Update(this);
            UnitEvolution.Update(this);
        }

        void FixedUpdate()
        {
            if (PauseMenu_HTML.IsPaused || GroupUtil.IsNeutral(Group)) return;

            foreach (var ab in AbilityList)
                ab.Update();


            GameMoveManager.OnMoveUpdate(this);

            if (Group == ""
                || !GroupUtil.isCreatureGroup(Group)
                || GroupUtil.IsNeutral(Group)
                || Owner == PlayersManager.GetMyPlayer()
            ) return;

            AI_Main.Update(this);
        }


        public void Init(int chunkNumber, Vector3Int pos, Player player, int prefIndex)
        {
            CurrentPos = pos;
            Owner = player;
            PrefabIndex = prefIndex;
            ChunkNumber = chunkNumber;
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


        public void DealDamage(int dmg, AbstractGameObject from)
        {
            UpgradedStats.Hp -= dmg;
            GameSoundManager.PlayOnTakeDamage(PrefabIndex);

            UnitBar_HTML.UpdateInfo(this);
            if (UpgradedStats.Hp > 0) return;

            KillSelf();
            QuestManager.OnKilling(from, this);
            UnitEvents.OnKilling(from, this);
            PlayerEvents.OnKilling(from, this);
        }


        public void Disable()
        {
            Disabled = true;
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            Disabled = false;
            gameObject.SetActive(true);
        }


        public override void KillSelf()
        {
            if (Destroyed) return;
            var chunk = ChunkManager.GetChunkByNum(ChunkNumber);
            chunk.SetIndex(CurrentPos, -1);
            chunk.RemoveObject(CurrentPos);

            if (pickUped != null)
                ItemEvents.OnDeathDropItem(pickUped, this);

            ClickManager.RemoveChosing(gameObject);

            GameMoveManager.CancelAllOrders(this);
            if (GroupUtil.isCreatureGroup(Group))
            {
                GameOrderManager.RemoveMarks(this);
                //SimpleOrderManager.CancelOrders(this);
                if (MovingPath != null)
                    MovingPath.Clear();
            }

            if (SecondaryGroundLvL.isSecondaryGroup(Group))
                SecondaryGroundLvL.RemovePos(ChunkNumber, CurrentPos);

            if (FlagManager.IsFlagAtPos(CurrentPos))
                FlagManager.RemoveFlag(CurrentPos);

            ProgressUnitBar.RemoveProgressBar(this);
            CreatureGroupManager.RemoveEntFromGroup(this);
            BuildingsGroupManager.RemoveBuildingFromGroup(this);

            RemoveAbilities();

            if (research != null)
            {
                var sameBuilds = BuildingsGroupManager.GetAllInChunkWithName(ChunkNumber, OriginalName);
                if (sameBuilds.Count == 0)
                    Owner.RemoveResearch(research);
            }
            Coloring.RecolorObject(ChunkUtil.GetDovvner(CurrentPos));
            Owner.foodCount -= foodCost;
            Owner.foodMax -= foodGive;

            if (gameObject != null)
                Destroy(gameObject);


            Destroyed = true;
        }
    }
}