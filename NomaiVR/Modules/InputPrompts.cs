using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR {
    class InputPrompts: MonoBehaviour {
        static PromptManager _manager;

        void Start () {
            _manager = Locator.GetPromptManager();
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Post<ProbePromptController>("LateInitialize", typeof(Patches), nameof(RemoveProbePrompts));
                NomaiVR.Post<ProbePromptController>("Awake", typeof(Patches), nameof(ChangeProbePrompts));
                NomaiVR.Post<ShipPromptController>("Awake", typeof(Patches), nameof(ChangeShipPrompts));
                NomaiVR.Post<NomaiTranslatorProp>("Awake", typeof(Patches), nameof(ChangeTranslatorPrompts));
                NomaiVR.Post<SignalscopePromptController>("Awake", typeof(Patches), nameof(ChangeSignalscopePrompts));
                NomaiVR.Post<SignalscopePromptController>("LateInitialize", typeof(Patches), nameof(RemoveSignalscopePrompts));
                NomaiVR.Post<ShipPromptController>("LateInitialize", typeof(Patches), nameof(RemoveShipPrompts));
                NomaiVR.Post<NomaiTranslatorProp>("LateInitialize", typeof(Patches), nameof(RemoveTranslatorPrompts));
                NomaiVR.Post<PlayerSpawner>("Awake", typeof(Patches), nameof(RemoveJoystickPrompts));
                NomaiVR.Post<ToolModeUI>("LateInitialize", typeof(Patches), nameof(RemoveToolModePrompts));
            }

            static void RemoveJoystickPrompts (ref bool ____lookPromptAdded) {
                ____lookPromptAdded = true;
            }

            static void RemoveToolModePrompts (
                ScreenPrompt ____freeLookPrompt,
                ScreenPrompt ____probePrompt,
                ScreenPrompt ____signalscopePrompt,
                ScreenPrompt ____flashlightPrompt,
                ScreenPrompt ____centerFlashlightPrompt,
                ScreenPrompt ____centerTranslatePrompt,
                ScreenPrompt ____centerProbePrompt,
                ScreenPrompt ____centerSignalscopePrompt
            ) {
                _manager.RemoveScreenPrompt(____freeLookPrompt);
                _manager.RemoveScreenPrompt(____probePrompt);
                _manager.RemoveScreenPrompt(____signalscopePrompt);
                _manager.RemoveScreenPrompt(____flashlightPrompt);
                _manager.RemoveScreenPrompt(____flashlightPrompt);
                _manager.RemoveScreenPrompt(____centerFlashlightPrompt);
                _manager.RemoveScreenPrompt(____centerTranslatePrompt);
                _manager.RemoveScreenPrompt(____centerProbePrompt);
                _manager.RemoveScreenPrompt(____centerSignalscopePrompt);
            }

            static void ChangeProbePrompts (
                ref ScreenPrompt ____launchPrompt,
                ref ScreenPrompt ____retrievePrompt,
                ref ScreenPrompt ____takeSnapshotPrompt,
                ref ScreenPrompt ____forwardCamPrompt
            ) {
                ____launchPrompt = new ScreenPrompt(InputLibrary.interact, ____launchPrompt.GetText());
                ____forwardCamPrompt = new ScreenPrompt(InputLibrary.interact, ____takeSnapshotPrompt.GetText());
                ____retrievePrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, UITextLibrary.GetString(UITextType.ProbeRetrievePrompt) + "   <CMD>");
                ____takeSnapshotPrompt = new ScreenPrompt(InputLibrary.interact, ____takeSnapshotPrompt.GetText());
            }

            static void ChangeShipPrompts (
                ref ScreenPrompt ____exitLandingCamPrompt,
                ref ScreenPrompt ____autopilotPrompt,
                ref ScreenPrompt ____abortAutopilotPrompt
            ) {
                ____exitLandingCamPrompt = new ScreenPrompt(InputLibrary.cancel, ____exitLandingCamPrompt.GetText());
                ____autopilotPrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, ____autopilotPrompt.GetText());
                ____abortAutopilotPrompt = new ScreenPrompt(InputLibrary.interact, ____abortAutopilotPrompt.GetText());
            }

            static void RemoveProbePrompts (
                ScreenPrompt ____unequipPrompt,
                ScreenPrompt ____aimPrompt,
                ScreenPrompt ____photoModePrompt,
                ScreenPrompt ____reverseCamPrompt,
                ScreenPrompt ____rotatePrompt,
                ScreenPrompt ____rotateCenterPrompt,
                ScreenPrompt ____launchModePrompt
            ) {
                //manager.RemoveScreenPrompt(____unequipPrompt);
                _manager.RemoveScreenPrompt(____aimPrompt);
                _manager.RemoveScreenPrompt(____photoModePrompt);
                _manager.RemoveScreenPrompt(____reverseCamPrompt);
                _manager.RemoveScreenPrompt(____rotatePrompt);
                _manager.RemoveScreenPrompt(____rotateCenterPrompt);
                _manager.RemoveScreenPrompt(____launchModePrompt);
            }

            static void ChangeSignalscopePrompts (ref ScreenPrompt ____zoomModePrompt) {
                ____zoomModePrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.SignalscopeZoomInPrompt) + "   <CMD>");
            }

            static void RemoveSignalscopePrompts (
                ScreenPrompt ____unequipPrompt,
                ScreenPrompt ____changeFrequencyPrompt,
                ScreenPrompt ____zoomLevelPrompt
            ) {
                //manager.RemoveScreenPrompt(____unequipPrompt);
                _manager.RemoveScreenPrompt(____changeFrequencyPrompt);
                _manager.RemoveScreenPrompt(____zoomLevelPrompt);
            }

            static void RemoveShipPrompts (
                ScreenPrompt ____freeLookPrompt,
                ScreenPrompt ____landingModePrompt,
                ScreenPrompt ____liftoffCamera
            ) {
                _manager.RemoveScreenPrompt(____freeLookPrompt);
                _manager.RemoveScreenPrompt(____landingModePrompt);
                _manager.RemoveScreenPrompt(____liftoffCamera);
            }
            static void RemoveTranslatorPrompts (
                ScreenPrompt ____scrollPrompt,
                ScreenPrompt ____pagePrompt
            ) {
                _manager.RemoveScreenPrompt(____scrollPrompt);
                _manager.RemoveScreenPrompt(____pagePrompt);
            }

            static void ChangeTranslatorPrompts (ref ScreenPrompt ____translatePrompt) {
                ____translatePrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, UITextLibrary.GetString(UITextType.TranslatorUsePrompt) + "   <CMD>");
            }
        }
    }
}
