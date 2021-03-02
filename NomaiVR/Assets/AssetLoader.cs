using System.IO;
using UnityEngine;
using Valve.Newtonsoft.Json;
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
        public static SteamVR_Skeleton_Pose GrabbingHandlePose;
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

            //var handsBundle = LoadBundle("hands");
            var skeletalHandsBundle = LoadBundle("skeletal-hands");
            HandPrefab = LoadAsset<GameObject>(skeletalHandsBundle, "Assets/skeletal_hand.prefab");
            //GlovePrefab = LoadAsset<GameObject>(skeletalHandsBundle, "Assets/skeletal_glove.prefab");

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
            GrabbingHandlePose = LoadModAssetFromJson<SteamVR_Skeleton_Pose>("poses/grabbing_handle.json");
        }

        private T LoadModAssetFromJson<T>(string modAssetPath)
        {
            string fullPath = NomaiVR.Helper.Manifest.ModFolderPath + modAssetPath;
            if (!File.Exists(fullPath))
                return default(T);
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(fullPath));
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