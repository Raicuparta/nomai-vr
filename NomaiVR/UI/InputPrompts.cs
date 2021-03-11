using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
                    Postfix<ProbePromptController>("LateInitialize", nameof(RemoveProbePrompts));
                    Postfix<ProbePromptController>("Awake", nameof(ChangeProbePrompts));

                    Postfix<ShipPromptController>("LateInitialize", nameof(RemoveShipPrompts));
                    Postfix<ShipPromptController>("Awake", nameof(ChangeShipPrompts));

                    Postfix<NomaiTranslatorProp>("LateInitialize", nameof(RemoveTranslatorPrompts));

                    Postfix<SignalscopePromptController>("LateInitialize", nameof(RemoveSignalscopePrompts));
                    Postfix<SignalscopePromptController>("Awake", nameof(ChangeSignalscopePrompts));

                    Postfix<PlayerSpawner>("Awake", nameof(RemoveJoystickPrompts));
                    Postfix<RoastingStickController>("LateInitialize", nameof(RemoveRoastingStickPrompts));
                    Postfix<ToolModeUI>("LateInitialize", nameof(RemoveToolModePrompts));
                    Postfix<ScreenPrompt>("SetVisibility", nameof(PostScreenPromptVisibility));

                    Prefix<ScreenPrompt>("Init", nameof(PrePromptInit));
                    Prefix<ScreenPrompt>("SetText", nameof(PrePromptSetText));
                    Postfix<ScreenPromptElement>("BuildTwoCommandScreenPrompt", nameof(PostBuildTwoCommandPromptElement));

                    // Replace Icons with empty version
                    var getButtonTextureMethod = typeof(ButtonPromptLibrary).GetMethod("GetButtonTexture", new[] { typeof(JoystickButton) });
                    Postfix(getButtonTextureMethod, nameof(ReturnEmptyTexture));
                    var getAxisTextureMethods = typeof(ButtonPromptLibrary).GetMethods().Where(method => method.Name == "GetAxisTexture");
                    foreach (var method in getAxisTextureMethods)
                    {
                        Postfix(method, nameof(ReturnEmptyTexture));
                    }

                    // Prevent probe launcher from moving the prompts around.
                    Empty<PromptManager>("OnProbeSnapshot");
                    Empty<PromptManager>("OnProbeSnapshotRemoved");
                    Empty<PromptManager>("OnProbeLauncherEquipped");
                    Empty<PromptManager>("OnProbeLauncherUnequipped");
                    Empty<ScreenPromptElement>("BuildInCommandImage");
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

                private static void AddTextIfNotExisting(string text, HashSet<string> actionTexts, VRActionInput actionInput)
                {
                    var actionInputTexts = actionInput.GetText();
                    foreach (var inputText in actionInputTexts)
                    {
                        if (!text.Contains(inputText))
                        {
                            actionTexts.Add(inputText);
                        }
                    }
                }

                private static void AddVRMappingToPrompt(ref string text, List<InputCommand> commandList)
                {
                    if (ControllerInput.buttonActions == null || ControllerInput.axisActions == null)
                    {
                        return;
                    }

                    var actionTexts = new HashSet<string>();


                    for (var i = 0; i < commandList.Count; i++)
                    {
                        var command = commandList[i];

                        if (command.GetType() == typeof(SingleAxisCommand))
                        {
                            var singleAxisCommand = (SingleAxisCommand)command;
                            var gamepadBinding = singleAxisCommand.GetGamepadBinding();
                            if (gamepadBinding != null)
                            {
                                var button = gamepadBinding.gamepadButtonPos;
                                if (ControllerInput.buttonActions.ContainsKey(button))
                                {
                                    AddTextIfNotExisting(text, actionTexts, ControllerInput.buttonActions[button]);
                                }
                                var axis = gamepadBinding.axisID;
                                if (ControllerInput.axisActions.ContainsKey(axis))
                                {
                                    AddTextIfNotExisting(text, actionTexts, ControllerInput.axisActions[axis]);
                                }
                            }
                        }
                        else if (command.GetType() == typeof(DoubleAxisCommand))
                        {
                            var doubleAxisCommand = (DoubleAxisCommand)command;
                            var axis = doubleAxisCommand.GetGamepadAxis();
                            if (ControllerInput.axisActions.ContainsKey(axis))
                            {
                                AddTextIfNotExisting(text, actionTexts, ControllerInput.axisActions[axis]);
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

                private static void ChangeShipPrompts(ref ScreenPrompt ____exitLandingCamPrompt)
                {
                    ____exitLandingCamPrompt = new ScreenPrompt(InputLibrary.cancel, ____exitLandingCamPrompt.GetText());
                }

                private static void RemoveProbePrompts(ScreenPrompt ____unequipPrompt)
                {
                    _toolUnequipPrompts.Add(____unequipPrompt);
                }

                private static void ChangeProbePrompts(ref ScreenPrompt ____retrievePrompt)
                {
                    ____retrievePrompt = new ScreenPrompt(InputLibrary.probeRetrieve, UITextLibrary.GetString(UITextType.ProbeRetrievePrompt) + "   <CMD>");
                }


                private static void ChangeSignalscopePrompts(ref ScreenPrompt ____zoomModePrompt)
                {
                    ____zoomModePrompt = new ScreenPrompt(InputLibrary.scopeView, UITextLibrary.GetString(UITextType.SignalscopeZoomInPrompt) + "   <CMD>");
                }

                private static void RemoveSignalscopePrompts(
                    ScreenPrompt ____unequipPrompt,
                    ScreenPrompt ____zoomLevelPrompt
                )
                {
                    _toolUnequipPrompts.Add(____unequipPrompt);
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

                private static void RemoveTranslatorPrompts(ScreenPrompt ____unequipPrompt)
                {
                    Manager.RemoveScreenPrompt(____unequipPrompt);
                }
            }
        }
    }
}
