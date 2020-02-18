using UnityEngine;

namespace NomaiVR {
    class ForceSettings: MonoBehaviour {
        void Awake () {
            var graphicSettings = PlayerData.GetGraphicSettings();
            graphicSettings.displayResHeight = 720;
            graphicSettings.displayResWidth = 1280;
            graphicSettings.aspectRatio = AspectRatio.SIXTEEN_NINE;

            graphicSettings.ApplyAllGraphicSettings();

            // Prevent changing graphics settings.
            NomaiVR.Empty<GraphicSettings>("ApplyAllGraphicSettings");
        }
    }
}
