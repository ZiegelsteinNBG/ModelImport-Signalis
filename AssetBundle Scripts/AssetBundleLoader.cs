using UnityEditor;
using UnityEngine;
using System.IO;

public class AssetBundleLoader : EditorWindow
{
    [MenuItem("Assets/Load AssetBundle")]
    static void LoadAssetBundle()
    {
        // Open file explorer to select an AssetBundle file
        string path = EditorUtility.OpenFilePanel("Select AssetBundle", "", ""); // Filters for any file type

        if (string.IsNullOrEmpty(path)) // Check if user canceled the selection
        {
            Debug.LogWarning("No file selected.");
            return;
        }

        if (!File.Exists(path))
        {
            Debug.LogError("AssetBundle not found!");
            return;
        }

        // Load the AssetBundle
        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            return;
        }

        // Load all assets from the bundle
        string[] assetNames = bundle.GetAllAssetNames();
        foreach (string assetName in assetNames)
        {
            GameObject obj = bundle.LoadAsset<GameObject>(assetName);
            if (obj != null)
            {
                GameObject instance = Instantiate(obj);
                instance.name = obj.name;
                Debug.Log($"Loaded: {obj.name}");
            }
        }

        // Unload bundle after use (optional)
        bundle.Unload(false);
    }
}