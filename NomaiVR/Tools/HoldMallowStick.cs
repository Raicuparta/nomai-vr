using OWML.ModHelper.Events;
using UnityEngine;

namespace NomaiVR {
    public class HoldMallowStick: MonoBehaviour {
        void Awake () {

            var scale = Vector3.one * 0.75f;
            var stickController = Locator.GetPlayerBody().transform.Find("RoastingSystem").GetComponent<RoastingStickController>();

            // Move the stick forward while not pressing RT.
            stickController.SetValue("_stickMinZ", 1f);

            var stickRoot = stickController.transform.Find("Stick_Root/Stick_Pivot");
            stickRoot.localScale = scale;
            Hands.HoldObject(stickRoot, Hands.RightHand, new Vector3(-0.02f, -0.023f, -0.34f));

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

            // Eat mallow by moving it to player head.
            var eatDetector = mallow.gameObject.AddComponent<ProximityDetector>();
            eatDetector.other = Common.PlayerHead;
            eatDetector.minDistance = 0.2f;
            eatDetector.onEnter += EatMallow;

            // Hide arms that are part of the stick object.
            var meshes = stickRoot.Find("Stick_Tip/Props_HEA_RoastingStick");
            meshes.Find("RoastingStick_Arm").GetComponent<Renderer>().enabled = false;
            meshes.Find("RoastingStick_Arm_NoSuit").GetComponent<Renderer>().enabled = false;

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
        }

        internal static class Patches {
            public static void Patch () {
                // Stop stick rotation animation.
                NomaiVR.Empty<RoastingStickController>("UpdateRotation");
            }
        }
    }
}
