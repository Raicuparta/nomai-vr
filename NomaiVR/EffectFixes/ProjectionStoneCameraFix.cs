﻿using UnityEngine;

namespace NomaiVR.EffectFixes
{
    internal class ProjectionStoneCameraFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, ProjectionStoneCameraFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Patch : NomaiVRPatch
        {
            public override void ApplyPatches()
            {
                Postfix<NomaiRemoteCameraPlatform>("SwitchToRemoteCamera", nameof(SwitchToRemoteCamera));
            }

            private static void SwitchToRemoteCamera(NomaiRemoteCameraPlatform ____slavePlatform, Transform ____playerHologram)
            {
                var camera = ____slavePlatform.GetOwnedCamera().transform;
                if (camera.parent.name.Contains("Prefab_NOM_RemoteViewer"))
                {
                    var parent = new GameObject().transform;
                    parent.parent = ____playerHologram;
                    parent.localPosition = new Vector3(0, -2.5f, 0);
                    parent.localRotation = Quaternion.identity;
                    ____slavePlatform.GetOwnedCamera().transform.parent = parent;
                    ____playerHologram.Find("Traveller_HEA_Player_v2").gameObject.SetActive(false);
                }
            }
        }
    }
}
