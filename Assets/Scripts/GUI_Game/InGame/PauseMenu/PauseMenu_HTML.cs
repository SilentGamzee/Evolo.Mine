using PowerUI;
using UnityEngine;
using Input = UnityEngine.Input;

namespace GUI_Game.InGame.PauseMenu
{
    public class PauseMenu_HTML : MonoBehaviour
    {
        private static bool _isPaused;

        public static bool IsPaused
        {
            get { return _isPaused; }
            private set { 
                _isPaused = value;
                SetVisible(value);
            }
        }

        void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                IsPaused = !IsPaused;
            }
        }

        private static void SetVisible(bool isVisible)
        {
            UI.document.Run("SetPauseMenuVisible", isVisible);
        }


        public static void OnResume(MouseEvent mouseEvent)
        {
            IsPaused = false;
        }
        
        public static void OnExit(MouseEvent mouseEvent)
        {
            Application.Quit();
        }
    }
}