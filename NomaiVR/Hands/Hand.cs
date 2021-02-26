using System.Linq;
using UnityEngine;
using Valve.VR;
using Valve.VR.Helpers;

namespace NomaiVR
{
    public class Hand : MonoBehaviour
    {
        public GameObject handPrefab;
        public SteamVR_Action_Pose pose;
        public SteamVR_Skeleton_Pose fallbackPoint;
        public SteamVR_Skeleton_Pose fallbackRelax;
        public SteamVR_Skeleton_Pose fallbackFist;
        public bool isLeft;

        public Transform Palm { get; private set; }

        private Renderer _handRenderer;
        private Renderer _gloveRenderer;
        private SteamVR_Behaviour_Skeleton _skeleton;

        internal void Start()
        {
            SetUpModel();
            SetUpVrPose();
            SetUpPoseEvents();
        }

        private void SetUpModel()
        {
            var handObject = Instantiate(handPrefab);
            handObject.SetActive(false);
            var hand = handObject.transform;

            var renderers = handObject.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            _handRenderer = renderers.Where(r => r.transform.name.Contains("Hand")).FirstOrDefault();
            _gloveRenderer = renderers.Where(r => r.transform.name.Contains("Glove")).FirstOrDefault();
            
            SetUpShaders(_handRenderer, "Outer Wilds/Character/Skin");
            SetUpShaders(_gloveRenderer, "Outer Wilds/Character/Clothes");
            
            _handRenderer.gameObject.AddComponent<ConditionalDisableRenderer>().getShouldRender += ShouldRenderHands;
            _gloveRenderer.gameObject.AddComponent<ConditionalDisableRenderer>().getShouldRender += ShouldRenderGloves;

            SetUpModel(hand);
            _skeleton = SetUpSkeleton(handObject, hand);

            Palm = handObject.transform.Find("skeletal_hand/Root/wrist_r/HoldPoint");

            handObject.SetActive(true);
        }

        private void SetUpShaders(Renderer renderer, string shader)
        {
            Shader toAssign = Shader.Find(shader);
            foreach (var material in renderer.materials)
                material.shader = toAssign;
        }

        private void SetUpModel(Transform model)
        {
            model.parent = transform;
            model.localPosition = Vector3.zero;
            model.localRotation = Quaternion.identity;
            model.localScale = Vector3.one;
        }

        internal void NotifyReachable(bool canReach)
        {
            if (canReach)
                _skeleton.BlendToAnimation();
            else
                _skeleton.BlendToSkeleton();
        }

        internal void NotifyAttachedTo(SteamVR_Skeleton_Poser poser)
        {
            _skeleton.BlendToPoser(poser);
        }

        internal void NotifyDetachedFrom(SteamVR_Skeleton_Poser poser)
        {
            _skeleton.BlendToSkeleton();
        }

        private static string BoneToTarget(string bone, bool isSource) => isSource ? $"SourceSkeleton/Root/{bone}" : $"skeletal_hand/Root/{bone}";
        
        private static string FingerBoneName(string fingerName, int depth)
        {
            string name = $"finger_{fingerName}_meta_r";
            for (int i = 0; i < depth; i++)
                name += $"/finger_{fingerName}_{i}_r";
            return name;
        }

        private static string ThumbBoneName(string fingerName, int depth)
        {
            string name = $"finger_{fingerName}_0_r";
            for (int i = 0; i < depth; i++)
                name += $"/finger_{fingerName}_{i + 1}_r";
            return name;
        }

