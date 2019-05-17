using System.Linq;
using Settings;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Menus
{
    public class GUI_MainMenu : MonoBehaviour
    {
        public GameObject ButtonsPanel;
        public GameObject settingsMenu;

        private Dropdown resolutionDrop;
        private Resolution[] resolutions;
        private Toggle fullscreenToggle;
        private Slider musicSlider;
        private Slider sfxSlider;


        void Awake()
        {
            settingsMenu.SetActive(false);
            var returnButton = settingsMenu.transform.GetChild(1).GetComponent<Button>();
            musicSlider = settingsMenu.transform.GetChild(2).GetChild(1).GetComponent<Slider>();
            sfxSlider = settingsMenu.transform.GetChild(3).GetChild(1).GetComponent<Slider>();
            resolutionDrop = settingsMenu.transform.GetChild(4).GetChild(1).GetComponent<Dropdown>();
            fullscreenToggle = settingsMenu.transform.GetChild(5).GetComponent<Toggle>();

            var startGame = ButtonsPanel.transform.GetChild(0).GetComponent<Button>();
            var settings = ButtonsPanel.transform.GetChild(1).GetComponent<Button>();
            var exitGame = ButtonsPanel.transform.GetChild(2).GetComponent<Button>();


            fullscreenToggle.onValueChanged.AddListener(delegate { OnFullscreenChange(); });


            //Resolution preset
            resolutions = Screen.resolutions;
            resolutions = resolutions.ToList().FindAll(x => x.width >= 800 && x.height >= 600).Distinct().ToArray();

            foreach (var res in resolutions)
            {
                resolutionDrop.options.Add(new Dropdown.OptionData(res.width + "x" + res.height));
            }
            
            var curIndex = resolutions.ToList().FindIndex(x =>
                x.width == Screen.currentResolution.width && x.height == Screen.currentResolution.height);
            resolutionDrop.value = curIndex;
            GameSettings.resolution = resolutions[curIndex];

            //Sliders preset
            musicSlider.value = GameSettings.musicVolume;
            sfxSlider.value = GameSettings.SfxVolume;
            
            
            //Listeners
            returnButton.onClick.AddListener(OnReturn);
            musicSlider.onValueChanged.AddListener(delegate { OnMusicValueChange(); });
            sfxSlider.onValueChanged.AddListener(delegate { OnSFXValueChanged(); });
            resolutionDrop.onValueChanged.AddListener(delegate { OnResolutionChange(); });

            startGame.onClick.AddListener(OnStartGame);
            settings.onClick.AddListener(OnSettings);
            exitGame.onClick.AddListener(OnExit);
        }

        void OnMusicValueChange()
        {
            GameSettings.musicVolume = musicSlider.value;
        }

        void OnSFXValueChanged()
        {
            GameSettings.SfxVolume = sfxSlider.value;
        }

        void OnFullscreenChange()
        {
            var curRes = GameSettings.resolution;
            GameSettings.IsFullscreen = fullscreenToggle.isOn;

            Screen.SetResolution(curRes.width, curRes.height, fullscreenToggle.isOn);
            // Screen.fullScreenMode = FullScreenMode.Windowed;
        }


        void OnResolutionChange()
        {
            var res = resolutions[resolutionDrop.value];
            GameSettings.resolution = res;
            Screen.SetResolution(
                res.width,
                res.height,
                Screen.fullScreen);
        }


        void OnStartGame()
        {
            SceneManager.LoadScene(1);
        }

        void OnSettings()
        {
            settingsMenu.SetActive(true);
        }

        void OnReturn()
        {
            settingsMenu.SetActive(false);
        }

        void OnExit()
        {
            Application.Quit();
        }
    }
}