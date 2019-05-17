using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using UnityEngine;

namespace GameUtils.UsualUtils
{
    public class ChunkUtil
    {
        public static int GetIndex(int chunkNumber, Vector3Int vecIndex)
        {
            if (ChunkManager.CurrentChunk == null 
                || ChunkManager.CurrentChunk.IndexMas == null) return -1;
            return ChunkManager.CurrentChunk.IndexMas[vecIndex.z][vecIndex.x][vecIndex.y];
        }

        public static bool IsStairsIndex(int index)
        {
            return ChunkManager.stairsIndexes.Contains(index);
        }

        public static bool IsGroundIndex(int index)
        {
            return ChunkManager.groundIndexes.Contains(index);
        }

        public static bool IsMarkedIndex(int index)
        {
            return index <= 0;
        }

        public static bool IsGroundIndex(int chunkNumber, Vector3Int vecIndex)
        {
            var index = GetIndex(chunkNumber, vecIndex);
            return IsGroundIndex(index);
        }

        public static bool IsStairsIndex(int chunkNumber, Vector3Int vecIndex)
        {
            var index = GetIndex(chunkNumber, vecIndex);
            return IsStairsIndex(index);
        }

        public static bool IsCanStayHere(int chunkNumber, Vector3Int vecIndex)
        {
            var index = GetIndex(chunkNumber, vecIndex);
            return (IsGroundIndex(index) || IsStairsIndex(index));
        }

        public static bool IsAnyEntity(int chunkNumber, Vector3Int vecIndex)
        {
            var index = GetIndex(chunkNumber, vecIndex);
            return index > 0;
        }

        public static Vector3Int GetUpper(Vector3Int vecIndex)
        {
            if (vecIndex.z + 1 >= ChunkManager.MaxGroundsLvls)
                return vecIndex;
            vecIndex.z++;
            return vecIndex;
        }

        public static Vector3Int GetDovvner(Vector3Int vecIndex)
        {
            if (vecIndex.z - 1 < 0)
                return vecIndex;
            vecIndex.z--;
            return vecIndex;
        }

        public static bool IsGround(int chunkNumber, Vector3Int vecIndex)
        {
            var index = GetIndex(chunkNumber, vecIndex);
            return IsGroundIndex(index);
        }


        public static bool IsEntity(int chunkNumber, Vector3Int vecIndex)
        {
            var index = GetIndex(chunkNumber, vecIndex);
            return !IsGroundIndex(index) && !IsStairsIndex(index) && !IsMarkedIndex(index);
        }

        public static bool IsMarkedIndex(int chunkNumber, Vector3Int vecIndex)
        {
            var index = GetIndex(chunkNumber, vecIndex);
            return IsMarkedIndex(index);
        }

        static int GetVNSurroundingTiles(int[][] mas, int x, int y, bool edgesAreWalls)
        {
            /* von Neumann Neighbourhood looks like this ('T' is our Tile, 'N' is our Neighbour)
            * 
            *   N 
            * N T N
            *   N
            *   
            */

            int tileCount = 0;

            //Keep the edges as walls
            if (edgesAreWalls && (x - 1 == 0 || x + 1 == mas.Length || y - 1 == 0 ||
                                  y + 1 == mas[0].Length))
            {
                tileCount++;
            }

            //Ensure we aren't touching the left side of the map
            if ((x - 1 > 0 && mas[x - 1][y] > 0)
                || (y - 1 > 0 && mas[x][y - 1] > 0)
                || (x + 1 < mas.Length && mas[x + 1][y] > 0)
                || (y + 1 < mas[0].Length && mas[x][y + 1] > 0))
            {
                tileCount += 1;
            }


            return tileCount;
        }


        public static int[][] GenerateCellularAutomata(int width, int height, float seed, int fillPercent,
            bool edgesAreWalls, Map map)
        {
            //Seed our random number generator
            System.Random rand = new System.Random(seed.GetHashCode());
    
            //Set up the size of our array
            var mas = new int[width][];
            for (var i = 0; i < height; i++)
                mas[i] = new int[height];


            //Start looping through setting the cells.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (edgesAreWalls && (x == 0 || x == width - 1 || y == 0 ||
                                          y == height - 1))
                    {
                        //Set the cell to be active if edges are walls
                        mas[x][y] = map.FirstGround;
                    }
                    else
                    {
                        //Set the cell to be active if the result of rand.Next() is less than the fill percentage
                        mas[x][y] = (rand.Next(0, 100) < fillPercent) ? map.FirstGround : 0;
                    }
                }
            }
            return mas;
        }

        public static int[][] SmoothVNCellularAutomata(int[][] mas, bool edgesAreWalls, int smoothCount, Map map)
        {
            for (int i = 0; i < smoothCount; i++)
            {
                for (int x = 0; x < mas.Length; x++)
                {
                    for (int y = 0; y < mas[0].Length; y++)
                    {
                        //Get the surrounding tiles
                        int surroundingTiles = GetVNSurroundingTiles(mas, x, y, edgesAreWalls);

                        //Debug.Log(surroundingTiles);
                        if (edgesAreWalls && (x == 0 || x == mas.Length - 1 || y == 0 ||
                                              y == mas[0].Length - 1))
                        {
                            //Keep our edges as walls
                            mas[x][y] = map.FirstGround;
                        }
                        //von Neuemann Neighbourhood requires only 3 or more surrounding tiles to be changed to a tile
                        else if (surroundingTiles > 2)
                        {
                            mas[x][y] = map.FirstGround;
                        }
                        else if (surroundingTiles < 2)
                        {
                            mas[x][y] = 0;
                        }
                    }
                }
            }
            //Return the modified map
            return mas;
        }
    }
}