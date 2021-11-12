using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Valve.Newtonsoft.Json;
using Valve.VR;

namespace NomaiVR.Assets
{
    public class AssetLoader
    {
        public static GameObject PostCreditsPrefab;
        public static RenderTexture PostCreditsRenderTexture;
        public static GameObject HandPrefab;
        public static Dictionary<string, SteamVR_Skeleton_Pose> Poses;
        public static SteamVR_Skeleton_Pose FallbackRelaxedPose;
        public static SteamVR_Skeleton_Pose FallbackPointPose;
        public static SteamVR_Skeleton_Pose FallbackFistPose;
        public static SteamVR_Skeleton_Pose ReachForPose;
        public static SteamVR_Skeleton_Pose GrabbingHandlePose;
        public static SteamVR_Skeleton_Pose GrabbingHandleGlovePose;
        public static GameObject FeetPositionPrefab;
        public static GameObject ScopeLensPrefab;
        public static GameObject HelmetPrefab;
        public static GameObject LookArrowPrefab;
        public static GameObject AutopilotButtonPrefab;
        public static GameObject ProbeScreenButtonsPrefab;
        public static GameObject SignalscopeScreenButtonsPrefab;
        public static GameObject TranslatorHandheldButtonsPrefab;
        public static GameObject ProbeLauncherHandheldScreenPrefab;
        public static GameObject SignalscopeHandheldButtonsPrefab;
        public static AssetBundle VRBindingTextures;
        public static Sprite SplashSprite;
        public static Texture2D EmptyTexture;

        public AssetLoader()
        {
            EmptyTexture = new Texture2D(1, 1);
            EmptyTexture.SetPixel(0, 0, Color.clear);
            EmptyTexture.Apply();

            VRBindingTextures = LoadBundle("vrbindings-textures");
            ShaderLoader.LoadBundle(LoadBundle("steamvr-shaders"));

            var postCreditsBundle = LoadBundle("cinema-camera");
            PostCreditsPrefab = LoadAsset<GameObject>(postCreditsBundle, "postcreditscamera.prefab");
            PostCreditsRenderTexture = LoadAsset<RenderTexture>(postCreditsBundle, "screen.renderTexture");

            var skeletalHandsBundle = LoadBundle("skeletal-hands");
            HandPrefab = LoadAsset<GameObject>(skeletalHandsBundle, "Assets/skeletal_hand.prefab");

            var feetPositionBundle = LoadBundle("feetposition");
            FeetPositionPrefab = LoadAsset<GameObject>(feetPositionBundle, "feetposition.prefab");

            var scopeLensBundle = LoadBundle("scope-lens");
            ScopeLensPrefab = LoadAsset<GameObject>(scopeLensBundle, "scopelens.prefab");

            var helmetBundle = LoadBundle("helmet");
            HelmetPrefab = LoadAsset<GameObject>(helmetBundle, "helmet.prefab");

            var lookArrowBundle = LoadBundle("look-arrow");
            LookArrowPrefab = LoadAsset<GameObject>(lookArrowBundle, "lookarrow.prefab");

            var autopilotBundle = LoadBundle("autopilot-button");
            AutopilotButtonPrefab = LoadAsset<GameObject>(autopilotBundle, "models/tools/autopilot/autopilot_button.prefab");

            var cockpitButtonsBundle = LoadBundle("cockpit-buttons");
            ProbeScreenButtonsPrefab = LoadAsset<GameObject>(cockpitButtonsBundle, "shipcockpit/probescreenbuttons/probescreenbuttons.prefab");
            SignalscopeScreenButtonsPrefab = LoadAsset<GameObject>(cockpitButtonsBundle, "shipcockpit/signalscopescreenbuttons.prefab");

            var handheldButtonsBundle = LoadBundle("handheld-buttons");
            TranslatorHandheldButtonsPrefab = LoadAsset<GameObject>(handheldButtonsBundle, "handheldtools/translatorhandheldbuttons.prefab");
            ProbeLauncherHandheldScreenPrefab = LoadAsset<GameObject>(handheldButtonsBundle, "handheldtools/probelauncherscreen.prefab");
            SignalscopeHandheldButtonsPrefab = LoadAsset<GameObject>(handheldButtonsBundle, "handheldtools/signalscopebuttons.prefab");

            var splashBundle = LoadBundle("splash-screen");
            SplashSprite = LoadAsset<Sprite>(splashBundle, "splash.png");

            LoadPoses();
            FallbackRelaxedPose = Poses["fallback_relaxed"];
            FallbackPointPose = Poses["fallback_point"];
            FallbackFistPose = Poses["fallback_fist"];
            ReachForPose = Poses["reachFor"];
            GrabbingHandlePose = Poses["grabbing_handle"];
            GrabbingHandleGlovePose = Poses["grabbing_handle_gloves"];
        }

        private void LoadPoses()
        {
            Poses = new Dictionary<string, SteamVR_Skeleton_Pose>();
            var posesPath = NomaiVR.ModFolderPath + "/poses";
            var fileNames = Directory.GetFiles(posesPath);

            foreach(var fileName in fileNames)
            {
                var path = Path.Combine(posesPath, fileName);
                var fileInfo = new FileInfo(path);

                if (fileInfo.Extension == ".json")
                {
                    var loadedPose = LoadModAssetFromJson<SteamVR_Skeleton_Pose>($"poses/{fileInfo.Name}");

                    if(loadedPose != null)
                        Poses.Add(fileInfo.Name.Replace(fileInfo.Extension, ""), loadedPose);
                    else
                        Logs.WriteError($"Failed to load pose {fileName}");
                }
            }
        }

        private T LoadModAssetFromJson<T>(string modAssetPath)
        {
            var fullPath = NomaiVR.ModFolderPath + modAssetPath;
            if (!File.Exists(fullPath))
                return default(T);

            if(typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
            {
                //ScriptableObjects should be instantiated through ScriptableObject.CreateInstance
                object scriptableObject = ScriptableObject.CreateInstance(typeof(T));
                JsonConvert.PopulateObject(File.ReadAllText(fullPath), scriptableObject);
                return (T)scriptableObject;
            }

            return JsonConvert.DeserializeObject<T>(File.ReadAllText(fullPath));
        }

        private T LoadAsset<T>(AssetBundle bundle, string prefabName) where T : UnityEngine.Object
        {
            return bundle.LoadAsset<T>($"assets/{prefabName}");
        }

        private static AssetBundle LoadBundle(string assetName)
        {
            var myLoadedAssetBundle =
                AssetBundle.LoadFromFile(
                    $"{NomaiVR.ModFolderPath}/Assets/{assetName}");
            if (myLoadedAssetBundle == null)
            {
                Logs.WriteError($"Failed to load AssetBundle {assetName}");
                return null;
            }

            return myLoadedAssetBundle;
        }
    }
}