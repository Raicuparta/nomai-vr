using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR
{
    internal class HoldSignalscope : NomaiVRModule<HoldSignalscope.Behaviour, HoldSignalscope.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            protected static Transform _reticule;
            protected static Transform _shipWindshield;
            protected static Signalscope _signalscope;
            private static Camera _lensCamera;
            private static Transform _lens;

            internal void Start()
            {
                if (LoadManager.GetCurrentScene() == OWScene.SolarSystem)
                {
                    _shipWindshield = GameObject.Find("ShipLODTrigger_Cockpit").transform;
                }

                _signalscope = Camera.main.transform.Find("Signalscope").GetComponent<Signalscope>();

                var holdSignalscope = _signalscope.gameObject.AddComponent<Holdable>();
                holdSignalscope.transform.localPosition = new Vector3(0.013f, 0.1f, 0.123f);
                holdSignalscope.transform.localRotation = Quaternion.Euler(32.8f, 0, 0);

                var signalScopeModel = _signalscope.transform.GetChild(0);
                // Tools have a special shader that draws them on top of everything
                // and screws with perspective. Changing to Standard shader so they look
                // like a normal 3D object.
                signalScopeModel.GetComponent<MeshRenderer>().material.shader = Shader.Find("Standard");
                signalScopeModel.localPosition = Vector3.up * -0.1f;
                signalScopeModel.localRotation = Quaternion.identity;

                // This child seems to be only for some kind of shader effect.
                // Disabling it since it looks glitchy and doesn't seem necessary.
                signalScopeModel.GetChild(0).gameObject.SetActive(false);

                var signalScopeHolster = Instantiate(signalScopeModel).gameObject;
                signalScopeHolster.SetActive(true);
                var holster = signalScopeHolster.AddComponent<HolsterTool>();
                holster.hand = HandsController.Behaviour.RightHand;
                holster.position = new Vector3(0.3f, 0, 0);
                holster.mode = ToolMode.SignalScope;
                holster.scale = 0.8f;
                holster.angle = Vector3.right * 90;
                holster.onUnequip = OnUnequip;

                var playerHUD = GameObject.Find("PlayerHUD").transform;
                _reticule = playerHUD.Find("HelmetOffUI/SignalscopeReticule");

                // Attatch Signalscope UI to the Signalscope.
                _reticule.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                _reticule.parent = _signalscope.transform;
                _reticule.localScale = Vector3.one * 0.0005f;
                _reticule.localPosition = Vector3.forward * 0.5f;
                _reticule.localRotation = Quaternion.identity;

                var helmetOff = playerHUD.Find("HelmetOffUI/SignalscopeCanvas");
                SetupSignalscopeUI(helmetOff, new Vector3(-0.05f, 0.45f, 0));

                var helmetOn = playerHUD.Find("HelmetOnUI/UICanvas/SigScopeDisplay");
                SetupSignalscopeUI(helmetOn, new Vector3(-0.05f, -0.2f, 0));
                LayerHelper.ChangeLayerRecursive(helmetOn.gameObject, "UI");
                SetupScopeLens();
            }

            private static void SetupSignalscopeUI(Transform parent, Vector3 position)
            {
                parent.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
                parent.parent = _signalscope.transform;
                parent.localScale = Vector3.one * 0.0005f;
                parent.localPosition = position;
                parent.localRotation = Quaternion.Euler(0, 90, 0);
            }

            private void OnUnequip()
            {
                _lens.gameObject.SetActive(false);
            }

            private static void SetupScopeLens()
            {
                _lens = Instantiate(AssetLoader.ScopeLensPrefab).transform;
                _lens.parent = _signalscope.transform;
                _lens.localPosition = Vector3.forward * 0.14f;
                _lens.localRotation = Quaternion.identity;
                _lens.localScale = Vector3.one * 2f;
                _lens.gameObject.SetActive(false);

                _lensCamera = _lens.GetComponentInChildren<Camera>();
                _lensCamera.gameObject.SetActive(false);
                _lensCamera.cullingMask = CameraMaskFix.Behaviour.DefaultCullingMask;
                _lensCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI")) & ~(1 << LayerMask.NameToLayer("VisibleToPlayer"));
                _lensCamera.fieldOfView = 5;
                _lensCamera.transform.parent = null;
                var followTarget = _lensCamera.gameObject.AddComponent<FollowTarget>();
                followTarget.target = _lens;
                followTarget.rotationSmoothTime = 0.1f;
                followTarget.positionSmoothTime = 0.1f;

                var owCamera = _lensCamera.gameObject.AddComponent<OWCamera>();
                owCamera.useFarCamera = true;
                owCamera.renderSkybox = true;
                owCamera.useViewmodels = true;
                owCamera.farCameraDistance = 50000;
                owCamera.viewmodelFOV = 70;
                var fogEffect = _lensCamera.gameObject.AddComponent<PlanetaryFogImageEffect>();
                fogEffect.fogShader = Locator.GetPlayerCamera().GetComponent<PlanetaryFogImageEffect>().fogShader;
                _lensCamera.farClipPlane = 2000f;
                _lensCamera.nearClipPlane = 0.1f;
                _lensCamera.depth = 0f;
                _lensCamera.clearFlags = CameraClearFlags.Color;
                _lensCamera.backgroundColor = Color.black;
                _lensCamera.gameObject.SetActive(true);
            }

            internal void Update()
            {
                if (OWInput.IsNewlyPressed(InputLibrary.scopeView, InputMode.All) && ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope, ToolGroup.Suit))
                {
                    _lens.gameObject.SetActive(!_lens.gameObject.activeSelf);
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Prefix<OWInput>("ChangeInputMode", nameof(ChangeInputMode));
                    Postfix<QuantumInstrument>("Update", nameof(PostQuantumInstrumentUpdate));
                    Empty<Signalscope>("EnterSignalscopeZoom");
                    Empty<Signalscope>("ExitSignalscopeZoom");
                }

                private static void PostQuantumInstrumentUpdate(QuantumInstrument __instance, bool ____gatherWithScope, bool ____waitToFlickerOut)
                {
                    if (____gatherWithScope && !____waitToFlickerOut && ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope))
                    {
                        var from = __instance.transform.position - _lensCamera.transform.position;
                        var num = Vector3.Angle(from, _lensCamera.transform.forward);
                        if (num < 1f && _lens.gameObject.activeSelf)
                        {
                            __instance.Invoke("Gather");
                        }
                    }
                }

                private static void ChangeInputMode(InputMode mode)
                {
                    if (!_reticule || !_shipWindshield || mode == InputMode.Menu || mode == InputMode.Map)
                    {
                        return;
                    }
                    if (mode == InputMode.ShipCockpit || mode == InputMode.LandingCam)
                    {
                        _reticule.parent = _shipWindshield;
                        _reticule.localScale = Vector3.one * 0.004f;
                        _reticule.localPosition = Vector3.forward * 3f;
                        _reticule.localRotation = Quaternion.identity;
                    }
                    else
                    {
                        _reticule.parent = _signalscope.transform;
                        _reticule.localScale = Vector3.one * 0.0003f;
                        _reticule.localPosition = Vector3.forward * 0.14f;
                        _reticule.localRotation = Quaternion.identity;
                    }
                }
            }
        }
    }
}