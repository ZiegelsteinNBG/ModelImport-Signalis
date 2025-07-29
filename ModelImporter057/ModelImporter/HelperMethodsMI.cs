
using MelonLoader;
using System;
using System.Collections.Generic;
using UnityEngine;
using static RootMotion.FinalIK.IKSolverVR;

namespace Model_Importer
{
    public class HelperMethodsMI
    {

        public static GameObject copyObjectDDOL(String originPath, String newName, bool dontDestroy)
        {
            GameObject originObject = GameObject.Find(originPath);
            if (originObject == null)
            {
                MelonLogger.Error($"Method copyObjectDDOL failed at originPath: {originPath}");
                return null;
            }
            GameObject copy = GameObject.Instantiate(originObject);

            copy.name = newName;
            if (dontDestroy) GameObject.DontDestroyOnLoad(copy);
            return copy;
        }

        public static bool copyModelSMR(GameObject originObject, GameObject destinationObject)
        {
            if (originObject == null)
            {
                MelonLogger.Error($"copyModel failed with OriginObject: {originObject}");
                return false;
            }
            if (destinationObject == null) return false;

            SkinnedMeshRenderer meshRendererOrigin = originObject.GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer meshRendererDestination = destinationObject.GetComponent<SkinnedMeshRenderer>();

            if (meshRendererOrigin == null)
            {
                MelonLogger.Error($"copyModel failed Mesh: {originObject}");
                return false;
            }
            if (meshRendererDestination == null)
            {
                MelonLogger.Error($"copyModel failed Mesh: {destinationObject.name}");
                return false;
            }
            meshRendererDestination.materials = meshRendererOrigin.materials;
            meshRendererDestination.sharedMesh = meshRendererOrigin.sharedMesh;
            return true;
        }
        public static void setParent(String parentPath, String childPath)
        {
            GameObject parentObject = GameObject.Find(parentPath);
            if (parentObject != null)
            {
                GameObject childObject = GameObject.Find(childPath);
                if (childObject != null)
                {
                    childObject.transform.SetParent(parentObject.transform);
                }
                else
                {
                    MelonLogger.Error($"setParent: Child GameObject '{childPath}' not found.");
                }
            }
            else
            {
                MelonLogger.Error($"setParent: Parent GameObject '{parentPath}' not found.");
            }
        }

        public static void setParent(String parentPath, GameObject child)
        {
            GameObject parentObject = GameObject.Find(parentPath);
            if (parentObject != null)
            {
                if (child != null)
                {
                    child.transform.SetParent(parentObject.transform);
                }
                else
                {
                    MelonLogger.Error($"setParent: Child GameObject '{child}' not found.");
                }
            }
            else
            {
                MelonLogger.Error($"setParent: Parent GameObject '{parentPath}' not found.");
            }
        }

        public static void setParent(GameObject parentObject, GameObject child)
        {
            if (parentObject != null)
            {
                if (child != null)
                {
                    child.transform.SetParent(parentObject.transform);
                }
                else
                {
                    MelonLogger.Error($"setParent: Child GameObject '{child}' not found.");
                }
            }
            else
            {
                MelonLogger.Error($"setParent: Parent GameObject '{parentObject}' not found.");
            }
        }

        public static bool setChildActive(String parentPath, String childActivate, bool active)
        {
            // Find Parent
            GameObject parentObject = GameObject.Find(parentPath);
            if (parentObject != null)
            {
                // Find Child
                Transform childTransform = parentObject.transform.Find(childActivate);
                if (childTransform != null)
                {
                    // Set ChildObject Active
                    GameObject child = childTransform.gameObject;
                    child.SetActive(active);
                    return true;
                }
                else
                {
                    MelonLogger.Error($"setChildActive: Child GameObject '{childActivate}' not found within '{parentObject.name}'.");
                    return false;
                }
            }
            else
            {
                MelonLogger.Error($"setChildActive: Parent GameObject '{parentPath}' not found.");
                return false;
            }
        }

        public static bool setChildActive(GameObject parent, String childActivate)
        {

            if (parent != null)
            {
                // Find Child
                Transform childTransform = parent.transform.Find(childActivate);
                if (childTransform != null)
                {
                    // Set ChildObject Active
                    GameObject child = childTransform.gameObject;
                    child.SetActive(true);
                    return true;
                }
                else
                {
                    MelonLogger.Error($"setChildActive: Child GameObject '{childActivate}' not found within '{parent.name}'.");
                    return false;
                }
            }
            else
            {
                MelonLogger.Error($"setChildActive: Parent GameObject '{parent}' not found.");
                return false;
            }
        }
        public static GameObject copyComponent(GameObject dest, String comp_or, String name, bool setNull)
        {
            GameObject comp_copy = copyObjectDDOL(comp_or, name, false);
            setParent(dest, comp_copy);
            if (setNull)
            {
                comp_copy.transform.localPosition = Vector3.zero;
                comp_copy.transform.localRotation = new Quaternion(0, 0, 0, 0);
            }
            else
            {
                comp_copy.transform.localPosition = GameObject.Find(comp_or).transform.localPosition;
                comp_copy.transform.localRotation = GameObject.Find(comp_or).transform.localRotation;
            }
            return comp_copy;
        }

