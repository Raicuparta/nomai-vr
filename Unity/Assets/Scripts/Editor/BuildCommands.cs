using UnityEditor;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;

/// <summary>
/// Set of instructions for automated builds
/// </summary>
public static class BuildCommands
{
    static void PerformBuild ()
    {
        //Export AssetBundles
        BuildPipeline.BuildAssetBundles("Assets/StreamingAssets", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        //Build player
        var buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.locationPathName = "../Unity/Build/OuterWildsVR.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;
        EditorUserBuildSettings.development = false;

        var buildReport = BuildPipeline.BuildPlayer(buildPlayerOptions);
        if (buildReport.summary.result == BuildResult.Succeeded)
            EditorApplication.Exit(0);
        else
            EditorApplication.Exit(1);
    }
}
