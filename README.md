# NomaiVR - Outer Wilds VR Mod

![NomaiVR](banner/banner.png)

[![Donate with PayPal](paypal-button.png)](https://paypal.me/raicuparta/5usd)

<!-- TOC -->

- [Installation](#installation)
  - [Easy installation (recommended)](#easy-installation-recommended)
  - [Manual installation](#manual-installation)
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

<!-- /TOC -->

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

## Requirements

- The latest version of the game (1.0.7) installed (both Epic and Steam are supported);
- A VR Headset;
- VR controllers (not playable with a regular game controller);
- A VR-Ready PC;
- Steam and SteamVR installed (even if you have the Epic version);
- For some people, it only works if SteamVR is already running before starting the game. For others, only when SteamVR is closed (SteamVR will open automatically). Try both and stick with what works for you;
- Strong VR legs (both due to the nature of the game, and due to the glitchiness of this VR implementation).

## Comfort

Only recommended for people who aren't usually prone to VR sickness:

- A good portion of the game is spent spinning around in zero-g, which can be very disorienting;
- No comfort features like teleport and snap turning were implemented;
- Performance tends to not be that great, which can cause nausea;

## VR Controller Inputs

The mod tries its best to teach you how to play the game in VR, but it's not always easy with all the hacky stuff going on:

- Your headset might not have any default bindings, in which case you'll have to make your own through SteamVR;
- You can interact with most stuff by aiming with the right-hand laser and pressing the interact button;
- Always pay attention to the input prompts on your right hand;
- Make sure you don't have the input prompts disabled in the game options when trying VR for the first time;
- Holding the right hand grip changes the "interact" button's function, so make sure you're not accidentally trigerring the grip while trying to interact with objects (especially if you use the Index Knuckles controllers).

## Performance

This game was not developed with VR in mind. It was also never a super lightweight game, and shoving VR down its throat isn't helping. You'll probably need to lower your quality settings to get acceptable performance.

Besides lowering the graphics in-game (shadows, antialiasing and ambient occlusion are the heaviest hitters), try lowering the rendering resolution in SteamVR's settings.SteamVR defaults to 150%, try something like 100% instead. Changing resolution and V-sync in-game has no effect.

### Framerate

Outer Wilds was originally locked to 60 FPS. To work around this, NomaiVR forces the game's physics refresh rate to match your VR headset's refresh rate. This can have a high impact on performance. Try lowering your refresh rate through SteamVR's settings (if your headset supports this) to get a more stable framerate.

If your headset doesn't support multiple refresh rates, you can override the game's physics refresh rate in NomaiVR's options (pause the game, select Mods > NomaiVR and change Physics Refresh Rate). This will cause a disconnect between the framerate of the VR camera rotation and the framerate of the game's physics, so it's not very pleasant. Setting it to zero makes it follow your VR headset's refresh rate.

## Compatibility with other mods

NomaiVR affects code in pretty much the whole game, and drastically changes things in ways that are sure to break other mods. If you are having issues, make sure you disable any other mods you might have installed.

## Reporting bugs / making requests

See if your problem was already reported by [searching for it in the issues list](https://github.com/Raicuparta/nomai-vr/issues?q=is%3Aissue). If you find that someone else already reported the same issue, feel free to add to it by commenting (even if the issue is already closed). Otherwise, [create a new issue](https://github.com/Raicuparta/nomai-vr/issues/new/choose) (GitHub account required).

## Contributing

Look through the [currently open issues](https://github.com/Raicuparta/NomaiVR/issues) and see if there's something you'd like to help with. If you find something you'd like to do, leave a comment on the issue. Fork the repo and make the changes you want. When you're done, open a PR from your fork to this one.

If you need help, leave a comment on the issue, or ask via [Discord](https://discord.gg/Sftcc9Z).

## Development Setup

- [Download the Outer Wilds Mod Manager](https://outerwildsmods.com/) and install it anywhere you like;
- Install OWML using the Mod Manager;
- Clone NomaiVR's source;
- Open the file `NomaiVR/NomaiVR.csproj.user` in your favorite text editor;
- Edit the entry `<GameDir>` to point to the directory where Outer Wilds is installed;
- Edit the entry `<OwmlDir>` to point to your OWML directory (it is installed inside the Mod Manager directory);
- Repeat this process for the file `SteamVR/SteamVR.csproj.user`;
- Open the project solution file `NomaiVR.sln` in Visual Studio;
- If needed, right click `References` in the Solution Explorer > Manage NuGet Packages > Update OWML to fix missing references;

After doing this, the project references should be working. When you build the solution, the dll and json files will be copied to `[Mod Manager directory]/OWML/NomaiVR`. If this process is successful, you should see the mod show up in the Mod Manager.

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
