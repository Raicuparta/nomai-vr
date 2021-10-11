using NomaiVR.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Valve.VR;

namespace NomaiVR.ReusableBehaviours
{
    //Some code from SteamVR_Fade behaviour
    public class VRMindProjectorImageEffect : MonoBehaviour
    {
        private readonly int _shaderPropID_UnscaledTime = Shader.PropertyToID("_UnscaledTime");

        public float eyeOpenness { get; set; }
        private Transform quadTransform;
        private Transform eyeDome;

        private Color currentColor = new Color(0, 0, 0, 0);
        private Material fadeMaterial = null;
        private int fadeMaterialColorID = -1;
        private Material projectionMaterial;

        private void Start()
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            GameObject.Destroy(quad.GetComponent<Collider>());
            GameObject.Destroy(quad.GetComponent<Rigidbody>());
            quad.name = "MindProjectorPlane";

            var dome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject.Destroy(dome.GetComponent<Collider>());
            GameObject.Destroy(dome.GetComponent<Rigidbody>());
            Mesh domeMesh = dome.GetComponent<MeshFilter>().mesh;
            domeMesh.triangles = domeMesh.triangles.Reverse().ToArray(); //We need a reverse dome
            dome.name = "MindProjectorEyeDome";

            var imageEffect = FindObjectOfType<MindProjectorImageEffect>();
            quad.GetComponent<Renderer>().material = imageEffect._localMaterial;
            imageEffect._localMaterial.shader = ShaderLoader.GetShader("NomaiVR/Mind_Projection_Fix");
            imageEffect._localMaterial.renderQueue = (int)RenderQueue.Overlay;
            projectionMaterial = imageEffect._localMaterial;

            fadeMaterial = new Material(ShaderLoader.GetShader("Custom/SteamVR_Fade_WorldSpace"));
            fadeMaterial.renderQueue = (int)RenderQueue.Overlay - 100;
            fadeMaterialColorID = Shader.PropertyToID("fadeColor");
            dome.GetComponent<Renderer>().material = fadeMaterial;

            dome.layer = LayerMask.NameToLayer("UI");
            quad.layer = LayerMask.NameToLayer("UI");

            var playerBody = Locator.GetPlayerBody().transform;

            quadTransform = quad.transform;
            quadTransform.SetParent(playerBody, false);
            quadTransform.localPosition = Vector3.forward * 4f;
            quadTransform.localScale = new Vector3(-3, 3, 3); //We need to flip the X axis
            quadTransform.LookAt(playerBody.transform, playerBody.transform.up);

            eyeDome = dome.transform;
            eyeDome.SetParent(playerBody, false);
            eyeDome.localPosition = Vector3.zero;
            eyeDome.localScale = Vector3.one * 10;

            this.enabled = false;
        }

        private void OnEnable()
        {
            //Projection start
            quadTransform?.gameObject.SetActive(true);
            eyeDome?.gameObject.SetActive(true);
        }

        private void OnDisable()
        {
            //Projection finished
            quadTransform?.gameObject.SetActive(false);
            eyeDome?.gameObject.SetActive(false);
        }

        void Update()
        {
            currentColor = Color.black;
            currentColor.a = (1 - eyeOpenness*eyeOpenness);
            fadeMaterial.SetColor(fadeMaterialColorID, currentColor);
            this.projectionMaterial.SetFloat(_shaderPropID_UnscaledTime, Time.unscaledTime);
        }
    }
}
