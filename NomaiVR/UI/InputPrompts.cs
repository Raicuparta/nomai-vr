using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NomaiVR.Input;
using NomaiVR.Input.ActionInputs;
using UnityEngine;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.UI
{
    internal class InputPrompts : NomaiVRModule<InputPrompts.Behaviour, InputPrompts.Behaviour.Patch>
    {
        public static InputPrompts.Behaviour Instance { get; internal set; }
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
            private Dictionary<ISteamVR_Action, string> _pathCache;

            public void Awake()
            {
                Instance = this;
                Platform = ActiveVRPlatform.Generic;

                _textureCache = new Dictionary<string, Texture2D>();
                _pathCache = new Dictionary<ISteamVR_Action, string>();
                foreach (var texturePath in AssetLoader.VRBindingTextures.GetAllAssetNames())
                {
                    var assetPath = texturePath.Substring(0, texturePath.LastIndexOf('.'));
                    _textureCache.Add(assetPath.ToLower(), AssetLoader.VRBindingTextures.LoadAsset<Texture2D>(texturePath));
                }
                _textureCache.Add("empty", new Texture2D(0, 0));

                RegisterToControllerChanges();
            }

            private void RegisterToControllerChanges()
            {
                Logs.Write($"Registering for controller binding changes");
                SteamVR_Actions._default.Grip.AddOnActiveBindingChangeListener((fromAction, fromSource, active) => ActiveDeviceChanged(fromAction), SteamVR_Input_Sources.Any);
                SteamVR_Actions.tools.Use.AddOnActiveBindingChangeListener((fromAction, fromSource, active) => InvalidateButtonImages(), SteamVR_Input_Sources.Any);
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
                    case "<unknown>":
                        return;
                    default:
                        Platform = ActiveVRPlatform.Generic;
                        break;
                }

                Logs.Write($"Controller platform: {Platform}");
                if (currentPlatform != Platform)
                {
                    _pathCache.Clear(); //Invalidate learned paths
                    InvalidateButtonImages();
                }
            }

            private void InvalidateButtonImages()
            {
                Manager?.OnButtonImagesChanged();
            }

            public string GetCachedPartName(ISteamVR_Action action)
            {
                _pathCache.TryGetValue(action, out var path);
                return path;
            }

            public void SetCachedPartName(ISteamVR_Action action, string path) => _pathCache[action] = path;

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
                    __result = __instance.textureList;

                    if (__instance.textureList.Count > 0 && !forceRefresh)
                    {
                        //Cache textures
                        return false;
                    }

                    __instance.textureList.Clear();

                    var vrInputAction = InputMap.GetActionInput(__instance.CommandType);
                    if (vrInputAction == null)
                    {
                        Logs.Write($"No action currently bound for texture {__instance.CommandType}");
                        return true;
                    }

                    if(vrInputAction is EmptyActionInput emptyInput
                        && !String.IsNullOrEmpty(emptyInput.TexturePath))
                    {
                        var emptyActionTexture = Instance.GetTexture($"{k_baseAssetPath}/{emptyInput.TexturePath}");
                        if (emptyActionTexture != null) __instance.textureList.Add(emptyActionTexture);
                        return __instance.textureList.Count == 0;
                    }

                    var steamVrAction = vrInputAction.Action;
                    if (steamVrAction == null)
                    {
                        __instance.textureList.Add(Instance.GetTexture("empty"));
                        return __instance.textureList.Count == 0;
                    }

                    var name = "";
                    if (vrInputAction.Active)
                    {
                        var activeInputDevice = vrInputAction.ActiveSource;
                        var hand = activeInputDevice == SteamVR_Input_Sources.RightHand
                            ? "Right"
                            : "Left";
                        name = $"{hand}/{steamVrAction.GetRenderModelComponentName(activeInputDevice)}";
                        Instance.SetCachedPartName(steamVrAction, name);
                    }
                    if (String.IsNullOrEmpty(name)) Instance.GetCachedPartName(steamVrAction);

                    Logs.Write($"Texture for {__instance.CommandType} is '{name}', action is '{steamVrAction.GetShortName()}'");
                    
                    var texture = Instance.GetTexture($"{k_baseAssetPath}/{Instance.Platform}/{name}");
                    if(texture != null) __instance.textureList.Add(texture);
                    return __instance.textureList.Count == 0;
                }
            }
        }
    }
}
