using OWML.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NomaiVR
{
    internal class ShadowFix : NomaiVRModule<ShadowFix.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class Behaviour : MonoBehaviour
        {
            internal void Awake()
            {
                GameObject brambleProxyShadowCaster = GameObject.Find("DarkBramble_Body/Sector_DB/Proxy_DB/LOD_DB_Proxy");
                GameObject.Destroy(brambleProxyShadowCaster.GetComponent<ProxyShadowCaster>());
            }
        }
    }
}
