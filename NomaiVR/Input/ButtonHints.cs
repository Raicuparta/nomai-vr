using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Valve.VR;

namespace NomaiVR
{
    //-------------------------------------------------------------------------
    public class ButtonHints : MonoBehaviour
    {
        public Material controllerMaterial = new Material(Shader.Find("Standard"));
        public Color flashColor = Color.yellow;

        public SteamVR_Action_Vibration hapticFlash = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");

        public bool autoSetWithControllerRangeOfMotion = true;

        [Header("Debug")]
        public bool debugHints = false;

        private SteamVR_RenderModel renderModel;

        private List<MeshRenderer> renderers = new List<MeshRenderer>();
        private List<MeshRenderer> flashingRenderers = new List<MeshRenderer>();
        private float startTime;
        private float tickCount;

        private enum OffsetType
        {
            Up,
            Right,
            Forward,
            Back
        }

        //Info for each of the buttons
        private class ActionHintInfo
        {
            public string componentName;
            public List<MeshRenderer> renderers;
            public Transform localTransform;
        }

        private Dictionary<ISteamVR_Action_In_Source, ActionHintInfo> actionHintInfos;

        private int colorID;

        public bool initialized { get; private set; }
        private Vector3 centerPosition = Vector3.zero;

        SteamVR_Events.Action renderModelLoadedAction;

        protected SteamVR_Input_Sources inputSource;

        //-------------------------------------------------
        void Awake()
        {
            renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(OnRenderModelLoaded);
            colorID = Shader.PropertyToID("_Color");
        }


        //-------------------------------------------------
        private void HintDebugLog(string msg)
        {
            if (debugHints)
            {
                NomaiVR.Log("<b>[SteamVR Interaction]</b> Hints: " + msg);
            }
        }


        //-------------------------------------------------
        void OnEnable()
        {
            renderModelLoadedAction.enabled = true;
        }


        //-------------------------------------------------
        void OnDisable()
        {
            renderModelLoadedAction.enabled = false;
            Clear();
        }


        //-------------------------------------------------
        private void OnParentHandInputFocusLost()
        {
            //Hide all the hints when the controller is no longer the primary attached object
            HideAllButtonHints();
        }


        public virtual void SetInputSource(SteamVR_Input_Sources newInputSource)
        {
            inputSource = newInputSource;
            if (renderModel != null)
                renderModel.SetInputSource(newInputSource);
        }

        //-------------------------------------------------
        // Gets called when the hand has been initialized and a render model has been set
        //-------------------------------------------------
        public void OnHandInitialized(int deviceIndex)
        {
            //Create a new render model for the controller hints
            renderModel = new GameObject("SteamVR_RenderModel").AddComponent<SteamVR_RenderModel>();
            renderModel.transform.parent = transform;
            renderModel.transform.localPosition = Vector3.zero;
            renderModel.transform.localRotation = Quaternion.identity;
            renderModel.transform.localScale = Vector3.one;

            renderModel.SetInputSource(inputSource);
            renderModel.SetDeviceIndex(deviceIndex);

            if (!initialized)
            {
                //The controller hint render model needs to be active to get accurate transforms for all the individual components
                renderModel.gameObject.SetActive(true);
            }
        }


        private Dictionary<string, Transform> componentTransformMap = new Dictionary<string, Transform>();

        //-------------------------------------------------
        void OnRenderModelLoaded(SteamVR_RenderModel renderModel, bool succeess)
        {
            //Only initialize when the render model for the controller hints has been loaded
            if (renderModel == this.renderModel)
            {
                NomaiVR.Log("<b>[SteamVR Interaction]</b> OnRenderModelLoaded: " + this.renderModel.renderModelName);
                if (initialized)
                {
                    componentTransformMap.Clear();
                    flashingRenderers.Clear();
                }

                renderModel.SetMeshRendererState(false);

                StartCoroutine(DoInitialize(renderModel));
                StartCoroutine(TestButtonHints());
            }
        }
        private IEnumerator DoInitialize(SteamVR_RenderModel renderModel)
        {
            NomaiVR.Log("DoInitialize");
            while (renderModel.initializedAttachPoints == false)
                yield return null;


            NomaiVR.Log("Post yield");

            //Get the button mask for each component of the render model

            var renderModels = OpenVR.RenderModels;
            if (renderModels != null)
            {
                string renderModelDebug = "";

                if (debugHints)
                    renderModelDebug = "Components for render model " + renderModel.index;

                for (int childIndex = 0; childIndex < renderModel.transform.childCount; childIndex++)
                {
                    Transform child = renderModel.transform.GetChild(childIndex);

                    if (componentTransformMap.ContainsKey(child.name))
                    {
                        if (debugHints)
                            renderModelDebug += "\n\t!    Child component already exists with name: " + child.name;
                    }
                    else
                        componentTransformMap.Add(child.name, child);

                    if (debugHints)
                        renderModelDebug += "\n\t" + child.name + ".";
                }

                //Uncomment to show the button mask for each component of the render model
                HintDebugLog(renderModelDebug);
            }


            NomaiVR.Log("actionHintInfos");
            actionHintInfos = new Dictionary<ISteamVR_Action_In_Source, ActionHintInfo>();

            for (int actionIndex = 0; actionIndex < SteamVR_Input.actionsNonPoseNonSkeletonIn.Length; actionIndex++)
            {
                ISteamVR_Action_In action = SteamVR_Input.actionsNonPoseNonSkeletonIn[actionIndex];

                if (action.GetActive(inputSource))
                    CreateAndAddButtonInfo(action, inputSource);
            }

            initialized = true;

            //Set the controller hints render model to not active
            renderModel.SetMeshRendererState(true);
            renderModel.gameObject.SetActive(false);
        }


