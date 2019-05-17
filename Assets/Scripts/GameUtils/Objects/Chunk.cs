using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharpLua;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic.Interact;
using GlobalMechanic.NonInteract;
using GlobalMechanic.NonInteract.UnitBar;
using ItemMechanic;
using MoonSharp.Interpreter;
using UnitsMechanic;
using UnitsMechanic.AI_Logic;
using UnitsMechanic.Groups;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GameUtils.Objects
{
    public class Chunk
    {
        public Map map { get; private set; }
        public int ChunkNumber { get; private set; }

        public static int ChunkCounter
        {
            get { return _chunkCounter; }
            private set { _chunkCounter = value; }
        }

        public readonly int[][][] IndexMas;
        public readonly GameEntity[][][] Ground;

        private GameObject _groundGroup, _anotherGroup;
        public int MapSize { get; private set; }
        private static bool firstCreate;
        private static int _chunkCounter = 0;

        private List<GameObject> borderList = new List<GameObject>();


        private readonly ArrayList _markList = new ArrayList();


        public Chunk(int MapSize, Map map)
        {
            var MaxGroundsLvls = ChunkManager.MaxGroundsLvls;
            IndexMas = new int[MaxGroundsLvls][][];
            Ground = new GameEntity[MaxGroundsLvls][][];
            this.map = map;

            this.MapSize = MapSize;


            ChunkNumber = ChunkCounter;
            ChunkCounter++;
        }


        private void SetupChunkBorder()
        {
            var borderObject = new GameObject();
            borderObject.name = "GroundBorder";
            var mapSize = ChunkManager.StaticMapSize;
            var borderSize = ChunkManager.ChunkBorder;

            for (int x = -borderSize; x < borderSize; x++)
            {
                for (int y = -borderSize; y < borderSize; y++)
                {
                    if (x < 0 || y < 0 || x >= mapSize || y >= mapSize)
                    {
                        var n = 1;
                        if ((y == mapSize && x <= mapSize && x > -1) || (x == -1 && y <= mapSize && y > -1))
                            n--;
                        for (int z = 1; z >= n; z--)
                        {
                            var objName = z == 0 ? map.ZeroGround[0].Value : map.BorderName;
                            var obj = ChunkManager.InitPrefab(objName, new Vector3Int(x, y, z));
                            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
                            spriteRenderer.color = new Color(0.6f, 0.6f, 0.6f);
                            obj.transform.SetParent(borderObject.transform);
                        }
                    }
                }
            }
        }

        // Use this for initialization
        public void SetupChunk()
        {
            SetupChunkBorder();
            firstCreate = false;
            if (Ground[0] == null)
            {
                firstCreate = true;
                if (ChunkManager.CurrentChunk == null)
                {
                    // var rand = Random.value;
                    //  Debug.Log("Random = " + rand);
                    var seed = Random.Range(int.MinValue, int.MaxValue);
                    // var n = (int) (Util.GetCountAfterPoint(rand) * rand);
                    Random.InitState(seed);
                }

                var mapZeroGround = map.ZeroGround;

                for (int z = 0; z < ChunkManager.MaxGroundsLvls; z++)
                {
                    Ground[z] = new GameEntity[MapSize][];
                    IndexMas[z] = new int[MapSize][];
                    for (int i = 0; i < MapSize; i++)
                    {
                        Ground[z][i] = new GameEntity[MapSize];
                        IndexMas[z][i] = new int[MapSize];
                        for (int j = 0; j < MapSize; j++)
                        {
                            if (z == 0 && mapZeroGround.Count != 0)
                            {
                                var nGround = map.ZeroGround[0].Value;
                                foreach (var kv in map.ZeroGround)
                                {
                                    if (!(Random.value <= kv.Key)) continue;
                                    nGround = kv.Value;
                                    break;
                                }

                                IndexMas[z][i][j] = Loader.GetIndexByName(nGround);
                            }
                            else
                            {
                                IndexMas[z][i][j] = 0;
                            }
                        }
                    }
                }

                //PreSetup Ground
                if (ChunkManager.staticFillMap)
                {
                    for (int z = 1; z < ChunkManager.MaxGroundsLvls - 1; z++)
                    {
                        IndexMas[z] = ChunkUtil.GenerateCellularAutomata(
                            MapSize, MapSize, 1, 90, true, map);
                        //IndexMas[z] = ChunkUtil.SmoothVNCellularAutomata(IndexMas[z], false, 2, map);
                    }
                }

                /*
                //PreSetuping indexes
                for (int z = 1; z < ChunkManager.MaxGroundsLvls; z++)
                {
                    _markList.Clear();
                    // IndexMas[z] = Randomize(IndexMas[z], rand / z, Interval);
                    // IndexMas[z] = AddingStairs(IndexMas[z], map);
                    //IndexMas[z] = AddingTreesInMapIndex(IndexMas[z], map);
                    if (z == ChunkManager.MaxGroundsLvls - 1) break;
                    IndexMas[z + 1] = MarkGroundUpper(IndexMas[z], IndexMas[z + 1]);
                    foreach (Vector2Int vec in _markList)
                    {
                        IndexMas[z + 1][vec.x][vec.y] = -1;
                    }
                }
                */
            }


            SetUpTiles(IndexMas);
            if (firstCreate)
            {
                SetupMapAdditionals();

                if (ChunkManager.StaticSpEnemies)
                    SetUpEnemies(map);
            }
/*
            
            var creatures = CreatureGroupManager.GetAllInChunk(ChunkNumber);
            foreach (var unit in creatures)
            {
                var pos = unit.CurrentPos;
                IndexMas[pos.z][pos.x][pos.y] = unit.PrefabIndex;
                Ground[pos.z][pos.x][pos.y] = unit;
                unit.Enable();
            }

            


            var chunkGround = SecondaryGroundLvL.GetChunkGround(ChunkNumber).ToArray();


            foreach (var KV in chunkGround)
            {
                var item = KV.Value;
                SetupItem(item.OriginalName, item.CurrentPos, item.Owner);
                //Destroy prev item must be
            }

            SetupTeleported();

            


            Coloring.RecolorAll();

            ItemGroup.PreInit();

            */
            //PathCalcManager.CalculatePathGroups();
            Debug.LogFormat("Preloaded in: {0} sec", Time.realtimeSinceStartup.ToString("F5"));
        }

        private void SetupMapAdditionals()
        {
            //TODO: revvork map additionals
        }

        private void SetupTeleported()
        {
            var tpList = TeleportManager.GetTeleportList(ChunkNumber);
            GameUnit unit = null;
            foreach (var npcName in tpList)
            {
                var pos2Int = GetFreePos();
                var pos3Int = new Vector3Int(pos2Int.x, pos2Int.y, 1);
                if (npcName.Contains("npc"))
                {
                    unit = SetupUnit(npcName, pos3Int, PlayersManager.GetMyPlayer());
                }
                else if (npcName.Contains("item") || npcName.Contains("building"))
                {
                    SetupItem(npcName, pos3Int, PlayersManager.GetMyPlayer());
                }
            }

            if (unit != null)
                CameraMove.SetCameraAt(new Vector2Int(unit.CurrentPos.x, unit.CurrentPos.y));
            TeleportManager.SetTeleported(ChunkNumber);
        }

        public Vector2Int GetFreePos()
        {
            var emptySlots = new List<Vector2Int>();

            for (var i = 0; i < IndexMas[1].Length; i++)
            {
                for (var j = 0; j < IndexMas[1][i].Length; j++)
                {
                    if (IndexMas[1][i][j] != -1) continue;
                    emptySlots.Add(new Vector2Int(i, j));
                }
            }
            if (emptySlots.Count == 0) return new Vector2Int(0, 0);

            var range = Random.Range(0, emptySlots.Count);


            return emptySlots[range];
        }

        public Vector3Int GetFreePosAnyLvl()
        {
            var emptySlots = new List<Vector3Int>();

            for (var i = 0; i < IndexMas[1].Length; i++)
            {
                for (var j = 0; j < IndexMas[1][i].Length; j++)
                {
                    for (int z = 1; z < ChunkManager.MaxGroundsLvls; z++)
                    {
                        if (IndexMas[1][i][j] != -1 ||
                            !ChunkUtil.IsGround(ChunkNumber,
                                ChunkUtil.GetDovvner(new Vector3Int(i, j, z)))) continue;
                        emptySlots.Add(new Vector3Int(i, j, z));
                    }
                }
            }
            if (emptySlots.Count == 0) return new Vector3Int(0, 0, 0);

            var range = Random.Range(0, emptySlots.Count);
            return emptySlots[range];
        }


        public GameEntity PreSetupObject(string name, Vector3Int pos, Player owner)
        {
            var index = Loader.GetIndexByName(name);
            var npc = LuaNpcGetter.GetNpcById(index);

            return ChunkManager.InitObject(ChunkNumber, name, pos, index, npc, owner);
        }

        public GameItem SetupItem(GameEntity ent, string name, Vector3Int pos, Player owner)
        {
            var index = Loader.GetIndexByName(name);
            var npc = LuaNpcGetter.GetNpcById(index);

            ent.Owner = owner;
            ent.OriginalName = name;
            ent.name = name;
            var item = ent as GameItem;

            var evoTo = LuaNpcGetter.GetEvolutionTo(npc);
            if (evoTo.Length > 0)
            {
                if (!UnitEvolution.IsHasSoloEvolution(name))
                    UnitEvolution.AddToSoloDict(name, evoTo);

                if (item != null)
                    item.SoloEvolution = true;
            }

            var evoCross = LuaNpcGetter.GetNpcEvoCrossing(npc);
            if (evoCross.Keys.Count > 0)
            {
                foreach (var pair in evoCross)
                {
                    UnitEvolution.AddToStackDict(name, pair.Key, pair.Value);

                    if (!string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                    {
                        UnitEvolution.AddToStackDict(pair.Key, name, pair.Value);
                    }
                }
            }
            if (GroupUtil.IsItem(ent.Group))
                SecondaryGroundLvL.SetGroundEnt(ChunkNumber, pos, item);

            if (GroupUtil.isBuilding(ent.Group))
                ChunkManager.AddVision(ent);
            ItemEvents.OnCreateItem(item, firstCreate);

            Coloring.RecolorObject(ChunkUtil.GetDovvner(ent.CurrentPos));

            return item;
        }


        public GameItem SetupItem(string name, Vector3Int pos, Player owner)
        {
            var ent = PreSetupObject(name, pos, owner);
            return SetupItem(ent, name, pos, owner);
        }


        public GameUnit SetupUnit(GameEntity ent, string name, Vector3Int pos, Player owner)
        {
            var index = Loader.GetIndexByName(name);
            var npc = LuaNpcGetter.GetNpcById(index);

            ent.Owner = owner;
            ent.OriginalName = name;
            ent.name = name;
            var unit = ent as GameUnit;

            var evoTo = LuaNpcGetter.GetEvolutionTo(npc);
            if (evoTo.Length > 0)
            {
                if (!UnitEvolution.IsHasSoloEvolution(name))
                    UnitEvolution.AddToSoloDict(name, evoTo);

                if (unit != null)
                    unit.SoloEvolution = true;
            }

            var evoCross = LuaNpcGetter.GetNpcEvoCrossing(npc);
            if (evoCross.Keys.Count > 0)
            {
                foreach (var pair in evoCross)
                {
                    UnitEvolution.AddToStackDict(name, pair.Key, pair.Value);

                    if (!string.Equals(pair.Key, name, StringComparison.OrdinalIgnoreCase))
                    {
                        UnitEvolution.AddToStackDict(pair.Key, name, pair.Value);
                    }
                }
            }

            ChunkManager.AddVision(ent);
            UnitEvents.OnUnitSpawned(unit);


            Coloring.RecolorObject(ChunkUtil.GetDovvner(ent.CurrentPos));

            return unit;
        }


        public GameUnit SetupUnit(string name, Vector3Int pos, Player owner)
        {
            var ent = PreSetupObject(name, pos, owner);


            return SetupUnit(ent, name, pos, owner);
        }


        public void SetUpEnemies()
        {
            SetUpEnemies(map);
        }

        public void SetUpEnemies(Map map)
        {
            var types = map.enemyList;

            if (types.Count == 0) return;
            var enemyPlayer1 = PlayersManager.NeutralEnemyPlayer;
            for (var i = 0; i < ChunkManager.staticEnemyCount; i++)
            {
                var randomChunkPos = PlayersManager.GetRandomChunkPos();
                PlayersManager.ClearPosesForPlayer(randomChunkPos, 1);

                var enemyType = types[Random.Range(0, types.Count)];


                foreach (var entName in enemyType)
                {
                    var setup = AI_Calculation.FindFreePosNearPos(ChunkNumber, randomChunkPos, false);
                    SetupUnit(entName, new Vector3Int(setup.x, setup.y, 1), enemyPlayer1);
                }
            }
        }


        int[][] MarkGroundUpper(int[][] map, int[][] upperMap)
        {
            for (int i = 0; i < map.Length; i++)
            {
                for (int j = 0; j < map[i].Length; j++)
                {
                    if (!ChunkUtil.IsGroundIndex(map[i][j]))
                    {
                        upperMap[i][j] = -1;
                    }
                }
            }

            return upperMap;
        }


        int[][] AddingStairs(int[][] mas, Map map)
        {
            for (int i = 1; i < mas.Length - 2; i++)
            {
                for (int j = 1; j < mas[i].Length - 2; j++)
                {
                    if (mas[i][j] != 2) continue;
                    if (mas[i][j + 1] == 0)
                    {
                        if (Random.value < 0.1f)
                        {
                            _markList.Add(new Vector2Int(i, j + 1));
                            mas[i][j + 1] = map.RightStairs;
                        }
                    }
                    if (mas[i + 1][j] == 0)
                    {
                        if (mas[i + 1][j + 1] != 1)
                            if (Random.value < 0.1f)
                            {
                                _markList.Add(new Vector2Int(i + 1, j));
                                mas[i + 1][j] = map.BackStairs;
                            }
                    }
                    if (mas[i][j - 1] != 0) continue;
                    if (mas[i][j - 2] != 0) continue;
                    if (!(Random.value < 0.1f)) continue;
                    _markList.Add(new Vector2Int(i, j - 1));
                    mas[i][j - 1] = map.LeftStairs;
                }
            }

            return mas;
        }

        void SetUpTiles(int[][][] mas)
        {
            for (var z = 0; z < ChunkManager.MaxGroundsLvls; z++)
            {
                for (var x = 0; x < MapSize; x++)
                {
                    for (var y = 0; y < MapSize; y++)
                    {
                        var index = mas[z][x][y];
                        if (index > 0)
                        {
                            var npc = LuaNpcGetter.GetNpcById(index);
                            var npcName = LuaNpcGetter.GetNpcName(npc);
                            ChunkManager.InitObject(ChunkNumber, npcName, new Vector3Int(x, y, z),
                                index, npc, PlayersManager.Empty);
                        }
                        else
                        {
                            IndexMas[z][x][y] = -1;
                        }
                    }
                }
            }
        }

        public void ReindexTiles()
        {
            for (int z = 0; z < ChunkManager.MaxGroundsLvls; z++)
            {
                for (int x = 0; x < MapSize; x++)
                {
                    for (int y = 0; y < MapSize; y++)
                    {
                        if (Ground[z][x][y] != null)
                        {
                            var ent = Ground[z][x][y].GetComponent<GameEntity>();
                            var index = (int) ent.PrefabIndex;
                            IndexMas[z][x][y] = index;
                        }
                        else
                        {
                            IndexMas[z][x][y] = -1;
                        }
                    }
                }
            }
        }

        public void SetIndex(Vector3Int vecIndex, int index)
        {
            if (IsMapPos(vecIndex)) IndexMas[vecIndex.z][vecIndex.x][vecIndex.y] = index;
        }

        public void RemoveObject(Vector3Int vecIndex)
        {
            if (IsMapPos(vecIndex))
                Ground[vecIndex.z][vecIndex.x][vecIndex.y] = null;
        }


        //Svvap indexes and objects
        public void MoveFromTo(Vector3Int indexFrom, Vector3Int indexTo)
        {
            var tileIndex = IndexMas[indexFrom.z][indexFrom.x][indexFrom.y];
            IndexMas[indexFrom.z][indexFrom.x][indexFrom.y] = IndexMas[indexTo.z][indexTo.x][indexTo.y];
            IndexMas[indexTo.z][indexTo.x][indexTo.y] = tileIndex;

            var tile = Ground[indexFrom.z][indexFrom.x][indexFrom.y];
            Ground[indexFrom.z][indexFrom.x][indexFrom.y] = Ground[indexTo.z][indexTo.x][indexTo.y];
            Ground[indexTo.z][indexTo.x][indexTo.y] = tile;
        }

        public void MoveTo(Vector3Int indexFrom, Vector3Int indexTo)
        {
            IndexMas[indexFrom.z][indexFrom.x][indexFrom.y] = IndexMas[indexTo.z][indexTo.x][indexTo.y];

            Ground[indexFrom.z][indexFrom.x][indexFrom.y] = Ground[indexTo.z][indexTo.x][indexTo.y];

            IndexMas[indexTo.z][indexTo.x][indexTo.y] = -1;
            Ground[indexTo.z][indexTo.x][indexTo.y] = null;
        }


        public GameEntity GetGameObjectByIndex(Vector3Int vecIndex)
        {
            return IsMapPos(vecIndex) ? Ground[vecIndex.z][vecIndex.x][vecIndex.y] : null;
        }

        public bool IsMapPos(Vector3Int vecIndex)
        {
            return vecIndex.z >= 0 && vecIndex.z < ChunkManager.MaxGroundsLvls
                   && vecIndex.x >= 0 && vecIndex.x < MapSize
                   && vecIndex.y >= 0 && vecIndex.y < MapSize;
        }

        public void SetObjectAtPos(Vector3Int pos, GameEntity ent)
        {
            if (IsMapPos(pos))
            {
                Ground[pos.z][pos.x][pos.y] = ent;
            }
        }


        public void FullRemove()
        {
            for (var z = 0; z < ChunkManager.MaxGroundsLvls; z++)
            {
                for (var x = 0; x < MapSize; x++)
                {
                    for (var y = 0; y < MapSize; y++)
                    {
                        var pos = new Vector3Int(x, y, z);
                        if (!SecondaryGroundLvL.IsEmptyPos(ChunkNumber, pos))
                            SecondaryGroundLvL.GetGroundEnt(ChunkNumber, pos).KillSelf();


                        IndexMas[z][x][y] = 0;
                        var obj = Ground[z][x][y];
                        if (obj == null) continue;
                        obj.KillSelf();
                    }
                }
            }
            SecondaryGroundLvL.FullRemoveChunk(ChunkNumber);
        }

        public void ClearChunk()
        {
            for (var z = 0; z < ChunkManager.MaxGroundsLvls; z++)
            {
                for (var x = 0; x < MapSize; x++)
                {
                    for (var y = 0; y < MapSize; y++)
                    {
                        var obj = Ground[z][x][y];
                        if (obj == null) continue;

                        if (obj is GameUnit)
                        {
                            var unit = obj as GameUnit;
                            unit.Disable();
                            IndexMas[z][x][y] = 0;
                            Ground[z][x][y] = null;
                        }
                        else
                        {
                            var index = obj.PrefabIndex;
                            obj.KillSelf();
                            IndexMas[z][x][y] = index;
                        }
                    }
                }
            }
            SecondaryGroundLvL.RemoveChunk(ChunkNumber);
        }
    }
}