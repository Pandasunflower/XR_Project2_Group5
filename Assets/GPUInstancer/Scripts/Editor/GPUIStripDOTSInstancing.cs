using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Build;
using UnityEditor.Rendering;

namespace GPUInstancer
{
    class GPUIStripDOTSInstancing : IPreprocessShaders
    {
        public int callbackOrder { get { return 0; } }
        private ShaderKeyword _DOTS_INSTANCING_ON_Keyword;
        private ShaderKeyword _PROCEDURAL_INSTANCING_ON_Keyword;

        public GPUIStripDOTSInstancing()
        {
            _DOTS_INSTANCING_ON_Keyword = new ShaderKeyword("DOTS_INSTANCING_ON");
            _PROCEDURAL_INSTANCING_ON_Keyword = new ShaderKeyword("PROCEDURAL_INSTANCING_ON");
        }

        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
        {
            bool isGPUIShader = shader.name.Contains("GPUInstancer");
            for (int i = 0; i < data.Count; ++i)
            {
                ShaderCompilerData compilerData = data[i];
                if (compilerData.shaderKeywordSet.IsEnabled(_DOTS_INSTANCING_ON_Keyword) && (isGPUIShader || compilerData.shaderKeywordSet.IsEnabled(_PROCEDURAL_INSTANCING_ON_Keyword))) // Remove variants with DOTS_INSTANCING_ON from GPUI shaders and remove variants with both DOTS_INSTANCING_ON and PROCEDURAL_INSTANCING_ON keyword
                {
                    data.RemoveAt(i);
                    --i;
                }
            }
        }
    }
}