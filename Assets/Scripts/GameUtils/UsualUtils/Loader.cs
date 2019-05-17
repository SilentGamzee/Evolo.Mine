using System;
using System.Collections.Generic;
using System.Linq;
using CSharpLua;
using Sound;
using UnityEngine;

namespace GameUtils.UsualUtils
{
    public class Loader
    {
        public const string PrefabPath = "Prefabs/";
        public const string SoundPath = "Audio/";
        public const string ScriptPath = "Scripts/";

        private static readonly Dictionary<string, int> IndexList = new Dictionary<string, int>();
        private static readonly Dictionary<int, GameObject> PrefabList = new Dictionary<int, GameObject>();

        public static GameObject LoadPrefab(string path)
        {
            return Resources.Load<GameObject>(path);
        }

        public static AudioClip LoadSound(string path)
        {
            return Resources.Load<AudioClip>(path);
        }

        public static TextAsset LoadScript(string path)
        {
            return Resources.Load<TextAsset>(ScriptPath + path);
        }

        public static AudioClip GetSoundByIndex(int index, float volume)
        {
            var clip = GameSoundManager.GetSound(index);
            if (clip != null) return clip;

            var clipPath = LuaNpcGetter.GetNpcSoundPathById(index);
            if (clipPath == "") return null;
            clip = LoadSound(SoundPath + clipPath);
            if (clip == null) Debug.LogException(new Exception("Error. Sound not found for `" + index + "`"));
            GameSoundManager.AddSound(index, clip, volume);
            return clip;
        }


        public static GameObject GetPrefabByIndex(int index)
        {
            //Debug.Log("Getting index: " + index);
            if (PrefabList.ContainsKey(index) && PrefabList[index] != null)
            {
                return PrefabList[index];
            }

            var path = LuaNpcGetter.GetNpcModelById(index);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Loading prefab error. Cant find path. Model id: " + index);
                return GetError();
            }
            var prefab = LoadPrefab(PrefabPath + path);
            PrefabList.Add(index, prefab);
            return prefab;
        }

        public static int GetIndexByName(string name)
        {
            if (IndexList.ContainsKey(name))
            {
                return IndexList[name];
            }

            var npc = LuaNpcGetter.GetNpcByName(name);


            var list = npc.Pairs.ToArray();

            if (list.Length == 0) return GetErrorIndex();

            var index = LuaNpcGetter.GetNpcId(npc);

            IndexList.Add(name, index);

            return index;
        }

        public static GameObject GetPrefabByName(string name)
        {
            int index;
            if (IndexList.ContainsKey(name))
            {
                index = IndexList[name];
                return GetPrefabByIndex(index);
            }

            var npc = LuaNpcGetter.GetNpcByName(name);

            var list = npc.Pairs.ToArray();
            if (list.Length == 0) return GetError();

            index = LuaNpcGetter.GetNpcId(npc);
            IndexList.Add(name, index);

            if (PrefabList.ContainsKey(index))
            {
                return PrefabList[index];
            }
            var path = LuaNpcGetter.GetNpcModel(npc);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Loading prefab error. Cant find path. Model id: " + index);
                return GetError();
            }
            var prefab = LoadPrefab(PrefabPath + path);
            PrefabList.Add(index, prefab);
            return prefab;
        }

        public static GameObject GetError()
        {
            var path = LuaNpcGetter.GetNpcModelById(GetErrorIndex());
            Debug.Log("Error path = "+path);
            return LoadPrefab(PrefabPath + path);
        }

        public static int GetErrorIndex()
        {
            return -3;
        }
    }
}