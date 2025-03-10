using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using Il2CppSystem.IO;
using UnityEngine.SceneManagement;
using Il2Cpp;
using Il2CppSystem;
using UnityEngine.UI;


namespace Model_Importer
{
    public class ModelImporter: MelonMod
    {
        private string dataPath;
        string currentDirectory;
        List<string> assetBundlePaths;
        Il2CppAssetBundle ab;
        GameObject player;
        Dictionary<string, int> boneDict;
        bool isLoaded = false;
        public GameObject uiPanel;
        public Slider heightSlider = new Slider();
        public Slider playerSizeSlider = new Slider();
        public Slider weaponSizeSlider = new Slider();
        public Toggle dynamicHolsterToggle = new Toggle();
        public Toggle[] weaponToggles = { new Toggle(), new Toggle() };
        UnityEngine.Events.UnityAction<float> sa;
        UnityEngine.Events.UnityAction<bool> ba;
        public override void OnUpdate()
        {
            if(!isLoaded && SceneManager.GetActiveScene().name == "MainMenu")
            {
                LoadModel();
                isLoaded = true;
            }

            //// Now add the listeners correctly
            //heightSlider.onValueChanged.AddListener(sa);
            //playerSizeSlider.onValueChanged.AddListener(sa);
            //weaponSizeSlider.onValueChanged.AddListener(sa);
            //dynamicHolsterToggle.onValueChanged.AddListener(ba);

            //for (int i = 0; i < weaponToggles.Length; i++)
            //{
            //    weaponToggles[i].onValueChanged.AddListener(ba);
            //}
        }

        private void LoadModel()
        {
            currentDirectory = System.Environment.CurrentDirectory;
            dataPath = Path.Combine(currentDirectory, "Mods", "ModelImporter_Data");
            MelonLogger.Msg($"Data Path: {dataPath} {Directory.Exists(dataPath)}");

            foreach (string file in Directory.GetFiles(dataPath))
            {
                string fileNameNoEx = Path.GetFileNameWithoutExtension(file);
                string fileName = Path.GetFileName(file);

                if (fileName == fileNameNoEx)
                {
                    MelonLogger.Msg($"File: {fileName} | Without Extension: {fileNameNoEx} | File: {file}");
                    if (!File.Exists(file))
                    {
                        MelonLogger.Error($"AssetBundle not found: {file}");
                        continue;
                    }
                    try
                    {
                        byte[] fileData = File.ReadAllBytes(file);
                        ab = Il2CppAssetBundleManager.LoadFromFile(file);
                    }
                    catch (System.Exception ex)
                    {
                        MelonLogger.Error($"Exception while loading AssetBundle: {ex.Message}{ex.StackTrace}");
                    }
                    if (ab == null)
                    {
                        MelonLogger.Error($"Failed to load AssetBundle: {file}");
                        continue;
                    }

                    player = GameObject.Instantiate(ab.LoadAsset<GameObject>("Ellie_Default").TryCast<GameObject>());
                    if (player == null)
                    {
                        MelonLogger.Error($"Failed to load asset 'Model' from AssetBundle: {file}");
                        continue;
                    }
                    foreach (Il2CppSystem.Object child in player.transform)
                    {
                        MelonLogger.Msg($"Child: {child.TryCast<Transform>().name}");
                    }
                    player.name = "Model";
                    GameObject.DontDestroyOnLoad(player);
                    ab.Unload(false);
                    break;
                }
            }
        }

        // WIP
        public void ApplyChanges()
        {
            Debug.Log("Applying changes...");
        }


        public void ResetToDefault()
        {
            Debug.Log("Reset to default settings.");
        }

        public void ToggleUI()
        {
            uiPanel.SetActive(!uiPanel.activeSelf);
        }
    }
}
