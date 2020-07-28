using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace NomaiVR
{
    public class Hand : MonoBehaviour
    {
        public GameObject handPrefab;
        public GameObject glovePrefab;
        public SteamVR_Action_Pose pose;
        public bool isLeft;
        //private List<SteamVR_RenderModel> _controllerModels;

        internal void Start()
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

            // TODO remove  timer
            Invoke(nameof(CreateControllerModel), 1);
        }

        private void CreateControllerModel()
        {
            var hintsObject = new GameObject();
            hintsObject.transform.parent = transform;
            hintsObject.transform.localPosition = Vector3.zero;
            hintsObject.transform.localRotation = Quaternion.identity;
            //hintsObject.SetActive(false);
            var buttonHints = hintsObject.AddComponent<ButtonHints>();
            buttonHints.debugHints = true;
            buttonHints.SetInputSource(isLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand);
            buttonHints.autoSetWithControllerRangeOfMotion = true;
            buttonHints.OnHandInitialized(isLeft ? 1 : 2);

            //buttonHints.ShowButtonHint(SteamVR_Actions._default.Jump);
            //buttonHints.ShowButtonHint(SteamVR_Actions._default.Back);
            // hintsObject.SetActive(true);

            //var renderModelObject = new GameObject();
            //renderModelObject.SetActive(false);
            //var controllerModel = renderModelObject.AddComponent<SteamVR_RenderModel>();
            //controllerModel.verbose = true;
            //controllerModel.updateDynamically = true;
            //controllerModel.createComponents = true;
            //controllerModel.transform.parent = hintsObject.transform;
            //controllerModel.transform.localPosition = Vector3.zero;
            //controllerModel.transform.localRotation = Quaternion.identity;
            //renderModelObject.SetActive(true);

            //_controllerModels.Add(controllerModel);
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
