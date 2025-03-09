#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        // Open file explorer to select a folder
        string folderPath = EditorUtility.SaveFolderPanel("Select AssetBundle Output Folder", "", "");

        // Check if the user canceled the selection
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogWarning("No folder selected. AssetBundle build canceled.");
            return;
        }

        // Refresh the AssetDatabase to ensure Unity recognizes asset bundle assignments
        AssetDatabase.Refresh();

        // Check if any assets are assigned to AssetBundles
        var assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        if (assetBundleNames.Length == 0)
        {
            Debug.LogError("No AssetBundle has been set for this build. Assign assets to a bundle first!");
            return;
        }

        foreach (var file in Directory.GetFiles(folderPath))
        {
            File.Delete(file);
        }
        // Build the Asset Bundles in the selected directory
        BuildPipeline.BuildAssetBundles(folderPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        Debug.Log($"Asset Bundles built successfully! Saved to: {folderPath}");
    }
}
#endif
