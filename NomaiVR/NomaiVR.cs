using OWML.Common;
using OWML.ModHelper;
using System.Collections;
using Valve.VR;

namespace NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        public static IModHelper Helper;
        public static ModSaveFile Save;
        public static ModConfig Config;
        public static NomaiVR Instance;

        internal void Start()
        {
            Instance = this;
            Helper.Console.WriteLine("Start NomaiVR");
            Save = ModHelper.Storage.Load<ModSaveFile>(ModSaveFile.FileName);

            InitializeSteamVR();

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
            new GesturePrompts();
            new PostCreditsFix();
            new LookArrow();
            new DisableDeathAnimation();
            new Menus();
        }

        public override void Configure(IModConfig config)
        {
            Helper = ModHelper;
            Config = new ModConfig(config);
        }

        public static void Log(string message, MessageType messageType = MessageType.Message)
        {
            if (Helper != null && (Config == null || Config.debugMode))
            {
                Helper.Console.WriteLine(message, messageType);
            }
        }

        private void InitializeSteamVR()
        {
            SteamVR_Settings.instance.actionsFilePath = Helper.Manifest.ModFolderPath + @"\bindings\actions.json";
            SteamVR.Initialize();
        }
    }
}
