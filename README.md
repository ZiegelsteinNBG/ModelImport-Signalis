# ModelImport-Signalis[WIP]
Everything is currently WIP: scripts, mod, guide...

This mod allows importing custom models into Signalis by loading AssetBundles.

## Model export guide
Please read this guide carefully. If you have any difficulties understanding specific steps, have suggestions for improvement or need any help, feel free to post an issue on GitHub or contact me on Discord (user: ziegelstein).

This guide provides information on the requirements your model must meet and how to export it from Unity as an AssetBundle. However, the process of importing your model from Blender and rigging it in the Unity Editor is up to you.
Please do me a favor and title your mod with the prefix "MI-" so it's visible that it's a customization mod (this will help make it clear to possible future mods ;)) when you publish it on NexusMods.

#### Requirements:
- [Unity Hub](https://docs.unity3d.com/hub/manual/InstallHub.html)
- [Required Version of Unity 2020.3.36](https://unity.com/releases/editor/whats-new/2020.3.36#release-notes)
- [AssetBundleBuilder Script](https://github.com/ZiegelsteinNBG/ModelImport-Signalis/tree/main/AssetBundle%20Scripts)
- Import the [AssetBundleExample](https://github.com/ZiegelsteinNBG/ModelImport-Signalis/tree/main/AssetBundleExample) into your Unity project along with the [AssetBundleLoader](https://github.com/ZiegelsteinNBG/ModelImport-Signalis/tree/main/AssetBundle%20Scripts) script to understand the correct structure and setup.
- Your AssetBundle must contain both your model and the metarig. The metarig should be named exactly as in the [example AssetBundle](https://github.com/ZiegelsteinNBG/ModelImport-Signalis/tree/main/AssetBundleExample) and must have the same bone count to ensure compatibility.
  
### 1. Download and Install Unity Hub & Unity
Download and install [Unity Hub](https://docs.unity3d.com/hub/manual/InstallHub.html)
Ensure you have the correct [Unity Version 2020.3.36](https://unity.com/releases/editor/whats-new/2020.3.36#release-notes) installed. 

### 2. Create a New Unity Project
Open Unity Hub and create a new 3D Core project. (Projects > New Project > Core > 3D(Built-In Render Pipeline), make here sure to chose the correct Unity Editor 2020.3.36)
<p align="center">
  <img align="center" src="img/CreateProject.png">
</p>

### 3. Import the AssetBundle Scripts
Import the script into Unity by placing it inside your Assets/Scripts/ folder. (Create the Scripts folder)
Drag the scripts into the Scripts folder.

### 4. Import the AssetBundleExample Model
Download the AssetBundleExample and go to Assets > Load AssetBundle in Unity. Select the example AssetBundle file.
If everything is set up correctly, the example model should appear in the scene.
<p align="center">
  <img align="center" src="img/ScriptAndImport.png">
</p>
