using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BepInEx;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NomaiVRPatcher
{
    /// <summary>
    /// Patches the game to use both old and new input system
    /// Moves VR Plugin files to the appropriate folders
    /// </summary>
    public static class NomaiVREnabler
    {
        //Called by Doorstop
        public static void Main()
        {
            var gameManagersPath = Path.Combine("OuterWilds_Data", "globalgamemanagers");
            var backupPath = BackupFile(gameManagersPath);

            //TODO: Copy relevant files?

            PatchGlobalGameManagers(gameManagersPath, backupPath, ".");
        }

        // List of assemblies to patch
        public static IEnumerable<string> TargetDLLs { get; } = new string[0];

        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly) { }

        // Called by BepInEx
        public static void Initialize()
        {
            var executablePath = Assembly.GetExecutingAssembly().Location;
            var gameManagersPath = Path.Combine(Path.Combine(Paths.ManagedPath, ".."), "globalgamemanagers");
            var backupPath = BackupFile(gameManagersPath);

            //TODO: Copy relevant files?

            PatchGlobalGameManagers(gameManagersPath, backupPath, Paths.PatcherPluginPath);
        }

        private static void PatchGlobalGameManagers(string gameManagersPath, string gameManagersBackup, string patchFilesPath)
        {
            AssetsManager assetsManager = new AssetsManager();
            assetsManager.LoadClassPackage(Path.Combine(Path.Combine(patchFilesPath, "NomaiVR"), "classdata.tpk"));
            AssetsFileInstance assetsFileInstance = assetsManager.LoadAssetsFile(gameManagersBackup, false);
            AssetsFile assetsFile = assetsFileInstance.file;
            AssetsFileTable assetsFileTable = assetsFileInstance.table;
            assetsManager.LoadClassDatabaseFromPackage(assetsFile.typeTree.unityVersion);

            List<AssetsReplacer> replacers = new List<AssetsReplacer>();

            AssetFileInfoEx playerSettings = assetsFileTable.GetAssetInfo(1);
            AssetTypeValueField playerSettingsBase = assetsManager.GetTypeInstance(assetsFile, playerSettings).GetBaseField();
            AssetTypeValueField disableOldInputManagerSupport = playerSettingsBase.Get("enableNativePlatformBackendsForNewInputSystem");
            disableOldInputManagerSupport.value = new AssetTypeValue(EnumValueTypes.ValueType_Bool, false);
            replacers.Add(new AssetsReplacerFromMemory(0, playerSettings.index, (int)playerSettings.curFileType, 0xffff, playerSettingsBase.WriteToByteArray()));


            AssetFileInfoEx buildSettings = assetsFileTable.GetAssetInfo(11);
            AssetTypeValueField buildSettingsBase = assetsManager.GetTypeInstance(assetsFile, buildSettings).GetBaseField();
            AssetTypeValueField enabledVRDevices = buildSettingsBase.Get("enabledVRDevices").Get("Array");
            AssetTypeTemplateField stringTemplate = enabledVRDevices.templateField.children[1];
            AssetTypeValueField[] vrDevicesList = new AssetTypeValueField[] { StringField("OpenVR", stringTemplate) };
            enabledVRDevices.SetChildrenList(vrDevicesList);
            replacers.Add(new AssetsReplacerFromMemory(0, buildSettings.index, (int)buildSettings.curFileType, 0xffff, buildSettingsBase.WriteToByteArray()));

            using (AssetsFileWriter writer = new AssetsFileWriter(File.OpenWrite(gameManagersPath)))
            {
                assetsFile.Write(writer, 0, replacers, 0);
            }
        }

        static AssetTypeValueField StringField(string str, AssetTypeTemplateField template)
        {
            return new AssetTypeValueField()
            {
                children = null,
                childrenCount = 0,
                templateField = template,
                value = new AssetTypeValue(EnumValueTypes.ValueType_String, str)
            };
        }

        private static string BackupFile(string fileName)
        {
            var backupName = fileName + ".bak";
            if (!File.Exists(backupName))
                File.Copy(fileName, backupName);
            return backupName;
        }
    }
}
