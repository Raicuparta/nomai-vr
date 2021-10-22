using OWML.Common;
using OWML.ModHelper;
using NomaiVR.Loaders.Harmony;
using NomaiVR.ModConfig;

namespace NomaiVR.Loaders
{
    public class NomaiVRLoaderOwml : ModBehaviour
    {
        private IModHelper Helper { get; set; }
        internal void Start()
        {
            NomaiVR.HarmonyInstance = new OwmlHarmonyInstance(ModHelper);
            NomaiVR.ModFolderPath = Helper.Manifest.ModFolderPath;
            NomaiVR.GameDataPath = Helper.OwmlConfig.DataPath;
            NomaiVR.ApplyMod();
        }

        public override void Configure(IModConfig config)
        {
            Helper = ModHelper;
            var settingsProvider = new ModConfig.OwmlSettingsProvider(config);
            ModSettings.SetProvider(settingsProvider);
        }
    }
}