        //-------------------------------------------------
        private void CreateAndAddButtonInfo(ISteamVR_Action_In action, SteamVR_Input_Sources inputSource)
        {
            Transform buttonTransform = null;
            List<MeshRenderer> buttonRenderers = new List<MeshRenderer>();

            StringBuilder buttonDebug = new StringBuilder();
            buttonDebug.Append("Looking for action: ");

            buttonDebug.AppendLine(action.GetShortName());

            buttonDebug.Append("Action localized origin: ");
            buttonDebug.AppendLine(action.GetLocalizedOrigin(inputSource));

            string actionComponentName = action.GetRenderModelComponentName(inputSource);

            if (componentTransformMap.ContainsKey(actionComponentName))
            {
                buttonDebug.AppendLine(string.Format("Found component: {0} for {1}", actionComponentName, action.GetShortName()));
                Transform componentTransform = componentTransformMap[actionComponentName];

                buttonTransform = componentTransform;

                buttonDebug.AppendLine(string.Format("Found componentTransform: {0}. buttonTransform: {1}", componentTransform, buttonTransform));

                buttonRenderers.AddRange(componentTransform.GetComponentsInChildren<MeshRenderer>());
            }
            else
            {
                buttonDebug.AppendLine(string.Format("Can't find component transform for action: {0}. Component name: \"{1}\"", action.GetShortName(), actionComponentName));
            }

            buttonDebug.AppendLine(string.Format("Found {0} renderers for {1}", buttonRenderers.Count, action.GetShortName()));

            foreach (MeshRenderer renderer in buttonRenderers)
            {
                buttonDebug.Append("\t");
                buttonDebug.AppendLine(renderer.name);
            }

            HintDebugLog(buttonDebug.ToString());

            if (buttonTransform == null)
            {
                HintDebugLog("Couldn't find buttonTransform for " + action.GetShortName());
                return;
            }

            ActionHintInfo hintInfo = new ActionHintInfo();
            actionHintInfos.Add(action, hintInfo);

            hintInfo.componentName = buttonTransform.name;
            hintInfo.renderers = buttonRenderers;

            //Get the local transform for the button
            for (int childIndex = 0; childIndex < buttonTransform.childCount; childIndex++)
            {
                Transform child = buttonTransform.GetChild(childIndex);
                if (child.name == SteamVR_RenderModel.k_localTransformName)
                    hintInfo.localTransform = child;
            }
        }

        //-------------------------------------------------
        public void ShowButtonHint(params ISteamVR_Action_In_Source[] actions)
        {
            NomaiVR.Log("A");
            renderModel.gameObject.SetActive(true);

            renderModel.GetComponentsInChildren<MeshRenderer>(renderers);
            for (int i = 0; i < renderers.Count; i++)
            {
                NomaiVR.Log($"B-{i}");
                Texture mainTexture = renderers[i].material.mainTexture;
                NomaiVR.Log($"B-2-{i}");
                renderers[i].sharedMaterial = controllerMaterial;
                NomaiVR.Log($"B-3-{i}");
                renderers[i].material.mainTexture = mainTexture;
                NomaiVR.Log($"B-4-{i}");

                // This is to poke unity into setting the correct render queue for the model
                renderers[i].material.renderQueue = controllerMaterial.shader.renderQueue;
                NomaiVR.Log($"B-5-{i}");
            }

            for (int i = 0; i < actions.Length; i++)
            {
                NomaiVR.Log($"C-{i}");
                NomaiVR.Log($"post-C-{actionHintInfos == null}");
                if (actionHintInfos.ContainsKey(actions[i]))
                {
                    NomaiVR.Log($"pre-D-{i}");
                    ActionHintInfo hintInfo = actionHintInfos[actions[i]];
                    NomaiVR.Log($"D-{i}");
                    foreach (MeshRenderer renderer in hintInfo.renderers)
                    {
                        NomaiVR.Log($"E-{i}-{renderer.name}");
                        if (!flashingRenderers.Contains(renderer))
                        {
                            flashingRenderers.Add(renderer);
                        }
                    }
                }
            }

            startTime = Time.realtimeSinceStartup;
            tickCount = 0.0f;
        }


