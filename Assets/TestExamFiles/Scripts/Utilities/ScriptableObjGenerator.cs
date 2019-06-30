using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

public class ScriptableObjGenerator : MonoBehaviour
{
    private static void Process(Object asset)
    {
        AssetDatabase.CreateAsset(asset, "Assets/" + asset.GetType().ToString() + ".asset");
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [MenuItem("Assets/Create/Create ResourceData Object")]
    public static void Create_ResourceData()
    {
        ResourceData asset = ScriptableObject.CreateInstance<ResourceData>();
        Process(asset);
    }

    [MenuItem("Assets/Create/Create BuildingData Object")]
    public static void Create_BuildingData()
    {
        BuildingData asset = ScriptableObject.CreateInstance<BuildingData>();
        Process(asset);
    }
}

#endif