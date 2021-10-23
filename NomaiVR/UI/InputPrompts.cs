﻿using System.Collections.Generic;
using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.Input;
using NomaiVR.Input.ActionInputs;
using UnityEngine;
using Valve.VR;

namespace NomaiVR.UI
{
    internal class InputPrompts : NomaiVRModule<InputPrompts.Behaviour, InputPrompts.Behaviour.Patch>
    {
        public static Behaviour Instance { get; internal set; }
        protected override bool IsPersistent => true;
        protected override OWScene[] Scenes => AllScenes;

        private static PromptManager Manager => Locator.GetPromptManager();

        public enum ActiveVRPlatform
        {
            Oculus, //G2 and oculus style controllers should be bound here
            Index,
            Vive,
            Wmr,
            Generic
        }

        public class Behaviour : MonoBehaviour
        {
            private const string baseAssetPath = "Assets/VRBindings/Texture2D";

            public ActiveVRPlatform Platform { get; private set; }
            private Dictionary<string, Texture2D> textureCache;
            private Dictionary<ISteamVR_Action, string> pathCache;

            public void Awake()
            {
                Instance = this;
                Platform = ActiveVRPlatform.Generic;

                textureCache = new Dictionary<string, Texture2D>();
                pathCache = new Dictionary<ISteamVR_Action, string>();
                foreach (var texturePath in AssetLoader.VRBindingTextures.GetAllAssetNames())
                {
                    var assetPath = texturePath.Substring(0, texturePath.LastIndexOf('.'));
                    textureCache.Add(assetPath.ToLower(), AssetLoader.VRBindingTextures.LoadAsset<Texture2D>(texturePath));
                }
                textureCache.Add("empty", new Texture2D(0, 0));

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
                    case "holographic_controller":
                        Platform = ActiveVRPlatform.Wmr;
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
                    pathCache.Clear(); //Invalidate learned paths
                    InvalidateButtonImages();
                }
            }

            private static void InvalidateButtonImages()
            {
                if (Manager == null) return;
                Manager.OnButtonImagesChanged();
            }

            public string GetCachedPartName(ISteamVR_Action action)
            {
                pathCache.TryGetValue(action, out var path);
                return path;
            }

            public void SetCachedPartName(ISteamVR_Action action, string path) => pathCache[action] = path;

            public Texture2D GetTexture(string path)
            {
                textureCache.TryGetValue(path.ToLower(), out var outTexture);
                return outTexture;
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Prefix<AbstractInputCommands<IVectorInputAction>>(nameof(AbstractInputCommands<IVectorInputAction>.GetUITextures), nameof(GetVruiTextures));
                    Prefix<AbstractInputCommands<IAxisInputAction>>(nameof(AbstractInputCommands<IAxisInputAction>.GetUITextures), nameof(GetVruiTextures));
                    Prefix<CompositeInputCommands>(nameof(CompositeInputCommands.GetUITextures), nameof(GetVruiTextures));
                    Postfix<ScreenPromptElement>(nameof(ScreenPromptElement.BuildScreenPrompt), nameof(MakePromptsDrawOnTop));
                }

                public static bool GetVruiTextures(bool forceRefresh, AbstractCommands __instance, ref List<Texture2D> __result)
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
                        && !string.IsNullOrEmpty(emptyInput.TexturePath))
                    {
                        var emptyActionTexture = Instance.GetTexture($"{baseAssetPath}/{emptyInput.TexturePath}");
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
                        //Solve for hand + controller part
                        var activeInputDevice = vrInputAction.ActiveSource;
                        var hand = activeInputDevice == SteamVR_Input_Sources.RightHand
                            ? "Right"
                            : "Left";
                        var partName = steamVrAction.GetRenderModelComponentName(activeInputDevice);
                        name = $"{hand}/{partName}";

                        //Special actions modifiers
                        if (vrInputAction is Vector2ActionInput vectorAction &&
                            !string.IsNullOrEmpty(vectorAction.TextureModifier))
                        {
                            name += "_" + vectorAction.TextureModifier;
                        }
                        else if (vrInputAction is BooleanActionInput booleanAction &&
                                booleanAction.Clickable &&
                                (partName == "trackpad" || partName == "thumbstick"))
                        {
                            //This would probably be the place to try and solve for click directions
                            name += "_click";
                        }

                        Instance.SetCachedPartName(steamVrAction, name);
                    }
                    if (string.IsNullOrEmpty(name)) name = Instance.GetCachedPartName(steamVrAction);

                    Logs.Write($"Texture for {__instance.CommandType} is '{name}', action is '{steamVrAction.GetShortName()}'");
                    
                    var texture = Instance.GetTexture($"{baseAssetPath}/{Instance.Platform}/{name}");
                    if(texture != null) __instance.textureList.Add(texture);
                    return __instance.textureList.Count == 0;
                }
                
                private static void MakePromptsDrawOnTop(ScreenPromptElement __instance)
                {
                    MaterialHelper.MakeGraphicChildrenDrawOnTop(__instance.gameObject);
                }
            }
        }
    }
}
