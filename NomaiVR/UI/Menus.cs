using OWML.Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NomaiVR
{
    public class Menus : NomaiVRModule<Menus.Behaviour, Menus.Behaviour.Patch>
    {
        protected override bool isPersistent => false;
        protected override OWScene[] scenes => AllScenes;

        public class Behaviour : MonoBehaviour
        {
            private void Start()
            {
                var scene = LoadManager.GetCurrentScene();

                if (scene == OWScene.SolarSystem)
                {
                    FixSleepTimerCanvas();
                }
                else if (scene == OWScene.TitleScreen)
                {
                    FixTitleMenuCanvases();
                }
                ScreenCanvasesToWorld();
            }

            private void FixSleepTimerCanvas()
            {
                // Make sleep timer canvas visible while eyes closed.
                Locator.GetUIStyleManager().transform.Find("SleepTimerCanvas").gameObject.layer = LayerMask.NameToLayer("VisibleToPlayer");
            }

            private void FixTitleMenuCanvases()
            {
                var titleMenu = GameObject.Find("TitleMenu").transform;

                // Hide the main menu while other menus are open,
                // to prevent selecting with laser.
                var titleCanvas = titleMenu.Find("TitleCanvas");
                titleMenu.Find("TitleCanvas").gameObject.AddComponent<ConditionalRenderer>().getShouldRender = () =>
                    MenuStackManager.SharedInstance.GetMenuCount() == 0;

                // Cant't get the footer to look good, so I'm hiding it.
                titleCanvas.Find("FooterBlock").gameObject.SetActive(false);
            }

            private void ScreenCanvasesToWorld()
            {
                var canvases = FindObjectsOfType<Canvas>();
                foreach (var canvas in canvases)
                {
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        canvas.renderMode = RenderMode.WorldSpace;
                        canvas.transform.localScale *= 0.001f;
                        var followTarget = canvas.gameObject.AddComponent<FollowTarget>();
                        followTarget.target = SceneHelper.IsInGame() ? Locator.GetPlayerTransform() : Camera.main.transform.parent;
                        var z = SceneHelper.IsInGame() ? 1f : 1.5f;
                        var y = SceneHelper.IsInGame() ? 0.5f : 1f;
                        followTarget.localPosition = new Vector3(0, y, z);
                    }
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    NomaiVR.Post<ProfileMenuManager>("PopulateProfiles", typeof(Patch), nameof(PostPopulateProfiles));
                    NomaiVR.Post<CanvasMarkerManager>("Start", typeof(Patch), nameof(PostMarkerManagerStart));
                }

                private static void PostPopulateProfiles(GameObject ____profileListRoot)
                {
                    foreach (Transform child in ____profileListRoot.transform)
                    {
                        child.localPosition = Vector3.zero;
                        child.localRotation = Quaternion.identity;
                        child.localScale = Vector3.one;
                    }
                }

                private static void PostMarkerManagerStart(CanvasMarkerManager __instance)
                {
                    __instance.GetComponent<Canvas>().planeDistance = 5;
                }
            }
        }
    }
}