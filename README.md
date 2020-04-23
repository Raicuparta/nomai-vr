# NomaiVR - Outer Wilds VR Mod

<img src="logo.png" width=300/>

[![NomaiVR Gameplay](https://i.imgur.com/utsUMNv.gif)](https://www.youtube.com/watch?v=BblIMEPq54M)

<!-- TOC -->

- [YOU WILL HAVE A BAD TIME](#you-will-have-a-bad-time)
- [Requirements](#requirements)
- [VR Controller Inputs](#vr-controller-inputs)
- [Installation](#installation)
  - [Easy installation (recommended)](#easy-installation-recommended)
  - [Manual installation](#manual-installation)
  - [If you have Oculus](#if-you-have-oculus)
- [Performance](#performance)
- [Compatibility with other mods](#compatibility-with-other-mods)
- [Contributing](#contributing)
- [Development Setup](#development-setup)
- [Help / Discuss development / Tell me about your day](#help--discuss-development--tell-me-about-your-day)

<!-- /TOC -->

## YOU WILL HAVE A BAD TIME

The game should be playable from start to finish in VR, but it's not gonna be super comfortable:

- A good portion of the game is spent spinning around in zero-g, completely disoriented;
- It's not uncommon to get motion sickness playing this game in pancake mode, let alone in VR;
- The game physics is locked to 60FPS;
- No comfort features like teleport and snap turning were implemented;
- It's all dirty hacks on top of even dirtier hacks.

Expect nausea, low performance, flashing images, crashes, blue screens, house fires, etc. Have a look at the [currently open issues](https://github.com/Raicuparta/NomaiVR/issues) to have an idea of some of the stuff that needs fixing.

## Requirements

- A VR Headset;
- VR controllers (not playable with a regular game controller);
- A VR-Ready PC;
- Steam and SteamVR installed and running prior to opening the game;
- Strong VR legs (both due to the nature of the game, and due to the glitchiness of this VR implementation).

## VR Controller Inputs

There are some extra in-game tutorials for teaching you VR inputs, but some stuff doesn't have any tutorials yet.

- You can't change the control bindings in-game (they will always reset);
- Your headset might not have any default bindings, in which case you'll have to make your own through SteamVR;
- You can interact with most stuff by aiming with the right-hand laser and pressing the interact button;
- Right hand grip can be used to grab tools from the tool belt;
- Change tool modes by holding a tool on your right hand and touching it with your left hand;
- While piloting the ship, you can aim at and interact with stuff inside the ship (interact with the screens to equip that tool);
- Turn on the flashlight by touching the side of your head with your right hand;
- Experiment with the controls. When in doubt, aim laser and press interact.

## Installation

### Easy installation (recommended)

- [Download the Outer Wilds Mod Manager](https://github.com/Raicuparta/ow-mod-manager);
- [Follow the instalation instructions on the Mod Manager's page](https://github.com/Raicuparta/ow-mod-manager#how-do-i-use-this);
- Install NomaiVR from the mod list displayed in the application;
- If you can't get the mod manager to work, follow the instructions for manual installation.

### Manual installation

- [Download OWML](https://github.com/amazingalek/owml/releases);
- [Follow OWML's instalation instructions](https://github.com/amazingalek/owml#installation);
- [Download the latest NomaiVR release](https://github.com/Raicuparta/NomaiVR/releases/latest);
- Extract the `NomaiVR` directory to the `OWML/Mods` directory;
- Run `OWML.Launcher.exe` to start the game.
- Make sure the translator tool is set to "manual" in the settings;
- Disable button prompts in the settings (they will be wrong and annoying anyway);

### If you have Oculus

- Find OWML's directory (either inside the mod manager's directory, or wherever extracted OWML to);
- Create a shortcut to `OWML.Launcher.exe` and add the params `-vrmode openvr` ([like in this image](https://i.imgur.com/5uv88Nk.png))
- Use the shortcut you just created to launch the game;

## Performance

- This game was not developed with VR in mind;
- It was never a super lightweight game, and shoving VR down its throat isn't helping;
- The game's physics is locked at 60 FPS. So even though the game reports more FPS than that, it will look stuttery;
- You'll probably need to lower your quality settings to get acceptable performance.

## Compatibility with other mods

NomaiVR affects code in pretty much the whole game, and drastically changes things in ways that are sure to break other mods. If you are having issues, make sure you disable any other mods you might have installed.

## Contributing

Look at through [currently open issues](https://github.com/Raicuparta/NomaiVR/issues) and see if there's something you'd like to help with. There's a few ways you can help:

- Test the game, find bugs, report them as a new issue;
- Open issue with feature requests;
- Contribute to the code base (fork the repo and open a PR).

## Development Setup

- [Install OWML](https://github.com/amazingalek/owml#installation) in the game's directory (should be something like `C:\Program Files\Epic Games\OuterWilds\OWML`);
- If you already have NomaiVR installed, remove it from the `OWML/Mods` directory;
- Clone NomaiVR's source;
- Open the project solution file `NomaiVR.sln` in Visual Studio;
- On the Solution Explorer (usually the right side panel), under the project-name (NomaiVR), double click "Properties";
- Go to "Debug" and change (if needed) "Working Directory" to **OWML's directory**;
- Do the same thing for the SteamVR project (also inside NomaiVR's solution);
- If needed, right click `References` in the Solution Explorer > Manage NuGet Packages > Update OWML to fix missing references;
- In the top menu go to "Project" > "Unload Project", and then "Project" > "Reload Project".

After doing this, the project references should be working. When you build the solution, the dll and json files will be copied to `OWML/NomaiVR`, so you can start the game and test right away. Pressing "Start" on Visual Studio will start the game through OWML.

If for some reason none of this is working, you might have to set everything manually:

- To fix the build paths and automatically copy the files to OWML, edit the "Build Events" in the properties menu.
- To fix the references, right-click "References" in the Solution Explorer > "Add Reference", and add all the missing DLLs (references with yello warning icon). You can find these DLLs in the game's directory (`OuterWilds\OuterWilds_Data\Managed`).

## Help / Discuss development / Tell me about your day

[Join the Outer Wilds Discord](https://discord.gg/Sftcc9Z), we have a nice `#modding` channel where you can discuss all types of things.
