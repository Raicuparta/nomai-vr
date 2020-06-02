using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NomaiVR
{
    public class LookArrow : NomaiVRModule<LookArrow.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private void Start()
            {
                var canvas = Instantiate(AssetLoader.LookArrow).GetComponent<Canvas>();
                //canvas.worldCamera = Locator.GetPlayerCamera().mainCamera;
                //canvas.renderMode = RenderMode.WorldSpace;
                canvas.transform.parent = Locator.GetPlayerCamera().transform;
                canvas.transform.localPosition = new Vector3(0, 0, 4);
                canvas.transform.localRotation = Quaternion.identity;

                var rightArrow = canvas.transform.Find("look-right");
                rightArrow.GetComponent<SpriteRenderer>().material = Canvas.GetDefaultCanvasMaterial();
                var letArrow = canvas.transform.Find("look-left");
                letArrow.GetComponent<SpriteRenderer>().material = Canvas.GetDefaultCanvasMaterial();

            }
        }
    }
}
