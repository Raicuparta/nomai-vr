using OWML.ModHelper.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Harmony;
using System.IO;

namespace NomaiVR {
    class InputPrompts: MonoBehaviour {
        static PromptManager _manager;

        void Start () {
            _manager = Locator.GetPromptManager();
        }

        internal static class Patches {
            static Texture2D textureX;

            public static void Patch () {
                NomaiVR.Post<ProbePromptController>("LateInitialize", typeof(Patches), nameof(RemoveProbePrompts));
                NomaiVR.Post<ProbePromptController>("Awake", typeof(Patches), nameof(ChangeProbePrompts));

                NomaiVR.Post<ShipPromptController>("LateInitialize", typeof(Patches), nameof(RemoveShipPrompts));
                NomaiVR.Post<ShipPromptController>("Awake", typeof(Patches), nameof(ChangeShipPrompts));

                NomaiVR.Post<NomaiTranslatorProp>("LateInitialize", typeof(Patches), nameof(RemoveTranslatorPrompts));
                NomaiVR.Post<NomaiTranslatorProp>("Awake", typeof(Patches), nameof(ChangeTranslatorPrompts));

                NomaiVR.Post<SignalscopePromptController>("LateInitialize", typeof(Patches), nameof(RemoveSignalscopePrompts));
                NomaiVR.Post<SignalscopePromptController>("Awake", typeof(Patches), nameof(ChangeSignalscopePrompts));

                NomaiVR.Post<PlayerSpawner>("Awake", typeof(Patches), nameof(RemoveJoystickPrompts));
                NomaiVR.Post<ToolModeUI>("LateInitialize", typeof(Patches), nameof(RemoveToolModePrompts));

                NomaiVR.Pre<LockOnReticule>("Init", typeof(Patches), nameof(InitLockOnReticule));

                // Prevent probe launcher from moving the prompts around.
                NomaiVR.Empty<PromptManager>("OnProbeSnapshot");
                NomaiVR.Empty<PromptManager>("OnProbeSnapshotRemoved");
                NomaiVR.Empty<PromptManager>("OnProbeLauncherEquipped");
                NomaiVR.Empty<PromptManager>("OnProbeLauncherUnequipped");

                // Load new icons.
                var harmony = HarmonyInstance.Create("nomaivr");
                var initMethod = typeof(InputTranslator).GetMethod("GetButtonTexture", new[] { typeof(XboxButton) });
                var harmonyMethod = new HarmonyMethod(typeof(Patches), nameof(PostInitTranslator));
                harmony.Patch(initMethod, null, harmonyMethod);

                var assetBundle = NomaiVR.Helper.Assets.LoadBundle("assets/input-icons");
                textureX = assetBundle.LoadAsset<Texture2D>("assets/hold.png");
                NomaiVR.Log("textureX", textureX.name);
            }

            static Texture2D PostInitTranslator (Texture2D __result, XboxButton button) {
                if (button == XboxButton.Y && textureX) {
                    return textureX;
                }
                return __result;
            }

            static bool InitLockOnReticule (
                ref ScreenPrompt ____lockOnPrompt,
                ref bool ____initialized,
                bool ____showFullLockOnPrompt,
                ref string ____lockOnPromptText,
                ref string ____lockOnPromptTextShortened,
                ScreenPromptList ____promptListBlock,
                ref JetpackPromptController ____jetpackPromptController,
                ref ScreenPrompt ____matchVelocityPrompt,
                Text ____readout
            ) {
                if (!____initialized) {
                    ____jetpackPromptController = Locator.GetPlayerTransform().GetComponent<JetpackPromptController>();
                    ____lockOnPromptText = "<CMD>" + UITextLibrary.GetString(UITextType.PressPrompt) + "   " + UITextLibrary.GetString(UITextType.LockOnPrompt);
                    ____lockOnPromptTextShortened = "<CMD>";
                    ____showFullLockOnPrompt = !PlayerData.GetPersistentCondition("HAS_PLAYER_LOCKED_ON");
                    if (____showFullLockOnPrompt) {
                        ____lockOnPrompt = new ScreenPrompt(InputLibrary.interact, ____lockOnPromptText, 0, false, false);
                    } else {
                        ____lockOnPrompt = new ScreenPrompt(InputLibrary.interact, ____lockOnPromptTextShortened, 0, false, false);
                    }
                    ____matchVelocityPrompt = new ScreenPrompt(InputLibrary.matchVelocity, "<CMD>" + UITextLibrary.GetString(UITextType.HoldPrompt) + "   " + UITextLibrary.GetString(UITextType.MatchVelocityPrompt), 0, false, false);
                    ____readout.gameObject.SetActive(false);
                    ____promptListBlock.Init();
                    Locator.GetPromptManager().AddScreenPrompt(____lockOnPrompt, ____promptListBlock, TextAnchor.MiddleLeft, 20, false);
                    Locator.GetPromptManager().AddScreenPrompt(____matchVelocityPrompt, ____promptListBlock, TextAnchor.MiddleLeft, 20, false);
                    ____initialized = true;
                }

                return false;
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
