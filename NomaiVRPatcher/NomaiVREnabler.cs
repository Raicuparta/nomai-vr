using AssetsTools.NET;
using AssetsTools.NET.Extra;
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
        //Called by OWML
        public static void Main(string[] args)
        {
            var basePath = args.Length > 0 ? args[0] : ".";
            var gameManagersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.Combine("OuterWilds_Data", "globalgamemanagers"));
            var backupPath = BackupFile(gameManagersPath);

            Cleanup(AppDomain.CurrentDomain.BaseDirectory);

            CopyGameFiles(AppDomain.CurrentDomain.BaseDirectory, Path.Combine(basePath, "files"));

            PatchGlobalGameManagers(gameManagersPath, backupPath, basePath);
        }

        // List of assemblies to patch
        public static IEnumerable<string> TargetDLLs { get; } = new string[0];

        // Patches the assemblies
        public static void Patch(AssemblyDefinition assembly) { }

        // Called by BepInEx
        public static void Initialize()
        {
            var executablePath = Assembly.GetExecutingAssembly().Location;
            var gameManagersPath = Path.Combine(Path.Combine(executablePath, "OuterWilds_Data"), "globalgamemanagers");
            var patchersPath = Path.Combine(Path.Combine(Path.Combine(executablePath, "BepInEx"), "patchers"), "NomaiVR");
            var backupPath = BackupFile(gameManagersPath);

            Cleanup(executablePath);

            CopyGameFiles(executablePath, Path.Combine(patchersPath, "files"));

            PatchGlobalGameManagers(gameManagersPath, backupPath, patchersPath);
        }

        // Clean up files left from previous versions of the mod
        private static void Cleanup(string gamePath)
        {
            var dataPath = Path.Combine(gamePath, "OuterWilds_Data");
            var managedPath = Path.Combine(dataPath, "Managed");
            var pluginsPath = Path.Combine(dataPath, "Plugins");
            var toDelete = new[]
            {
                Path.Combine(managedPath, "SteamVR.dll"),
                Path.Combine(managedPath, "SteamVR_Actions.dll"),
                Path.Combine(pluginsPath, "openvr_api.dll"),
                Path.Combine(pluginsPath, "OVRPlugin.dll"),
            };

            foreach(var file in toDelete)
                if (File.Exists(file)) File.Delete(file);
        }

        private static void CopyGameFiles(string gamePath, string filesPath)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(filesPath);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + filesPath);
            }

            var dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(gamePath);

            // Get the files in the directory and copy them to the new location.
            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var tempPath = Path.Combine(gamePath, file.Name);
                file.CopyTo(tempPath, true);
            }

            foreach (var subdir in dirs)
            {
                var tempPath = Path.Combine(gamePath, subdir.Name);
                CopyGameFiles(tempPath, subdir.FullName);
            }
        }

        private static void PatchGlobalGameManagers(string gameManagersPath, string gameManagersBackup, string patchFilesPath)
        {
            var assetsManager = new AssetsManager();
            assetsManager.LoadClassPackage(Path.Combine(patchFilesPath, "classdata.tpk"));
            var assetsFileInstance = assetsManager.LoadAssetsFile(gameManagersBackup, false);
            var assetsFile = assetsFileInstance.file;
            var assetsFileTable = assetsFileInstance.table;
            assetsManager.LoadClassDatabaseFromPackage(assetsFile.typeTree.unityVersion);

            var replacers = new List<AssetsReplacer>();

            var playerSettings = assetsFileTable.GetAssetInfo(1);
            var playerSettingsBase = assetsManager.GetTypeInstance(assetsFile, playerSettings).GetBaseField();
            var disableOldInputManagerSupport = playerSettingsBase.Get("enableNativePlatformBackendsForNewInputSystem");
            disableOldInputManagerSupport.value = new AssetTypeValue(EnumValueTypes.ValueType_Bool, false);
            replacers.Add(new AssetsReplacerFromMemory(0, playerSettings.index, (int)playerSettings.curFileType, 0xffff, playerSettingsBase.WriteToByteArray()));


            var buildSettings = assetsFileTable.GetAssetInfo(11);
            var buildSettingsBase = assetsManager.GetTypeInstance(assetsFile, buildSettings).GetBaseField();
            var enabledVRDevices = buildSettingsBase.Get("enabledVRDevices").Get("Array");
            var stringTemplate = enabledVRDevices.templateField.children[1];
            var vrDevicesList = new AssetTypeValueField[] { StringField("OpenVR", stringTemplate) };
            enabledVRDevices.SetChildrenList(vrDevicesList);
            replacers.Add(new AssetsReplacerFromMemory(0, buildSettings.index, (int)buildSettings.curFileType, 0xffff, buildSettingsBase.WriteToByteArray()));

            using (var writer = new AssetsFileWriter(File.OpenWrite(gameManagersPath)))
            {
                assetsFile.Write(writer, 0, replacers, 0);
            }
        }

        private static AssetTypeValueField StringField(string str, AssetTypeTemplateField template)
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
            File.Copy(fileName, backupName, true);
            return backupName;
        }
    }
}
