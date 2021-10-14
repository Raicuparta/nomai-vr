using UnityEngine;
using Valve.VR;

namespace NomaiVR.Input.ActionInputs
{
    public class EmptyActionInput : ActionInput<ISteamVR_Action_In>
    {
        public override Vector2 Value => Vector2.zero;
        public override bool Active => false;
        public string TexturePath { get; private set; }

        public EmptyActionInput(string texturePath = null) : base(null)
        {
            TexturePath = texturePath;
        }
    }
}