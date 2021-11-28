using System.IO;
using UnityEditor;
using UnityEngine;

public class ExportToJSONEditor : Editor
{
    [MenuItem("Tools/To JSON", false, 30)]
    public static void ExportToJSON()
    {
        if(Selection.activeObject == null)
        {
            EditorUtility.DisplayDialog("No Object Selected", "Please select any GameObject to Export to FBX", "Okay");
            return;
        }
        
        ScriptableObject activeObject = Selection.activeObject as ScriptableObject;
        
        if(activeObject == null)
        {
            EditorUtility.DisplayDialog("Warning", "Item selected is not a GameObject", "Okay");
            return;
        }

        var filename = EditorUtility.SaveFilePanel( "Save Json File", Application.dataPath,
            activeObject.name + ".json", "json" );
        if ( !string.IsNullOrEmpty( filename ) )
        {
            File.WriteAllText(filename, JsonUtility.ToJson(activeObject));
        }
    }
}