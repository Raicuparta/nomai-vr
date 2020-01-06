# NomaiVR
 
## Installing

[Download the latest release](https://github.com/Raicuparta/NomaiVR/releases/latest). There should be two directories and one `exe` file inside. Extract it somewhere (doesn't matter where, you'll move them later).

Before moving on to the next sections, find out where your game directory is. If you bought through Epic, it should be `C:\Program Files\Epic Games\OuterWilds`.

Now to install everything:

### Adding the OpenVR and Oculus libraries
Move the contents of the `Plugins` directory from the mod to `[Outer Wilds]\OuterWilds_Data\Plugins` where `[Outer Wilds]` is the directory mentioned before.

### Enabling VR mode
Run `UnityXREnable.exe` and follow the instalation. On the third step, make sure to select the base game's directory, as mentioned before. One file will be patched, and a backup of it will be made in the same directory. You can delete the `.exe` file if you want now.

### Installing the VR fixes mod with OWML
(If you're already familiar with OWML just install the mod normally and skip to 4.)

1. Download [Outer Worlds Mod Loader](https://github.com/amazingalek/owml/releases/latest);
2. Extract it anywhere;
3. From the NomaiVR mod files, move `Mods/NomaiVR` to the Mods directory of `OWML`;
4. Make sure your VR headset is enabled and ready (not on standby);
5. Have either SteamVR or Oculus app running;
6. Get the puke bucket ready;
7. Launch the game through `OWML.Launcher.exe`.
