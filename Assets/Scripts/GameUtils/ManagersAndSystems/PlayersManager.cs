using System.Collections.Generic;
using GameUtils.Objects;
using UnitsMechanic.AI_Logic;
using UnityEngine;

namespace GameUtils.ManagersAndSystems
{
    public class PlayersManager : MonoBehaviour
    {
        public static Player Empty { get; private set; }
        public static Player NeutralEnemyPlayer { get; private set; }
        private static readonly Dictionary<int, Player> PlayerList = new Dictionary<int, Player>();
        private const int MyId = 1;

        private const int clearRadius = 2;


        void Awake()
        {
            Empty = new Player(-1);
            NeutralEnemyPlayer = new Player(0);
        }

        public static Player GetPlayerById(int id)
        {
            return PlayerList.ContainsKey(id) ? PlayerList[id] : new Player();
        }

        public static void AddPlayer(int id, Player player)
        {
            if (!PlayerList.ContainsKey(id))
            {
                PlayerList.Add(id, player);
            }
        }

        public static Player GetMyPlayer()
        {
            return PlayerList.ContainsKey(MyId) ? PlayerList[MyId] : new Player(MyId);
        }

        public static void ClearPlayerList()
        {
            PlayerList.Clear();
        }


        public static Vector3Int GetRandomChunkPos()
        {
            var mapSize = ChunkManager.StaticMapSize;
            var x = Random.Range(7, mapSize - 1);
            var y = Random.Range(7, mapSize - 1);
            return new Vector3Int(x, y, 1);
        }

        public static void ClearPosesForPlayer(Vector3Int pos, int tileRadius)
        {
            var chunk = ChunkManager.CurrentChunk;
            for (int x = pos.x - tileRadius; x < pos.x + tileRadius; x++)
            {
                for (int y = pos.y - tileRadius; y < pos.y + tileRadius; y++)
                {
                    var ent = chunk.GetGameObjectByIndex(new Vector3Int(x, y, 1));
                    if (ent == null) continue;
                    if (GroupUtil.IsNeutral(ent.Group))
                        ent.KillSelf();
                }
            }
        }


        public static void SetupPlayer()
        {
            var chunk = ChunkManager.CurrentChunk;
            if (chunk == null) return;
            var setup = new Vector2Int(chunk.MapSize / 2, chunk.MapSize / 2);
            var player = PlayersManager.GetMyPlayer();


            GlobalMechanic.Interact.CameraMove.SetCameraAt(setup);

            chunk.SetupItem("building_secret_box", new Vector3Int(0, 0, 1), player);
            chunk.SetupItem("building_magic_box", new Vector3Int(0, 1, 1), player);


            player.goldCount = 100;
        }
    }
}