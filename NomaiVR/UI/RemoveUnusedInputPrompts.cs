using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR.UI
{
    internal class RemoveUnusedInputPrompts : NomaiVRModule<RemoveUnusedInputPrompts.Behaviour, RemoveUnusedInputPrompts.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static readonly List<ScreenPrompt> toolUnequipPrompts = new List<ScreenPrompt>(2);

            private static PromptManager Manager => Locator.GetPromptManager();

            internal void LateUpdate()
            {
                var isInShip = ToolHelper.Swapper.GetToolGroup() == ToolGroup.Ship;
                if (!isInShip && !InputHelper.IsStationaryToolMode())
                {
                    //TODO: Maybe a better way than this?
                    foreach (var prompt in toolUnequipPrompts)
                    {
                        prompt.SetVisibility(false);
                    }
                }
            }

            internal void OnDestroy()
            {
                toolUnequipPrompts.Clear();
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Postfix<ProbePromptController>(nameof(ProbePromptController.LateInitialize), nameof(RemoveProbePrompts));

                    Postfix<ShipPromptController>(nameof(ShipPromptController.LateInitialize), nameof(RemoveShipPrompts));
                    Postfix<ShipPromptController>(nameof(ShipPromptController.Awake), nameof(ChangeShipPrompts));

                    Postfix<NomaiTranslatorProp>(nameof(NomaiTranslatorProp.LateInitialize), nameof(RemoveTranslatorPrompts));

                    Postfix<SignalscopePromptController>(nameof(SignalscopePromptController.LateInitialize), nameof(RemoveSignalscopePrompts));
                    Postfix<SignalscopePromptController>(nameof(SignalscopePromptController.Awake), nameof(ChangeSignalscopePrompts));

                    Postfix<PlayerSpawner>(nameof(PlayerSpawner.Awake), nameof(RemoveJoystickPrompts));
                    Postfix<RoastingStickController>(nameof(RoastingStickController.LateInitialize), nameof(RemoveRoastingStickPrompts));
                    Postfix<ToolModeUI>(nameof(ToolModeUI.LateInitialize), nameof(RemoveToolModePrompts));
                    
                    Postfix<ScreenPromptElement>(nameof(ScreenPromptElement.BuildScreenPrompt), nameof(PostScreenPromptVisibility)); // TODO move this elswhere.

                    // Prevent probe launcher from moving the prompts around.
                    Empty<PromptManager>(nameof(PromptManager.OnProbeSnapshot));
                    Empty<PromptManager>(nameof(PromptManager.OnProbeSnapshotRemoved));
                    Empty<PromptManager>(nameof(PromptManager.OnProbeLauncherEquipped));
                    Empty<PromptManager>(nameof(PromptManager.OnProbeLauncherUnequipped));
                }

                private static void PostScreenPromptVisibility(ScreenPromptElement __instance)
                {
                    MaterialHelper.MakeGraphicChildrenDrawOnTop(__instance.gameObject);
                }

                private static void RemoveJoystickPrompts(PlayerSpawner __instance)
                {
                    __instance._lookPromptAdded = true;
                }

                private static void RemoveRoastingStickPrompts(RoastingStickController __instance)
                {
                    Manager.RemoveScreenPrompt(__instance._tiltPrompt);
                    Manager.RemoveScreenPrompt(__instance._mallowPrompt);
                }

                private static void RemoveToolModePrompts(ToolModeUI __instance)
                {
                    // Manager.RemoveScreenPrompt(__instance._freeLookPrompt);
                    Manager.RemoveScreenPrompt(__instance._probePrompt);
                    Manager.RemoveScreenPrompt(__instance._signalscopePrompt);
                    Manager.RemoveScreenPrompt(__instance._flashlightPrompt);
                    Manager.RemoveScreenPrompt(__instance._centerFlashlightPrompt);
                    Manager.RemoveScreenPrompt(__instance._centerTranslatePrompt);
                    Manager.RemoveScreenPrompt(__instance._centerProbePrompt);
                    Manager.RemoveScreenPrompt(__instance._centerSignalscopePrompt);
                }

                private static void ChangeShipPrompts(ShipPromptController __instance)
                {
                    __instance._exitLandingCamPrompt = new ScreenPrompt(InputLibrary.cancel, __instance._exitLandingCamPrompt.GetText());
                }

                private static void RemoveProbePrompts(ProbePromptController __instance)
                {
                    toolUnequipPrompts.Add(__instance._unequipPrompt);
                }

                private static void ChangeSignalscopePrompts(SignalscopePromptController __instance)
                {
                    __instance._zoomModePrompt = new ScreenPrompt(InputLibrary.signalscope, UITextLibrary.GetString(UITextType.SignalscopeZoomInPrompt) + "   <CMD>");
                }

                private static void RemoveSignalscopePrompts(SignalscopePromptController __instance)
                {
                    toolUnequipPrompts.Add(__instance._unequipPrompt);
                    Manager.RemoveScreenPrompt(__instance._zoomLevelPrompt);
                }

                private static void RemoveShipPrompts(ShipPromptController __instance)
                {
                    Manager.RemoveScreenPrompt(__instance._freeLookPrompt);
                    Manager.RemoveScreenPrompt(__instance._landingModePrompt);
                    Manager.RemoveScreenPrompt(__instance._liftoffCamera);
                    Manager.RemoveScreenPrompt(__instance._autopilotPrompt);
                    Manager.RemoveScreenPrompt(__instance._abortAutopilotPrompt);
                }

                private static void RemoveTranslatorPrompts(NomaiTranslatorProp __instance)
                {
                    Manager.RemoveScreenPrompt(__instance._unequipPrompt);
                }
            }
        }
    }
}
