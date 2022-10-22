using AssetsTools.NET;
using AssetsTools.NET.Extra;
using System;
using System.Collections.Generic;
using System.IO;

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
            var gamePath = AppDomain.CurrentDomain.BaseDirectory;
            var gameManagersPath = Path.Combine(gamePath, Path.Combine(GetDataPath(gamePath), "globalgamemanagers"));
            var backupPath = BackupFile(gameManagersPath);

            Cleanup(gamePath);

            CopyGameFiles(gamePath, Path.Combine(basePath, "files"));

            //PatchGlobalGameManagers(gameManagersPath, backupPath, basePath);
        }

        private static string GetExecutableName(string gamePath)
        {
            var executableNames = new[] {"Outer Wilds.exe", "OuterWilds.exe"};
            foreach (var executableName in executableNames)
            {
                var executablePath = Path.Combine(gamePath, executableName);
                if (File.Exists(executablePath))
                {
                    return Path.GetFileNameWithoutExtension(executablePath);
                }
            }

            throw new FileNotFoundException($"Outer Wilds exe file not found in {gamePath}");
        }

        private static string GetDataDirectoryName()
        {
            var gamePath = AppDomain.CurrentDomain.BaseDirectory;
            return $"{GetExecutableName(gamePath)}_Data";
        }
        
        private static string GetDataPath(string gamePath)
        {
            return Path.Combine(gamePath, $"{GetDataDirectoryName()}");
        }

        // Clean up files left from previous versions of the mod
        private static void Cleanup(string gamePath)
        {
            var dataPath = GetDataPath(gamePath);
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
                CopyGameFiles(tempPath.Replace("OuterWilds_Data", GetDataDirectoryName()), subdir.FullName);
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

            #if !UNITYEXPLORER
            var playerSettings = assetsFileTable.GetAssetInfo(1);
            var playerSettingsBase = assetsManager.GetTypeInstance(assetsFile, playerSettings).GetBaseField();
            var disableOldInputManagerSupport = playerSettingsBase.Get("enableNativePlatformBackendsForNewInputSystem");
            disableOldInputManagerSupport.value = new AssetTypeValue(EnumValueTypes.ValueType_Bool, false);
            replacers.Add(new AssetsReplacerFromMemory(0, playerSettings.index, (int)playerSettings.curFileType, 0xffff, playerSettingsBase.WriteToByteArray()));
            #endif


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
