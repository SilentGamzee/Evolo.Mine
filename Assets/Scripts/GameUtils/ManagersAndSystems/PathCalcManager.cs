using System;
using System.Linq;
using CSharpLua;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.UsualUtils;
using MoonSharp.Interpreter;
using UnitsMechanic;
using UnityEngine;

namespace GlobalMechanic.NonInteract
{
    public class PathCalcManager : MonoBehaviour
    {
        private static int[][][] ReachMasInts;
        private static bool _preCalculated;


        private Vector3Int _last = new Vector3Int(0, 0, 0);
        private float _iTime = 0;

        public static void InitLuaModule()
        {
            var script = LuaManager.ScriptObj;

            var t = new Table(script);
            t["IsReaching"] = (Func<Vector3Int, Vector3Int, bool>) (IsReaching);

            LuaManager.Vm.SetGlobal("PathCalcManager", t);
        }
        
        public static void PreSetup()
        {
            var lvls = ChunkManager.MaxGroundsLvls;
            var mapSize = ChunkManager.StaticMapSize;
            if (ReachMasInts == null)
            {
                ReachMasInts = new int[lvls][][];

                for (int i = 0; i < lvls; i++)
                {
                    ReachMasInts[i] = new int[mapSize][];
                    for (int j = 0; j < mapSize; j++)
                    {
                        ReachMasInts[i][j] = new int[mapSize];
                    }
                }
            }
        }

        public static void CalculatePathGroups()
        {
            for (int z = 0; z < ChunkManager.MaxGroundsLvls; z++)
            {
                for (int x = 0; x < ChunkManager.CurrentChunk.MapSize; x++)
                {
                    for (int y = 0; y < ChunkManager.CurrentChunk.MapSize; y++)
                    {
                        if (ReachMasInts[z][x][y] == 0)
                        {
                            OnElementCalculate(x, y, z);
                        }
                    }
                }
            }
            _preCalculated = true;
        }

        private static void OnElementCalculate(int x, int y, int z)
        {
            if (ReachMasInts == null) return;
            var helpVec = new Vector3Int(x,y,z);
            if (PathFind.IsInvalidPath(helpVec))
            {
                ReachMasInts[z][x][y] = -1;
                return;
            }


            var pathFind = new PathFind();
            for (int i = 1; i < 100; i++)
            {
                var vec = FindFirstReachInt(i);
                if (vec.x != -1)
                {
                    var path = pathFind.FindPath(helpVec, vec);
                    if (path == null)
                    {
                        ReachMasInts[z][x][y] = -1;
                        continue;
                    }
                    ReachMasInts[z][x][y] = i;
                    return;
                }

                ReachMasInts[z][x][y] = i;
                return;
            }
        }


        private static Vector3Int FindFirstReachInt(int reachInt)
        {
            var lvls = ChunkManager.MaxGroundsLvls;
            var mapSize = ChunkManager.CurrentChunk.MapSize;
            for (int z = 0; z < lvls; z++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    for (int y = 0; y < mapSize; y++)
                    {
                        if (ReachMasInts[z][x][y] == reachInt)
                        {
                            return new Vector3Int(x, y, z);
                        }
                    }
                }
            }
            return new Vector3Int(-1, -1, -1);
        }

        public static void CalculatePoint(Vector3Int pos)
        {
            if (!_preCalculated) return;
            var groundPos = ChunkUtil.GetDovvner(pos);


            var neighbourPoints = new Vector3Int[4];
            neighbourPoints[0] = new Vector3Int(pos.x + 1, pos.y, pos.z - 1);
            neighbourPoints[1] = new Vector3Int(pos.x - 1, pos.y, pos.z - 1);
            neighbourPoints[2] = new Vector3Int(pos.x, pos.y + 1, pos.z - 1);
            neighbourPoints[3] = new Vector3Int(pos.x, pos.y - 1, pos.z - 1);

            var chunk = ChunkManager.CurrentChunk;
            foreach (var point in neighbourPoints)
            {
                if (!chunk.IsMapPos(point)) continue;
                OnElementCalculate(point.x, point.y, point.z);

                var ent = chunk.GetGameObjectByIndex(point);
                if (ent != null)
                    ent.PathInt = ReachMasInts[point.z][point.x][point.y];
            }
            OnElementCalculate(groundPos.x, groundPos.y, groundPos.z);
        }


        private static int GetGroupByIndex(Vector3Int index)
        {
            return ReachMasInts[index.z][index.x][index.y];
        }


        private static int GetCountInGroup(int group)
        {
            var c = 0;

            var lvls = ChunkManager.MaxGroundsLvls;
            var mapSize = ChunkManager.CurrentChunk.MapSize;
            for (int z = 0; z < lvls; z++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    for (int y = 0; y < mapSize; y++)
                    {
                        if (ReachMasInts[z][x][y] == group)
                        {
                            c++;
                        }
                    }
                }
            }

            return c;
        }


        public static bool IsReaching(Vector3Int from, Vector3Int to)
        {
            var neighbourPoints = new Vector3Int[4];
            neighbourPoints[0] = new Vector3Int(from.x + 1, from.y, from.z);
            neighbourPoints[1] = new Vector3Int(from.x - 1, from.y, from.z);
            neighbourPoints[2] = new Vector3Int(from.x, from.y + 1, from.z);
            neighbourPoints[3] = new Vector3Int(from.x, from.y - 1, from.z);

            var chunk = ChunkManager.CurrentChunk;
            var groups = neighbourPoints.Select(p =>
            {
                if (chunk.IsMapPos(p))
                    return GetGroupByIndex(p);

                return -1;
            }).ToList();

            var destGroup = GetGroupByIndex(to);

            return groups.Contains(destGroup);
        }

        public static void ClearPathCalc()
        {
            ReachMasInts = null;
        }

        // Update is called once per frame
        void Update()
        {
            //too many garbage
            return;
            var chunk = ChunkManager.CurrentChunk;

            _iTime += Time.deltaTime;
            if (_iTime < 1f) return;
            _iTime = 0;

            OnElementCalculate(_last.x, _last.y, _last.z);

            _last.y++;
            if (chunk.IsMapPos(_last)) return;
            _last.y = 0;
            _last.x++;
            if (chunk.IsMapPos(_last)) return;
            _last.x = 0;
            _last.z++;
            if (!chunk.IsMapPos(_last))
                _last.z = 0;

            //OnElementCalculate(_last.x, _last.y, _last.z);
        }
    }
}