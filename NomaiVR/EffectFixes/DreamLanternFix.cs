using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NomaiVR
{
    internal class DreamLanternFix : NomaiVRModule<NomaiVRModule.EmptyBehaviour, DreamLanternFix.Patch>
    {
        protected override bool IsPersistent => false;
        protected override OWScene[] Scenes => PlayableScenes;

        

        public class Patch : NomaiVRPatch
        {
            static Shader prepassShader = Shader.Find("Outer Wilds/Utility/View Model Prepass");
            static Shader standardVM = Shader.Find("Outer Wilds/Utility/View Model");
            static Shader standard = Shader.Find("Standard");
            static Shader standardTranslucent = Shader.Find("Standard (Translucent)");
            static Shader flameVM = Shader.Find("Outer Wilds/Effects/Flame (View Model)");
            static Shader flame = Shader.Find("Outer Wilds/Effects/Flame"); 

            static Dictionary<Shader, Shader> s_dreamShaderFix = new Dictionary<Shader, Shader>()
            {
                [standardVM] = standard,
                [flameVM] = flame
            };

            public override void ApplyPatches()
            {
                Postfix<DreamLanternItem>(nameof(DreamLanternItem.Start), nameof(Fixup));
            }

            public static void Fixup(DreamLanternItem __instance)
            {
                //Fix rotations
                var viewModelTransform = __instance.transform.Find("Props_IP_Artifact_ViewModel") ?? __instance.transform.Find("ViewModel/Props_IP_Artifact_ViewModel");
                if(viewModelTransform != null)
                {
                    viewModelTransform.transform.localRotation = Quaternion.identity;
                    viewModelTransform.transform.localPosition = new Vector3(0, 0.35f, 0);
                }

                //Fix materials
                MaterialHelper.DisableRenderersWithShaderInChildren(__instance.gameObject, prepassShader);

                //Replace all the others
                MaterialHelper.ReplaceShadersInChildren(__instance.gameObject, s_dreamShaderFix);

                //Change the glass material to be translucent again
                var glassElement = viewModelTransform.Find("artifact_glass");

                if (glassElement != null)
                {
                    var renderer = glassElement.GetComponent<Renderer>();
                    renderer.material.shader = standardTranslucent;
                }
            }
        }
    }
}
