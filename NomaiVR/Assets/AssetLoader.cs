using System.IO;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    public class AssetLoader
    {
        public static GameObject PostCreditsPrefab;
        public static RenderTexture PostCreditsRenderTexture;
        public static GameObject HandPrefab;
        public static SteamVR_Skeleton_Pose FallbackRelaxedPose;
        public static SteamVR_Skeleton_Pose FallbackPointPose;
        public static SteamVR_Skeleton_Pose FallbackFistPose;
        public static SteamVR_Skeleton_Pose ReachForPose;
        public static GameObject FeetPositionPrefab;
        public static GameObject ScopeLensPrefab;
        public static GameObject HelmetPrefab;
        public static GameObject LookArrowPrefab;
        public static Sprite SplashSprite;
        public static Texture2D EmptyTexture;

        public AssetLoader()
        {
            EmptyTexture = new Texture2D(1, 1);
            EmptyTexture.SetPixel(0, 0, Color.clear);
            EmptyTexture.Apply();

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

            var splashBundle = LoadBundle("splash-screen");
            SplashSprite = LoadAsset<Sprite>(splashBundle, "splash.png");


            FallbackRelaxedPose = LoadModAssetFromJson<SteamVR_Skeleton_Pose>("poses/fallback_relaxed.json");
            FallbackPointPose = LoadModAssetFromJson<SteamVR_Skeleton_Pose>("poses/fallback_point.json");
            FallbackFistPose = LoadModAssetFromJson<SteamVR_Skeleton_Pose>("poses/fallback_fist.json");
            ReachForPose = LoadModAssetFromJson<SteamVR_Skeleton_Pose>("poses/reachFor.json");
        }

        private T LoadModAssetFromJson<T>(string modAssetPath)
        {
            string fullPath = NomaiVR.Helper.Manifest.ModFolderPath + modAssetPath;
            if (!File.Exists(fullPath))
                return default(T);

            if(typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
            {
                //ScriptableObjects should be instantiated through ScriptableObject.CreateInstance
                object scriptableObject = ScriptableObject.CreateInstance(typeof(T));
                JsonUtility.FromJsonOverwrite(File.ReadAllText(fullPath), scriptableObject);
                return (T)scriptableObject;
            }

            return JsonUtility.FromJson<T>(File.ReadAllText(fullPath));
        }

        private T LoadAsset<T>(AssetBundle bundle, string prefabName) where T : UnityEngine.Object
        {
            return bundle.LoadAsset<T>($"assets/{prefabName}");
        }

        private AssetBundle LoadBundle(string fileName)
        {
            return NomaiVR.Helper.Assets.LoadBundle($"assets/{fileName}");
        }
    }
}