
using NomaiVR.Assets;
using NomaiVR.EffectFixes;
using NomaiVR.Helpers;
using NomaiVR.ReusableBehaviours;
using UnityEngine;

namespace NomaiVR.Tools
{
    internal class HoldSignalscope : NomaiVRModule<HoldSignalscope.Behaviour, HoldSignalscope.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            protected static Transform Reticule;
            protected static Transform ShipWindshield;
            protected static Signalscope Signalscope;
            private static Camera lensCamera;
            private static Transform lens;

            private OWCamera owLensCamera;

            internal void Start()
            {
                if (LoadManager.GetCurrentScene() == OWScene.SolarSystem)
                {
                    ShipWindshield = GameObject.Find("ShipLODTrigger_Cockpit").transform;
                }

                Signalscope = Camera.main.transform.Find("Signalscope").GetComponent<Signalscope>();

                var holdSignalscope = Signalscope.gameObject.AddComponent<Holdable>();
                holdSignalscope.SetPositionOffset(new Vector3(0.0074f, 0.0808f, 0.1343f), new Vector3(0.0046f, 0.1255f, 0.1625f));

                var signalScopeModel = Signalscope.transform.GetChild(0);
                // Tools have a special shader that draws them on top of everything
                // and screws with perspective. Changing to Standard shader so they look
                // like a normal 3D object.
                signalScopeModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
                signalScopeModel.localPosition = Vector3.zero;
                signalScopeModel.localRotation = Quaternion.identity;

                // This child seems to be only for some kind of shader effect.
                // Disabling it since it looks glitchy and doesn't seem necessary.
                signalScopeModel.GetChild(0).gameObject.SetActive(false);

                var signalScopeHolster = Instantiate(signalScopeModel).gameObject;
                signalScopeHolster.SetActive(true);
                var holster = signalScopeHolster.AddComponent<HolsterTool>();
                holster.position = new Vector3(0.3f, 0, 0);
                holster.mode = ToolMode.SignalScope;
                holster.scale = 0.8f;
                holster.angle = Vector3.right * 90;
                holster.ONUnequip = OnUnequip;

                var playerHUD = GameObject.Find("PlayerHUD").transform;
                Reticule = playerHUD.Find("HelmetOffUI/SignalscopeReticule");

                // Attatch Signalscope UI to the Signalscope.
                Reticule.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                SetupReticule(Reticule);

                var helmetOff = playerHUD.Find("HelmetOffUI/SignalscopeCanvas");
                SetupSignalscopeUI(helmetOff, new Vector3(-0.05f, 0.1714f, 0));

                var helmetOn = playerHUD.Find("HelmetOnUI/UICanvas/SigScopeDisplay");
                SetupSignalscopeUI(helmetOn, new Vector3(-0.05f, -0.05f, 0));
                LayerHelper.ChangeLayerRecursive(helmetOn.gameObject, "UI");
                SetupScopeLens();
                SetupButtons(signalScopeModel);
            }

            private static void SetupReticule(Transform reticule)
            {
                reticule.parent = Signalscope.transform;
                reticule.localScale = Vector3.one * 0.0003f;
                reticule.localPosition = new Vector3(0, 0.125f, 0.14f);
                reticule.localRotation = Quaternion.identity;
            }

            private static void SetupSignalscopeUI(Transform parent, Vector3 position)
            {
                parent.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                parent.SetParent(Signalscope.transform, false);
                parent.localScale = Vector3.one * 0.0005f;
                parent.localPosition = position;
                parent.localRotation = Quaternion.Euler(0, 90, 0);
            }

            private static void SetupButtons(Transform signalscopeModel)
            {
                var buttons = Instantiate(AssetLoader.SignalscopeHandheldButtonsPrefab).transform;
                buttons.parent = signalscopeModel;
                buttons.localPosition = Vector3.zero;
                buttons.localScale = Vector3.one;
                buttons.localRotation = Quaternion.identity;

                for (int i = 0; i < buttons.childCount; i++)
                    buttons.GetChild(i).gameObject.AddComponent<TouchButton>();
            }

            private void OnUnequip()
            {
                owLensCamera.SetEnabled(false);
                lens.gameObject.SetActive(false);
            }

