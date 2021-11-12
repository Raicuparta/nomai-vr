
using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.ReusableBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.Tools
{
    internal class HoldProbeLauncher : NomaiVRModule<HoldProbeLauncher.Behaviour, HoldProbeLauncher.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform probeLauncherModel;
            private GameObject probeLauncherHolster;
            private static ProbeLauncherUI probeUI;

            internal void Start()
            {
                var probeLauncher = Camera.main.transform.Find("ProbeLauncher");
                probeLauncher.localScale = Vector3.one * 0.3f;

                var holdProbeLauncher = probeLauncher.gameObject.AddComponent<Holdable>();
                holdProbeLauncher.SetPositionOffset(new Vector3(-0.0014f, 0.2272f, -0.0593f));
                holdProbeLauncher.SetPoses("grabbing_probelauncher", "grabbing_probelauncher_gloves");
                holdProbeLauncher.CanFlipX = false;

                probeLauncherModel = probeLauncher.Find("Props_HEA_ProbeLauncher");
                probeLauncherModel.gameObject.layer = 0;
                probeLauncherModel.localPosition = Vector3.zero;
                probeLauncherModel.localRotation = Quaternion.identity;

                probeLauncherModel.Find("Props_HEA_ProbeLauncher_Prepass").gameObject.SetActive(false);
                probeLauncherModel.Find("Props_HEA_Probe_Prelaunch/Props_HEA_Probe_Prelaunch_Prepass").gameObject.SetActive(false);

                var renderers = probeLauncher.gameObject.GetComponentsInChildren<MeshRenderer>(true);

                foreach (var renderer in renderers)
                {
                    if (renderer.name == "RecallEffect")
                    {
                        renderer.GetComponent<SingularityController>()._targetRadius = renderer.sharedMaterial.GetFloat("_Radius") * 0.2f;
                        continue;
                    }
                    foreach (var material in renderer.materials)
                    {
                        material.shader = Shader.Find("Standard");
                    }
                }

                // This one is used only for rendering the probe launcher to the screen in pancake mode,
                // so we can remove it.
                probeLauncher.Find("Props_HEA_ProbeLauncher_ProbeCamera").gameObject.SetActive(false);

                // This transform defines the origin and direction of the launched probe.
                var launchOrigin = Camera.main.transform.Find("ProbeLauncherTransform").transform;
                launchOrigin.parent = probeLauncherModel;
                launchOrigin.localPosition = Vector3.forward * 0.2f;
                launchOrigin.localRotation = Quaternion.identity;

                // Create and adjust hip holster model.
                probeLauncherHolster = Instantiate(probeLauncherModel).gameObject;
                probeLauncherHolster.SetActive(false);
                var holster = probeLauncherHolster.AddComponent<HolsterTool>();
                holster.position = new Vector3(0, 0, 0.2f);
                holster.mode = ToolMode.Probe;
                holster.scale = 0.15f;
                holster.angle = Vector3.right * 90;

                // Move probe picture to probe launcher.
                var playerHUD = GameObject.Find("PlayerHUD").transform;
                var display = playerHUD.Find("HelmetOffUI/ProbeDisplay");
                display.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                display.parent = probeLauncherModel;
                display.localScale = Vector3.one * 0.0011f;
                display.localRotation = Quaternion.identity;
                display.localPosition = Vector3.forward * -0.8f;
                probeUI = display.GetComponent<ProbeLauncherUI>();

                var uiCanvas = playerHUD.Find("HelmetOnUI/UICanvas");
                uiCanvas.Find("HUDProbeDisplay/Image").gameObject.SetActive(false);

                var hudProbeDisplay = uiCanvas.Find("HUDProbeDisplay");
                hudProbeDisplay.parent = display;
                hudProbeDisplay.localPosition = Vector3.zero;
                hudProbeDisplay.localRotation = Quaternion.identity;

                // Adjust probe picture position.
                var displayImage = display.GetChild(0).GetComponent<RectTransform>();
                displayImage.anchorMin = Vector2.one * 0.5f;
                displayImage.anchorMax = Vector2.one * 0.5f;
                displayImage.pivot = Vector2.one * 0.5f;
                displayImage.localPosition = Vector3.zero;
                displayImage.localRotation = Quaternion.identity;

                // Move photo mode bracket to probe launcher.
                var bracketImage = uiCanvas.Find("BracketImage");
                bracketImage.transform.parent = display;
                bracketImage.localPosition = Vector3.zero;
                bracketImage.localRotation = Quaternion.identity;
                bracketImage.localScale *= 0.4f;

                var probeLauncherScreen = Instantiate(AssetLoader.ProbeLauncherHandheldScreenPrefab).transform;
                probeLauncherScreen.parent = probeLauncherModel;
                probeLauncherScreen.localPosition = Vector3.zero;
                probeLauncherScreen.localScale = Vector3.one;
                probeLauncherScreen.localRotation = Quaternion.identity;
                var probeLauncherButtons = probeLauncherScreen.Find("Buttons");
                foreach (Transform child in probeLauncherButtons)
                {
                    var touchButton = child.gameObject.AddComponent<TouchButton>();
                    if (child.name == "Camera")
                        touchButton.CheckEnabled = () => probeUI._probeLauncher.GetActiveProbe() == null;
                    else if (child.name != "Shoot")
                        touchButton.CheckEnabled = () => probeUI._probeLauncher.GetActiveProbe() != null;
                }

                LayerHelper.ChangeLayerRecursive(probeLauncher.gameObject, "VisibleToPlayer");

                GlobalMessenger.AddListener("SuitUp", OnSuitUp);
                GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
            }

            internal void OnDestroy()
            {
                GlobalMessenger.RemoveListener("SuitUp", OnSuitUp);
                GlobalMessenger.RemoveListener("RemoveSuit", OnRemoveSuit);
            }

            private void OnSuitUp()
            {
                if (probeLauncherHolster)
                {
                    probeLauncherHolster.SetActive(true);
                }
            }

            private void OnRemoveSuit()
            {
                if (probeLauncherHolster)
                {
                    probeLauncherHolster.SetActive(false);
                }
            }

            public class Patch : NomaiVRPatch
            {
                private static ToolMode? currentToolMode;
                private static bool isWearingSuit;

                public override void ApplyPatches()
                {
                    Prefix<PlayerSpacesuit>("SuitUp", nameof(SuitUp));
                    Prefix<PlayerSpacesuit>("RemoveSuit", nameof(RemoveSuit));
                    Postfix<ProbeLauncherUI>("HideProbeHUD", nameof(PostHideHUD));

                    // Prevent probe prompt zones from equipping / unequipping the probe launcher.
                    Prefix<ProbePromptReceiver>("LoseFocus", nameof(PreLoseFocus));
                    Postfix<ProbePromptReceiver>("LoseFocus", nameof(PostLoseFocus));
                    Prefix<ProbePromptReceiver>("GainFocus", nameof(PreGainFocus));
                    Postfix<ProbePromptReceiver>("GainFocus", nameof(PostGainFocus));
                }

                private static void PreLoseFocus()
                {
                    ToolMode? toolMode = ToolHelper.Swapper._currentToolMode;
                    if (toolMode == null)
                    {
                        return;
                    }
                    currentToolMode = toolMode;
                    ToolHelper.Swapper.SetValue("_currentToolMode", null);
                }

                private static void PostLoseFocus()
                {
                    if (currentToolMode == null)
                    {
                        return;
                    }
                    ToolHelper.Swapper._currentToolMode = (ToolMode)currentToolMode;
                }

                private static void PreGainFocus()
                {
                    isWearingSuit = Locator.GetPlayerSuit()._isWearingSuit;
                    Locator.GetPlayerSuit()._isWearingSuit = false;
                }

                private static void PostGainFocus()
                {
                    Locator.GetPlayerSuit()._isWearingSuit = isWearingSuit;
                }

                private static void PostHideHUD(Canvas ____canvas)
                {
                    // Prevent the photo mode bracket from disappearing.
                    if (____canvas != null)
                    {
                        ____canvas.enabled = true;
                    }
                }

                private static void SuitUp()
                {
                    probeUI._nonSuitUI = false;
                }

                private static void RemoveSuit()
                {
                    probeUI._nonSuitUI = true;
                }
            }
        }
    }
}