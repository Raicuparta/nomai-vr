# ![NomaiVR](RepoAssets/banner.png)

NomaiVR is a VR mod for [Outer Wilds](https://store.steampowered.com/app/753640/Outer_Wilds/), with full motion control support.

[![Support on Patreon](https://img.shields.io/badge/dynamic/json?style=flat-square&color=%23e85b46&label=Patreon&query=data.attributes.patron_count&suffix=%20patrons&url=https%3A%2F%2Fwww.patreon.com%2Fapi%2Fcampaigns%2F7004713&logo=patreon)](https://www.patreon.com/raivr) [![Donate with PayPal](https://img.shields.io/badge/PayPal-Donate-blue?style=flat-square&color=blue&logo=paypal)](https://paypal.me/raicuparta/5usd)

- [Installation](#installation)
  - [Easy installation (recommended)](#easy-installation-recommended)
  - [Manual installation](#manual-installation)
  - [Xbox app / Game Pass version](#xbox-app--game-pass-version)
  - [Uninstalling](#uninstalling)
- [Requirements](#requirements)
- [Enabling Fixed Foveated Rendering](#enabling-fixed-foveated-rendering)
- [Enabling FSR](#enabling-fsr)
- [Troubleshooting](#troubleshooting)
  - [Error when starting game](#error-when-starting-game)
  - [VR Controller Inputs](#vr-controller-inputs)
  - [Missing button icons](#missing-button-icons)
  - [Poor performance](#poor-performance)
  - [Game doesn't work after removing NomaiVR](#game-doesnt-work-after-removing-nomaivr)
  - [Game doesn't start in VR / Desktop Game Theatre shenanigans](#game-doesnt-start-in-vr--desktop-game-theatre-shenanigans)
  - [Stuck in initial loading screen (loading forever)](#stuck-in-initial-loading-screen-loading-forever)
- [Support](#support)
- [People](#people)
- [Development Setup](#development-setup)

## Installation

### Easy installation (recommended)

- Get the Mod Manager from the [Outer Wilds Mods](https://outerwildsmods.com/) website;
- Install OWML
- Install NomaiVR from the mod list displayed in the application;
- If you can't get the mod manager to work, follow the instructions for manual installation.

### Manual installation

- [Install OWML](https://github.com/amazingalek/owml#installation);
- [Download the latest NomaiVR release (Raicuparta.NomaiVR.zip)](https://github.com/Raicuparta/NomaiVR/releases/latest);
- Extract the `Raicuparta.NomaiVR` directory to the `OWML/Mods` directory;
- Run `OWML.Launcher.exe` to start the game.

### Xbox app / Game Pass version

If you got the game from the PC Xbox app, or from your PC Xbox Game Pass Subscription, you'll need to follow some steps to make the game moddable:

- Close the Xbox app (close it in the system tray too, to make sure it's gone completely).
- Get the [Xbox Insider Hub app](https://www.microsoft.com/en-us/p/xbox-insider-hub/9pldpg46g47z).
- Start the Xbox Insider Hub app.
- Select "Previews", and then "Windows Gaming".
- Click "Join" and wait for the process to finish.
- At this point, you might need to let the Xbox app install some updates, I'm not sure. I opened the Windows App Store and let it install all pending updates just to be sure.
- Open the Xbox app.
- On the top right, click your avatar, then, "Settings".
- Select "General".
- Under "Game install options", check "Use advanced installation and management features".
- Exit the settings menu.
- Select Outer Wilds in your library.
- Open the three dots menu, and select "Manage".
- Under "Advanced management features", click "Enable".
- Wait for the process to finish.
- In the same "Manage" menu, go to the "Files" tab.
- Click "Browse...". This should open a folder with your Xbox PC games.
- Open the Outer Wilds folder, and then the Content folder.
- You should now see the game files, including Outer Wilds.exe.
- Now start the [ Outer Wilds Mod Manager](https://outerwildsmods.com/mod-manager), install NomaiVR, and try to start the game.
- It should automatically detect that you have the Xbox app version of the game, unless you have multiple versions of the game.
- If the manager has trouble finding it, you can go to the mod manager's options and change the game path to that "Content" folder I mentioned before.
- You'll have to log in with your Xbox account the first time you launch the game (don't skip it).
- If the game gets stuck in the initial loading screen, try launching it from the Xbox app, and don't skip the intro logos (seems to be a bug in the base game).

### Uninstalling

- Uninstall NomaiVR from the Mod Manager, or delete the mod folder in `OWML/Mods/Raicuparta.NomaiVR`;
- **Important**! Verify game file integrity:
  - **Steam**: Library > Right-click Outer Wilds > Properties > Local Files > Verify integrity of game files.
  - **Epic**: Library > Click three dots under Outer Wilds > Verify.

## Requirements

- Version 1.1.10+ of the game installed;
- Supports all PC versions of the game (Steam, Epic or Game Pass);
- Echoes of the Eye DLC is fully supported but not required;
- A VR Headset;
- VR controllers (not playable with a regular game controller);
- A VR-Ready PC;
- Steam and SteamVR installed (even if you're using a non-Steam version of the game);
- Strong VR legs (it can be a very intense VR experience);

## Enabling Fixed Foveated Rendering

For RXT Cards, you can [install the Fixed Foveated Rendering addon](https://outerwildsmods.com/mods/nomaivrffr) to get better performance.

## Enabling FSR

NomaiVR is shipped with [openvr_fsr](https://github.com/fholger/openvr_fsr), which can greatly improve performance. It is disabled by default. Follow these steps to enable it and change its settings:

- Open the NomaiVR directory (in the Outer Wilds Mod Manager, click the three dots next to NomaiVR and select "Show in explorer");
- Navigate to `Raicuparta.NomaiVR\patcher\files\OuterWilds_Data\Plugins\x86_64`;
- Edit `openvr_mod.cfg`;
- Change the line `"enabled": false,` to `"enabled": true,`;
- Read the [openvr_fsr documentation](https://github.com/fholger/openvr_fsr) to learn more.

These settings will be reset every time you update or reinstall NomaiVR.

## Troubleshooting

### Error when starting game

If you get an error saying something like "Failed to initialize player" or "Failed to load PlayerSettings", make sure you are **NOT** in the Steam beta branch for Outer Wilds 1.0.7, you need to be on Outer Wilds 1.10.0. Then, make sure to verify the integrity of your game files before running NomaiVR:

- **Steam**: Library > Right-click Outer Wilds > Properties > Local Files > Verify integrity of game files.
- **Epic**: Library > Click three dots under Outer Wilds > Verify.

If all else fails, try completely deleting the game folder (not just uninstalling from Steam / Epic). And then install it again. Your saves are in another folder, so you won't lose them.

### VR Controller Inputs

The mod tries its best to teach you how to play the game in VR, but it's not always easy with all the hacky stuff going on:

- Your headset might not have any default bindings, in which case you'll have to make your own through SteamVR;
- You can interact with most stuff by aiming with your dominant hand laser and pressing the interact button;
- Always pay attention to the input prompts on your hand;
- Make sure you don't have the input prompts disabled in the game options when trying VR for the first time;
- Left hand mode is currently in the game but you need to manually change the SteamVR bindings;
- If something is very broken with the controls, go to SteamVR settings while the game is running, and reset the input bindings. Make sure you're using the default bindings and not some custom bindings (because a NomaiVR update can break your custom bindings).

### Missing button icons

Some controllers, like WMR, might be missing icons in the prompts. If you have one of these devices and want to help, please [contact us](#support) so we can add these icons in.

### Poor performance

See [Enabling FSR](#enabling-fsr) for an easy way to improve performance.

This game was not developed with VR in mind. It was also never a super lightweight game, and shoving VR down its throat isn't helping. You'll probably need to lower your quality settings to get acceptable performance.

Besides lowering the graphics in-game (shadows, antialiasing and ambient occlusion are the heaviest hitters), try lowering the rendering resolution in SteamVR's settings. SteamVR defaults to 150%, try something like 100% instead. Changing resolution and V-sync in-game has no effect.

### Game doesn't work after removing NomaiVR

Follow the steps in [Uninstalling](#uninstalling). If all else fails, completely delete the game folder and install it again. Your saves are in another folder, so you won't lose them.

### Game doesn't start in VR / Desktop Game Theatre shenanigans

This only helps if you have the game on Steam:

- Right-click Outer Wilds on your Steam library
- Select 'Properties...'
- Disable 'Use Desktop Game Theatre.

### Stuck in initial loading screen (loading forever)

If the game fails to connect to the launcher/store app (Steam, Epic, or Xbox App), it will be forever stuck in the first loading screen, and won't show any menu options to select. If the game is already loading in VR but you are stuck in this screen before being able to see the main menu, try launching the game from the store app. You'll need to start the game from the Mod Manager at least once, every time you install or update NomaiVR. After that you can go back to launching via the original store app.

This can also happen in the Xbox app / Game Pass version of the game, if you skip the Xbox login prompt. Don't skip this prompt, log in with your Xbox account. It should only request you for this information once, after that it won't show up again.

## Support

- Via GitHub issues:
  - See if your problem was already reported by [searching for it in the issues list](https://github.com/Raicuparta/nomai-vr/issues?q=is%3Aissue);
  - If you find that someone else already reported the same issue, feel free to add to it by commenting (even if the issue is already closed);
  - Otherwise, [create a new issue](https://github.com/Raicuparta/nomai-vr/issues/new/choose) (GitHub account required).
- Via Discord:
  - [Join the Flatscreen to VR Discord](https://discord.gg/MwAHbNBdqE);
  - Follow the instructions to join the Outer Wilds VR channels.

## People

NomaiVR is made by **[Raicuparta](https://github.com/Raicuparta)** and **[artumino](https://github.com/artumino)**. Special thanks to everyone helped us along the way:

- **[amazingalek](https://github.com/amazingalek)**, for making OWML and teaching me how to mod the game to begin with;
- **[TAImatem](https://github.com/TAImatem)** and **[misternebula](https://github.com/misternebula)**, for improving the VR patch and for helpful discussions about the game's code;
- **Logan Ver Hoef**, for assisting us with the game's code;
- **[Xelu](https://thoseawesomeguys.com/prompts/)**, for the icons we used in the input prompts;
- **No Chill** from the Flatscreen to VR Discord, for helping adapt some of the icons;
- Everyone over at the **Outer Wilds Discord** server for all the support;
- **Mobius Digital** for making a neat game.

## Development Setup

See [NomaiVR Development Setup](SETUP.md)
