using System.Collections.Generic;

namespace GameUtils.ManagersAndSystems
{
    public class TeleportManager
    {
        private static readonly Dictionary<int, List<string>> ChunkTpOutDict =
            new Dictionary<int, List<string>>();


        public static bool IsChunkTpContainsNpc(int chunk, string npc)
        {
            return ChunkTpOutDict.ContainsKey(chunk) && ChunkTpOutDict[chunk].Contains(npc);
        }

        public static void TeleportToChunk(int toChunk, string npc)
        {
            if (!ChunkTpOutDict.ContainsKey(toChunk)) ChunkTpOutDict[toChunk] = new List<string>();

            ChunkTpOutDict[toChunk].Add(npc);
        }

        public static void SetTeleported(int chunk)
        {
            if (!ChunkTpOutDict.ContainsKey(chunk)) return;
            ChunkTpOutDict[chunk].Clear();
            ChunkTpOutDict.Remove(chunk);
        }

        public static List<string> GetTeleportList(int chunkNumber)
        {
            return !ChunkTpOutDict.ContainsKey(chunkNumber)
                ? new List<string>()
                : ChunkTpOutDict[chunkNumber];
        }
    }
}