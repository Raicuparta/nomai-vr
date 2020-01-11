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

## Development

- Install OWML and the mod (with Vortex or without, does't matter) as per the instalation instructions above;
- Run the game with the mod enabled at least once, and make sure it's working;
- Clone NomaiVR's source;
- Open the project solution file `NomaiVR.sln` in Visual Studio;
- Add the necessary references (`Assembly-CSharp` and all the needed `UnityEngine.*` dlls) from `Outer Wilds\OuterWilds_Data\Managed`;
- Fix the post-build step under build settings (right click on project > Properties > Build Events), so you don't have to move the built dll to the mods folder every time. Make sure it points to the same place where you installed the NomaiVR mod;
- Make changes;
- Build;
- Run the game through OWML;
- Confirm that NomaiVR is running.

## Help / Discuss development / Tell me about your day

[Join the Outer Wilds Discord](https://discord.gg/Sftcc9Z), we have a nice `#modding` channel where you can discuss all types of things.
