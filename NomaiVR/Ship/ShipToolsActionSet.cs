using NomaiVR.Helpers;
using NomaiVR.UI;
using UnityEngine;
using Valve.VR;

namespace NomaiVR.Ship
{
    // Activates the ship_tools action set while the ship scout or signalscope is active.
    internal class ShipToolsActionSet : NomaiVRModule<ShipToolsActionSet.Behaviour, NomaiVRModule.EmptyPatch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => SolarSystemScene;

        public class Behaviour : MonoBehaviour
        {
            private bool isActive;

            internal void Update()
            {
                var shouldBeActive = ToolHelper.IsUsingAnyTool(ToolGroup.Ship);
                if (shouldBeActive != isActive) SetActive(shouldBeActive);
            }

            internal void OnDestroy()
            {
                if (isActive) SetActive(false);
            }

            private void SetActive(bool active)
            {
                isActive = active;
                if (active)
                {
                    SteamVR_Actions.ship_tools.Activate(SteamVR_Input_Sources.Any, 1);
                }
                else
                {
                    SteamVR_Actions.ship_tools.Deactivate(SteamVR_Input_Sources.Any);
                }
                InputPrompts.Behaviour.InvalidateButtonImages();
            }
        }
    }
}
