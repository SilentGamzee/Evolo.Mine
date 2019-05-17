using System.Collections.Generic;
using GameUtils.UsualUtils;
using Settings;
using UnityEngine;

namespace Sound
{
    public class GameSoundManager : MonoBehaviour
    {
        private static readonly Dictionary<int, AudioClip> audioDict = new Dictionary<int, AudioClip>();
        private static readonly Dictionary<int, float> audioVolumeDict = new Dictionary<int, float>();
        private static AudioSource staticMusicSource;
        private static AudioSource staticSfxSource;
        public AudioSource musicSource;
        public AudioSource sfxSource;

        private static AudioClip EvoSound;
        private static float EvoVolume = 0.35f;

        void Awake()
        {
            musicSource.volume = GameSettings.musicVolume;
            sfxSource.volume = GameSettings.SfxVolume;

            staticMusicSource = musicSource;
            staticSfxSource = sfxSource;
            var defaultTakingDamage = Loader.LoadSound(Loader.SoundPath + "Default_taking_damage");
            EvoSound = Loader.LoadSound(Loader.SoundPath + "evolution");

            AddSound(0, defaultTakingDamage, 0.35f);
            
        }

        public static void AddSound(int id, AudioClip clip)
        {
            AddSound(id, clip, 1f);
        }

        public static void AddSound(int id, AudioClip clip, float volume)
        {
            if (volume > 1f) volume = 1f;
            else if (volume < 0) volume = 0;

            audioDict[id] = clip;
            audioVolumeDict[id] = volume;
        }


        public static AudioClip GetSound(int id)
        {
            return !audioDict.ContainsKey(id) ? null : audioDict[id];
        }

        public static void SetIDVolume(int id, float volume)
        {
            audioVolumeDict[id] = volume;
        }

        public static void PlayOnTakeDamage(int id)
        {
            var sound = GetSound(id) ?? GetSound(0);
            if (sound == null) return;
            var volume = audioVolumeDict.ContainsKey(id) ? audioVolumeDict[id] : audioVolumeDict[0];
            //staticSfxSource.PlayOneShot(sound, volume);
        }

        public static void PlayOnEvolution()
        {
            if (EvoSound == null) return;
            staticSfxSource.PlayOneShot(EvoSound, EvoVolume);
        }
    }
}