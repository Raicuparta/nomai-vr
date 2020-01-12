using OWML.Common;
using OWML.ModHelper;
using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        public static IModHelper Helper;
        static NomaiVR _instance;
        SubmitActionLoadScene _submit;
        Button _resumeGameButton;

        public static void Log(string s) {
            _instance.ModHelper.Console.WriteLine("NomaiVR: " + s);
        }

        void Start() {
            _instance = this;

            Log("Start Main");

            Helper = ModHelper;

            // Add all modules here.
            gameObject.AddComponent<Menus>();
            gameObject.AddComponent<FogFix>();
            gameObject.AddComponent<PlayerBodyPosition>();

            //_titleScreenStreaming = FindObjectOfType<TitleScreenStreaming>();
            _resumeGameButton = GameObject.Find("Button-ResumeGame").GetAddComponent<Button>();
            Log("resume active " + _resumeGameButton.gameObject.activeInHierarchy);

            //GameObject.Find("TitleScreenManagers").GetComponent<TitleScreenManager>().Invoke("SetUpMenu");

            Log("o hno");
            FindObjectOfType<TitleScreenAnimation>().SetValue("_fadeDuration", 0);
            FindObjectOfType<TitleScreenAnimation>().SetValue("_gamepadSplash", false);
            FindObjectOfType<TitleScreenAnimation>().SetValue("_introPan", false);
            FindObjectOfType<TitleScreenManager>().Invoke("FadeInTitleLogo");


            FindObjectOfType<TitleAnimationController>().SetValue("_logoFadeDelay", 0.001f);
            FindObjectOfType<TitleAnimationController>().SetValue("_logoFadeDuration", 0.001f);
            FindObjectOfType<TitleAnimationController>().SetValue("_optionsFadeDelay", 0.001f);
            FindObjectOfType<TitleAnimationController>().SetValue("_optionsFadeDuration", 0.001f);
            FindObjectOfType<TitleAnimationController>().SetValue("_optionsFadeSpacing", 0.001f);

            //GameObject.Find("GamepadSplashCanvas").SetActive(false);

            //_resumeGameButton.GetComponent<SubmitActionLoadScene>().EnableConfirm(false);


            Log("gonna load....");
            //LoadManager.LoadSceneAsync(OWScene.SolarSystem, false, LoadManager.FadeType.ToBlack, 1f, false);
            Log("loaded....");

            //NomaiVR.Helper.Events.Subscribe<TitleScreenStreaming>(Events.BeforeAwake);
            //NomaiVR.Helper.Events.OnEvent += OnEvent;

            Invoke("Resume", 0.5f);
        }

        void Resume() {
            Log("Resume!!!!!!!!!!!!!!");
            ExecuteEvents.Execute(_resumeGameButton.gameObject, null, ExecuteEvents.submitHandler);
        }


        void Update() {
            //_resumeGameButton.onClick.Invoke();
            //Log("is?");
            //if (LoadManager.IsAsyncLoadComplete() && _titleScreenStreaming.AreRequiredAssetsLoaded()) {
            //    Log("try....");
            //    LoadManager.EnableAsyncLoadTransition();
            //    Log("do....");
            //}
        }
    }
}
