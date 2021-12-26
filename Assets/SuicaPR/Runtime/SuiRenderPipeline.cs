using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


public class SuiRenderPipeline : RenderPipeline
{
    private SuiCameraRenderer m_Renderer = new SuiCameraRenderer();
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            m_Renderer.Render(context, camera);
        }
    }
}
