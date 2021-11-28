# NomaiVR Development Setup

- Install Unity 2019.4.27
- [Download the Outer Wilds Mod Manager](https://outerwildsmods.com/) and install it anywhere you like;
- Install OWML using the Mod Manager;
- Clone NomaiVR's source;
- Create the file `NomaiVR/NomaiVR.csproj.user` in your favorite text editor;

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ProjectView>ProjectFiles</ProjectView>
    <GameDir>C:\Program Files\Steam\steamapps\common\Outer Wilds</GameDir>
    <OwmlDir>C:\Users\USER\AppData\Roaming\OuterWildsModManager\OWML</OwmlDir>
    <UnityEditor>C:\Program Files (x86)\Unity\2019.4.27f1\Editor\Unity.exe</UnityEditor>
    <ModDir>NomaiVR</ModDir>
  </PropertyGroup>
</Project>
```

- Edit the entry `<GameDir>` to point to the directory where Outer Wilds is installed;
- Edit the entry `<OwmlDir>` to point to your OWML directory (it is installed inside the Mod Manager directory);
- Edit the entry `<UnityEditor>` to point to your Unity 2019.4.27 editor executable;
- Download the [AssemblyPublicizer](https://github.com/Raicuparta/AssemblyPublicizer/releases) and extract the exe anywhere;
- Drag the file `OuterWilds\OuterWilds_Data\Managed\Assembly-CSharp.dll` and drop it on top of `AssemblyPublicizer.exe`;
- Confirm that it generated a new file `OuterWilds\OuterWilds_Data\Managed\publicized_assemblies\Assembly-CSharp_publicized.dll`;
- Open Unity and import the project under `Unity`, some dependencies should be downloaded
- When asked about VR support select `Legacy VR`
- Close Unity when the project has finished importing
- Open the project solution file `NomaiVR.sln` in Visual Studio;
- If needed, right click `References` in the Solution Explorer > Manage NuGet Packages > Update OWML to fix missing references;

After doing this you should compile a release build from Visual Studio (the configuration to select is `OWML`), it'll let unity compile the support project and assetbundles and then compile the mod.
The project references should now be working. When you build the solution, the dll and json files will be copied to `[Mod Manager directory]/OWML/NomaiVR`. If this process is successful, you should see the mod show up in the Mod Manager.

The available build configurations are:

- `Debug` which compiles only the mod and patcher binaries for BepInEx
- `DebugOWML` which compiles only the mod and patcher binaries for OWML
- `Release` which compiles both the unity project and the mod + patcher for BepInEx
- `OWML` which compiles both the unity project and the mod + patcher for OWML

If for some reason none of this is working, you might have to set everything manually:

- To fix the references, right-click "References" in the Solution Explorer > "Add Reference", and add all the missing DLLs (references with yellow warning icon). You can find these DLLs in the game's directory (`OuterWilds\OuterWilds_Data\Managed`);
- If Visual Studio isn't able to automatically copy the files, you'll have to copy the built dlls manually to OWML.
