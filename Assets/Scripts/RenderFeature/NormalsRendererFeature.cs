using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace RenderFeature
{
    internal class NormalsRenderPass : ScriptableRenderPass
    {
        #region Fields

        const string kShader = "Custom/ObjectNormals";
        string kNormalsTexture;
        const string kProfilingTag = "GetNormalTexture";

        static readonly string[] s_ShaderTags = new string[]
        {
            "UniversalForward",
            "LightweightForward",
        };

        RenderTargetHandle m_NormalsHandle;
        Material m_Material;
        private Int32 cullingLayer;

        #endregion

        #region Constructors

        internal NormalsRenderPass()
        {
            // Set data
        }

        #endregion

        #region State

        internal void Setup(RenderPassEvent RenderPassEvent,LayerMask layerMask,string TextureName)
        {
            m_Material = new Material(Shader.Find(kShader));
            renderPassEvent = RenderPassEvent;
            cullingLayer = layerMask;
            kNormalsTexture = TextureName;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            // Configure Render Target
            m_NormalsHandle.Init(kNormalsTexture);
            cmd.GetTemporaryRT(m_NormalsHandle.id, cameraTextureDescriptor, FilterMode.Point);
            ConfigureTarget(m_NormalsHandle.Identifier(), m_NormalsHandle.Identifier());
            cmd.SetRenderTarget(m_NormalsHandle.Identifier(), m_NormalsHandle.Identifier());

            // TODO: Why do I have to clear here?
            cmd.ClearRenderTarget(true, true, Color.black, 1.0f);
        }

        #endregion

        #region Execution

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // Get data
            var camera = renderingData.cameraData.camera;

            // Never draw in Preview
            if (camera.cameraType == CameraType.Preview)
                return;

            // Profiling command
            CommandBuffer cmd = CommandBufferPool.Get(kProfilingTag);
            using (new ProfilingSample(cmd, kProfilingTag))
            {
                ExecuteCommand(context, cmd);

                camera.depthTextureMode |= DepthTextureMode.Depth;

                // Drawing
                DrawObjectNormals(context, ref renderingData, cmd, camera);
            }

            ExecuteCommand(context, cmd);
        }

        DrawingSettings GetDrawingSettings(ref RenderingData renderingData)
        {
            // Drawing Settings
            var camera = renderingData.cameraData.camera;
            var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
            var drawingSettings = new DrawingSettings(ShaderTagId.none, sortingSettings)
            {
                perObjectData = renderingData.perObjectData,
                mainLightIndex = renderingData.lightData.mainLightIndex,
                enableDynamicBatching = renderingData.supportsDynamicBatching,
                enableInstancing = true,
            };

            // Shader Tags
            for (int i = 0; i < s_ShaderTags.Length; ++i)
            {
                drawingSettings.SetShaderPassName(i, new ShaderTagId(s_ShaderTags[i]));
            }

            // Material
            drawingSettings.overrideMaterial = m_Material;
            drawingSettings.overrideMaterialPassIndex = 0;
            return drawingSettings;
        }

        void DrawObjectNormals(ScriptableRenderContext context, ref RenderingData renderingData, CommandBuffer cmd,
            Camera camera)
        {
            // Get CullingParameters
            var cullingParameters = new ScriptableCullingParameters();
            if (!camera.TryGetCullingParameters(out cullingParameters))
                return;

            // Culling Results
            var cullingResults = context.Cull(ref cullingParameters);

            var drawingSettings = GetDrawingSettings(ref renderingData);
            var filteringSettings = new FilteringSettings(RenderQueueRange.all, cullingLayer);
            var renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

            // Draw Renderers
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings, ref renderStateBlock);
        }

        #endregion

        #region Cleanup

        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");

            // Reset Render Target
            if (m_NormalsHandle != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(m_NormalsHandle.id);
                m_NormalsHandle = RenderTargetHandle.CameraTarget;
            }
        }

        #endregion

        #region CommandBufer

        void ExecuteCommand(ScriptableRenderContext context, CommandBuffer cmd)
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
        }

        #endregion
    }

    internal class NormalsRendererFeature : ScriptableRendererFeature
    {
        #region Fields

        public RenderPassEvent Event;//和官方的一样用来表示什么时候插入Pass，默认在渲染完不透明物体后
        public LayerMask LayerMask;
        public string  TextureName;
        static NormalsRendererFeature s_Instance;
        readonly NormalsRenderPass m_NormalsRenderPass;

        #endregion

        #region Constructors

        internal NormalsRendererFeature()
        {
            // Set data
            s_Instance = this;
            m_NormalsRenderPass = new NormalsRenderPass();
        }

        #endregion

        #region Initialization

        public override void Create()
        {
            name = "Normals";
        }

        #endregion
        
        #region RenderPass
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Motion vector pass
            m_NormalsRenderPass.Setup(Event,LayerMask,TextureName);
            renderer.EnqueuePass(m_NormalsRenderPass);
        }

        #endregion
    }
}