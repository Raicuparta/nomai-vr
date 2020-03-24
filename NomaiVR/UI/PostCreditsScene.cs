using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

namespace NomaiVR {
    class PostCreditsScene: MonoBehaviour {
        static AssetBundle _assetBundle;
        static GameObject _prefab;

        void Start () {
            if (!_assetBundle) {
                _assetBundle = NomaiVR.Helper.Assets.LoadBundle("assets/cinema-camera");
                _prefab = _assetBundle.LoadAsset<GameObject>("assets/postcreditscamera.prefab");
            }


            var originalCamera = Camera.main;

            //var vrCamera = new GameObject().AddComponent<Camera>();
            //vrCamera.gameObject.SetActive(false);
            //vrCamera.transform.parent = originalCamera.transform;
            //vrCamera.transform.localPosition = Vector3.zero;
            //vrCamera.transform.localRotation = Quaternion.identity;
            //vrCamera.nearClipPlane = 0.01f;
            //vrCamera.farClipPlane = 10000;
            //vrCamera.cullingMask = originalCamera.cullingMask;
            //vrCamera.backgroundColor = Color.black;
            //vrCamera.clearFlags = CameraClearFlags.Color;
            //var owCamera = vrCamera.gameObject.AddComponent<OWCamera>();
            //owCamera.renderSkybox = true;
            //originalCamera.enabled = false;
            //vrCamera.gameObject.SetActive(true);
            //var postProcessing = vrCamera.gameObject.AddComponent<PostProcessingBehaviour>();
            //postProcessing.profile = originalCamera.gameObject.GetComponent<PostProcessingBehaviour>().profile;
            //var canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
            ////canvas.
            //Destroy(GameObject.Find("DisconnectedPauseMenu"));

            //originalCamera.enabled = false;
            originalCamera.tag = "Untagged";

            var camera = Instantiate(_prefab);
            camera.transform.GetChild(0).parent = null;
            //camera.tag = "MainCamera";

            var renderTexture = _assetBundle.LoadAsset<RenderTexture>("assets/screen.renderTexture");

            originalCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("UNUSED"));
            originalCamera.transform.position = new Vector3(1075, 505, -765);
            originalCamera.transform.rotation = Quaternion.identity;
            originalCamera.rect.Set(0, 0, 1, 0.5f);
            originalCamera.targetTexture = renderTexture;

        }

        void Update () {
            if (Input.GetKey(KeyCode.Equals)) {
                Time.timeScale = 10;
            } else {
                Time.timeScale = 1;
            }
        }
    }
}
