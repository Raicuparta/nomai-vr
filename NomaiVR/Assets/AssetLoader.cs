using UnityEngine;

namespace NomaiVR
{
    public class AssetLoader
    {
        public static GameObject PostCreditsPrefab;
        public static RenderTexture PostCreditsRenderTexture;
        public static GameObject HandPrefab;
        public static GameObject GlovePrefab;
        public static GameObject FeetPositionPrefab;
        public static GameObject ScopeLensPrefab;
        public static GameObject HelmetPrefab;
        public static GameObject LookArrowPrefab;
        public static Sprite SplashSprite;
        public static Texture2D HoldIcon;
        public static Texture2D InteractIcon;
        public static Texture2D JumpIcon;
        public static Texture2D BackIcon;

        public AssetLoader()
        {
            var postCreditsBundle = LoadBundle("cinema-camera");
            PostCreditsPrefab = LoadAsset<GameObject>(postCreditsBundle, "postcreditscamera.prefab");
            PostCreditsRenderTexture = LoadAsset<RenderTexture>(postCreditsBundle, "screen.renderTexture");

            var handsBundle = LoadBundle("hands");
            HandPrefab = LoadAsset<GameObject>(handsBundle, "righthandprefab.prefab");
            GlovePrefab = LoadAsset<GameObject>(handsBundle, "rightgloveprefab.prefab");

            var feetPositionBundle = LoadBundle("feetposition");
            FeetPositionPrefab = LoadAsset<GameObject>(feetPositionBundle, "feetposition.prefab");

            var scopeLensBundle = LoadBundle("scope-lens");
            ScopeLensPrefab = LoadAsset<GameObject>(scopeLensBundle, "scopelens.prefab");

            var helmetBundle = LoadBundle("helmet");
            HelmetPrefab = LoadAsset<GameObject>(helmetBundle, "helmet.prefab");

            var inputIconsBundle = LoadBundle("input-icons");
            HoldIcon = LoadAsset<Texture2D>(inputIconsBundle, "hold.png");
            InteractIcon = LoadAsset<Texture2D>(inputIconsBundle, "interact.png");
            JumpIcon = LoadAsset<Texture2D>(inputIconsBundle, "jump.png");
            BackIcon = LoadAsset<Texture2D>(inputIconsBundle, "back.png");

            var lookArrowBundle = LoadBundle("look-arrow");
            LookArrowPrefab = LoadAsset<GameObject>(lookArrowBundle, "lookarrow.prefab");

            var splashBundle = LoadBundle("splash-screen");
            SplashSprite = LoadAsset<Sprite>(splashBundle, "splash.png");
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