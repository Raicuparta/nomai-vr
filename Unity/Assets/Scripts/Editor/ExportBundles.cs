using UnityEditor;

public class ExportBundles
{
    [MenuItem ("Tools/Build AssetBundles")]
    static void BuildAllAssetBundles ()
    {
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}