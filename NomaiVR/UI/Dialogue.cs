using NomaiVR.Assets;
using NomaiVR.Helpers;
using NomaiVR.ReusableBehaviours;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR.UI
{
    internal class Dialogue : NomaiVRModule<Dialogue.Behaviour, Dialogue.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Transform canvasTransform;
            private static Transform attentionPoint;
            private const float dialogeRenderSize = 0.0015f;

            internal void Start()
            {
                // TODO: Oof, shouldn't rely on GameObject.Find for this.
                canvasTransform = GameObject.Find("DialogueCanvas").transform;

                canvasTransform.localScale *= dialogeRenderSize;

                // Prevent dialogue box from flying off after a while.
                canvasTransform.parent = new GameObject("VrDialogueWrapper").transform;
                canvasTransform.parent.gameObject.AddComponent<FollowTarget>().Target = Locator.GetPlayerTransform();

                var canvas = canvasTransform.gameObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
            }

            internal void Update()
            {
                if (attentionPoint != null && canvasTransform != null)
                {
                    var headPosition = PlayerHelper.PlayerHead.position;

                    canvasTransform.LookAt(2 * attentionPoint.position - headPosition, PlayerHelper.PlayerHead.up);

                    // Move so it is 1 unit away from the player
                    var offset = (attentionPoint.position - headPosition).normalized;
                    canvasTransform.position = headPosition + offset;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Prefix<CharacterDialogueTree>(nameof(CharacterDialogueTree.StartConversation), nameof(PreStartConversation));
                    Postfix<CharacterDialogueTree>(nameof(CharacterDialogueTree.StartConversation), nameof(PostStartConversation));
                    Prefix<CharacterDialogueTree>(nameof(CharacterDialogueTree.EndConversation), nameof(PreEndConversation));
                    Postfix<DialogueOptionUI>(nameof(DialogueOptionUI.Awake), nameof(PostDialogueOptionAwake));
                    Postfix<DialogueOptionUI>(nameof(DialogueOptionUI.SetSelected), nameof(PreSetButtonPromptImage));
                }

                private static void PreSetButtonPromptImage(Image ____buttonPromptImage)
                {
                    var texture = AssetLoader.EmptyTexture;
                    ____buttonPromptImage.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width);
                }

                private static void PostDialogueOptionAwake(DialogueOptionUI __instance)
                {
                    var text = __instance.GetComponentInChildren<Text>();
                    var collider = __instance.gameObject.AddComponent<BoxCollider>();

                    var rectTransform = text.GetComponent<RectTransform>();
                    var thickness = 10f;
                    var height = 40;
                    var width = rectTransform.rect.width;
                    collider.size = new Vector3(width, height, thickness);
                    collider.center = new Vector3(0, -height * 0.5f, thickness * 0.5f);

                    MaterialHelper.MakeGraphicChildrenDrawOnTop(__instance.gameObject);
                }

                private static void PreStartConversation(CharacterDialogueTree __instance)
                {
                    attentionPoint = __instance._attentionPoint;
                }

                private static void PostStartConversation()
                {
                    MaterialHelper.MakeGraphicChildrenDrawOnTop(canvasTransform.gameObject);
                }

                private static void PreEndConversation()
                {
                    attentionPoint = null;
                }
            }
        }
    }
}