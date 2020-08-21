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

        internal void Start()
        {
            SetUpHandModel();
            SetUpGloveModel();
            SetUpVrPose();
        }

        private void SetUpHandModel()
        {
            var hand = Instantiate(handPrefab).transform;
            hand.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderHands;
            hand.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Outer Wilds/Character/Skin");
            SetUpModel(hand);
        }

        private void SetUpGloveModel()
        {
            var glove = Instantiate(glovePrefab).transform;
            glove.gameObject.AddComponent<ConditionalRenderer>().getShouldRender += ShouldRenderGloves;
            glove.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Outer Wilds/Character/Clothes");
            SetUpModel(glove);
        }

        private void SetUpModel(Transform model)
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

        private void SetUpVrPose()
        {
            gameObject.SetActive(false);
            var poseDriver = transform.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;
            gameObject.SetActive(true);
        }

        private bool ShouldRenderGloves()
        {
            return SceneHelper.IsInGame() && PlayerHelper.IsWearingSuit();
        }

        private bool ShouldRenderHands()
        {
            return !SceneHelper.IsInGame() || !PlayerHelper.IsWearingSuit();
        }
    }
}
