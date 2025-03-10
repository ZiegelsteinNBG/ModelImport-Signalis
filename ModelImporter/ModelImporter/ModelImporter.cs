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
using static Il2CppUTJ.FrameCapturer.DataPath;
using static Model_Importer.ModelImporter;
using System.Xml.Serialization;


namespace Model_Importer
{
    public class ModelImporter: MelonMod
    {
        private string dataPath;
        string currentDirectory;
        List<string> assetBundlePaths;
        Il2CppAssetBundle ab;
        static SkinnedMeshRenderer body_skin;
        GameObject player;
        
        Dictionary<string, int> boneDict;
        

        public override void OnUpdate()
        {
            if(player == null && GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default") != null)
            {
                LoadModel();
            }
            if (body_skin != null && GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default") != null)
            {
                updatePose();
            }
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
                    string[] assetNames = ab.AllAssetNames();
                    string objectName =Path.GetFileNameWithoutExtension(assetNames[0]);

                    player = GameObject.Instantiate(ab.LoadAsset<GameObject>(objectName).TryCast<GameObject>());// TODO
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
                    ab.Unload(false);
                    break;
                }
            }

            deactivateDefaultModels(iniModData());

            if (player != null)
            {
                body_skin = player.transform.Find("Body").GetComponent<SkinnedMeshRenderer>();
                if (body_skin == null)
                {
                    MelonLogger.Error("Failed to load player model.");
                    return;
                }
                boneDict = HelperMethodsMI.dictList(body_skin);
                body_skin = HelperMethodsMI.insertAlternative(player, "imp_Mo", "Body", "metarig", boneDict); // TODO
            }
            else
            {
                MelonLogger.Error("Failed to load player model.");
                return;
            }


        }


        public void updatePose()
        {
            SkinnedMeshRenderer defaultSkin = GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default/Normal/Body").GetComponent<SkinnedMeshRenderer>();
            HelperMethodsMI.updatePose(defaultSkin, body_skin, boneDict);
        }

        private string[] modelNames = new string[] { "Normal", "Armored", "Crippled", "EVA", "Isa_Past" };
        public void deactivateDefaultModels(ModData modData)
        {
            string modelEx = "";
            string modelPartEx = "";
            GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/WeaponMount/").SetActive(false);
               
            try
            {
                foreach (string modeld in modelNames)
                {
                    modelEx = modeld;
                    ModelData modelData = modData.FindModelDataByName(modeld);

                    for (int i = 0; i < modelData.modelParts.Length; i++)
                    {
                        HelperMethodsMI.setChildActive("__Prerequisites__/Character Origin/Character Root/Ellie_Default/", modeld, true);

                        string part = modelData.modelParts[i];
                        modelPartEx = part;
                        GameObject gameObject = GameObject.Find($"__Prerequisites__/Character Origin/Character Root/Ellie_Default/{modeld}/{part}");
                        if (modeld == "Normal" && part == "Body")
                        {
                            gameObject.SetActive(true);
                            SkinnedMeshRenderer meshRendererOrigin = gameObject.GetComponent<SkinnedMeshRenderer>();
                            meshRendererOrigin.GetComponent<Renderer>().castShadows = false;
                            Material material = meshRendererOrigin.material;
                            material.color = new Color(0, 0, 0, -1);
                            continue;
                        }
                        gameObject.SetActive(false);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"deactivateDefaultModels failed at EllieDefault: __Prerequisites__/Character Origin/Character Root/Ellie_Default/{modelEx}/{modelPartEx}");
                MelonLogger.Error($"Exception: {ex.Message}");
                MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
            }
        }

        public static ModData iniModData()
        {
            ModData modData = new ModData
            {
                modelData = new List<ModelData>()
            };
            

            // Elster Models (0-4)
            string[] modelParts_ELNormal = { "Body", "Hair", "Tasche", "HairHead" };
            modData.modelData.Add(new ModelData("Normal", modelParts_ELNormal));
            string[] modelParts_ELArmor = { "Body", "Hair", "HairHead", "Armor", "TascheArmor" };
            modData.modelData.Add(new ModelData("Armored", modelParts_ELArmor));

            string[] modelParts_ELEVA = { "Body", "Helmet", "Neck", "Backpack", "TascheArmor", "Visor", "Visor Layer2" };
            modData.modelData.Add(new ModelData("EVA", modelParts_ELEVA));

            string[] modelParts_ELCrippled = { "Body", "Organs", "Hair", "HairHead" };
            modData.modelData.Add(new ModelData("Crippled", modelParts_ELCrippled));

            string[] modelParts_IsaPast = { "Body", "Hair", "HairHead", "Skirt", "Braid" };
            modData.modelData.Add(new ModelData("Isa_Past", modelParts_IsaPast));

            return modData;
        }

        public class ModData
        {
            public List<ModelData> modelData { get; set; }

            public ModelData FindModelDataByName(string modelName)
            {
                return modelData.FirstOrDefault(md => md.modelName == modelName);
            }
        }

        public class ModelData
        {
            public string modelName { get; set; }

            public string[] modelParts { get; set; }

            public ModelData() { }
            public ModelData(string modelName, string[] modelParts)
            {
                this.modelName = modelName;
                this.modelParts = modelParts;
            }

            public override int GetHashCode()
            {
                return modelName.GetHashCode();
            }
        }
        ////////////////////////////////////////////////////////////////////////

        // WIP (may or may not worked on)

        ////////////////////////////////////////////////////////////////////////

        public GameObject uiPanel;
        public Slider heightSlider = new Slider();
        public Slider playerSizeSlider = new Slider();
        public Slider weaponSizeSlider = new Slider();
        public Toggle dynamicHolsterToggle = new Toggle();
        public Toggle[] weaponToggles = { new Toggle(), new Toggle() };

        UnityEngine.Events.UnityAction<float> sa;
        UnityEngine.Events.UnityAction<bool> ba;
        public void onUI()
        {
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