            private void SetupScopeLens()
            {
                lens = Instantiate(AssetLoader.ScopeLensPrefab).transform;
                lens.parent = Signalscope.transform;
                lens.localPosition = new Vector3(0, 0.1f, 0.14f);
                lens.localRotation = Quaternion.identity;
                lens.localScale = Vector3.one * 2f;
                lens.gameObject.SetActive(false);

                lensCamera = lens.GetComponentInChildren<Camera>();
                lensCamera.gameObject.SetActive(false);
                lensCamera.cullingMask = CameraMaskFix.Behaviour.DefaultCullingMask;
                lensCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI")) & ~(1 << LayerMask.NameToLayer("VisibleToPlayer"));
                lensCamera.fieldOfView = 5;
                lensCamera.transform.SetParent(lens, false);

                owLensCamera = lensCamera.gameObject.AddComponent<OWCamera>();
                owLensCamera.useFarCamera = true;
                owLensCamera.renderSkybox = true;
                owLensCamera.useViewmodels = true;
                owLensCamera.farCameraDistance = 50000;
                owLensCamera.viewmodelFOV = 70;
                var fogEffect = lensCamera.gameObject.AddComponent<PlanetaryFogImageEffect>();
                fogEffect.fogShader = Locator.GetPlayerCamera().GetComponent<PlanetaryFogImageEffect>().fogShader;
                lensCamera.farClipPlane = 2000f;
                lensCamera.nearClipPlane = 0.1f;
                lensCamera.depth = 0f;
                lensCamera.clearFlags = CameraClearFlags.Color;
                lensCamera.backgroundColor = Color.black;
                lensCamera.gameObject.SetActive(true);

                // The camera on this prefab would istantiate an AudioListener enabled by default
                // which would break 3DAudio and tie it to the hands.
                owLensCamera.SetEnabled(false);
                Destroy(lensCamera.gameObject.GetComponent<AudioListener>());
            }

            internal void Update()
            {
                UpdateSignalscopeReticuleVisibility();
                UpdateSignalscipeZoom();
            }

            private void UpdateSignalscopeReticuleVisibility()
            {
                if (Reticule == null)
                {
                    return;
                }

                if (Reticule.gameObject.activeSelf && OWTime.IsPaused())
                {
                    Reticule.gameObject.SetActive(false);
                }
                else if (!Reticule.gameObject.activeSelf && !OWTime.IsPaused())
                {
                    Reticule.gameObject.SetActive(true);
                }
            }

            private void UpdateSignalscipeZoom()
            {
                if (OWInput.IsNewlyPressed(InputLibrary.toolActionPrimary, InputMode.All) && ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope, ToolGroup.Suit))
                {
                    lens.gameObject.SetActive(!lens.gameObject.activeSelf);

                    if(owLensCamera.gameObject != null)
                        owLensCamera.SetEnabled(lens.gameObject.activeSelf);
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Prefix<OWInput>(nameof(OWInput.ChangeInputMode), nameof(ChangeInputMode));
                    Postfix<QuantumInstrument>(nameof(QuantumInstrument.Update), nameof(PostQuantumInstrumentUpdate));
                    Empty<Signalscope>(nameof(global::Signalscope.EnterSignalscopeZoom));
                    Empty<Signalscope>(nameof(global::Signalscope.ExitSignalscopeZoom));
                }

                private static void PostQuantumInstrumentUpdate(QuantumInstrument __instance, bool ____gatherWithScope, bool ____waitToFlickerOut)
                {
                    if (____gatherWithScope && !____waitToFlickerOut && ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope))
                    {
                        var from = __instance.transform.position - lensCamera.transform.position;
                        var num = Vector3.Angle(from, lensCamera.transform.forward);
                        if (num < 1f && lens.gameObject.activeSelf)
                        {
                            __instance.Gather();
                        }
                    }
                }

                private static void ChangeInputMode(InputMode mode)
                {
                    if (!Reticule || !ShipWindshield || mode == InputMode.Menu || mode == InputMode.Map)
                    {
                        return;
                    }
                    if (mode == InputMode.ShipCockpit || mode == InputMode.LandingCam)
                    {
                        Reticule.parent = ShipWindshield;
                        Reticule.localScale = Vector3.one * 0.004f;
                        Reticule.localPosition = Vector3.forward * 3f;
                        Reticule.localRotation = Quaternion.identity;
                    }
                    else
                        SetupReticule(Reticule);
                }
            }
        }
    }
}