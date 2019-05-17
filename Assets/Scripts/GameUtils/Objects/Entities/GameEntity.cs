using System.Collections.Generic;
using GameUtils.ManagersAndSystems;
using GameUtils.UsualUtils;
using GlobalMechanic.Interact;
using GlobalMechanic.NonInteract;
using GUI_Game.InGame.UnitBar;
using MoonSharp.Interpreter;
using UnitsMechanic;
using UnityEngine;

namespace GameUtils.Objects.Entities
{
    [MoonSharpUserData]
    public class GameEntity : MonoBehaviour
    {
        [SerializeField]
        public int ChunkNumber { get; protected set; }

        [SerializeField] protected Vector3Int _currentPos;
        [SerializeField] public int PrefabIndex;
        [SerializeField] protected string _originalName;

        

        public int foodGive { get; set; }
        public int foodCost { get; set; }
        
        public KeyValuePair<float, float> goldDrop { get; set; }

        public Vector3 ExtraPos
        {
            get { return _extraPos; }
            set { _extraPos = value; }
        }

        private Player _owner;
        public int PlayerID;

        public Player Owner
        {
            get { return _owner; }
            set
            {
                _owner = value;
                PlayerID = value.Id;
            }
        }

        public string OriginalName
        {
            get { return _originalName; }
            set { _originalName = value; }
        }

        public string Group
        {
            get { return _group; }
            set { _group = value; }
        }


        public bool Destroyed { get; protected set; }


        public bool AlreadyAttacked { get; set; }
        public int PathInt = -2;

        public Vector3 Current3Dpos { get; protected set; }
        public TilePosition CurrentTilePosition { get; protected set; }


        public TilePosition MovingTo { get; set; }


        public int EntityIndex { get; protected set; }
        public static int LastEntIndex;


        private Vector3 _extraPos = Vector3.zero;
        private string _group = "";


        public void Init(int chunkNumber, Vector3Int pos, bool isGround, int prefIndex)
        {
            CurrentPos = pos;
            Owner = PlayersManager.Empty;
            PrefabIndex = prefIndex;
            ChunkNumber = chunkNumber;
        }


        public virtual Vector3Int CurrentPos
        {
            get { return _currentPos; }
            set
            {
                if (_currentPos == value) return;


                Current3Dpos = Util.Get3DPosByIndex(value) + ExtraPos;
                if (CurrentTilePosition == null)
                    CurrentTilePosition = new TilePosition(Current3Dpos, value);
                else
                {
                    CurrentTilePosition.Index = value;
                    CurrentTilePosition.Pos3D = Current3Dpos;
                }


                var prev = _currentPos;
                _currentPos = value;

                Coloring.RecolorObject(ChunkUtil.GetDovvner(prev));
                Coloring.RecolorObject(ChunkUtil.GetDovvner(value));
            }
        }


        public virtual void KillSelf()
        {
            if (Destroyed) return;
            var chunk = ChunkManager.GetChunkByNum(ChunkNumber);


            chunk.SetIndex(CurrentPos, -1);
            chunk.RemoveObject(CurrentPos);


            var ent = chunk.GetGameObjectByIndex(ChunkUtil.GetDovvner(CurrentPos));
            if (ent != null)
            {
                Coloring.RecolorObject(ent.CurrentPos);
            }


            Destroy(gameObject);

            if (SecondaryGroundLvL.isSecondaryGroup(Group))
                SecondaryGroundLvL.RemovePos(ChunkNumber, CurrentPos);


            Destroyed = true;
        }

      

        public void AddGold(int n)
        {
            UnitNumbers.AddNumber(this, n);
            Owner.goldCount += n;
        }

        void Start()
        {
            Destroyed = false;
            EntityIndex = LastEntIndex;
            LastEntIndex++;
           

            /*
            _GroundInfoIndex = ChunkUtil.GetIndex(ChunkNumber, CurrentPos);
            _GroundInfoObject = ChunkManager.GetChunkByNum(ChunkNumber)
                ?.GetGameObjectByIndex(CurrentPos);
            _chunkNumber = ChunkNumber;
            */
        }

        public override string ToString()
        {
            return "GameEntity[" + " name:" + OriginalName + " id:" + EntityIndex + " PrefIndex:" + PrefabIndex +
                   " Pos:" + _currentPos + " Group:" +
                   Group + " ]";
        }
    }
}