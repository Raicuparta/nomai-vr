using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class InputPrompts : NomaiVRModule<InputPrompts.Behaviour, InputPrompts.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static readonly List<ScreenPrompt> _toolUnequipPrompts = new List<ScreenPrompt>(2);

            private static PromptManager Manager => Locator.GetPromptManager();

            internal void LateUpdate()
            {
                var isInShip = ToolHelper.Swapper.GetToolGroup() == ToolGroup.Ship;
                var isUsingFixedProbeTool = OWInput.IsInputMode(InputMode.StationaryProbeLauncher) || OWInput.IsInputMode(InputMode.SatelliteCam);
                if (!isInShip && !isUsingFixedProbeTool)
                {
                    foreach (var prompt in _toolUnequipPrompts)
                    {
                        prompt.SetVisibility(false);
                    }
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Post<ProbePromptController>("LateInitialize", typeof(Patch), nameof(RemoveProbePrompts));
                    NomaiVR.Post<ProbePromptController>("Awake", typeof(Patch), nameof(ChangeProbePrompts));

                    NomaiVR.Post<ShipPromptController>("LateInitialize", typeof(Patch), nameof(RemoveShipPrompts));
                    NomaiVR.Post<ShipPromptController>("Awake", typeof(Patch), nameof(ChangeShipPrompts));

                    NomaiVR.Post<NomaiTranslatorProp>("LateInitialize", typeof(Patch), nameof(RemoveTranslatorPrompts));
                    NomaiVR.Post<NomaiTranslatorProp>("Awake", typeof(Patch), nameof(ChangeTranslatorPrompts));

                    NomaiVR.Post<SignalscopePromptController>("LateInitialize", typeof(Patch), nameof(RemoveSignalscopePrompts));
                    NomaiVR.Post<SignalscopePromptController>("Awake", typeof(Patch), nameof(ChangeSignalscopePrompts));

                    NomaiVR.Post<SatelliteSnapshotController>("OnPressInteract", typeof(Patch), nameof(RemoveSatellitePrompts));
                    NomaiVR.Post<SatelliteSnapshotController>("Awake", typeof(Patch), nameof(ChangeSatellitePrompts));

                    NomaiVR.Post<PlayerSpawner>("Awake", typeof(Patch), nameof(RemoveJoystickPrompts));
                    NomaiVR.Post<RoastingStickController>("LateInitialize", typeof(Patch), nameof(RemoveRoastingStickPrompts));
                    NomaiVR.Post<ToolModeUI>("LateInitialize", typeof(Patch), nameof(RemoveToolModePrompts));
                    NomaiVR.Post<ScreenPrompt>("SetVisibility", typeof(Patch), nameof(PostScreenPromptVisibility));

                    NomaiVR.Pre<LockOnReticule>("Init", typeof(Patch), nameof(InitLockOnReticule));

                    NomaiVR.Pre<ScreenPrompt>("Init", typeof(Patch), nameof(PrePromptInit));
                    NomaiVR.Pre<ScreenPrompt>("SetText", typeof(Patch), nameof(PrePromptSetText));
                    NomaiVR.Post<ScreenPromptElement>("BuildTwoCommandScreenPrompt", typeof(Patch), nameof(PostBuildTwoCommandPromptElement));

                    // Replace Icons with empty version
                    var getButtonTextureMethod = typeof(ButtonPromptLibrary).GetMethod("GetButtonTexture", new[] { typeof(JoystickButton) });
                    NomaiVR.Helper.HarmonyHelper.AddPostfix(getButtonTextureMethod, typeof(Patch), nameof(ReturnEmptyTexture));
                    var getAxisTextureMethods = typeof(ButtonPromptLibrary).GetMethods().Where(method => method.Name == "GetAxisTexture");
                    foreach (var method in getAxisTextureMethods)
                    {
                        NomaiVR.Helper.HarmonyHelper.AddPostfix(method, typeof(Patch), nameof(ReturnEmptyTexture));
                    }

                    // Prevent probe launcher from moving the prompts around.
                    NomaiVR.Empty<PromptManager>("OnProbeSnapshot");
                    NomaiVR.Empty<PromptManager>("OnProbeSnapshotRemoved");
                    NomaiVR.Empty<PromptManager>("OnProbeLauncherEquipped");
                    NomaiVR.Empty<PromptManager>("OnProbeLauncherUnequipped");
                    NomaiVR.Empty<ScreenPromptElement>("BuildInCommandImage");
                }

                [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unusued parameter is needed for return value passthrough.")]
                private static Texture2D ReturnEmptyTexture(Texture2D _result)
                {
                    return AssetLoader.EmptyTexture;
                }

                [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unusued parameter is needed for return value passthrough.")]
                private static List<string> PostBuildTwoCommandPromptElement(List<string> _result, string promptText)
                {
                    var newText = promptText.Replace("<CMD1>", "").Replace("<CMD2>", "");
                    return new List<string> { newText };
                }

                private static void AddVRMappingToPrompt(ref string text, List<InputCommand> ____commandList)
                {
                    if (ControllerInput.buttonActions == null || ControllerInput.axisActions == null)
                    {
                        return;
                    }
                    var actionTexts = new List<string>();
                    for (var i = 0; i < ____commandList.Count; i++)
                    {
                        var command = ____commandList[i];

                        if (command.GetType() == typeof(SingleAxisCommand))
                        {
                            var singleAxisCommand = (SingleAxisCommand)command;
                            var gamepadBinding = singleAxisCommand.GetGamepadBinding();
                            if (gamepadBinding != null)
                            {
                                var button = gamepadBinding.gamepadButtonPos;
                                if (ControllerInput.buttonActions.ContainsKey(button))
                                {
                                    actionTexts.Add(ControllerInput.buttonActions[button].GetText());
                                }
                                var axis = gamepadBinding.axisID;
                                if (ControllerInput.axisActions.ContainsKey(axis))
                                {
                                    actionTexts.Add(ControllerInput.axisActions[axis].GetText());
                                }
                            }
                        }
                        else if (command.GetType() == typeof(DoubleAxisCommand))
                        {
                            var doubleAxisCommand = (DoubleAxisCommand)command;
                            var axis = doubleAxisCommand.GetGamepadAxis();
                            if (ControllerInput.axisActions.ContainsKey(axis))
                            {
                                actionTexts.Add(ControllerInput.axisActions[axis].GetText());
                            }
                        }
                    }

                    actionTexts.Reverse();
                    var cleanOriginalText = text.Replace("+", "");
                    var actionText = string.Join(" + ", actionTexts.ToArray());
                    text = $"{actionText} {cleanOriginalText}";
                }

                private static void PrePromptSetText(ref string text, List<InputCommand> ____commandList)
                {
                    AddVRMappingToPrompt(ref text, ____commandList);
                }

                private static void PrePromptInit(ref string prompt, List<InputCommand> ____commandList)
                {
                    AddVRMappingToPrompt(ref prompt, ____commandList);
                }

                private static void PostScreenPromptVisibility(bool isVisible)
                {
                    if (isVisible)
                    {
                        MaterialHelper.MakeGraphicChildrenDrawOnTop(Locator.GetPromptManager().gameObject);
                    }
                }

                private static bool InitLockOnReticule(
                    ref ScreenPrompt ____lockOnPrompt,
                    ref bool ____initialized,
                    ref bool ____showFullLockOnPrompt,
                    ref string ____lockOnPromptText,
                    ref string ____lockOnPromptTextShortened,
                    ScreenPromptList ____promptListBlock,
                    ref JetpackPromptController ____jetpackPromptController,
                    ref ScreenPrompt ____matchVelocityPrompt,
                    Text ____readout
                )
                {
                    if (!____initialized)
                    {
                        ____jetpackPromptController = Locator.GetPlayerTransform().GetComponent<JetpackPromptController>();
                        ____lockOnPromptText = "<CMD>" + UITextLibrary.GetString(UITextType.PressPrompt) + "   " + UITextLibrary.GetString(UITextType.LockOnPrompt);
                        ____lockOnPromptTextShortened = "<CMD>";
                        ____showFullLockOnPrompt = !PlayerData.GetPersistentCondition("HAS_PLAYER_LOCKED_ON");
                        if (____showFullLockOnPrompt)
                        {
                            ____lockOnPrompt = new ScreenPrompt(InputLibrary.interact, ____lockOnPromptText, 0, false, false);
                        }
                        else
                        {
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

                private static void ChangeSatellitePrompts(ref ScreenPrompt ____forwardPrompt)
                {
                    ____forwardPrompt = new ScreenPrompt(InputLibrary.interact, ____forwardPrompt.GetText(), 0, false, false);
                }

                private static void RemoveSatellitePrompts(ScreenPrompt ____rearviewPrompt)
                {
                    Manager.RemoveScreenPrompt(____rearviewPrompt);
                }

                private static void RemoveJoystickPrompts(ref bool ____lookPromptAdded)
                {
                    ____lookPromptAdded = true;
                }

                private static void RemoveRoastingStickPrompts(
                    ScreenPrompt ____tiltPrompt,
                    ScreenPrompt ____mallowPrompt
                )
                {
                    Manager.RemoveScreenPrompt(____tiltPrompt);
                    Manager.RemoveScreenPrompt(____mallowPrompt);
                }

                private static void RemoveToolModePrompts(
                    ScreenPrompt ____freeLookPrompt,
                    ScreenPrompt ____probePrompt,
                    ScreenPrompt ____signalscopePrompt,
                    ScreenPrompt ____flashlightPrompt,
                    ScreenPrompt ____centerFlashlightPrompt,
                    ScreenPrompt ____centerTranslatePrompt,
                    ScreenPrompt ____centerProbePrompt,
                    ScreenPrompt ____centerSignalscopePrompt
                )
                {
                    Manager.RemoveScreenPrompt(____freeLookPrompt);
                    Manager.RemoveScreenPrompt(____probePrompt);
                    Manager.RemoveScreenPrompt(____signalscopePrompt);
                    Manager.RemoveScreenPrompt(____flashlightPrompt);
                    Manager.RemoveScreenPrompt(____centerFlashlightPrompt);
                    Manager.RemoveScreenPrompt(____centerTranslatePrompt);
                    Manager.RemoveScreenPrompt(____centerProbePrompt);
                    Manager.RemoveScreenPrompt(____centerSignalscopePrompt);
                }

                private static void ChangeProbePrompts(
                    ref ScreenPrompt ____launchPrompt,
                    ref ScreenPrompt ____retrievePrompt,
                    ref ScreenPrompt ____takeSnapshotPrompt,
                    ref ScreenPrompt ____forwardCamPrompt
                )
                {
                    ____launchPrompt = new ScreenPrompt(InputLibrary.interact, ____launchPrompt.GetText());
                    ____forwardCamPrompt = new ScreenPrompt(InputLibrary.interact, ____takeSnapshotPrompt.GetText());
                    ____retrievePrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, UITextLibrary.GetString(UITextType.ProbeRetrievePrompt) + "   <CMD>");
                    ____takeSnapshotPrompt = new ScreenPrompt(InputLibrary.interact, ____takeSnapshotPrompt.GetText());
                }

                private static void ChangeShipPrompts(
                    ref ScreenPrompt ____exitLandingCamPrompt,
                    ref ScreenPrompt ____autopilotPrompt,
                    ref ScreenPrompt ____abortAutopilotPrompt
                )
                {
                    ____exitLandingCamPrompt = new ScreenPrompt(InputLibrary.cancel, ____exitLandingCamPrompt.GetText());
                    ____autopilotPrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, ____autopilotPrompt.GetText());
                    ____abortAutopilotPrompt = new ScreenPrompt(InputLibrary.interact, ____abortAutopilotPrompt.GetText());
                }

                private static void RemoveProbePrompts(
                    ScreenPrompt ____unequipPrompt,
                    ScreenPrompt ____photoModePrompt,
                    ScreenPrompt ____rotatePrompt,
                    ScreenPrompt ____rotateCenterPrompt,
                    ScreenPrompt ____launchModePrompt
                )
                {
                    _toolUnequipPrompts.Add(____unequipPrompt);
                    Manager.RemoveScreenPrompt(____photoModePrompt);
                    Manager.RemoveScreenPrompt(____rotatePrompt);
                    Manager.RemoveScreenPrompt(____rotateCenterPrompt);
                    Manager.RemoveScreenPrompt(____launchModePrompt);
                }

                private static void ChangeSignalscopePrompts(ref ScreenPrompt ____zoomModePrompt)
                {
                    ____zoomModePrompt = new ScreenPrompt(InputLibrary.interact, UITextLibrary.GetString(UITextType.SignalscopeZoomInPrompt) + "   <CMD>");
                }

                private static void RemoveSignalscopePrompts(
                    ScreenPrompt ____unequipPrompt,
                    ScreenPrompt ____changeFrequencyPrompt,
                    ScreenPrompt ____zoomLevelPrompt
                )
                {
                    _toolUnequipPrompts.Add(____unequipPrompt);
                    Manager.RemoveScreenPrompt(____changeFrequencyPrompt);
                    Manager.RemoveScreenPrompt(____zoomLevelPrompt);
                }

                private static void RemoveShipPrompts(
                    ScreenPrompt ____freeLookPrompt,
                    ScreenPrompt ____landingModePrompt,
                    ScreenPrompt ____liftoffCamera
                )
                {
                    Manager.RemoveScreenPrompt(____freeLookPrompt);
                    Manager.RemoveScreenPrompt(____landingModePrompt);
                    Manager.RemoveScreenPrompt(____liftoffCamera);
                }

                private static void RemoveTranslatorPrompts(
                    ScreenPrompt ____unequipPrompt,
                    ScreenPrompt ____scrollPrompt,
                    ScreenPrompt ____pagePrompt
                )
                {
                    Manager.RemoveScreenPrompt(____unequipPrompt);
                    Manager.RemoveScreenPrompt(____scrollPrompt);
                    Manager.RemoveScreenPrompt(____pagePrompt);
                }

                private static void ChangeTranslatorPrompts(ref ScreenPrompt ____translatePrompt)
                {
                    ____translatePrompt = new ScreenPrompt(InputLibrary.swapShipLogMode, UITextLibrary.GetString(UITextType.TranslatorUsePrompt) + "   <CMD>");
                }
            }
        }
    }
}
