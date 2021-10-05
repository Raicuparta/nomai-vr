using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace NomaiVR.ReusableBehaviours
{
    public class VRMindProjectorImageEffect : MonoBehaviour
    {
        private const float k_ProjectionMeters = 1;
        
        public float slideFade 
        { 
            set
            {
                if(_textureImage != null)
                    _textureImage.CrossFadeAlpha((1 - value), 0, true);
            }
        }

        public Texture slideTexture 
        { 
            set
            {
                if(_textureImage != null && value != null)
                {
                    _textureImage.texture = value;
                }
            }
        }

        public float eyeOpenness
        {
            set
            {
                //Hopefully this is enough
                Color color = Color.black;
                color.a = (1 - value);
                SteamVR_Fade.Start(color, 0f, true);
            }
        }

        private Canvas _slideCanvas;
        private RawImage _textureImage;

        private void OnEnable()
        {
            //Projection start
            var playerBody = Locator.GetPlayerBody().transform;
            var playerCamera = Locator.GetPlayerCamera();
            var projectionCanvas = new GameObject("MindProjectionCanvas").transform;
            var projectionImage = new GameObject("ProjectionImage").transform;
            projectionCanvas.SetParent(playerCamera.transform, false);
            projectionCanvas.localPosition = Vector3.forward * 4f;
            projectionCanvas.LookAt(playerCamera.transform, playerBody.transform.up);
            projectionCanvas.SetParent(playerBody, true);
            projectionCanvas.gameObject.layer = LayerMask.NameToLayer("UI");
            _slideCanvas = projectionCanvas.gameObject.AddComponent<Canvas>();
            _slideCanvas.renderMode = RenderMode.WorldSpace;
            _slideCanvas.worldCamera = playerCamera.mainCamera;
            _slideCanvas.transform.localScale *= 0.75f;
            //_slideCanvas.transform.localScale = new Vector3(k_ProjectionMeters / 1920, k_ProjectionMeters / 1920, k_ProjectionMeters / 1920); //too small?
            projectionImage.SetParent(projectionCanvas, false);
            projectionImage.localPosition = Vector3.zero;
            _textureImage = projectionImage.gameObject.AddComponent<RawImage>();
            _textureImage.material = MaterialHelper.GetOverlayMaterial();

            LayerHelper.ChangeLayerRecursive(_slideCanvas.gameObject, "VisibleToPlayer");
        }

        private void OnDisable()
        {
            //Projection finished
            if(_slideCanvas != null)
            {
                GameObject.Destroy(_slideCanvas.gameObject);
                _textureImage = null;
                _slideCanvas = null;
            }
        }

        private void Awake()
        {
            this.enabled = false;
        }
    }
}
