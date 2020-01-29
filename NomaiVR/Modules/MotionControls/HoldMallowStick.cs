using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class HoldMallowStick: MonoBehaviour {
        void Awake () {

            // Stop stick rotation animation.
            NomaiVR.Helper.HarmonyHelper.EmptyMethod<RoastingStickController>("UpdateRotation");

            var scale = Vector3.one * 0.75f;
            var stickController = Locator.GetPlayerBody().transform.Find("RoastingSystem").GetComponent<RoastingStickController>();

            // Move the stick forward while not pressing RT.
            stickController.SetValue("_stickMinZ", 1f);

            var stickRoot = stickController.transform.Find("Stick_Root/Stick_Pivot");
            stickRoot.localScale = scale;
            Hands.HoldObject(stickRoot, Hands.RightHand, new Vector3(-0.08f, -0.07f, -0.32f));

            var mallow = stickRoot.Find("Stick_Tip/Mallow_Root").GetComponent<Marshmallow>();

            void EatMallow () {
                if (mallow.GetState() != Marshmallow.MallowState.Gone) {
                    mallow.Eat();
                }
            }

            void ReplaceMallow () {
                if (mallow.GetState() == Marshmallow.MallowState.Gone) {
                    mallow.SpawnMallow(true);
                }
            }

            bool ShouldRenderMallowClone () {
                return stickController.enabled && mallow.GetState() == Marshmallow.MallowState.Gone;
            }

            bool ShouldRenderStickClone () {
                return !stickController.enabled;
            }

            // Eat mallow by moving it to player head.
            var eatDetector = mallow.gameObject.AddComponent<ProximityDetector>();
            eatDetector.other = Common.PlayerHead;
            eatDetector.onEnter += EatMallow;

            // Hide arms that are part of the stick object.
            var meshes = stickRoot.Find("Stick_Tip/Props_HEA_RoastingStick");
            meshes.Find("RoastingStick_Arm").gameObject.SetActive(false);
            meshes.Find("RoastingStick_Arm_NoSuit").gameObject.SetActive(false);

            // Hold mallow in left hand for replacing the one in the stick.
            var mallowModel = mallow.transform.Find("Props_HEA_Marshmallow");
            var mallowClone = Instantiate(mallowModel);
            mallowClone.GetComponent<MeshRenderer>().material.color = Color.white;
            mallowClone.localScale = scale;
            Hands.HoldObject(mallowClone, Hands.LeftHand, new Vector3(0.06f, -0.03f, -0.02f));

            // Replace right hand mallow on proximity with left hand mallow.
            var replaceDetector = mallowClone.gameObject.AddComponent<ProximityDetector>();
            replaceDetector.other = mallow.transform;
            replaceDetector.onEnter += ReplaceMallow;

            // Render left hand mallow only when right hand mallow is not present.
            mallowClone.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderMallowClone;

            // Add a stick to every campfire.
            // Picking up the stick starts roasting mode.
            var campfires = GameObject.FindObjectsOfType<Campfire>();
            foreach (var campfire in campfires) {
                void StartRoasting () {
                    campfire.Invoke("StartRoasting");
                }
                var stickClone = Instantiate(meshes.Find("RoastingStick_Stick"));
                var stickCloneMallow = Instantiate(mallowModel);
                stickCloneMallow.parent = stickClone;
                stickCloneMallow.localPosition = new Vector3(0, 0, 1.8f);
                stickCloneMallow.localRotation = Quaternion.Euler(145, -85, -83);
                stickClone.gameObject.SetActive(true);
                stickClone.localScale = scale;
                stickClone.parent = campfire.transform;
                stickClone.localPosition = new Vector3(1.44f, 0, .019f);
                stickClone.localRotation = Quaternion.Euler(-100, 125, -125);

                var detector = stickCloneMallow.gameObject.AddComponent<ProximityDetector>();
                detector.other = Hands.RightHand;
                detector.minDistance = 0.4f;
                detector.onEnter += StartRoasting;

                stickClone.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderStickClone;
            }

            // Disable prompts.
            stickController.SetValue("_tiltPrompt", new ScreenPrompt(""));
            stickController.SetValue("_extendPrompt", new ScreenPrompt(""));
            stickController.SetValue("_mallowPrompt", new ScreenPrompt(""));
            stickController.SetValue("_removePrompt", new ScreenPrompt(""));
            stickController.SetValue("_exitPrompt", new ScreenPrompt(""));
            //stickController.GetValue<ScreenPrompt>("_tiltPrompt").SetValue("_commandList", new List<InputCommand>());
            //stickController.GetValue<ScreenPrompt>("_extendPrompt").SetValue("_commandList", new List<InputCommand>());
            //stickController.GetValue<ScreenPrompt>("_mallowPrompt").SetValue("_commandList", new List<InputCommand>());
            //stickController.GetValue<ScreenPrompt>("_removePrompt").SetValue("_commandList", new List<InputCommand>());
            //stickController.GetValue<ScreenPrompt>("_exitPrompt").SetValue("_commandList", new List<InputCommand>());
        }
    }
}
