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

            CopyGameFiles(Assembly.GetExecutingAssembly().Location, Path.Combine(patchersPath, "files"));

            PatchGlobalGameManagers(gameManagersPath, backupPath, patchersPath);
        }

        private static void CopyGameFiles(string gamePath, string filesPath)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(filesPath);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + filesPath);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(gamePath);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(gamePath, file.Name);
                file.CopyTo(tempPath, true);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(gamePath, subdir.Name);
                CopyGameFiles(tempPath, subdir.FullName);
            }
        }

        private static void PatchGlobalGameManagers(string gameManagersPath, string gameManagersBackup, string patchFilesPath)
        {
            AssetsManager assetsManager = new AssetsManager();
            assetsManager.LoadClassPackage(Path.Combine(patchFilesPath, "classdata.tpk"));
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
