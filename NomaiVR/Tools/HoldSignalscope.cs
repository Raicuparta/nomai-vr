using OWML.Utils;
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

            private OWCamera _owLensCamera;

            internal void Start()
            {
                if (LoadManager.GetCurrentScene() == OWScene.SolarSystem)
                {
                    _shipWindshield = GameObject.Find("ShipLODTrigger_Cockpit").transform;
                }

                _signalscope = Camera.main.transform.Find("Signalscope").GetComponent<Signalscope>();

                var holdSignalscope = _signalscope.gameObject.AddComponent<Holdable>();
                holdSignalscope.transform.localPosition = new Vector3(0.0074f, 0.0808f, 0.1343f);
                holdSignalscope.transform.localRotation = Quaternion.identity;

                var signalScopeModel = _signalscope.transform.GetChild(0);
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
                SetupReticule(_reticule);

                var helmetOff = playerHUD.Find("HelmetOffUI/SignalscopeCanvas");
                SetupSignalscopeUI(helmetOff, new Vector3(-0.05f, 0.75f, 0));

                var helmetOn = playerHUD.Find("HelmetOnUI/UICanvas/SigScopeDisplay");
                SetupSignalscopeUI(helmetOn, new Vector3(-0.05f, -0.05f, 0));
                LayerHelper.ChangeLayerRecursive(helmetOn.gameObject, "UI");
                SetupScopeLens();
            }

            private static void SetupReticule(Transform reticule)
            {
                reticule.parent = _signalscope.transform;
                reticule.localScale = Vector3.one * 0.0003f;
                reticule.localPosition = new Vector3(0, 0.125f, 0.14f);
                reticule.localRotation = Quaternion.identity;
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
                _owLensCamera.SetEnabled(false);
                _lens.gameObject.SetActive(false);
            }

            private void SetupScopeLens()
            {
                _lens = Instantiate(AssetLoader.ScopeLensPrefab).transform;
                _lens.parent = _signalscope.transform;
                _lens.localPosition = new Vector3(0, 0.1f, 0.14f);
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

                _owLensCamera = _lensCamera.gameObject.AddComponent<OWCamera>();
                _owLensCamera.useFarCamera = true;
                _owLensCamera.renderSkybox = true;
                _owLensCamera.useViewmodels = true;
                _owLensCamera.farCameraDistance = 50000;
                _owLensCamera.viewmodelFOV = 70;
                var fogEffect = _lensCamera.gameObject.AddComponent<PlanetaryFogImageEffect>();
                fogEffect.fogShader = Locator.GetPlayerCamera().GetComponent<PlanetaryFogImageEffect>().fogShader;
                _lensCamera.farClipPlane = 2000f;
                _lensCamera.nearClipPlane = 0.1f;
                _lensCamera.depth = 0f;
                _lensCamera.clearFlags = CameraClearFlags.Color;
                _lensCamera.backgroundColor = Color.black;
                _lensCamera.gameObject.SetActive(true);

                // The camera on this prefab would istantiate an AudioListener enabled by default
                // which would break 3DAudio and tie it to the hands.
                _owLensCamera.SetEnabled(false);
                _lensCamera.gameObject.GetComponent<AudioListener>().enabled = false;
            }

            internal void Update()
            {
                UpdateSignalscopeReticuleVisibility();
                UpdateSignalscipeZoom();
            }

            private void UpdateSignalscopeReticuleVisibility()
            {
                if (_reticule == null)
                {
                    return;
                }

                if (_reticule.gameObject.activeSelf && OWTime.IsPaused())
                {
                    _reticule.gameObject.SetActive(false);
                }
                else if (!_reticule.gameObject.activeSelf && !OWTime.IsPaused())
                {
                    _reticule.gameObject.SetActive(true);
                }
            }

            private void UpdateSignalscipeZoom()
            {
                if (OWInput.IsNewlyPressed(InputLibrary.scopeView, InputMode.All) && ToolHelper.Swapper.IsInToolMode(ToolMode.SignalScope, ToolGroup.Suit))
                {
                    _lens.gameObject.SetActive(!_lens.gameObject.activeSelf);

                    if(_owLensCamera.gameObject != null)
                        _owLensCamera.SetEnabled(_lens.gameObject.activeSelf);
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
                            __instance.Gather();
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
                        SetupReticule(_reticule);
                }
            }
        }
    }
}