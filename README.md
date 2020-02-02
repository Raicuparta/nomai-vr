# NomaiVR - Outer Wilds VR Mod

<img src="logo.png" width=300/>

[![NomaiVR Gameplay](https://i.imgur.com/utsUMNv.gif)](https://www.youtube.com/watch?v=BblIMEPq54M)

The aim of this mod is to enable VR mode in Outer Wilds, and eventually fix the problems that come with forcing VR in a game that's obviously not prepared for it.

## YOU WILL HAVE A BAD TIME

While the game is playable at a surprisingly decent level, a lot of things range from mildly glitchy to completely broken. Expect nausea, low performance, crashes, blue screen, house fires, big explosions visibe from space. Have a look at the [currently open issues](https://github.com/Raicuparta/NomaiVR/issues) to have an idea of some of the stuff that needs fixing.

## Requirements

* A VR Headset;
* VR controllers (if you want to play with a regular controller, use NomaiVR 0.5 instead of the latest);
* A VR-Ready PC (and even with that, you'll probably not have amazing performance);
* Steam and SteamVR installed and running prior to opening the game;
* Strong VR legs (both due to the nature of the game, and due to the glitchiness of this VR implementation).

## VR Controller Inputs

Since most VR controllers have less buttons than a normal game controller, input mappings can't be one-to-one. So I had to change the way some of the controls work:

* LB/L1 (usually mapped to left hand grip) will act as DPad-Up when piloting the ship (for auto-pilot);
* RB/R1 (usually mapped to right hand grip) is used for interactions. Picking up tools in the toolbelt, picking up objects, talking with characters, interacting with objects, etc.

On top of these changes, you also need SteamVR to do the mapping from VR controller to XBox controller inputs. I have included default mappings for Oculus, Index and Vive (but only tested Oculus).

## Installation

* [Download OWML](https://github.com/amazingalek/owml/releases);
* [Follow OWML's instalation instructions](https://github.com/amazingalek/owml#installation);
* [Download the latest NomaiVR release](https://github.com/Raicuparta/NomaiVR/releases/latest);
* Extract the `NomaiVR` directory to the `OWML/Mods` directory;
* Run `OWML.Launcher.exe` to start the game.
* Make sure the translator tool is set to "manual" in the settings;
* Disable button prompts in the settings (they will be wrong and annoying anyway);

### If you have Oculus

* Create a shortcut to `OWML.Launcher.exe` and add the params `-vrmode openvr` ([like in this image](https://i.imgur.com/5uv88Nk.png))
* Use the shortcut you just created to launch the game;

## Contributing

Look at through [currently open issues](https://github.com/Raicuparta/NomaiVR/issues) and see if there's something you'd like to help with. For investigation issues, you can help just by running the mod normally and testing stuff! For other issues, you'll need to follow the instructions in the Development section. Just fork the project and open a PR when you want to submit a change.

If your desired contribution doesn't fit one of the existing issues, create an issue first so we can discuss it.

## Development Setup

* [Install OWML](https://github.com/amazingalek/owml#installation) in the game's directory (should be something like `C:\Program Files\Epic Games\OuterWilds\OWML`);
* If you already have NomaiVR installed, remove it from the `OWML/Mods` directory;
* Clone NomaiVR's source;
* Open the project solution file `NomaiVR.sln` in Visual Studio;
* On the Solution Explorer (usually the right side panel), under the project-name (NomaiVR), double click "Properties";
* Go to "Debug" and change (if needed) "Working Directory" to **OWML's directory**;
* Do the same thing for the SteamVR project (also inside NomaiVR's solution);
* If needed, right click `References` in the Solution Explorer > Manage NuGet Packages > Update OWML to fix missing references;
* In the top menu go to "Project" > "Unload Project", and then "Project" > "Reload Project".

After doing this, the project references should be working. When you build the solution, the dll and json files will be copied to `OWML/NomaiVR`, so you can start the game and test right away. Pressing "Start" on Visual Studio will start the game through OWML.

If for some reason none of this is working, you might have to set everything manually:

* To fix the build paths and automatically copy the files to OWML, edit the "Build Events" in the properties menu.
* To fix the references, right-click "References" in the Solution Explorer > "Add Reference", and add all the missing DLLs (references with yello warning icon). You can find these DLLs in the game's directory (`OuterWilds\OuterWilds_Data\Managed`).

## Help / Discuss development / Tell me about your day

[Join the Outer Wilds Discord](https://discord.gg/Sftcc9Z), we have a nice `#modding` channel where you can discuss all types of things.
