using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class HoldProbeLauncher: MonoBehaviour {
        protected static Transform ProbeLauncherModel;
        protected static ProbeLauncherUI ProbeUI;

        void Awake () {
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlayerSpacesuit>("SuitUp", typeof(Patches), "SuitUp");
            NomaiVR.Helper.HarmonyHelper.AddPrefix<PlayerSpacesuit>("RemoveSuit", typeof(Patches), "RemoveSuit");

            var probeLauncher = Common.MainCamera.transform.Find("ProbeLauncher");
            probeLauncher.localScale = Vector3.one * 0.2f;
            Hands.HoldObject(probeLauncher, Hands.RightHand, new Vector3(-0.04f, 0.09f, 0.03f), Quaternion.Euler(45, 0, 0));

            ProbeLauncherModel = probeLauncher.Find("Props_HEA_ProbeLauncher");
            ProbeLauncherModel.gameObject.layer = 0;
            ProbeLauncherModel.localPosition = Vector3.zero;
            ProbeLauncherModel.localRotation = Quaternion.identity;

            ProbeLauncherModel.Find("Props_HEA_ProbeLauncher_Prepass").gameObject.SetActive(false);
            ProbeLauncherModel.Find("Props_HEA_Probe_Prelaunch/Props_HEA_Probe_Prelaunch_Prepass").gameObject.SetActive(false);

            var renderers = probeLauncher.gameObject.GetComponentsInChildren<MeshRenderer>(true);

            foreach (var renderer in renderers) {
                if (renderer.name == "RecallEffect") {
                    continue;
                }
                foreach (var material in renderer.materials) {
                    material.shader = Shader.Find("Standard");
                }
            }

            // This one is used only for rendering the probe launcher to the screen in pancake mode,
            // so we can remove it.
            probeLauncher.Find("Props_HEA_ProbeLauncher_ProbeCamera").gameObject.SetActive(false);

            // This transform defines the origin and direction of the launched probe.
            var launchOrigin = Common.MainCamera.transform.Find("ProbeLauncherTransform").transform;
            launchOrigin.parent = ProbeLauncherModel;
            launchOrigin.localPosition = Vector3.forward * 0.2f;
            launchOrigin.localRotation = Quaternion.identity;

            var probeLauncherHolster = Instantiate(ProbeLauncherModel).gameObject;
            probeLauncherHolster.SetActive(true);
            var holster = probeLauncherHolster.AddComponent<HolsterTool>();
            holster.hand = Hands.RightHand;
            holster.position = new Vector3(0, 0.35f, 0.2f);
            holster.mode = ToolMode.Probe;
            holster.scale = 0.15f;
            holster.angle = Vector3.right * 90;

            var playerHUD = GameObject.Find("PlayerHUD").transform;
            var display = playerHUD.Find("HelmetOffUI/ProbeDisplay");
            display.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            display.parent = ProbeLauncherModel;
            display.localScale = Vector3.one * 0.0012f;
            display.localRotation = Quaternion.identity;
            display.localPosition = Vector3.forward * -0.67f;
            ProbeUI = display.GetComponent<ProbeLauncherUI>();

            var displayImage = display.GetChild(0).GetComponent<RectTransform>();
            displayImage.anchorMin = Vector2.one * 0.5f;
            displayImage.anchorMax = Vector2.one * 0.5f;
            displayImage.pivot = Vector2.one * 0.5f;
            displayImage.localPosition = Vector3.zero;
            displayImage.localRotation = Quaternion.identity;

            playerHUD.Find("HelmetOnUI/UICanvas/HUDProbeDisplay/Image").gameObject.SetActive(false);
        }

        void Update () {
            var probe = Locator.GetProbe().transform.Find("CameraPivot");
            probe.rotation = ProbeLauncherModel.rotation;
            probe.Rotate(Vector3.right * 90);
        }

        internal static class Patches {
            static void SuitUp () {
                ProbeUI.SetValue("_nonSuitUI", false);
            }

            static void RemoveSuit () {
                ProbeUI.SetValue("_nonSuitUI", true);
            }
        }
    }
}
