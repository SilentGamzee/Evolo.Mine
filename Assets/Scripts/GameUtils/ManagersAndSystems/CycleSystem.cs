using GameUtils.Objects;
using GameUtils.Objects.Entities;
using UnityEngine;

namespace GameUtils.ManagersAndSystems
{
    public class CycleSystem : MonoBehaviour
    {
        public static bool SystemEnabled;
        //Day 300/ Night 600
        public const int DayTime = 60;

        const float OneTick = 1f / DayTime;
        public static int CurrentTime { get; private set; }

        private bool _goingToDay;

        //Unity debugger
        public int timeUntilDayTimeChange;

        private bool GoingToDay
        {
            get { return _goingToDay; }
            set
            {
                _goingToDay = value;
                OnDayTimeChange();
            }
        }

        public bool EnableVisualSystem = false;

        private const float minColor = 0.2f;
        private static float _t = 0f;

        private static bool _enabled;

        // Use this for initialization
        void Start()
        {
            if (!SystemEnabled) return;
            CurrentTime = DayTime - 1;
            _goingToDay = CurrentTime != DayTime - 1;


            _enabled = EnableVisualSystem;
        }

        private void OnDayTimeChange()
        {
            if (!SystemEnabled) return;
            if (ChunkManager.StaticSpEnemies)
                ChunkManager.CurrentChunk.SetUpEnemies();
            //ChunkManager.CurrentChunk.SetupTrees();
        }

        // Update is called once per frame
        void Update()
        {
            if (!SystemEnabled) return;
            _t += Time.deltaTime;
            if (_t < 1f) return;
            _t = 0;


            if (GoingToDay)
                CurrentTime += 1;
            else
                CurrentTime -= 1;

            if (CurrentTime == DayTime)
                GoingToDay = false;
            else if (CurrentTime == 0)
                GoingToDay = true;

            timeUntilDayTimeChange = DayTime - CurrentTime;
        }

        public static Color GetColorByDayTime()
        {
            var c = CurrentTime * OneTick;
            if (c < minColor) c = minColor;
            return new Color(c, c, c);
        }

        public static void UpdateColor(GameEntity ent)
        {
            if (ent.Destroyed || !_enabled) return;
            ent.GetComponent<SpriteRenderer>().color = GetColorByDayTime();
        }
    }
}