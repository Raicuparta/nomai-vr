using UnityEngine;
using Valve.VR;

namespace NomaiVR
{
    public class Hand : MonoBehaviour
    {
        public GameObject handPrefab;
        public GameObject glovePrefab;
        public SteamVR_Action_Pose pose;
        public bool isLeft;

        private void Start()
        {
            var hand = Instantiate(handPrefab).transform;
            var glove = Instantiate(glovePrefab).transform;
            hand.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderHands;
            glove.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderGloves;

            void setupHandModel(Transform model)
            {
                model.parent = transform;
                model.localPosition = transform.localPosition;
                model.localRotation = transform.localRotation;
                model.localScale = Vector3.one * 6;
                if (isLeft)
                {
                    model.localScale = new Vector3(-model.localScale.x, model.localScale.y, model.localScale.z);
                }
            }

            setupHandModel(hand);
            setupHandModel(glove);

            gameObject.SetActive(false);
            var poseDriver = transform.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;
            gameObject.SetActive(true);
        }

        private bool ShouldRenderGloves()
        {
            return SceneHelper.IsInGame() && Locator.GetPlayerSuit().IsWearingSuit(true);
        }

        private bool ShouldRenderHands()
        {
            return !SceneHelper.IsInGame() || !Locator.GetPlayerSuit().IsWearingSuit(true);
        }
    }
}