        public static Dictionary<String, int> dictList(SkinnedMeshRenderer skin)
        {
            if (skin == null)
            {
                MelonLogger.Error("boneList: Invalid param->null");
                return null;
            }
            Dictionary<String, int> keyValuePairs = new Dictionary<String, int>();
            int idx = 0;
            foreach (Transform bone in skin.bones)
            {

                if (bone != null)
                {
                    keyValuePairs.Add(bone.name, idx++);
                    MelonLogger.Msg($"Bone:{bone.name}");
                }
            }
            return keyValuePairs;
        }

        public static void updatePose(SkinnedMeshRenderer origin, SkinnedMeshRenderer dest, Dictionary<String, int> dict, float armY = 1.0f, float armZ = 1.0f)
        {
            try
            {
                foreach (Transform bone in origin.bones)
                {
                    if (!dict.ContainsKey(bone.name)) continue;
                    Transform dest_Transform = dest.bones[dict[bone.name]];

                    // Placeholder
                    if ((bone.name == "shoulder_L") && PlayerState.aiming) 
                    {
                        Vector3 eul = bone.localEulerAngles;
                        eul.y = eul.y * armY;
                        eul.z = eul.z * armZ;
                        dest_Transform.localEulerAngles = eul;
                        continue;
                    }

                    dest_Transform.localRotation = bone.localRotation;
                    if (bone.name == "hips") dest_Transform.localPosition = bone.localPosition;
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error($"updatePost: failed updating Bones \nException: {e.Message}");
            }
        }

        public static SkinnedMeshRenderer insertAlternative(GameObject model_or, String name, String body, String normal, Dictionary<String, int> dict)
        {
            try
            {
                setParent("__Prerequisites__/Character Origin/Character Root/Ellie_Default", model_or);
                MelonLogger.Msg($"Set Parent for Model: {model_or.name}");
                GameObject root = GameObject.Find($"__Prerequisites__/Character Origin/Character Root/Ellie_Default/{model_or.name}/{normal}/Root/hips");
                if(root == null)
                {
                    MelonLogger.Error($"Hips not found for {model_or.name}");
                    return null;
                }
                //root.name = $"Root_{name}";
                setParent("__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root", root);
                root.transform.localPosition = Vector3.zero;
                root.transform.localRotation = new Quaternion(0, 0, 0.7071f, 0.7071f);

                SkinnedMeshRenderer skin = model_or.transform.Find(body).GetComponent<SkinnedMeshRenderer>();
                skin.bones[dict["hips"]].localPosition = Vector3.zero;

                GameObject weaponMod = copyComponent(skin.bones[dict["hand_R"]].gameObject, "__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/WeaponMount/", "WeaponMount", true);
                copyComponent(skin.bones[dict["hips"]].gameObject, "__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/VisibleEquip/", "VisibleEquip", true);
                copyComponent(skin.bones[dict["chest"]].gameObject, "__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/spine/chest/Nitro Model/", "Nitro Model", false);
                copyComponent(skin.bones[dict["chest"]].gameObject, "__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/spine/chest/FlashLightFlare/", "FlashLightFlare", true);
                weaponMod.SetActive(true);
                return skin;
                }
            catch (Exception ex)
            {
                MelonLogger.Error($"Exception: {ex.GetType().Name}");
                MelonLogger.Error($"Message: {ex.Message}");
                MelonLogger.Error($"StackTrace: {ex.StackTrace}");
            }
            return null;
        }

        public static SkinnedMeshRenderer insertAlternative(GameObject model_or, GameObject model_dest, String body, String normal, Dictionary<String, int> dict)
        {

            setParent(model_dest.transform.parent.gameObject, model_or);
            GameObject hips = model_or.transform.Find("Body").GetComponent<SkinnedMeshRenderer>().rootBone.gameObject;
            GameObject hipsDest = model_dest.GetComponent<SkinnedMeshRenderer>().rootBone.parent.gameObject;
            if (hips.name != "hips")
            {
                try
                {
                    hips = model_or.transform.Find("metarig/Root/hips").gameObject;
                }
                catch (Exception e)
                {
                    MelonLogger.Error($"insertAlternative: no hips found\n{e.Message}\n{e.StackTrace}");
                }
            }
            setParent(hipsDest, hips);
            hips.transform.localPosition = Vector3.zero;
            hips.transform.localRotation = new Quaternion(0, 0, 0.7071f, 0.7071f);

            SkinnedMeshRenderer skin = model_or.transform.Find(body).GetComponent<SkinnedMeshRenderer>();
            skin.bones[dict["hips"]].localPosition = Vector3.zero;
            return skin;
        }
    }
}
