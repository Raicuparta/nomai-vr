using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    internal class HoldProbeLauncher : NomaiVRModule<HoldProbeLauncher.Behaviour, HoldProbeLauncher.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private Transform _probeLauncherModel;
            private GameObject _probeLauncherHolster;
            private static ProbeLauncherUI _probeUI;

            internal void Start()
            {
                var probeLauncher = Camera.main.transform.Find("ProbeLauncher");
                probeLauncher.localScale = Vector3.one * 0.3f;

                var holdProbeLauncher = probeLauncher.gameObject.AddComponent<Holdable>();
                holdProbeLauncher.transform.localPosition = new Vector3(0f, 0.21f, 0.05f);
                holdProbeLauncher.transform.localRotation = Quaternion.Euler(45, 0, 0);

                _probeLauncherModel = probeLauncher.Find("Props_HEA_ProbeLauncher");
                _probeLauncherModel.gameObject.layer = 0;
                _probeLauncherModel.localPosition = Vector3.zero;
                _probeLauncherModel.localRotation = Quaternion.identity;

                _probeLauncherModel.Find("Props_HEA_ProbeLauncher_Prepass").gameObject.SetActive(false);
                _probeLauncherModel.Find("Props_HEA_Probe_Prelaunch/Props_HEA_Probe_Prelaunch_Prepass").gameObject.SetActive(false);

                var renderers = probeLauncher.gameObject.GetComponentsInChildren<MeshRenderer>(true);

                foreach (var renderer in renderers)
                {
                    if (renderer.name == "RecallEffect")
                    {
                        renderer.GetComponent<SingularityController>().SetValue("_targetRadius", renderer.sharedMaterial.GetFloat("_Radius") * 0.2f);
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
                launchOrigin.parent = _probeLauncherModel;
                launchOrigin.localPosition = Vector3.forward * 0.2f;
                launchOrigin.localRotation = Quaternion.identity;

                // Create and adjust hip holster model.
                _probeLauncherHolster = Instantiate(_probeLauncherModel).gameObject;
                _probeLauncherHolster.SetActive(false);
                var holster = _probeLauncherHolster.AddComponent<HolsterTool>();
                holster.hand = HandsController.Behaviour.RightHand;
                holster.position = new Vector3(0, 0, 0.2f);
                holster.mode = ToolMode.Probe;
                holster.scale = 0.15f;
                holster.angle = Vector3.right * 90;

                // Move probe picture to probe launcher.
                var playerHUD = GameObject.Find("PlayerHUD").transform;
                var display = playerHUD.Find("HelmetOffUI/ProbeDisplay");
                display.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                display.parent = _probeLauncherModel;
                display.localScale = Vector3.one * 0.0014f;
                display.localRotation = Quaternion.identity;
                display.localPosition = Vector3.forward * -0.8f;
                _probeUI = display.GetComponent<ProbeLauncherUI>();

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
                bracketImage.localScale *= 0.5f;

                GlobalMessenger.AddListener("SuitUp", OnSuitUp);
                GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
            }

            private void OnSuitUp()
            {
                if (_probeLauncherHolster)
                {
                    _probeLauncherHolster.SetActive(true);
                }
            }

            private void OnRemoveSuit()
            {
                if (_probeLauncherHolster)
                {
                    _probeLauncherHolster.SetActive(false);
                }
            }

            public class Patch : NomaiVRPatch
            {
                private static ToolMode? currentToolMode = null;
                private static bool isWearingSuit = false;

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
                    ToolMode? toolMode = ToolHelper.Swapper.GetValue<ToolMode>("_currentToolMode");
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
                    ToolHelper.Swapper.SetValue("_currentToolMode", currentToolMode);
                }

                private static void PreGainFocus()
                {
                    isWearingSuit = Locator.GetPlayerSuit().GetValue<bool>("_isWearingSuit");
                    Locator.GetPlayerSuit().SetValue("_isWearingSuit", false);
                }

                private static void PostGainFocus()
                {
                    Locator.GetPlayerSuit().SetValue("_isWearingSuit", isWearingSuit);
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
                    _probeUI.SetValue("_nonSuitUI", false);
                }

                private static void RemoveSuit()
                {
                    _probeUI.SetValue("_nonSuitUI", true);
                }
            }
        }
    }
}