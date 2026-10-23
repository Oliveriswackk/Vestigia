
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class PauseBlurRendererFeature : ScriptableRendererFeature
{
    public static bool BlurEnabled { get; set; }

    [SerializeField] private Shader blurShader;

    [Range(0.001f, 0.02f)]
    [SerializeField] private float horizontalBlur = 0.006f;

    [Range(0.001f, 0.02f)]
    [SerializeField] private float verticalBlur = 0.006f;

    private Material blurMaterial;
    private PauseBlurPass blurPass;

    public override void Create()
    {
        if (blurShader == null)
            blurShader = Shader.Find("CustomEffects/PauseBlur");

        if (blurShader == null)
        {
            Debug.LogError("PauseBlur: no se encontró el shader CustomEffects/PauseBlur.");
            return;
        }

        blurMaterial = CoreUtils.CreateEngineMaterial(blurShader);
        blurPass = new PauseBlurPass(blurMaterial);
        blurPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (!BlurEnabled || blurPass == null)
            return;

        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        blurPass.SetBlur(horizontalBlur, verticalBlur);
        renderer.EnqueuePass(blurPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(blurMaterial);
    }

    private class PauseBlurPass : ScriptableRenderPass
    {
        private readonly Material material;

        public PauseBlurPass(Material material)
        {
            this.material = material;
        }

        public void SetBlur(float horizontal, float vertical)
        {
            material.SetFloat("_HorizontalBlur", horizontal);
            material.SetFloat("_VerticalBlur", vertical);
        }

        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            var resources = frameData.Get<UniversalResourceData>();
            var cameraData = frameData.Get<UniversalCameraData>();

            if (resources.isActiveTargetBackBuffer)
                return;

            TextureHandle source = resources.activeColorTexture;

            var descriptor = cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            TextureHandle temporary = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                descriptor,
                "_PauseBlurTemporary",
                false
            );

            var verticalBlit = new RenderGraphUtils.BlitMaterialParameters(
                source, temporary, material, 0
            );

            renderGraph.AddBlitPass(
                verticalBlit,
                "Pause Blur Vertical"
            );

            var horizontalBlit = new RenderGraphUtils.BlitMaterialParameters(
                temporary, source, material, 1
            );

            renderGraph.AddBlitPass(
                horizontalBlit,
                "Pause Blur Horizontal"
            );
        }
    }
}
