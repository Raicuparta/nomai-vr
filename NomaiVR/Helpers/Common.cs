using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR
{
    public class Common : NomaiVRModule<NomaiVRModule.EmptyBehaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool isPersistent => true;
        protected override OWScene[] scenes => PlayableScenes;

        public static Transform PlayerHead { get; private set; }

        protected override void OnSceneLoad()
        {
            base.OnSceneLoad();
            PlayerHead = GameObject.FindObjectOfType<ToolModeUI>().transform;
            PlayerHead.localPosition = new Vector3(PlayerHead.localPosition.x, PlayerHead.localPosition.y, 0);
        }
    }
}
