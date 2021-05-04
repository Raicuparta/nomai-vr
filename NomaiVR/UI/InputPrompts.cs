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
            private static readonly List<ScreenPrompt> s_toolUnequipPrompts = new List<ScreenPrompt>(2);
            private static readonly HashSet<ScreenPrompt> s_vrActionPrompts = new HashSet<ScreenPrompt>();
            private static readonly Dictionary<VRActionInput, HashSet<ScreenPrompt>> s_vrActionPromptDependencies = new Dictionary<VRActionInput, HashSet<ScreenPrompt>>();
            private static readonly Dictionary<ScreenPrompt, string> s_vrActionProptLastText = new Dictionary<ScreenPrompt, string>();

            private static PromptManager Manager => Locator.GetPromptManager();

            internal void LateUpdate()
            {
                var isInShip = ToolHelper.Swapper.GetToolGroup() == ToolGroup.Ship;
                if (!isInShip && !InputHelper.IsStationaryToolMode())
                {
                    //TODO: Maybe a better way than this?
                    foreach (var prompt in s_toolUnequipPrompts)
                    {
                        prompt.SetVisibility(false);
                    }
                }
            }

            internal void OnDestroy()
            {
                s_toolUnequipPrompts.Clear();
                s_vrActionPrompts.Clear();
                s_vrActionPromptDependencies.Clear();
                s_vrActionProptLastText.Clear();
            }

            public static void UpdatePrompts(VRActionInput[] actionsToUpdate)
            {
                foreach(var action in actionsToUpdate)
                {
                    if(s_vrActionPromptDependencies.ContainsKey(action))
                    {
                        foreach (ScreenPrompt prompt in s_vrActionPromptDependencies[action])
                            prompt.SetText(s_vrActionProptLastText[prompt]);
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
                private static Texture2D ReturnEmptyTexture(Texture2D __result)
                {
                    return AssetLoader.EmptyTexture;
                }

                [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Unusued parameter is needed for return value passthrough.")]
                private static List<string> PostBuildTwoCommandPromptElement(List<string> __result, string promptText)
                {
                    var newText = promptText.Replace("<CMD1>", "").Replace("<CMD2>", "");
                    return new List<string> { newText };
                }

                private static VRActionInput GetActionInputFromCommand(InputCommand command)
                {
                    if (command is SingleAxisCommand singleAxisCommand)
                    {
                        var gamepadBinding = singleAxisCommand.GetGamepadBinding();
                        if (gamepadBinding != null)
                        {
                            var button = gamepadBinding.gamepadButtonPos;
                            if (ControllerInput.buttonActions.ContainsKey(button))
                                return ControllerInput.buttonActions[button];

                            var axis = gamepadBinding.axisID;
                            if (ControllerInput.axisActions.ContainsKey(axis))
                                return ControllerInput.axisActions[axis];
                        }
                    }
                    else if (command.GetType() == typeof(DoubleAxisCommand))
                    {
                        var doubleAxisCommand = (DoubleAxisCommand)command;
                        var axis = doubleAxisCommand.GetGamepadAxis();
                        if (ControllerInput.axisActions.ContainsKey(axis))
                            return ControllerInput.axisActions[axis];
                    }
                    return null;
                }

                private static void RegisterVRActionPrompt(ScreenPrompt screenPrompt)
                {
                    if (ControllerInput.buttonActions == null || ControllerInput.axisActions == null)
                    {
                        return;
                    }

                    foreach (var command in screenPrompt._commandList)
                    {
                        var actionInput = GetActionInputFromCommand(command);
                        //Keep track of prompts tied to VRActionInputs
                        if (actionInput != null)
                        {
                            if (!s_vrActionPrompts.Contains(screenPrompt)) s_vrActionPrompts.Add(screenPrompt);

                            if (!s_vrActionPromptDependencies.ContainsKey(actionInput))
                                s_vrActionPromptDependencies.Add(actionInput, new HashSet<ScreenPrompt>());
                            s_vrActionPromptDependencies[actionInput].Add(screenPrompt);
                        }
                    }
                }

                private static void AddTextIfNotExisting(string text, HashSet<string> actionTexts, VRActionInput actionInput, ScreenPrompt screenPrompt)
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

                private static void AddVRMappingToPrompt(ref string text, ScreenPrompt screenPrompt)
                {
                    if (ControllerInput.buttonActions == null || ControllerInput.axisActions == null)
                    {
                        return;
                    }

                    var actionTexts = new HashSet<string>();

                    foreach (var command in screenPrompt._commandList)
                    {
                        var actionInput = GetActionInputFromCommand(command);
                        if (actionInput != null) AddTextIfNotExisting(text, actionTexts, actionInput, screenPrompt);
                    }

                    actionTexts.Reverse();
                    var cleanOriginalText = text.Replace("+", "");
                    var actionText = string.Join(" + ", actionTexts.ToArray());
                    text = $"{actionText} {cleanOriginalText}";
                }

                private static void PrePromptSetText(ref string text, ScreenPrompt __instance)
                {
                    if (s_vrActionPrompts.Contains(__instance))
                        s_vrActionProptLastText[__instance] = text;

                    AddVRMappingToPrompt(ref text, __instance); //Only changes the string we pass to the PromptManager
                }

                private static void PrePromptInit(ref string prompt, ScreenPrompt __instance)
                {
                    RegisterVRActionPrompt(__instance);

                    if (s_vrActionPrompts.Contains(__instance))
                        s_vrActionProptLastText[__instance] = prompt;

                    AddVRMappingToPrompt(ref prompt, __instance);
                }

                private static void PostScreenPromptVisibility(bool isVisible, ScreenPrompt __instance)
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
                    s_toolUnequipPrompts.Add(____unequipPrompt);
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
                    s_toolUnequipPrompts.Add(____unequipPrompt);
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
