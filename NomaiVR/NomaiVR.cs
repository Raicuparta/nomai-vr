using NomaiVR.Assets;
using Valve.VR;
using NomaiVR.EffectFixes;
using NomaiVR.Hands;
using NomaiVR.Helpers;
using NomaiVR.Input;
using NomaiVR.Tools;
using NomaiVR.UI;
using NomaiVR.Loaders;
using NomaiVR.Loaders.Harmony;
using NomaiVR.Player;
using NomaiVR.Saves;

namespace NomaiVR
{
    public class NomaiVR
    {
        public static IHarmonyInstance HarmonyInstance;
        public static ModSaveFile Save;
        public static string ModFolderPath;
        public static string GameDataPath;

        internal static void ApplyMod()
        {
            Save = ModSaveFile.LoadSaveFile();
            new FatalErrorChecker();
            new AssetLoader();

            InitSteamVR();
            
            //// Load all modules.
            //// I'm sorry to say that order does matter here.
            new ForceSettings();
            new ControllerInput();
            new Dialogue();
            new FogFix();
            new ShadowsFix();
            new LoopTransitionFix();
            new VisorEffectsFix();
            new DreamLanternFix();
            new DreamFix();
            new ProjectionStoneCameraFix();
            new PeepholeCameraFix();
            new CameraMaskFix();
            new MapFix();
            new PlayerBodyPosition();
            new VRToolSwapper();
            new HandsController();
            new ShipTools();
            new AutopilotButtonPatch();
            new FlashlightGesture();
            new HoldMallowStick();
            new HoldProbeLauncher();
            new HoldSignalscope();
            new HoldTranslator();
            new HoldItem();
            new HoldPrompts();
            new LaserPointer();
            new FeetMarker();
            new HelmetHUD();
            new InputPrompts();
            new RemoveUnusedInputPrompts();
            new ControllerModels();
            new GesturePrompts();
            new PostCreditsFix();
            new LookArrow();
            new DisableDeathAnimation();
            new VirtualKeyboard();
            new Menus();
        }

        private static void InitSteamVR()
        {
            try
            {
                SteamVR_Actions.PreInitialize();
                SteamVR.Initialize();
                SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;

                ApplicationManifestHelper.UpdateManifest(GameDataPath + @"\StreamingAssets\outerwilds.vrmanifest",
                                                        "steam.app.753640",
                                                        "https://steamcdn-a.akamaihd.net/steam/apps/753640/header.jpg",
                                                        "Outer Wilds VR",
                                                        "NomaiVR mod for Outer Wilds",
                                                        steamBuild: SteamManager.Initialized,
                                                        steamAppId: 753640);

                OpenVR.Input.SetActionManifestPath(ModFolderPath + @"\bindings\actions.json");
            }
            catch
            {
                FatalErrorChecker.ThrowSteamVRError();
            }
        }
    }
}
