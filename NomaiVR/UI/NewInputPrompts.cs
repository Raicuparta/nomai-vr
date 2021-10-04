using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NomaiVR.Input;
using UnityEngine;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.UI
{
    internal class NewInputPrompts : NomaiVRModule<NewInputPrompts.Behaviour, NewInputPrompts.Behaviour.Patch>
    {
        public static NewInputPrompts.Behaviour Instance { get; internal set; }
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => AllScenes;

        private static PromptManager Manager => Locator.GetPromptManager();

        public enum ActiveVRPlatform
        {
            Oculus, //G2 and oculus style controllers should be bound here
            Index,
            Vive,
            WMR,
            Generic
        }

        public class Behaviour : MonoBehaviour
        {
            private const string k_baseAssetPath = "Assets/VRBindings/Texture2D";

            public ActiveVRPlatform Platform { get; private set; }
            private Dictionary<string, Texture2D> _textureCache;

            public void Awake()
            {
                Instance = this;
                Platform = ActiveVRPlatform.Generic;

                _textureCache = new Dictionary<string, Texture2D>();
                foreach(var texturePath in AssetLoader.VRBindingTextures.GetAllAssetNames())
                {
                    var assetPath = texturePath.Substring(0, texturePath.LastIndexOf('.'));
                    _textureCache.Add(assetPath.ToLower(), AssetLoader.VRBindingTextures.LoadAsset<Texture2D>(texturePath));
                }

                RegisterToControllerChanges();
            }

            private void RegisterToControllerChanges()
            {
                Logs.Write($"Registering for controller binding changes");
                SteamVR_Actions._default.Grip.AddOnActiveBindingChangeListener((fromAction, fromSource, active) => ActiveDeviceChanged(fromAction), SteamVR_Input_Sources.Any);

                ActiveDeviceChanged(SteamVR_Actions._default.Grip);
            }

            private void ActiveDeviceChanged(SteamVR_Action_Boolean fromAction)
            {
                var activeDevice = SteamVR.instance.GetStringProperty(ETrackedDeviceProperty.Prop_ControllerType_String, fromAction.trackedDeviceIndex);
                Logs.Write($"#### Got active device: {activeDevice}");
                var currentPlatform = Platform;
                switch(activeDevice)
                {
                    case "oculus_touch":
                        Platform = ActiveVRPlatform.Oculus;
                        break;
                    case "vive_controller":
                        Platform = ActiveVRPlatform.Vive;
                        break;
                    case "knuckles":
                        Platform = ActiveVRPlatform.Index;
                        break;
                    case "holographic":
                        Platform = ActiveVRPlatform.WMR;
                        break;
                    default:
                        Platform = ActiveVRPlatform.Generic;
                        break;
                }

                Logs.Write($"Controller platform: {Platform}");
                if (currentPlatform != Platform) Manager?.OnButtonImagesChanged();
            }

            public Texture2D GetTexture(string path)
            {
                _textureCache.TryGetValue(path.ToLower(), out var outTexture);
                return outTexture;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Prefix<AbstractInputCommands<IVectorInputAction>>(nameof(AbstractInputCommands<IVectorInputAction>.GetUITextures), nameof(GetVRUITextures));
                    Prefix<AbstractInputCommands<IAxisInputAction>>(nameof(AbstractInputCommands<IAxisInputAction>.GetUITextures), nameof(GetVRUITextures));
                    Prefix<CompositeInputCommands>(nameof(CompositeInputCommands.GetUITextures), nameof(GetVRUITextures));
                }

                public static bool GetVRUITextures(bool gamepad, bool forceRefresh, AbstractCommands __instance, ref List<Texture2D> __result)
                {
                    if (__instance.textureList.Count > 0 && !forceRefresh)
                    {
                        //Cache textures
                        __result = __instance.textureList;
                        return false;
                    }
                    __instance.textureList.Clear();
                    var vrInputAction = InputMap.GetActionInput(__instance.CommandType);
                    if (vrInputAction == null) return true;

                    var steamVrAction = vrInputAction.Action;
                    var name = "";
                    var hand = steamVrAction.activeDevice == SteamVR_Input_Sources.RightHand
                        ? "Right"
                        : "Left";
                    name = $"{hand}/{steamVrAction.renderModelComponentName}";

                    Logs.Write($"Texture for {__instance.CommandType} is '{name}', action is '{steamVrAction.GetShortName()}'");
                    
                    var texture = Instance.GetTexture($"{k_baseAssetPath}/{Instance.Platform}/{name}");
                    if(texture != null) __instance.textureList.Add(texture);
                    __result = __instance.textureList;
                    return __instance.textureList.Count == 0;
                }
            }
        }
    }
}
