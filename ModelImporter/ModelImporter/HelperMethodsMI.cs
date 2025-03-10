using Il2Cpp;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        public static void copyComponent(GameObject dest, String comp_or, String name, bool setNull)
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
                keyValuePairs.Add(bone.name, idx++);
            }
            return keyValuePairs;
        }

        public static void updatePose(SkinnedMeshRenderer origin, SkinnedMeshRenderer dest, Dictionary<String, int> dict)
        {
            try
            {
                foreach (Transform bone in origin.bones)
                {
                    Transform dest_Transform = dest.bones[dict[bone.name]];
                     if ((bone.name == "shoulder_R" || bone.name == "shoulder_L") && (dest.name.Contains("Placeholder")) && PlayerState.aiming && InventoryManager.EquippedWeapon.name != "Shotgun") // Placeholder
                    {
                        Vector3 eul = bone.localEulerAngles;
                        eul.y = eul.y * 1.065f;
                        eul.z = eul.z * 0.99f;
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
 
            setParent("__Prerequisites__/Character Origin/Character Root/Ellie_Default", model_or);
            MelonLogger.Msg($"Set Parent for Model: {model_or.name}");
            GameObject root = GameObject.Find($"__Prerequisites__/Character Origin/Character Root/Ellie_Default/{model_or.name}/{normal}/Root/");
            if(root == null)
            {
                MelonLogger.Error($"Root not found for {model_or.name}");
                return null;
            }
            root.name = $"Root_{name}";
            setParent("__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/", root);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = new Quaternion(0, 0, 0.7071f, 0.7071f);

            SkinnedMeshRenderer skin = model_or.transform.Find(body).GetComponent<SkinnedMeshRenderer>();
            skin.bones[dict["hips"]].localPosition = Vector3.zero;

            copyComponent(skin.bones[dict["hand_R"]].gameObject, "__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/spine/chest/shoulder_R/upper_arm_R/forearm_R/hand_R/WeaponMount/", "WeaponMount", true);
            copyComponent(skin.bones[dict["hips"]].gameObject, "__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/VisibleEquip/", "VisibleEquip", true);
            copyComponent(skin.bones[dict["chest"]].gameObject, "__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/spine/chest/Nitro Model/", "Nitro Model", false);
            copyComponent(skin.bones[dict["chest"]].gameObject, "__Prerequisites__/Character Origin/Character Root/Ellie_Default/metarig/Root/hips/spine/chest/FlashLightFlare/", "FlashLightFlare", true);

            return skin;
        }
    }
}
