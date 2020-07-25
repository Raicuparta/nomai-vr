using OWML.ModHelper.Events;
using UnityEngine;
using UnityEngine.UI;

namespace NomaiVR
{
    internal class Dialogue : NomaiVRModule<Dialogue.Behaviour, Dialogue.Behaviour.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        public class Behaviour : MonoBehaviour
        {
            private static Transform _canvasTransform;
            private static Transform _attentionPoint = null;
            private const float _dialogeRenderSize = 0.0015f;

            internal void Start()
            {
                _canvasTransform = GameObject.Find("DialogueCanvas").transform;

                _canvasTransform.localScale *= _dialogeRenderSize;

                // Prevent dialogue box from flying off after a while.
                _canvasTransform.parent = new GameObject().transform;
                _canvasTransform.parent.gameObject.AddComponent<FollowTarget>().target = Locator.GetPlayerTransform();

                var canvas = _canvasTransform.gameObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
            }

            internal void Update()
            {
                if (_attentionPoint != null && _canvasTransform != null)
                {
                    var headPosition = PlayerHelper.PlayerHead.position;

                    _canvasTransform.LookAt(2 * _attentionPoint.position - headPosition, PlayerHelper.PlayerHead.up);

                    // Move so it is 1 unit away from the player
                    var offset = (_attentionPoint.position - headPosition).normalized;
                    _canvasTransform.position = headPosition + offset;
                }
            }

            public class Patch : NomaiVRPatch
            {
                public override void ApplyPatches()
                {
                    Pre<CharacterDialogueTree>("StartConversation", nameof(PreStartConversation));
                    Post<CharacterDialogueTree>("StartConversation", nameof(PostStartConversation));
                    Pre<CharacterDialogueTree>("EndConversation", nameof(PreEndConversation));
                    Post<DialogueOptionUI>("Awake", nameof(PostDialogueOptionAwake));
                    Post<DialogueOptionUI>("SetSelected", nameof(PreSetButtonPromptImage));
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
                    _attentionPoint = __instance.GetValue<Transform>("_attentionPoint");
                }

                private static void PostStartConversation()
                {
                    MaterialHelper.MakeGraphicChildrenDrawOnTop(_canvasTransform.gameObject);
                }

                private static void PreEndConversation()
                {
                    _attentionPoint = null;
                }
            }
        }
    }
}
