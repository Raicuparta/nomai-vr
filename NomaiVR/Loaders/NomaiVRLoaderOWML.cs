using OWML.Common;
using OWML.ModHelper;
using System.IO;

namespace NomaiVR.Loaders
{
    public class NomaiVRLoaderOWML : ModBehaviour
    {
        private IModHelper Helper { get; set; }
        internal void Start()
        {
            NomaiVR.HarmonyInstance = new OWMLHarmonyInstance(ModHelper);
            NomaiVR.ModFolderPath = Helper.Manifest.ModFolderPath;
            NomaiVR.ApplyMod();
        }

        public override void Configure(IModConfig config)
        {
            Helper = ModHelper;
            var settingsProvider = new ModConfig.OWMLSettingsProvider(config);
            ModSettings.SetProvider(settingsProvider);
        }
    }
}
