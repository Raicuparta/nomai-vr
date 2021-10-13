#if !OWML
extern alias BepInEx;
using BepInEx;
using System.IO;

namespace NomaiVR.Loaders.BepInEx
{
    [BepInPlugin("raicuparta.nomaivr", "NomaiVR", "2.0.0")]
    public class NomaiVRLoaderBIE : BaseUnityPlugin
    {
        internal void Awake()
        {
            var harmony = new BepInEx::HarmonyLib.Harmony("raicuparta.nomaivr");
            NomaiVR.HarmonyInstance = new BIEHarmonyInstance(harmony);
            NomaiVR.ModFolderPath = $"{Directory.GetCurrentDirectory()}/BepInEx/plugins/NomaiVR/";
            var settingsProvider = new ModConfig.BepInExSettingsProvider(Config);
            ModSettings.SetProvider(settingsProvider);
        }

        internal void Start()
        {
            NomaiVR.ApplyMod();
        }
    }
}
#endif