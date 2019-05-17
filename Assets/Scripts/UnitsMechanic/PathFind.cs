using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.UsualUtils;
using UnityEngine;

namespace UnitsMechanic
{
    public class PathFind
    {
        public class PathNode
        {
            // Координаты точки на карте.
            public Vector3Int Position { get; set; }

            // Длина пути от старта (G).
            public int PathLengthFromStart { get; set; }

            // Точка, из которой пришли в эту точку.
            public PathNode CameFrom { get; set; }

            // Примерное расстояние до цели (H).
            public int HeuristicEstimatePathLength { get; set; }

            // Ожидаемое полное расстояние до цели (F).
            public int EstimateFullPathLength()
            {
                return PathLengthFromStart + HeuristicEstimatePathLength;
            }
        }


        public List<Vector3Int> FindPath(Vector3Int start, Vector3Int goal)
        {
            // Шаг 1.
            var closedSet = new Collection<PathNode>();
            var openSet = new Collection<PathNode>();
            // Шаг 2.
            PathNode startNode = new PathNode()
            {
                Position = start,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
            };
            openSet.Add(startNode);
            while (openSet.Count > 0)
            {
                // Шаг 3.
                var currentNode = openSet.OrderBy(node =>
                    node.EstimateFullPathLength()).First();
                // Шаг 4.
                if (currentNode.Position == goal)
                    return GetPathForNode(currentNode);
                // Шаг 5.
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                // Шаг 6.
                foreach (var neighbourNode in GetNeighbours(currentNode, goal))
                {
                    // Шаг 7.
                    if (closedSet.Count(node => node.Position == neighbourNode.Position) > 0)
                        continue;
                    var openNode = openSet.FirstOrDefault(node => node.Position == neighbourNode.Position);
                    // Шаг 8.
                    if (openNode == null)
                        openSet.Add(neighbourNode);
                    else if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
                    {
                        // Шаг 9.
                        openNode.CameFrom = currentNode;
                        openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                    }
                }
            }
            // Шаг 10.
            return null;
        }

        private static int GetDistanceBetweenNeighbours()
        {
            return 1;
        }

        private static int GetHeuristicPathLength(Vector3Int from, Vector3Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
        }

        private static Collection<PathNode> GetNeighbours(PathNode pathNode,
            Vector3Int goal)
        {
            var result = new Collection<PathNode>();

            // Соседними точками являются соседние по стороне клетки.
            Vector3Int[] neighbourPoints = new Vector3Int[4];
            neighbourPoints[0] = new Vector3Int(pathNode.Position.x + 1, pathNode.Position.y, pathNode.Position.z);
            neighbourPoints[1] = new Vector3Int(pathNode.Position.x - 1, pathNode.Position.y, pathNode.Position.z);
            neighbourPoints[2] = new Vector3Int(pathNode.Position.x, pathNode.Position.y + 1, pathNode.Position.z);
            neighbourPoints[3] = new Vector3Int(pathNode.Position.x, pathNode.Position.y - 1, pathNode.Position.z);

            var chunk = ChunkManager.CurrentChunk;
            foreach (var point in neighbourPoints)
            {
                // Проверяем, что не вышли за границы карты.
                if (point.x < 0 || point.x >= chunk.MapSize)
                    continue;
                if (point.y < 0 || point.y >= chunk.MapSize)
                    continue;

                if (ChunkUtil.IsStairsIndex(chunk.ChunkNumber, ChunkUtil.GetUpper(point)))
                {
                    var p = ChunkUtil.GetUpper(point);
                    point.Set(p.x, p.y, p.z);
                }
                else if (!ChunkUtil.IsAnyEntity(chunk.ChunkNumber, point)
                         && ChunkUtil.IsStairsIndex(chunk.ChunkNumber, pathNode.Position))
                {
                    var p = ChunkUtil.GetDovvner(point);
                    point.Set(p.x, p.y, p.z);
                }


                if (point.z < 0 || point.z >= ChunkManager.MaxGroundsLvls)
                    continue;


                // Проверяем, что по клетке можно ходить.
                if (IsInvalidPath(point)
                )
                    continue;
                // Заполняем данные для точки маршрута.
                var neighbourNode = new PathNode()
                {
                    Position = point,
                    CameFrom = pathNode,
                    PathLengthFromStart = pathNode.PathLengthFromStart +
                                          GetDistanceBetweenNeighbours(),
                    HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
                };
                result.Add(neighbourNode);
            }
            return result;
        }

        private static List<Vector3Int> GetPathForNode(PathNode pathNode)
        {
            var result = new List<Vector3Int>();
            var currentNode = pathNode;
            while (currentNode != null)
            {
                result.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }
            result.Reverse();
            return result;
        }

        public static bool IsInvalidPath(Vector3Int point)
        {
            var chunkNumber = ChunkManager.CurrentChunk.ChunkNumber;
            return (point.z + 1 < ChunkManager.MaxGroundsLvls
                    && ChunkUtil.IsAnyEntity(chunkNumber, ChunkUtil.GetUpper(point)))
                   || ChunkUtil.IsMarkedIndex(chunkNumber, point)
                   || ChunkUtil.IsEntity(chunkNumber, point);
        }
    }
}