using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NomaiVR
{
    internal class DreamLanternFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, DreamLanternFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        

        public class Patch : NomaiVRPatch
        {
            static Shader prepassShader = Shader.Find("Outer Wilds/Utility/View Model Prepass");
            static Shader standardVM = Shader.Find("Outer Wilds/Utility/View Model");
            static Shader standard = Shader.Find("Standard");
            static Shader standardTranslucent = Shader.Find("Standard (Translucent)");
            static Shader flameVM = Shader.Find("Outer Wilds/Effects/Flame (View Model)");
            static Shader flame = Shader.Find("Outer Wilds/Effects/Flame"); 

            static Dictionary<Shader, Shader> s_dreamShaderFix = new Dictionary<Shader, Shader>()
            {
                [standardVM] = standard,
                [flameVM] = flame
            };

            public override void ApplyPatches()
            {
                Postfix<DreamLanternItem>(nameof(DreamLanternItem.Start), nameof(FixupModel));
                Postfix<DreamLanternItem>(nameof(DreamLanternItem.CheckIsDroppable), nameof(FixDropBehaviourInDream));
                Prefix<ToolModeSwapper>(nameof(ToolModeSwapper.Update), nameof(PreItemToolSwapperUpdate));
                Prefix<ItemTool>(nameof(ItemTool.UpdateInteract), nameof(PreItemToolUpdateInteract));
                Postfix<ToolModeSwapper>(nameof(ToolModeSwapper.Update), nameof(AllowLanternDropWhenFocusing));
            }

            public static void FixupModel(DreamLanternItem __instance)
            {
                //Fix rotations
                var viewModelTransform = __instance.transform.Find("Props_IP_Artifact_ViewModel") ?? __instance.transform.Find("ViewModel/Props_IP_Artifact_ViewModel");
                if(viewModelTransform != null)
                {
                    viewModelTransform.transform.localRotation = Quaternion.identity;
                    viewModelTransform.transform.localPosition = new Vector3(0, 0.35f, 0);
                }

                var prortotypeViewModelTransform = __instance.transform.Find("ViewModel/Props_IP_DreamLanternItem_Malfunctioning (1)");
                if (prortotypeViewModelTransform != null)
                {
                    prortotypeViewModelTransform.transform.localRotation = Quaternion.identity;
                    prortotypeViewModelTransform.transform.localPosition = Vector3.zero;
                }

                //Fix materials
                MaterialHelper.DisableRenderersWithShaderInChildren(__instance.gameObject, prepassShader);

                //Replace all the others
                MaterialHelper.ReplaceShadersInChildren(__instance.gameObject, s_dreamShaderFix);

                //Change the glass material to be translucent again
                var glassElement = viewModelTransform?.Find("artifact_glass");
                if (glassElement != null)
                {
                    var renderer = glassElement.GetComponent<Renderer>();
                    renderer.material.shader = standardTranslucent;
                }
            }

            public static void FixDropBehaviourInDream(DreamLanternItem __instance, ref bool __result)
            {
                var originalCheck = __instance._lanternController != null && (__instance._lanternController.IsFocused(0.1f) || __instance._lanternController.IsConcealed());
                if (!__result && !originalCheck)
                {
                    bool flag1 = Vector3.SignedAngle(Locator.GetPlayerTransform().forward, __instance.transform.forward, Locator.GetPlayerTransform().forward) < -70f;
                    bool flag2 = Locator.GetPlayerController().GetRelativeGroundVelocity().sqrMagnitude < Locator.GetPlayerController().GetWalkSpeedMagnitude() * Locator.GetPlayerController().GetWalkSpeedMagnitude();
                    __result = flag1 && flag2;
                }
            }

            private static bool _itemToolUpdateInteractCalled = false;
            public static void PreItemToolSwapperUpdate() => _itemToolUpdateInteractCalled = false;
            public static void PreItemToolUpdateInteract() => _itemToolUpdateInteractCalled = true;
            public static void AllowLanternDropWhenFocusing(ToolModeSwapper __instance)
            {
                if (_itemToolUpdateInteractCalled) return; //Avoid calling UpdateInteract twice when unsocketing the lantern

                if (__instance._shipDestroyed && __instance._currentToolGroup == ToolGroup.Ship)
                {
                    return;
                }
                if (__instance._currentToolMode == ToolMode.Item 
                            && OWInput.IsNewlyPressed(InputLibrary.probeLaunch, InputMode.Character | InputMode.ShipCockpit) 
                            && __instance._currentToolGroup == ToolGroup.Suit 
                            && __instance._itemCarryTool.GetHeldItemType() == ItemType.DreamLantern)
                {
                    __instance._itemCarryTool.UpdateInteract(__instance._firstPersonManipulator, __instance.IsItemToolBlocked());
                }
            }
        }
    }
}
