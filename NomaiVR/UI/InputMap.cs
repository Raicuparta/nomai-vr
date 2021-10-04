using System.Collections.Generic;
using Valve.VR;
using static InputConsts;

namespace NomaiVR.UI
{
    public static class InputMap
    {
        public static Dictionary<InputCommandType, ISteamVR_Action> Map = new Dictionary<InputCommandType, ISteamVR_Action>()
        {
            { InputCommandType.INTERACT, SteamVR_Actions._default.Interact },
            { InputCommandType.JUMP, SteamVR_Actions._default.Jump }
        };
    }
}