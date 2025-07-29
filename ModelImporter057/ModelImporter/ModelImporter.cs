using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using MeshCombineStudio;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using static UnityEngine.GUI;


namespace Model_Importer
{
    public class ModelImporter: MelonMod
    {
        private string dataPath;
        string currentDirectory;
        List<string> assetBundlePaths;
        Il2CppAssetBundle ab;
        static SkinnedMeshRenderer body_skin;
        GameObject placeholder;
        GameObject player;
        

        
        Dictionary<string, int> boneDict;
        string modelName = "";
        Dictionary<string, bool> bodyPartS;
        Dictionary<string, Transform> bodyPart;

        // Cutscenehandler
        List<(GameObject, SkinnedMeshRenderer)> cutScenemodels;
        // CutSceneObject; CutSceneModel, boneDict
        List<(GameObject, SkinnedMeshRenderer, SkinnedMeshRenderer)> cutScenemodelsIdx;
        Dictionary<int, Dictionary<string, Transform>> cutSceneTransformDict;

        // GUI
        private bool showGUI = false;
        bool start = false, passBodyPartS = false, newNames = true;
        private enum GUIMode { Main, Settings }
        private GUIMode currentMode = GUIMode.Main;
        ModelImporterGUI modelImporterGUI;

        void startGui()
        {
            ClassInjector.RegisterTypeInIl2Cpp<ModelImporterGUI>();

            GameObject guiObject = new GameObject("ModelImporterGUI");
            UnityEngine.Object.DontDestroyOnLoad(guiObject);
            guiObject.AddComponent<ModelImporterGUI>();
            guiObject.GetComponent<ModelImporterGUI>().enabled = true;
            modelImporterGUI = guiObject.GetComponent<ModelImporterGUI>();
        }