        //-------------------------------------------------
        private void HideAllButtonHints()
        {
            Clear();

            if (renderModel != null && renderModel.gameObject != null)
                renderModel.gameObject.SetActive(false);
        }


        //-------------------------------------------------
        private void HideButtonHint(params ISteamVR_Action_In_Source[] actions)
        {
            Color baseColor = controllerMaterial.GetColor(colorID);
            for (int i = 0; i < actions.Length; i++)
            {
                if (actionHintInfos.ContainsKey(actions[i]))
                {
                    ActionHintInfo hintInfo = actionHintInfos[actions[i]];
                    foreach (MeshRenderer renderer in hintInfo.renderers)
                    {
                        renderer.material.color = baseColor;
                        flashingRenderers.Remove(renderer);
                    }
                }
            }

            if (flashingRenderers.Count == 0)
            {
                renderModel.gameObject.SetActive(false);
            }
        }




        //-------------------------------------------------
        private bool IsButtonHintActive(ISteamVR_Action_In_Source action)
        {
            if (actionHintInfos.ContainsKey(action))
            {
                ActionHintInfo hintInfo = actionHintInfos[action];
                foreach (MeshRenderer buttonRenderer in hintInfo.renderers)
                {
                    if (flashingRenderers.Contains(buttonRenderer))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        //-------------------------------------------------
        private IEnumerator TestButtonHints()
        {
            while (true)
            {
                for (int actionIndex = 0; actionIndex < SteamVR_Input.actionsNonPoseNonSkeletonIn.Length; actionIndex++)
                {
                    ISteamVR_Action_In action = SteamVR_Input.actionsNonPoseNonSkeletonIn[actionIndex];
                    if (action.GetActive(inputSource))
                    {
                        ShowButtonHint(action);
                        yield return new WaitForSeconds(1.0f);
                    }
                    yield return null;
                }
            }
        }


        //-------------------------------------------------
        void Update()
        {
            if (renderModel != null && renderModel.gameObject.activeInHierarchy && flashingRenderers.Count > 0)
            {
                Color baseColor = controllerMaterial.GetColor(colorID);

                float flash = (Time.realtimeSinceStartup - startTime) * Mathf.PI * 2.0f;
                flash = Mathf.Cos(flash);
                flash = Valve.VR.InteractionSystem.Util.RemapNumberClamped(flash, -1.0f, 1.0f, 0.0f, 1.0f);

                float ticks = (Time.realtimeSinceStartup - startTime);
                if (ticks - tickCount > 1.0f)
                {
                    tickCount += 1.0f;
                    hapticFlash.Execute(0, 0.005f, 0.005f, 1, inputSource);
                }

                for (int i = 0; i < flashingRenderers.Count; i++)
                {
                    Renderer r = flashingRenderers[i];
                    r.material.SetColor(colorID, Color.Lerp(baseColor, flashColor, flash));
                }
            }
        }


        //-------------------------------------------------
        private void Clear()
        {
            renderers.Clear();
            flashingRenderers.Clear();
        }


        //-------------------------------------------------
        // These are the static functions which are used to show/hide the hints
        //-------------------------------------------------

        //-------------------------------------------------
        private static ButtonHints GetButtonHints(Hand hand)
        {
            if (hand != null)
            {
                ButtonHints hints = hand.GetComponentInChildren<ButtonHints>();
                if (hints != null && hints.initialized)
                {
                    return hints;
                }
            }

            return null;
        }


        //-------------------------------------------------
        public static void ShowButtonHint(Hand hand, params ISteamVR_Action_In_Source[] actions)
        {
            ButtonHints hints = GetButtonHints(hand);
            if (hints != null)
            {
                hints.ShowButtonHint(actions);
            }
            else
            {
                NomaiVR.Log("it was indeed null");
            }
        }


        //-------------------------------------------------
        public static void HideButtonHint(Hand hand, params ISteamVR_Action_In_Source[] actions)
        {
            ButtonHints hints = GetButtonHints(hand);
            if (hints != null)
            {
                hints.HideButtonHint(actions);
            }
        }


        //-------------------------------------------------
        public static void HideAllButtonHints(Hand hand)
        {
            ButtonHints hints = GetButtonHints(hand);
            if (hints != null)
            {
                hints.HideAllButtonHints();
            }
        }


        //-------------------------------------------------
        public static bool IsButtonHintActive(Hand hand, ISteamVR_Action_In_Source action)
        {
            ButtonHints hints = GetButtonHints(hand);
            if (hints != null)
            {
                return hints.IsButtonHintActive(action);
            }

            return false;
        }
    }
}
