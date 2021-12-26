using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SuiCameraRenderer
{
    private ScriptableRenderContext m_Context;
    private Camera m_Camera;

    private const string bufferName = "Render Camera";
    private CommandBuffer m_CommandBuffer = new CommandBuffer{name = bufferName};

    private CullingResults m_CullingResults;
    
    // Temp
    private static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    
    // Legacy Shader
    static ShaderTagId[] legacyShaderTagIds = {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM")
    };
    
    
    // Main Render Area
    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.m_Camera = camera;
        this.m_Context = context;

        if (!Cull())
        {
            return;
        }

        Setup();
        DrawOpaque();
        
        context.DrawSkybox(m_Camera);
        
        DrawTransparent();
        
        Submit();
        
    }

    void Setup()
    {
        m_Context.SetupCameraProperties(m_Camera); //先执行相机参数设置, 则不需要一个全屏面片去覆写
        m_CommandBuffer.ClearRenderTarget(true, true, Color.clear);
        m_CommandBuffer.BeginSample(bufferName);
        ExecuteBuffer();
        // m_Context.SetupCameraProperties(m_Camera); //若帧缓冲清楚命令在摄像机参数设置之前，则会用一个full-screen quad去覆写帧缓冲
    }
    
    void Submit()
    {   
        m_CommandBuffer.EndSample(bufferName);
        ExecuteBuffer();
        m_Context.Submit();
    }

    void ExecuteBuffer()
    {
        m_Context.ExecuteCommandBuffer(m_CommandBuffer);
        m_CommandBuffer.Clear();
    }

    bool Cull()
    {
        if (m_Camera.TryGetCullingParameters(out ScriptableCullingParameters parameters))
        {
            m_CullingResults = m_Context.Cull(ref parameters);
            return true;
        }

        return false;
    }
    
    // Draw Function

    void DrawOpaque()
    {
        var sortingSettings = new SortingSettings(m_Camera)
        {
            criteria = SortingCriteria.CommonOpaque
        }; 
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
        
        m_Context.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);
    }

    void DrawTransparent()
    {
        var sortingSettings = new SortingSettings(m_Camera)
        {
            criteria = SortingCriteria.CommonTransparent
        }; 
        var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        var filteringSettings = new FilteringSettings(RenderQueueRange.transparent);
        
        m_Context.DrawRenderers(m_CullingResults, ref drawingSettings, ref filteringSettings);
    }

    void DrawUnsupportedShaders()
    {
        var drawingSettings = new DrawingSettings()
    }

}
