using UnityEngine;

namespace Settings
{
    public class GameSettings
    {
        private static float _sfxVolume = 1f;
        private static float _musicVolume = 1f;


        public static Resolution resolution { get; set; }
        public static bool IsFullscreen { get; set; }

        public static float musicVolume
        {
            get { return _musicVolume; }
            set { _musicVolume = value; }
        }

        public static float SfxVolume
        {
            get { return _sfxVolume; }
            set { _sfxVolume = value; }
        }
    }
}