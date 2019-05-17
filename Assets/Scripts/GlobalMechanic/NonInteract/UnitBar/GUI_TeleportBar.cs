using System.Collections.Generic;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects;
using GameUtils.Objects.Entities;
using GlobalMechanic.Interact;
using UnityEngine;
using UnityEngine.UI;

namespace GlobalMechanic.NonInteract.UnitBar
{
    public class GUI_TeleportBar : MonoBehaviour
    {
        [Header("Next chunk bar")] public GameObject nextBar;
        [Header("Prev chunk bar")] public GameObject prevBar;

        private static GameObject staticNextBar;
        private static GameObject staticPrevBar;

        public static Dictionary<GameEntity, GameObject> TpDict = new Dictionary<GameEntity, GameObject>();

        void Start()
        {
            staticNextBar = nextBar;
            staticPrevBar = prevBar;
        }

        public static void SetupNextBar(GameEntity ent, int nextChunk, Map.MapTypes type)
        {
            if (TpDict.ContainsKey(ent)) return;
            
            var bar = staticNextBar.GetComponent<RectTransform>();

            var objPos = ent.Current3Dpos;

            var rend = ent.GetComponent<SpriteRenderer>();
            var posR = ClickManager.GetPosForChooser(objPos, rend.size,new Vector3());


            var nextBar = Instantiate(bar.gameObject,
                objPos,
                bar.rotation);

            nextBar.transform.SetParent(ent.transform);
            nextBar.transform.position = new Vector3(objPos.x, posR.y + bar.rect.height, -3);

            var button = nextBar.transform.GetChild(0).GetComponent<Button>();

            ChunkConnecter.SetConnection(type, ent.ChunkNumber, nextChunk);
            var map = Map.GetMapByType(type);
            button.onClick.AddListener(delegate { ChunkManager.ChangeChunkTo(nextChunk, map); });

            TpDict.Add(ent, nextBar);
        }
        
        public static void SetupNextBar(GameEntity ent, Map.MapTypes type)
        {
            Debug.LogError(TpDict.ContainsKey(ent));
            if (TpDict.ContainsKey(ent)) return;

            var nextChunk = ChunkConnecter.IsConnectedToType(type, ent.ChunkNumber)
                ? ChunkConnecter.GetToConnection(type, ent.ChunkNumber)
                : Chunk.ChunkCounter;

            SetupNextBar(ent, nextChunk, type);
        }

        public static void SetupPrevBar(GameEntity ent, Map.MapTypes type)
        {
            if (TpDict.ContainsKey(ent)) return;

            var bar = staticPrevBar.GetComponent<RectTransform>();

            var objPos = ent.Current3Dpos;

            var rend = ent.GetComponent<SpriteRenderer>();
            var posR = ClickManager.GetPosForChooser(objPos, rend.size,new Vector3());


            var prevBar = Instantiate(bar.gameObject,
                objPos,
                bar.rotation);

            prevBar.transform.SetParent(ent.transform);
            prevBar.transform.position = new Vector3(objPos.x, posR.y + bar.rect.height, -3);

            var button = prevBar.transform.GetChild(0).GetComponent<Button>();

            var prevChunk = ChunkConnecter.GetFromConnection(type, ent.ChunkNumber);

            button.onClick.AddListener(delegate { ChunkManager.ChangeChunkTo(prevChunk); });

            TpDict.Add(ent, prevBar);
        }
    }
}