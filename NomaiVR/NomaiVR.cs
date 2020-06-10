﻿using OWML.Common;
using OWML.ModHelper;
using System;
using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    public class NomaiVR : ModBehaviour
    {
        public static IModHelper Helper;
        public static ModSaveFile Save;
        public static ModConfig Config;

        private void Start()
        {
            Helper.Console.WriteLine("Start NomaiVR");
            Save = ModHelper.Storage.Load<ModSaveFile>(ModSaveFile.FileName);

            SteamVR.Initialize();
            new AssetLoader();

            // Load all modules.
            // I'm sorry to say that order does matter here.
            new ForceSettings();
            new ControllerInput();
            new Dialogue();
            new FogFix();
            new LoopTransitionFix();
            new WaterFix();
            new ProjectionStoneCameraFix();
            new CameraMaskFix();
            new MapFix();
            new PlayerBodyPosition();
            new HandsController();
            new ShipTools();
            new FlashlightGesture();
            new HoldMallowStick();
            new HoldProbeLauncher();
            new HoldSignalscope();
            new HoldTranslator();
            new HoldItem();
            new HoldPrompts();
            new LaserPointer();
            new FeetMarker();
            new HelmetHUD();
            new InputPrompts();
            new VRTutorial();
            new PostCreditsFix();
            new LookArrow();
            new Menus();
        }

        public override void Configure(IModConfig config)
        {
            Helper = ModHelper;
            Config = new ModConfig
            {
                debugMode = config.GetSettingsValue<bool>("debugMode"),
                showMirrorView = config.GetSettingsValue<bool>("showMirrorView"),
                overrideRefreshRate = config.GetSettingsValue<int>("overrideRefreshRate"),
                preventCursorLock = config.GetSettingsValue<bool>("preventCursorLock"),
                showHelmet = config.GetSettingsValue<bool>("showHelmet"),
                vibrationStrength = config.GetSettingsValue<float>("vibrationStrength"),
            };
        }

        public static void Log(params object[] strings)
        {
            if (Helper != null && (Config == null || Config.debugMode))
            {
                Helper.Console.WriteLine(strings);
            }
        }

        public static void Pre<T>(string methodName, Type patchType, string patchMethodName)
        {
            Helper.HarmonyHelper.AddPrefix<T>(methodName, patchType, patchMethodName);
        }

        public static void Post<T>(string methodName, Type patchType, string patchMethodName)
        {
            Helper.HarmonyHelper.AddPostfix<T>(methodName, patchType, patchMethodName);
        }

        public static void Empty<T>(string methodName)
        {
            Helper.HarmonyHelper.EmptyMethod<T>(methodName);
        }
    }
}
