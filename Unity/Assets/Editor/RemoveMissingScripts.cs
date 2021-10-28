using UnityEditor;
using UnityEngine;

public class RemoveMissingScripts : Editor
{
    [MenuItem("Tools/Remove missing scripts from selected object")]
    private static void RecursiveCleanupMissingScripts()
    {
        var allTransforms = Selection.gameObjects[0].GetComponentsInChildren<Transform>(true);
        foreach (var transform in allTransforms)
        {
            var gameObject = transform.gameObject;

            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(gameObject);
        }
    }
}