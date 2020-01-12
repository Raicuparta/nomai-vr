![NomaiVR](logo.png)

# NomaiVR - Outer Wilds VR Mod

The aim of this mod is to enable VR mode in Outer Wilds, and eventually fix the problems that come with forcing VR in a game that's obviously not prepared for it.

## YOU WILL HAVE A BAD TIME

While the game is playable at a surprisingly decent level, a lot of things range from mildly glitchy to completely broken. Expect low performance, crashes, blue screen, house fires, big explosions visibe from space. Have a look at the [currently open issues](https://github.com/Raicuparta/NomaiVR/issues) to have an idea of some of the stuff that needs fixing.
 
## Installation
Easy way:
* [Follow the instructions to install Vortex and the Outer Wilds Mod Manager](https://www.nexusmods.com/outerwilds/mods/1);
* [Install the mod through Vortex](https://www.nexusmods.com/outerwilds/mods/7);
* Run the game through Vortex.

Manually:
* [Download OWML](https://github.com/amazingalek/owml/releases);
* [Follow the instalation instructions](https://github.com/amazingalek/owml#installation);
* [Download a release](https://github.com/Raicuparta/NomaiVR/releases);
* Extract the `NomaiVR` directory to the `OWML/Mods` directory.
* Run the game with `OWML.Launcher.exe`.

## Contributing

Look at through [currently open issues](https://github.com/Raicuparta/NomaiVR/issues) and see if there's something you'd like to help with. For investigation issues, you can help just by running the mod normally and testing stuff! For other issues, you'll need to follow the instructions in the Development section. Just fork the project and open a PR when you want to submit a change.

If your desired contribution doesn't fit one of the existing issues, create an issue first so we can discuss it.

## Development Setup

* [Install OWML](https://github.com/amazingalek/owml#installation) in the game's directory (should be something like `C:\Program Files\Epic Games\OuterWilds\OWML`);
* If you already have NomaiVR installed, remove it from the `OWML/Mods` directory;
* Clone NomaiVR's source;
* Open the project solution file `NomaiVR.sln` in Visual Studio;
* On the Solution Explorer (usually the right side panel), under the project-name (NomaiVR), double click "Properties";
* Go to "Debug" and change (if needed) "Working Directory" to **OWML's directory** (no need to change anything else);
* In the top menu go to "Project" > "Unload Project", and then "Project" > "Reload Project".

After doing this, the project references should be working. If for some reason they're not, you'll have to set everything manually. To fix the build paths and automatically copy the files to OWML, edit the "Build Events" in the properties menu. To fix the references, right-click "References" in the Solution Explorer > "Add Reference", and add all the missing DLLs (references with yello warning icon).

## Help / Discuss development / Tell me about your day

[Join the Outer Wilds Discord](https://discord.gg/Sftcc9Z), we have a nice `#modding` channel where you can discuss all types of things.
