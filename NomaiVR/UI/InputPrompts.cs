using System.Collections.Generic;
using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.Input;
using NomaiVR.Input.ActionInputs;
using NomaiVR.Tools;
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

        public class Behaviour : MonoBehaviour
        {
            private const string baseAssetPath = "Assets/VRBindings/Texture2D";

            private Dictionary<string, Texture2D> textureCache;
            private Dictionary<ISteamVR_Action, string> pathCache;

            public void Awake()
            {
                Instance = this;

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
                SteamVR_Actions.tools.Use.AddOnActiveBindingChangeListener((fromAction, fromSource, active) => InvalidateButtonImages(), SteamVR_Input_Sources.Any);

                VRToolSwapper.ToolEquipped += InvalidateButtonImages;
                VRToolSwapper.UnEquipped += InvalidateButtonImages;

                RefreshWhenBindingChanges(
                    SteamVR_Actions.tools.Secondary,
                    SteamVR_Actions.tools.DPadUp,
                    SteamVR_Actions.tools.DPadDown,
                    SteamVR_Actions.tools.DPadLeft,
                    SteamVR_Actions.tools.DPadRight,
                    SteamVR_Actions.ship_tools.Use,
                    SteamVR_Actions.ship_tools.Secondary,
                    SteamVR_Actions.ship_tools.DPadUp,
                    SteamVR_Actions.ship_tools.DPadDown,
                    SteamVR_Actions.ship_tools.DPadLeft,
                    SteamVR_Actions.ship_tools.DPadRight);

                ControllerPlatform.Changed += OnPlatformChanged;
                ControllerPlatform.Initialize();
            }

            private static void RefreshWhenBindingChanges(params SteamVR_Action_Boolean[] actions)
            {
                foreach (var action in actions)
                {
                    action.AddOnActiveBindingChangeListener((fromAction, fromSource, active) => InvalidateButtonImages(), SteamVR_Input_Sources.Any);
                }
            }

            private void OnPlatformChanged()
            {
                pathCache.Clear(); //Invalidate learned paths
                InvalidateButtonImages();
            }

            internal static void InvalidateButtonImages()
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
                    // CompositeInputCommands inherits GetUITextures, Harmony can only patch the declaring class.
                    Prefix<AbstractCompositeInputCommands<IInputActionPair>>(nameof(AbstractCompositeInputCommands<IInputActionPair>.GetUITextures), nameof(GetVruiTextures));
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
                    
                    var texture = Instance.GetTexture($"{baseAssetPath}/{ControllerPlatform.Current}/{name}");
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