# NomaiVR - Outer Wilds VR Mod

[![Support on Patreon](https://img.shields.io/badge/dynamic/json?style=flat-square&color=%23e85b46&label=Patreon&query=data.attributes.patron_count&suffix=%20patrons&url=https%3A%2F%2Fwww.patreon.com%2Fapi%2Fcampaigns%2F7004713&logo=patreon)](https://www.patreon.com/raivr) [![Donate with PayPal](https://img.shields.io/badge/PayPal-Donate-blue?style=flat-square&color=blue&logo=paypal)](https://paypal.me/raicuparta/5usd)

- [Installation](#installation)
  - [Easy installation (recommended)](#easy-installation-recommended)
  - [Manual installation](#manual-installation)
  - [Uninstalling](#uninstalling)
- [Requirements](#requirements)
- [Troubleshooting](#troubleshooting)
  - [VR Controller Inputs](#vr-controller-inputs)
  - [Poor performance](#poor-performance)
  - [Game doesn't work after removing NomaiVR](#game-doesnt-work-after-removing-nomaivr)
  - [Game doesn't start in VR / Desktop Game Theatre shenanigans](#game-doesnt-start-in-vr--desktop-game-theatre-shenanigans)
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
- Strong VR legs (it can be a very intense VR experience);
- Not compatible with any other mods.

## Troubleshooting

### VR Controller Inputs

The mod tries its best to teach you how to play the game in VR, but it's not always easy with all the hacky stuff going on:

- Your headset might not have any default bindings, in which case you'll have to make your own through SteamVR;
- You can interact with most stuff by aiming with your dominant hand laser and pressing the interact button;
- Always pay attention to the input prompts on your hand;
- Make sure you don't have the input prompts disabled in the game options when trying VR for the first time;
- Depending on the hand you use to hold tools, movement or rotation will be disabled to allow you to interact with the tools functions. Controllers that have an additional trackpad will not suffer from this shortcoming (Index and old WMR controllers);
- Left hand mode is currently in the game but you need to manually change the SteamVR bindings;

### Missing button icons

Some controllers, like WMR, might be missing icons in the prompts. If you have one of these devices and want to help, please [contact us](#support) so we can add these icons in.

### Poor performance

This game was not developed with VR in mind. It was also never a super lightweight game, and shoving VR down its throat isn't helping. You'll probably need to lower your quality settings to get acceptable performance.

Besides lowering the graphics in-game (shadows, antialiasing and ambient occlusion are the heaviest hitters), try lowering the rendering resolution in SteamVR's settings. SteamVR defaults to 150%, try something like 100% instead. Changing resolution and V-sync in-game has no effect.

### Game doesn't work after removing NomaiVR

Follow the steps in [Uninstalling](#uninstalling). If all else fails, completely delete the game folder and install it again. Your saves are in another folder, so you won't lose them.

### Game doesn't start in VR / Desktop Game Theatre shenanigans

This only helps if you have the game on Steam:

- Right-click Outer Wilds on your Steam library
- Select 'Properties...'
- Disable 'Use Desktop Game Theatre.

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
