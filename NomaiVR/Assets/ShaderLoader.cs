using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace NomaiVR.Assets
{
    public class ShaderLoader
    {
        private static ShaderLoader instance = null;
        public static ShaderLoader Instance => instance ?? (instance = new ShaderLoader());

        private readonly Dictionary<string, Shader> shaderCache;

        private ShaderLoader() 
        {
            shaderCache = new Dictionary<string, Shader>();
        }

        private void LoadAllFromBundle(AssetBundle bundle)
        {
            if (bundle == null) return;
            foreach(var shader in bundle.LoadAllAssets<Shader>())
            {
                Logs.Write("Shader loaded: " + shader.name);
                shaderCache[shader.name] = shader;
            }
        }

        private Shader GetCachedShader(string shaderName)
        {
            return shaderCache[shaderName];
        }

        public static Shader GetShader(string shaderName) => ShaderLoader.Instance.GetCachedShader(shaderName);
        public static void LoadBundle(AssetBundle bundle) => ShaderLoader.Instance.LoadAllFromBundle(bundle);
    }
}
