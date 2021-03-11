using UnityEditor;

public class ExportBundles
{
    [MenuItem ("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles ()
    {
        BuildPipeline.BuildAssetBundles ("Assets/assets", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}