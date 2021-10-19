# NomaiVR - Outer Wilds VR Mod

![NomaiVR](RepoAssets/banner.png)

[![Support on Patreon](https://img.shields.io/badge/dynamic/json?style=for-the-badge&color=%23e85b46&label=Patreon&query=data.attributes.patron_count&suffix=%20patrons&url=https%3A%2F%2Fwww.patreon.com%2Fapi%2Fcampaigns%2F7004713&logo=patreon)](https://www.patreon.com/raivr) [![Donate with PayPal](https://img.shields.io/badge/PayPal-Donate-blue?style=for-the-badge&color=blue&logo=paypal)](https://paypal.me/raicuparta/5usd)

[![NomaiVR Video](RepoAssets/video-thumbnail.jpg)](https://www.youtube.com/watch?v=gPFiYRMm8Ok)

- [Installation](#installation)
  - [Easy installation (recommended)](#easy-installation-recommended)
  - [Manual installation](#manual-installation)
  - [Uninstalling](#uninstalling)
- [Requirements](#requirements)
- [Comfort](#comfort)
- [VR Controller Inputs](#vr-controller-inputs)
- [Performance](#performance)
  - [Framerate](#framerate)
- [Compatibility with other mods](#compatibility-with-other-mods)
- [Reporting bugs / making requests](#reporting-bugs--making-requests)
- [Contributing](#contributing)
- [Development Setup](#development-setup)
- [Special Thanks](#special-thanks)
- [Help / Discuss development / Tell me about your day](#help--discuss-development--tell-me-about-your-day)

## Installation

### Easy installation (recommended)

- Get the Mod Manager from the [Outer Wilds Mods](https://outerwildsmods.com/) website;
- Install NomaiVR from the mod list displayed in the application;
- If you can't get the mod manager to work, follow the instructions for manual installation.

### Manual installation

- [Install OWML](https://github.com/amazingalek/owml#installation);
- [Download the latest NomaiVR release (Raicuparta.NomaiVR.zip)](https://github.com/Raicuparta/NomaiVR/releases/latest);
- Extract the `Raicuparta.NomaiVR` directory to the `OWML/Mods` directory;
- Run `OWML.Launcher.exe` to start the game.

### Uninstalling

- Uninstall NomaiVR from the Mod Manager, or delete the mod folder in `OWML/Mods/Raicuparta.NomaiVR`;
- **Important**! Verify game file integrity:
  - **Steam**: Library > Right-click Outer Wilds > Properties > Local Files > Verify integrity of game files.
  - **Epic**: Library > Click three dots under Outer Wilds > Verify.

## Requirements

- Version 1.10.0 of the game installed (both Epic and Steam are supported).
- Echoes of the Eye DLC is supported and fully playable, although not thoroughly tested;
- A VR Headset;
- VR controllers (not playable with a regular game controller);
- A VR-Ready PC;
- Steam and SteamVR installed (even if you have the Epic version);
- Strong VR legs (both due to the nature of the game, and due to the glitchiness of this VR implementation).

## Comfort

Only recommended for people who aren't usually prone to VR sickness:

- A good portion of the game is spent spinning around in zero-g, which can be very disorienting;
- No comfort features like teleport and snap turning were implemented;
- Performance tends to not be that great, which can cause nausea;

## VR Controller Inputs

The mod tries its best to teach you how to play the game in VR, but it's not always easy with all the hacky stuff going on:

- Your headset might not have any default bindings, in which case you'll have to make your own through SteamVR;
- You can interact with most stuff by aiming with your dominant hand laser and pressing the interact button;
- Always pay attention to the input prompts on your hand;
- Make sure you don't have the input prompts disabled in the game options when trying VR for the first time;
- Depending on the hand you use to hold tools, movement or rotation will be disabled to allow you to interact with the tools functions. Controllers that have an additional trackpad will not suffer from this shortcoming (Index and old WMR controllers);
- Left hand mode is currently in the game but you need to manually change the SteamVR bindings;

## Performance

This game was not developed with VR in mind. It was also never a super lightweight game, and shoving VR down its throat isn't helping. You'll probably need to lower your quality settings to get acceptable performance.

Besides lowering the graphics in-game (shadows, antialiasing and ambient occlusion are the heaviest hitters), try lowering the rendering resolution in SteamVR's settings.SteamVR defaults to 150%, try something like 100% instead. Changing resolution and V-sync in-game has no effect.

## Compatibility with other mods

NomaiVR affects code in pretty much the whole game, and drastically changes things in ways that are sure to break other mods. If you are having issues, make sure you disable any other mods you might have installed.

## Reporting bugs / making requests

See if your problem was already reported by [searching for it in the issues list](https://github.com/Raicuparta/nomai-vr/issues?q=is%3Aissue). If you find that someone else already reported the same issue, feel free to add to it by commenting (even if the issue is already closed). Otherwise, [create a new issue](https://github.com/Raicuparta/nomai-vr/issues/new/choose) (GitHub account required).

## Contributing

Look through the [currently open issues](https://github.com/Raicuparta/NomaiVR/issues) and see if there's something you'd like to help with. If you find something you'd like to do, leave a comment on the issue. Fork the repo and make the changes you want. When you're done, open a PR from your fork to this one.

If you need help, leave a comment on the issue, or ask via [Discord](https://discord.gg/Sftcc9Z).

## Development Setup

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

## Special Thanks

- **[amazingalek](https://github.com/amazingalek)**, for making OWML and teaching me how to mod the game to begin with;
- **[TAImatem](https://github.com/TAImatem)** and **[misternebula](https://github.com/misternebula)**, for improving the VR patch and for helpful discussions about the game's code;
- **Logan Ver Hoef**, for assisting us with the game's code;
- Everyone over at the **Outer Wilds Discord** server for all the support;
- **Mobius Digital** for making a neat game.

## Help / Discuss development / Tell me about your day

[Join the Outer Wilds Discord](https://discord.gg/Sftcc9Z), we have a nice `#modding` channel where you can discuss all types of things.
