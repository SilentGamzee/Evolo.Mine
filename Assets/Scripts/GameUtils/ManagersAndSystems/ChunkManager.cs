using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharpLua;
using CSharpLua.LuaGetters;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GameUtils.UsualUtils;
using GlobalMechanic.Interact;
using GlobalMechanic.NonInteract;
using GUI_Game.InGame.AbilityBar;
using MoonSharp.Interpreter;
using UnitsMechanic.Groups;
using UnityEngine;
using UnityEngine.UI;

namespace GameUtils.ManagersAndSystems
{
    public class ChunkManager : MonoBehaviour
    {
        [Header("Map size (in tiles)")] public int MapSize = 2;
        [Header("Map generation")] public bool fillMap;
        [Header("SpEnemies")] public bool spEnemies = true;
        [Header("Vision Object")] public GameObject vision;
        [Header("Cycle system")] public bool CycleSystemCheck;
        [Header("Enemy count")] public int enemyCount;
        [Header("Fog enabled")] public bool fogEnabled;
        [Header("Flags enabled")] public bool flags;


        public static GameObject static_vision;
        public static int StaticMapSize { get; set; }

        public static int MaxGroundsLvls
        {
            get { return _maxGroundsLvls; }
        }

        public static int ChunkBorder = 20;

        public static Chunk CurrentChunk { get; private set; }


        private static Transform BATYAtrans;
        public static int staticEnemyCount;
        public static bool StaticSpEnemies;
        public static bool staticFogEnabled;
        public static bool staticFillMap;
        public static bool staticFlagsEnabled;

        private static readonly ArrayList Groups = new ArrayList();
        private static readonly Dictionary<int, Chunk> ChunkDict = new Dictionary<int, Chunk>();


        //Save indexes for game logic
        public static List<int> stairsIndexes = new List<int>();

        public static List<int> groundIndexes = new List<int>();
        public static List<int> treeIndexes = new List<int>();

        //CONSTS
        private const int _maxGroundsLvls = 3;


        void Awake()
        {
            CycleSystem.SystemEnabled = CycleSystemCheck;
            StaticMapSize = MapSize;
            BATYAtrans = transform;
            StaticSpEnemies = spEnemies;
            static_vision = vision;
            staticEnemyCount = enemyCount;
            staticFogEnabled = fogEnabled;
            staticFillMap = fillMap;
            staticFlagsEnabled = flags;


            CreatureGroupManager.ClearGroups();

            GroupsList.PreInit();
        }

        void Start()
        {
            LuaLibrariesIniter.PostInit();
            PathCalcManager.PreSetup();

            LanguageTextManager.Init();
            var map = new DefaultMap();

            SetupNewChunk(map);
            var center = CurrentChunk.Ground[0][(int) (StaticMapSize / 2f)][(int) (StaticMapSize / 2f)].CurrentPos;
            //SetCameraAt(new Vector2Int(center.x, center.y));

            PlayersManager.SetupPlayer();
        }

        private static void UpdateIndexesLists(Map map)
        {
            var listStairs = new List<int>
            {
                map.BackStairs,
                map.LeftStairs,
                map.RightStairs
            };
            stairsIndexes.AddRange(listStairs);


            var listGrounds = new List<int>
            {
                map.FirstGround
            };

            var list = map.ZeroGround.Select(kv => Loader.GetIndexByName(kv.Value)).Where(num => num != 404).ToList();

            groundIndexes.AddRange(list);
            groundIndexes.AddRange(listGrounds);

            treeIndexes.Add(map.Tree);
        }


        public static GameObject InitPrefab(string name, Vector3Int pos)
        {
            var index = Loader.GetIndexByName(name);
            var prefab = Loader.GetPrefabByIndex(index);
            var vec = Util.Get3DPosByIndex(pos);

            var obj = Instantiate(prefab, vec + prefab.transform.position, new Quaternion());
            return obj;
        }

