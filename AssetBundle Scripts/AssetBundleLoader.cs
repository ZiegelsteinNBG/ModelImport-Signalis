using UnityEditor;
using UnityEngine;
using System.IO;

public class AssetBundleLoader : EditorWindow
{
    [MenuItem("Assets/Load AssetBundle")]
    static void LoadAssetBundle()
    {
        string path = "C:\\Users\\..."; //Please insert here your AssetBundle file Path
        if (!File.Exists(path))
        {
            Debug.LogError("AssetBundle not found!");
            return;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            Debug.LogError("Failed to load AssetBundle!");
            return;
        }

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
    }
}