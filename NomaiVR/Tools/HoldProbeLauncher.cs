using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    public class HoldProbeLauncher : MonoBehaviour
    {
        Transform _probeLauncherModel;
        GameObject _probeLauncherHolster;
        static ProbeLauncherUI _probeUI;

        void Awake()
        {
            var probeLauncher = Camera.main.transform.Find("ProbeLauncher");
            probeLauncher.localScale = Vector3.one * 0.35f;

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
            holster.hand = HandsController.RightHand;
            holster.position = new Vector3(0, -0.55f, 0.2f);
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

            probeLauncher.gameObject.AddComponent<ToolModeInteraction>();

            GlobalMessenger.AddListener("SuitUp", OnSuitUp);
            GlobalMessenger.AddListener("RemoveSuit", OnRemoveSuit);
        }

        void OnSuitUp()
        {
            _probeLauncherHolster.SetActive(true);
        }

        void OnRemoveSuit()
        {
            _probeLauncherHolster.SetActive(false);
        }

        internal static class Patches
        {
            public static void Patch()
            {
                NomaiVR.Pre<PlayerSpacesuit>("SuitUp", typeof(Patches), nameof(Patches.SuitUp));
                NomaiVR.Pre<PlayerSpacesuit>("RemoveSuit", typeof(Patches), nameof(Patches.RemoveSuit));
                NomaiVR.Post<ProbeLauncherUI>("HideProbeHUD", typeof(Patches), nameof(Patches.PostHideHUD));
            }

            static void PostHideHUD(Canvas ____canvas)
            {
                // Prevent the photo mode bracket from disappearing.
                if (____canvas != null)
                {
                    ____canvas.enabled = true;
                }
            }

            static void SuitUp()
            {
                _probeUI.SetValue("_nonSuitUI", false);
            }

            static void RemoveSuit()
            {
                _probeUI.SetValue("_nonSuitUI", true);
            }
        }
    }
}
