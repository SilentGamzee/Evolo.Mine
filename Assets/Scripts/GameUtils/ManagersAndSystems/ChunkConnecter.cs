using System.Collections.Generic;
using GameUtils.Objects;

namespace GameUtils.ManagersAndSystems
{
    public class ChunkConnecter
    {
        // <from, <type, to> >
        private static readonly Dictionary<int, Dictionary<Map.MapTypes, int>> ConnectToDict =
            new Dictionary<int, Dictionary<Map.MapTypes, int>>();

        public static void SetConnection(Map.MapTypes type, int from, int to)
        {
            if (ConnectToDict.ContainsKey(from) && ConnectToDict[from].ContainsKey(type)) return;

            if (!ConnectToDict.ContainsKey(from))
                ConnectToDict.Add(from, new Dictionary<Map.MapTypes, int>());

            ConnectToDict[from].Add(type, to);
        }

        public static int GetFromConnection(Map.MapTypes type, int to)
        {
            foreach (var _1 in ConnectToDict)
            {
                var _2 = _1.Value;
                if (_2 != null
                    && _2.ContainsKey(type)
                    && _2[type] == to)
                    return _1.Key;
            }

            return -1;
        }

        public static int GetToConnection(Map.MapTypes type, int from)
        {
            if (!ConnectToDict.ContainsKey(from) || !ConnectToDict[from].ContainsKey(type)) return -1;

            return ConnectToDict[from][type];
        }

        public static bool IsConnectedToType(Map.MapTypes type, int from)
        {
            return ConnectToDict.ContainsKey(from) && ConnectToDict[from].ContainsKey(type);
        }
        
        
    }
}