using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NomaiVR {
    public class HoldSignalscope: MonoBehaviour {
        protected static Transform SignalscopeReticule;
        protected static Transform ShipWindshield;
        protected static Signalscope SignalScope;
        static AssetBundle _assetBundle;
        Signalscope _signalScope;
        static Camera _scopeLensCamera;
        static OWCamera _playerCamera;
        Transform _scopeLens;
        static GameObject _scopeLensPrefab;

        void Awake () {
            if (SceneManager.GetActiveScene().name == "SolarSystem") {
                ShipWindshield = GameObject.Find("ShipLODTrigger_Cockpit").transform;
            }

            _signalScope = Camera.main.transform.Find("Signalscope").GetComponent<Signalscope>();
            Hands.HoldObject(_signalScope.transform, Hands.RightHand, new Vector3(-0.047f, 0.053f, 0.143f), Quaternion.Euler(32.8f, 0, 0));
            SignalScope = _signalScope.GetComponent<Signalscope>();

            var signalScopeModel = _signalScope.transform.GetChild(0);
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
            holster.hand = Hands.RightHand;
            holster.position = new Vector3(0.3f, -0.55f, 0);
            holster.mode = ToolMode.SignalScope;
            holster.scale = 0.8f;
            holster.angle = Vector3.right * 90;
            holster.onEquip = OnEquip;
            holster.onUnequip = OnUnequip;

            var playerHUD = GameObject.Find("PlayerHUD").transform;
            SignalscopeReticule = playerHUD.Find("HelmetOffUI/SignalscopeReticule");
            var helmetOn = playerHUD.Find("HelmetOnUI/UICanvas/SigScopeDisplay");
            var helmetOff = playerHUD.Find("HelmetOffUI/SignalscopeCanvas");

            // Attatch Signalscope UI to the Signalscope.
            SignalscopeReticule.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            SignalscopeReticule.parent = _signalScope.transform;
            SignalscopeReticule.localScale = Vector3.one * 0.0005f;
            SignalscopeReticule.localPosition = Vector3.forward * 0.5f;
            SignalscopeReticule.localRotation = Quaternion.identity;

            helmetOff.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            helmetOff.parent = _signalScope.transform;
            helmetOff.localScale = Vector3.one * 0.0005f;
            helmetOff.localPosition = Vector3.zero;
            helmetOff.localRotation = Quaternion.identity;

            helmetOn.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            helmetOn.parent = _signalScope.transform;
            helmetOn.localScale = Vector3.one * 0.0005f;
            helmetOn.localPosition = Vector3.down * 0.5f;
            helmetOn.localRotation = Quaternion.identity;

            LoadScopeLens();
        }

        void OnEquip () {
            _scopeLens.gameObject.SetActive(true);
            _signalScope.SetValue("_inZoomMode", true);
        }

        void OnUnequip () {
            _scopeLens.gameObject.SetActive(false);
            _signalScope.SetValue("_inZoomMode", false);
        }

        void LoadScopeLens () {
            if (!_assetBundle) {
                _assetBundle = NomaiVR.Helper.Assets.LoadBundle("assets/scope-lens");
                _scopeLensPrefab = _assetBundle.LoadAsset<GameObject>("assets/scopelens.prefab");
            }
            _scopeLens = Instantiate(_scopeLensPrefab).transform;
            _scopeLens.parent = _signalScope.transform;
            _scopeLens.localPosition = Vector3.zero;
            _scopeLens.localRotation = Quaternion.identity;
            _scopeLens.localScale = Vector3.one * 1.5f;
            _scopeLens.gameObject.SetActive(false);

            _scopeLensCamera = _scopeLens.GetComponentInChildren<Camera>();
            _scopeLensCamera.gameObject.SetActive(false);
            _scopeLensCamera.cullingMask = Camera.main.cullingMask;
            _scopeLensCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UI"));
            _scopeLensCamera.fieldOfView = 5f;

            var owCamera = _scopeLensCamera.gameObject.AddComponent<OWCamera>();
            owCamera.useFarCamera = true;
            owCamera.renderSkybox = true;
            owCamera.useViewmodels = true;
            owCamera.farCameraDistance = 50000;
            owCamera.viewmodelFOV = 70;
            var fogEffect = _scopeLensCamera.gameObject.AddComponent<PlanetaryFogImageEffect>();
            fogEffect.fogShader = Locator.GetPlayerCamera().GetComponent<PlanetaryFogImageEffect>().fogShader;
            _scopeLensCamera.farClipPlane = 2000f;
            _scopeLensCamera.nearClipPlane = 0.1f;
            _scopeLensCamera.depth = 0f;
            _scopeLensCamera.clearFlags = CameraClearFlags.Color;
            _scopeLensCamera.backgroundColor = Color.black;
            _scopeLensCamera.gameObject.SetActive(true);
        }

        internal static class Patches {
            public static void Patch () {
                NomaiVR.Pre<OWInput>("ChangeInputMode", typeof(Patches), nameof(ChangeInputMode));
                NomaiVR.Post<ShipCockpitUI>("Start", typeof(Patches), nameof(ShipCockpitStart));
                NomaiVR.Pre<QuantumInstrument>("Update", typeof(Patches), nameof(PreQuantumInstrumentUpdate));
                NomaiVR.Post<QuantumInstrument>("Update", typeof(Patches), nameof(PostQuantumInstrumentUpdate));
            }

            static void ShipCockpitStart (Signalscope ____signalscopeTool) {
                ____signalscopeTool = SignalScope;
            }

            static void PreQuantumInstrumentUpdate () {
                if (!_playerCamera) {
                    _playerCamera = Locator.GetPlayerCamera();
                }

                typeof(Locator).SetValue("_playerCamera", _scopeLensCamera);
            }

            static void PostQuantumInstrumentUpdate () {
                typeof(Locator).SetValue("_playerCamera", _playerCamera);
            }

            static void ChangeInputMode (InputMode mode) {
                if (!SignalscopeReticule || !ShipWindshield || mode == InputMode.Menu) {
                    return;
                }
                if (mode == InputMode.ShipCockpit || mode == InputMode.LandingCam) {
                    SignalscopeReticule.parent = ShipWindshield;
                    SignalscopeReticule.localScale = Vector3.one * 0.004f;
                    SignalscopeReticule.localPosition = Vector3.forward * 3f;
                    SignalscopeReticule.localRotation = Quaternion.identity;
                } else {
                    SignalscopeReticule.parent = SignalScope.transform;
                    SignalscopeReticule.localScale = Vector3.one * 0.0005f;
                    SignalscopeReticule.localPosition = Vector3.forward * 0.5f;
                    SignalscopeReticule.localRotation = Quaternion.identity;
                }
            }
        }
    }
}
