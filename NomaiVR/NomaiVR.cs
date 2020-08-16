using OWML.Common;
using OWML.ModHelper;
using Valve.VR;

namespace NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        public static IModHelper Helper;
        public static ModSaveFile Save;

        internal void Start()
        {
            Helper.Console.WriteLine("Start NomaiVR");
            Save = ModSaveFile.LoadSaveFile();
            new FatalErrorChecker();

            InitSteamVR();

            new AssetLoader();


            // Load all modules.
            // I'm sorry to say that order does matter here.
            new ForceSettings();
            new ControllerInput();
            new Dialogue();
            new FogFix();
            new LoopTransitionFix();
            new WaterFix();
            new ProjectionStoneCameraFix();
            new CameraMaskFix();
            new MapFix();
            new PlayerBodyPosition();
            new VRToolSwapper();
            new HandsController();
            new ShipTools();
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
            new ControllerModels();
            new GesturePrompts();
            new PostCreditsFix();
            new LookArrow();
            new DisableDeathAnimation();
            new Menus();
        }

        private void InitSteamVR()
        {
            try
            {
                SteamVR.Initialize();
                SteamVR_Settings.instance.pauseGameWhenDashboardVisible = true;
                OpenVR.Input.SetActionManifestPath(Helper.Manifest.ModFolderPath + @"\bindings\actions.json");
            }
            catch
            {
                FatalErrorChecker.ThrowSteamVRError();
            }
        }

        public override void Configure(IModConfig config)
        {
            Helper = ModHelper;
            ModSettings.SetConfig(config);
        }
    }
}