        private SteamVR_Behaviour_Skeleton SetUpSkeleton(GameObject prefabObject, Transform prefabTransform)
        {
            var skeletonDriver = prefabObject.AddComponent<SteamVR_Behaviour_Skeleton>();
            skeletonDriver.inputSource = isLeft ? SteamVR_Input_Sources.LeftHand : SteamVR_Input_Sources.RightHand;
            skeletonDriver.rangeOfMotion = EVRSkeletalMotionRange.WithoutController;
            skeletonDriver.skeletonRoot = prefabTransform.Find("SourceSkeleton/Root");
            skeletonDriver.updatePose = false;
            skeletonDriver.skeletonBlend = 0.5f;
            skeletonDriver.fallbackPoser = prefabObject.AddComponent<SteamVR_Skeleton_Poser>();

            if (isLeft)
            {
                //Flip X axis of skeleton and skinned meshes
                for (int i = 0; i < prefabTransform.childCount; i++)
                    prefabTransform.GetChild(i).localScale = new Vector3(-1, 1, 1);

                //Enable SteamVR skeleton mirroring
                skeletonDriver.mirroring = SteamVR_Behaviour_Skeleton.MirrorType.RightToLeft;
            }
            skeletonDriver.skeletonAction = isLeft ? SteamVR_Actions.default_SkeletonLeftHand : SteamVR_Actions.default_SkeletonRightHand;

            skeletonDriver.fallbackCurlAction = SteamVR_Actions.default_Squeeze;
            skeletonDriver.enabled = true;

            var skeletonRetargeter = prefabObject.AddComponent<CustomSkeletonHelper>();
            var sourceWristTransform = prefabTransform.Find(BoneToTarget("wrist_r", true));
            var targetWristTransform = prefabTransform.Find(BoneToTarget("wrist_r", false));
            skeletonRetargeter.wrist = new CustomSkeletonHelper.Retargetable(sourceWristTransform, targetWristTransform);
            skeletonRetargeter.thumbs = new CustomSkeletonHelper.Thumb[1] {
                new CustomSkeletonHelper.Thumb(
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 0)), targetWristTransform.Find(ThumbBoneName("thumb", 0))), //Metacarpal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 1)), targetWristTransform.Find(ThumbBoneName("thumb", 1))), //Middle
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(ThumbBoneName("thumb", 2)), targetWristTransform.Find(ThumbBoneName("thumb", 2))), //Distal
                    prefabTransform.Find(BoneToTarget("finger_thumb_r_aux", true)) //aux
                )
            };
            skeletonRetargeter.fingers = new CustomSkeletonHelper.Finger[2]
            {
                new CustomSkeletonHelper.Finger(
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 0)), targetWristTransform.Find(FingerBoneName("index", 0))), //Metacarpal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 1)), targetWristTransform.Find(FingerBoneName("index", 1))), //Proximal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 2)), targetWristTransform.Find(FingerBoneName("index", 2))), //Middle
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("index", 3)), targetWristTransform.Find(FingerBoneName("index", 3))), //Distal
                    prefabTransform.Find(BoneToTarget("finger_index_r_aux", true)) //aux
                ),
                new CustomSkeletonHelper.Finger(
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 0)), targetWristTransform.Find(FingerBoneName("ring", 0))), //Metacarpal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 1)), targetWristTransform.Find(FingerBoneName("ring", 1))), //Proximal
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 2)), targetWristTransform.Find(FingerBoneName("ring", 2))), //Middle
                    new CustomSkeletonHelper.Retargetable(sourceWristTransform.Find(FingerBoneName("ring", 3)), targetWristTransform.Find(FingerBoneName("ring", 3))), //Distal
                    prefabTransform.Find(BoneToTarget("finger_ring_r_aux", true)) //aux
                ),
            };


            var skeletonPoser = skeletonDriver.fallbackPoser;
            skeletonPoser.skeletonMainPose = fallbackRelax;
            skeletonPoser.skeletonAdditionalPoses.Add(fallbackPoint);
            skeletonPoser.skeletonAdditionalPoses.Add(fallbackFist);

            //Point Fallback
            skeletonPoser.blendingBehaviours.Add(new SteamVR_Skeleton_Poser.PoseBlendingBehaviour()
            {
                action_bool = SteamVR_Actions.default_Grip,
                enabled = true,
                influence = 1,
                name = "point",
                pose = 1,
                value = 0,
                type = SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.BooleanAction,
                previewEnabled = true,
                smoothingSpeed = 16
            });

            //Fist Fallback
            skeletonPoser.blendingBehaviours.Add(new SteamVR_Skeleton_Poser.PoseBlendingBehaviour()
            {
                action_bool = SteamVR_Actions.default_GrabPinch,
                enabled = true,
                influence = 1,
                name = "fist",
                pose = 2,
                value = 0,
                type = SteamVR_Skeleton_Poser.PoseBlendingBehaviour.BlenderTypes.BooleanAction,
                previewEnabled = true,
                smoothingSpeed = 16
            });

            //Setup Limit Poser (not working :/)
            var defaultPoser = prefabObject.AddComponent<SteamVR_Skeleton_Poser>();
            defaultPoser.skeletonMainPose = AssetLoader.FallbackFistPose;
            skeletonDriver.BlendToPoser(defaultPoser);

            return skeletonDriver;
        }

        private void SetUpVrPose()
        {
            gameObject.SetActive(false);

            var poseDriver = transform.gameObject.AddComponent<SteamVR_Behaviour_Pose>();
            poseDriver.poseAction = pose;

            gameObject.SetActive(true);
        }

        private void SetUpPoseEvents()
        {
            GlobalMessenger<bool>.AddListener(ControllerModels.CONTROLLER_VISIBILITY_CHANGED, OnControllerModelsVisibilityChanged);
        }

        internal void OnDestroy()
        {
            GlobalMessenger<bool>.RemoveListener(ControllerModels.CONTROLLER_VISIBILITY_CHANGED, OnControllerModelsVisibilityChanged);
        }

        private void OnControllerModelsVisibilityChanged(bool isShown)
        {
            EVRSkeletalMotionRange newMotionRange = isShown ? EVRSkeletalMotionRange.WithController : EVRSkeletalMotionRange.WithoutController;
            _skeleton.rangeOfMotion = newMotionRange;
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