        public override void OnUpdate()
        {
            try
            {
                if (player == null && GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default") != null)
                {
                    LoadModel();
                    if (player != null)
                    {
                        insertModelCutScenes();
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"loadingModel failed at {ex.Data} {ex.Source}");
                MelonLogger.Error($"Exception: {ex.Message}");
                MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
            }
            try
            {
                if (body_skin != null && GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default") != null)
                {
                    if (player != null) updatePose();

                    foreach ((GameObject, SkinnedMeshRenderer, SkinnedMeshRenderer) model in cutScenemodelsIdx)
                    {

                        GameObject parent = model.Item3.gameObject.transform.parent.gameObject;
                        if (model.Item1.activeInHierarchy)
                        {
                            HelperMethodsMI.updatePose(model.Item2, model.Item3, boneDict);
                            parent.SetActive(true);
                            if(start)updateActive(cutSceneTransformDict[parent.GetHashCode()]);
                        }
                        else
                        {
                            parent.SetActive(false);
                        }
                    }

                    Transform edgecase = GameObject.Find("Cutscenes/Hatch Failure - Beyond/LAB_Hatch_1 Hatch Grab/Hatch_1/CharSpace/Ellie_Default")?.transform;
                    if(edgecase != null )
                    {
                        edgecase.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                        edgecase.localPosition = new Vector3(1.3796f, 2.2134f, - 2.3104f);
                    }
                    if (start)
                    {
                        if (modelName != "" && !passBodyPartS)
                        {
                            modelImporterGUI.bodyParts = bodyPartS;
                            passBodyPartS = true;
                        }
                        
                        updateActive(bodyPart);
                    }
                }

                if (Input.GetKeyDown(KeyCode.F7))
                {
                    if (!start)
                    {
                        startGui();
                        start = true;
                    }

                    ModelImporterGUI.Instance?.ToggleGUI();

                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"inserting failed at {ex.Data} {ex.Source}");
                MelonLogger.Error($"Exception: {ex.Message}");
                MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
            }
        }

        void LoadModel()
        {
            // TODO Support multiple AB
            if(ab != null) ab.Unload(true);
            currentDirectory = Directory.GetCurrentDirectory();
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
                    MelonLogger.Msg($"Found model: {objectName}");
                    // TODO handle this better you idiot
                    player = GameObject.Instantiate(ab.LoadAsset<GameObject>(objectName).TryCast<GameObject>());
                    if (player == null)
                    {
                        MelonLogger.Error($"Failed to load asset 'Model' from AssetBundle: {file}");
                        continue;
                    }
                    
                    foreach (Il2CppSystem.Object child in player.transform)
                    {
                        MelonLogger.Msg($"Child: {child.TryCast<Transform>().name}");
                    }
                    player.name = fileName;
                    
                    break;
                }
            }

            if (player != null)
            {
                GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/WeaponMount/")?.SetActive(false);
                deactivateDefaultModels(GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default/"));

                body_skin = player.transform.Find("Body").GetComponent<SkinnedMeshRenderer>();
                if (body_skin == null)
                {
                    ab.Unload(true);
                    MelonLogger.Error("Failed to load player model.");
                    return;
                }
                if(body_skin.rootBone.name != "hips") body_skin.rootBone = player.transform.Find("metarig/Root/hips");
                boneDict = HelperMethodsMI.dictList(body_skin);
                placeholder = GameObject.Instantiate(player);
                placeholder.SetActive(false);
                body_skin = HelperMethodsMI.insertAlternative(player, "imp_Mo", "Body", "metarig", boneDict);
                player.transform.localPosition = Vector3.zero;

                
                
                
                if (modelName != player.name)
                {
                    bodyPartS = new Dictionary<string, bool>();
                    passBodyPartS = false;
                }
                bodyPart = new Dictionary<string, Transform>();
                for (int i = 0; i < player.transform.childCount; i++)
                {
                    if(player.transform.GetChild(i).name == "metarig") continue;
                    if (modelName != player.name) bodyPartS.Add(player.transform.GetChild(i).name, true);
                    bodyPart.Add(player.transform.GetChild(i).name, player.transform.GetChild(i));
                }
                modelName = player.name;

            }
            else
            {
                MelonLogger.Error("Failed to load player model.");
                ab.Unload(true);
                return;
            }
        }

        void updatePose()
        {
            SkinnedMeshRenderer defaultSkin = GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default/Normal/Body").GetComponent<SkinnedMeshRenderer>();
            if (!start) {
                HelperMethodsMI.updatePose(defaultSkin, body_skin, boneDict);
            }
            else
            {
                HelperMethodsMI.updatePose(defaultSkin, body_skin, boneDict, modelImporterGUI.sliderValueY, modelImporterGUI.sliderValueZ);
            }
            
        }

        void updateActive(Dictionary<string, Transform> bodyPartTransform)
        {
            if (passBodyPartS)
            {
                MelonLogger.Msg($"Updating body parts for {modelName} {modelImporterGUI.bodyParts}");
                foreach (KeyValuePair<string, bool> kvp in modelImporterGUI.bodyParts)
                {
                    if (bodyPartTransform.ContainsKey(kvp.Key))
                    {
                        MelonLogger.Msg($"Setting {kvp.Key} to {kvp.Value}");
                        bodyPartTransform[kvp.Key].gameObject.SetActive(kvp.Value);
                    }
                }
            }
        }

        void insertModelCutScenes()
        {
            cutScenemodels = new List<(GameObject, SkinnedMeshRenderer)>();
            cutScenemodelsIdx = new List<(GameObject, SkinnedMeshRenderer, SkinnedMeshRenderer)>();
            deactivateDefaultModels(GameObject.Find("Cutscenes"), ref cutScenemodels, false);
            if (SceneManager.GetActiveScene().name == "PEN_Wreck")
            {
                deactivateDefaultModels(GameObject.Find("Maintenance (Start)/Chunk/Ellie_Cutscene_Wakeup_Pivot/Ellie_Cutscene_Wakeup/"), ref cutScenemodels, false);
            }else if( SceneManager.GetActiveScene().name == "LAB_Emptiness" )
            {
                deactivateDefaultModels(GameObject.Find("Cutscenes/The Crash Site - Infinity/LAB_Death_5 Elster Transparent/Wide_1/GameObject/Pivot/"), ref cutScenemodels, true);
                deactivateDefaultModels(GameObject.Find("Cutscenes/Hatch Failure - Beyond/LAB_Hatch_2 Silhouette Pull/Wide_1/Elster/"), ref cutScenemodels, true);
            }
            int idx = 0;
            cutSceneTransformDict = new Dictionary<int, Dictionary<string, Transform>>();
            foreach ((GameObject, SkinnedMeshRenderer) model in cutScenemodels)
            {
                GameObject cutSceneModel = GameObject.Instantiate(placeholder);
                SkinnedMeshRenderer[] skMats = cutSceneModel.GetComponentsInChildren<SkinnedMeshRenderer>();
                foreach (SkinnedMeshRenderer skMat in skMats)
                {
                    Material mat2 = skMat.material;
                    if (model.Item2.material.name.Contains("monsterFX (Instance)"))
                    {
                        skMat.material = model.Item2.material;
                        continue;
                    }
                    mat2.shader = Shader.Find("Toon/Cutoff");
                }
                cutSceneModel.name = $"cutsceneModel{idx++}";
                HelperMethodsMI.insertAlternative(cutSceneModel, model.Item1, "Body", "metarig", boneDict);
                cutSceneModel.SetActive(false);
                cutScenemodelsIdx.Add((model.Item1, model.Item2, cutSceneModel.transform.Find("Body").GetComponent<SkinnedMeshRenderer>()));
                cutSceneModel.transform.localPosition = Vector3.zero;

                MelonLogger.Msg($"Cutscenemode: {cutSceneModel.name} {cutSceneModel.GetHashCode()}");
                Dictionary<string, Transform> modelPartDict = new Dictionary<string, Transform>();
                for (int i = 0; i < cutSceneModel.transform.childCount; i++)
                {
                    if (cutSceneModel.transform.GetChild(i).name == "metarig") continue;
                    modelPartDict.Add(cutSceneModel.transform.GetChild(i).name, cutSceneModel.transform.GetChild(i));
                }
                cutSceneTransformDict.Add(cutSceneModel.GetHashCode(), modelPartDict);
            }

        }

        // Make all DefaultModels invisble
        void deactivateDefaultModels(GameObject parent, ref List<(GameObject, SkinnedMeshRenderer)> models, bool ign)
        {
            try
            {
                foreach (SkinnedMeshRenderer sk in parent?.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    // Assuming all LSTR models use a material with the name "elster" inside
                    if (sk.material.name.Contains("elster") || ign)
                    {
                        GameObject model = sk.gameObject;
                        // Again assuming all LSTR models use a armature with the name "metarig" 
                        if (!models.Contains((model, sk)) && model.name.ToLower().Contains("body"))
                        {
                            models.Add((model, sk));
                            SkinnedMeshRenderer[] rest = model.transform.parent.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
                            foreach (SkinnedMeshRenderer sk2 in rest)
                            {
                                sk2.GetComponent<Renderer>().castShadows = false;
                                Material material = sk2.material;
                                material.color = new Color(0, 0, 0, -1);
                            }
                        }
                    }
                    
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"deactivateModels failed at {parent.name}");
                MelonLogger.Error($"Exception: {ex.Message}");
                MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
            }
        }

        public void deactivateDefaultModels(GameObject parent)
        {
            try
            {
                foreach (SkinnedMeshRenderer sk in parent?.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    // Assuming all LSTR models use a material with the name "elster" inside
                    if (sk.material.name.Contains("elster"))
                    {
                        sk.GetComponent<Renderer>().castShadows = false;
                        Material material = sk.material;
                        material.color = new Color(0, 0, 0, -1);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"deactivateModels failed at {parent.name}");
                MelonLogger.Error($"Exception: {ex.Message}");
                MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
            }
        }
    }
    [RegisterTypeInIl2Cpp]
    public class ModelImporterGUI : MonoBehaviour
    {
        public static ModelImporterGUI Instance;
        public bool showGUI = false;

        public enum GUIMode { Main, Settings }
        public GUIMode currentMode = GUIMode.Main;

        public float sliderValueY = 1.0f;
        public float sliderValueZ = 1.0f;
        public Dictionary<string, bool> bodyParts;
        private Vector2 scrollViewVector = Vector2.zero;

        private Il2CppReferenceArray<GUILayoutOption> option =
            (Il2CppReferenceArray<GUILayoutOption>)new GUILayoutOption[] { GUILayout.Width(200f) };
        private Rect windowRect = new Rect(0, 0, 500, 600);

        public ModelImporterGUI(IntPtr ptr) : base(ptr) { }
        public void ToggleGUI()
        {
            showGUI = !showGUI;
            if(showGUI) currentMode = GUIMode.Main;
            MelonLogger.Msg($"GUI is now {(showGUI ? "visible" : "hidden")}");
        }

        void Awake()
        {
            Instance = this;
        }

        void OnGUI()
        {
            //GUI.Label(new Rect(10, 10, 200, 20), "DEBUG: GUI is drawing!");
            if (textAreaStringZ == "null") textAreaStringZ = $"{sliderValueZ}";
            if (textAreaStringY == "null") textAreaStringY = $"{sliderValueY}";
            windowRect = GUILayout.Window(0, windowRect, (WindowFunction)WindowFunction, "Model Importer", option);
        }

        void WindowFunction(int windowID)
        {
            if (!showGUI) return;

            //GUILayout.BeginArea(new Rect(0, 0, 900, 600));
            GUILayout.BeginHorizontal(option);
            if (GUILayout.Button("Arm adjustment", option)) currentMode = GUIMode.Main;
            if (GUILayout.Button("Body parts", option)) currentMode = GUIMode.Settings;
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            switch (currentMode)
            {
                case GUIMode.Main:
                    GUILayout.BeginHorizontal(option);
                    GUILayout.Label("Adjust the relativ arm distance to fix overlapping arms when aiming\nY is horizontal, Z vertical", option);
                    GUILayout.Label("----------------------------------------------", option);

                    GUILayout.Space(10);
                    GUILayout.EndHorizontal();
                    DrawMainTab();
                    break;
                case GUIMode.Settings:
                    if (bodyParts == null || bodyParts.Count == 0)
                    {
                        GUILayout.Label("No body parts found. Please load a model first.", option);
                        return;
                    }
                    DrawSettingsTab();
                    break;
            }

            //GUILayout.EndArea();
        }
        string textAreaStringZ = "null";
        string textAreaStringY = "null";
        void DrawMainTab()
        {
            GUILayout.BeginHorizontal(option);
            if (GUILayout.Button("Reset left arm y-axis", option))
            {
                sliderValueY = 1.0f;
                textAreaStringY = $"{sliderValueY}";
            }

            GUILayout.BeginVertical(option);

            GUILayout.Label("Left arm y-axis distance", option);
            textAreaStringY = GUILayout.TextArea(textAreaStringY, option);
            if (float.TryParse(textAreaStringY, out float result))
            {
                sliderValueY = result;
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(option);

            if (GUILayout.Button("Reset left arm z-axis", option))
            {
                sliderValueZ = 1.0f;
                textAreaStringZ = $"{sliderValueZ}";
            }

            GUILayout.BeginVertical(option);
            GUILayout.Label("Left arm z-axis distance", option);
            textAreaStringZ = GUILayout.TextArea(textAreaStringZ, option);
            if (float.TryParse(textAreaStringZ, out float result1))
            {
                sliderValueZ = result1;
            }
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        void DrawSettingsTab()
        {
            scrollViewVector = GUILayout.BeginScrollView(scrollViewVector, option);
            for (int i = 0; i < bodyParts.Count; i++)
            {
                string key = bodyParts.Keys.ElementAt(i);
                GUILayout.Label(key, option);
                if (GUILayout.Button(bodyParts[key] ? $"Hide {bodyParts[key]}" : $"Show {bodyParts[key]}", option))
                {
                    bodyParts[key] = !bodyParts[key];
                }
            }
            GUILayout.EndScrollView();
        }
    }
    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    // OUTDATED
    // ModData: deactivate the default models
    //private string[] modelNames = new string[] { "Normal", "Armored", "Crippled", "EVA", "Isa_Past" };
    //public void deactivateDefaultModels(ModData modData)
    //{
    //    string modelEx = "";
    //    string modelPartEx = "";
    //    GameObject.Find("__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/WeaponMount/")?.SetActive(false);

    //    try
    //    {
    //        foreach (string modeld in modelNames)
    //        {
    //            modelEx = modeld;
    //            ModelData modelData = modData.FindModelDataByName(modeld);

    //            for (int i = 0; i < modelData.modelParts.Length; i++)
    //            {
    //                HelperMethodsMI.setChildActive("__Prerequisites__/Character Origin/Character Root/Ellie_Default/", modeld, true);

    //                string part = modelData.modelParts[i];
    //                modelPartEx = part;
    //                GameObject gameObject = GameObject.Find($"__Prerequisites__/Character Origin/Character Root/Ellie_Default/{modeld}/{part}");
    //                if (modeld == "Normal" && part == "Body")
    //                {
    //                    gameObject.SetActive(true);
    //                    SkinnedMeshRenderer meshRendererOrigin = gameObject.GetComponent<SkinnedMeshRenderer>();
    //                    meshRendererOrigin.GetComponent<Renderer>().castShadows = false;
    //                    Material material = meshRendererOrigin.material;
    //                    material.color = new Color(0, 0, 0, -1);
    //                    continue;
    //                }
    //                gameObject.SetActive(false);
    //            }
    //        }
    //    }
    //    catch (System.Exception ex)
    //    {
    //        MelonLogger.Error($"deactivateDefaultModels failed at EllieDefault: __Prerequisites__/Character Origin/Character Root/Ellie_Default/{modelEx}/{modelPartEx}");
    //        MelonLogger.Error($"Exception: {ex.Message}");
    //        MelonLogger.Error($"Stack Trace: {ex.StackTrace}");
    //    }
    //}

    //public static ModData iniModData()
    //{
    //    ModData modData = new ModData
    //    {
    //        modelData = new List<ModelData>()
    //    };


    //    // Elster Models (0-4)
    //    string[] modelParts_ELNormal = { "Body", "Hair", "Tasche", "HairHead" };
    //    modData.modelData.Add(new ModelData("Normal", modelParts_ELNormal));
    //    string[] modelParts_ELArmor = { "Body", "Hair", "HairHead", "Armor", "TascheArmor" };
    //    modData.modelData.Add(new ModelData("Armored", modelParts_ELArmor));

    //    string[] modelParts_ELEVA = { "Body", "Helmet", "Neck", "Backpack", "TascheArmor", "Visor", "Visor Layer2" };
    //    modData.modelData.Add(new ModelData("EVA", modelParts_ELEVA));

    //    string[] modelParts_ELCrippled = { "Body", "Organs", "Hair", "HairHead" };
    //    modData.modelData.Add(new ModelData("Crippled", modelParts_ELCrippled));

    //    string[] modelParts_IsaPast = { "Body", "Hair", "HairHead", "Skirt", "Braid" };
    //    modData.modelData.Add(new ModelData("Isa_Past", modelParts_IsaPast));

    //    return modData;
    //}

    //public class ModData
    //{
    //    public List<ModelData> modelData { get; set; }

    //    public ModelData FindModelDataByName(string modelName)
    //    {
    //        return modelData.FirstOrDefault(md => md.modelName == modelName);
    //    }
    //}

    //public class ModelData
    //{
    //    public string modelName { get; set; }

    //    public string[] modelParts { get; set; }

    //    public ModelData() { }
    //    public ModelData(string modelName, string[] modelParts)
    //    {
    //        this.modelName = modelName;
    //        this.modelParts = modelParts;
    //    }

    //    public override int GetHashCode()
    //    {
    //        return modelName.GetHashCode();
    //    }
    //}
}
