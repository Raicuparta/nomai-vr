using System.Collections.Generic;
using UnityEngine;

namespace NomaiVR
{
    public class Common : NomaiVRModule<NomaiVRModule.EmptyBehaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool isPersistent => true;
        protected override OWScene[] scenes => PlayableScenes;

        public static PlayerCharacterController PlayerBody { get; private set; }
        public static Transform PlayerHead { get; private set; }
        public static ToolModeSwapper ToolSwapper { get; private set; }

        protected override void OnSceneLoad()
        {
            base.OnSceneLoad();
            PlayerBody = GameObject.Find("Player_Body").GetComponent<PlayerCharacterController>();
            PlayerHead = GameObject.FindObjectOfType<ToolModeUI>().transform;
            PlayerHead.localPosition = new Vector3(PlayerHead.localPosition.x, PlayerHead.localPosition.y, 0);
            ToolSwapper = GameObject.FindObjectOfType<ToolModeSwapper>();
        }

        public static bool IsUsingAnyTool()
        {
            return ToolSwapper.IsInToolMode(ToolMode.Probe) || ToolSwapper.IsInToolMode(ToolMode.Translator) || ToolSwapper.IsInToolMode(ToolMode.SignalScope);
        }

        public static bool IsInGame()
        {
            return LoadManager.GetCurrentScene() == OWScene.SolarSystem || LoadManager.GetCurrentScene() == OWScene.EyeOfTheUniverse;
        }
    }
}
