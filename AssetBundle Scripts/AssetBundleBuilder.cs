#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAssetBundles
{
    [MenuItem ("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles ()
    {
        // Define the output directory
        string folderPath =  System.Environment.CurrentDirectory+"\\AssetBundleExport";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Refresh the AssetDatabase to ensure Unity recognizes asset bundle assignments
        AssetDatabase.Refresh();

        // Check if any assets are assigned to AssetBundles
        var assetBundleNames = AssetDatabase.GetAllAssetBundleNames();
        if (assetBundleNames.Length == 0)
        {
            Debug.LogError(" No AssetBundle has been set for this build. Assign assets to a bundle first!");
            return;
        }

        // Build the Asset Bundles
        BuildPipeline.BuildAssetBundles (folderPath, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);

        Debug.Log($" Asset Bundles built successfully! {folderPath}");
    }
}
#endif