        public static GameEntity InitObject(int chunkNumber, string name, Vector3Int vecInt, int index,
            Table npc, Player owner)
        {
            var chunk = GetChunkByNum(chunkNumber);
            if (chunk == null) return null;

            var vec = Util.Get3DPosByIndex(vecInt.x, vecInt.y, vecInt.z);

            var vecIntPos = new Vector3Int(vecInt.x, vecInt.y, vecInt.z);


            var prefab = Loader.GetPrefabByIndex(index);
            var obj = Instantiate(prefab, vec + prefab.transform.position, new Quaternion());
            if (IsGroupVVithName(prefab.name))
            {
                var trans = GetGroupVVithName(prefab.name);
                obj.transform.SetParent(trans);
            }
            else
            {
                var o = new GameObject {name = prefab.name};

                o.transform.SetParent(BATYAtrans);
                Groups.Add(o);

                obj.transform.SetParent(o.transform);
            }


            var group = LuaNpcGetter.GetNpcGroup(npc);

            Stats stats = null;
            if (!GroupUtil.IsGround(group))
            {
                stats = LuaNpcGetter.GetStatsFromTable(npc);
                obj.layer = 9;


                if (staticFogEnabled)
                    obj.AddComponent<FogCoverable>();
            }


            var abilities = new List<Ability>();
            if (!GroupUtil.IsGround(group))
            {
                if (!GroupUtil.IsItem(group) && staticFogEnabled)
                {
                    var spRend = obj.GetComponent<SpriteRenderer>();
                    if (spRend != null)
                        spRend.enabled = false;
                }

                if (!GroupUtil.IsNeutral(group) && !GroupUtil.IsItem(group))
                {
                    var npcAbilitiesNames = LuaNpcGetter.GetNpcAbilitiesNames(npc);
                    foreach (var KV in npcAbilitiesNames)
                    {
                        var ability = LuaAbilitiesGetter.GetAbility(KV.Value);
                        ability.Owner = owner;
                        abilities.Add(ability);
                    }
                }
            }

            GameEntity ent;
            if (GroupUtil.IsGround(group))
            {
                ent = obj.AddComponent<GameEntity>();
                ent.Init(chunkNumber, vecIntPos, false, index);
                ent.Group = group;

                chunk.SetIndex(vecIntPos, index);

                obj.layer = 8;
                chunk.IndexMas[vecInt.z][vecInt.x][vecInt.y] = index;
                chunk.Ground[vecInt.z][vecInt.x][vecInt.y] = ent;
            }
            else if (GroupUtil.IsItem(group))
            {
                var item = obj.AddComponent<GameItem>();

                item.Init(chunkNumber, vecIntPos, false, index, stats);
                item.Group = group;

                var npcModifiersNames = LuaNpcGetter.GetNpcModifiersNames(npc);
                item.ModifierNamesList.AddRange(npcModifiersNames);

                ent = item;
            }
            else
            {
                var unit = obj.AddComponent<GameUnit>();

                unit.Init(chunkNumber, vecIntPos, false, index, stats);
                unit.Group = group;

                var soundVolume = LuaNpcGetter.GetNpcSoundVolume(npc);
                var soundByIndex = Loader.GetSoundByIndex(index, soundVolume);

                unit.GameAudio = soundByIndex;
                unit.itemDrop = LuaNpcGetter.GetNpcItemDrop(npc);

                CreatureGroupManager.Init(unit);

                if (GroupUtil.isBuilding(group))
                {
                    var researchName = LuaNpcGetter.GetNpcResearch(npc);
                    if (researchName.Length > 0)
                    {
                        var research = ResearchManager.SetupResearch(researchName);
                        unit.research = research;
                        owner.AddResearch(research);
                        AbilityBar_HTML.Update();
                    }
                    BuildingsGroupManager.Init(unit);
                }

                foreach (var ab in abilities)
                    unit.AddAbility(ab);

                ent = unit;
                chunk.IndexMas[vecInt.z][vecInt.x][vecInt.y] = index;
                chunk.Ground[vecInt.z][vecInt.x][vecInt.y] = ent;
            }
            ent.OriginalName = name;
            ent.ExtraPos = prefab.transform.position;


            ent.foodCost = LuaNpcGetter.GetNpcFoodCost(npc);
            ent.foodGive = LuaNpcGetter.GetNpcFoodGive(npc);

            owner.foodCount += ent.foodCost;
            owner.foodMax += ent.foodGive;


            ent.goldDrop = LuaNpcGetter.GetNpcGoldDrop(npc);


            //obj.name = prefab.name + "_" + vecInt.z + "_" + vecInt.x + "_" + vecInt.y;


            PathCalcManager.CalculatePoint(vecInt);
            PathCalcManager.CalculatePoint(ChunkUtil.GetDovvner(vecInt));


            return ent;
        }

        static bool IsGroupVVithName(string pname)
        {
            foreach (GameObject group in Groups)
            {
                if (group != null && group.name.Equals(pname))
                {
                    return true;
                }
            }

            return false;
        }

        static Transform GetGroupVVithName(string pname)
        {
            foreach (GameObject group in Groups)
            {
                if (group != null && group.name.Equals(pname))
                {
                    return group.transform;
                }
            }

            return new RectTransform();
        }

        public static Chunk GetChunkByNum(int n)
        {
            return ChunkDict.ContainsKey(n) ? ChunkDict[n] : null;
        }

        public static Chunk SetupNewChunk(Map map)
        {
            return SetupNewChunk(map, Chunk.ChunkCounter);
        }

        private static Chunk SetupNewChunk(Map map, int chunkNum)
        {
            UpdateIndexesLists(map);
            var chunk = new Chunk(StaticMapSize, map);
            ChunkDict.Add(chunkNum, chunk);
            if (CurrentChunk == null) CurrentChunk = chunk;
            chunk.SetupChunk();
            FieldOfView.UpdateFog();
            return chunk;
        }

        public static void AddVision(GameEntity ent)
        {
            if (ent.Owner.Id != PlayersManager.GetMyPlayer().Id) return;
            var vision = Instantiate(static_vision, ent.Current3Dpos, Quaternion.identity);
            vision.transform.SetParent(ent.transform);
        }

        public static void ChangeChunkTo(int chunkNumber)
        {
            ChangeChunkTo(chunkNumber, new DefaultMap());
        }

        public static void ChangeChunkTo(int chunkNumber, Map map)
        {
            CurrentChunk.ClearChunk();

            CurrentChunk = GetChunkByNum(chunkNumber);

            if (CurrentChunk == null)
                CurrentChunk = SetupNewChunk(map, chunkNumber);
            else
                CurrentChunk.SetupChunk();
        }


        public static void ClearChunks()
        {
            foreach (var chunk in ChunkDict.Values)
            {
                chunk.FullRemove();
            }
            CurrentChunk = null;
        }
    }
}