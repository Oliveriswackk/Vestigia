
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class CameraBlurRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class BlurSettings
    {
        public Shader blurShader;

        [Range(0f, 10f)]
        public float blurSize = 8f;
    }

    public BlurSettings settings = new BlurSettings();

    private Material blurMaterial;
    private BlurRenderPass blurPass;

    public override void Create()
    {
        CoreUtils.Destroy(blurMaterial);
        blurMaterial = null;
        blurPass = null;

        if (settings.blurShader == null)
            return;

        blurMaterial = CoreUtils.CreateEngineMaterial(settings.blurShader);

        blurPass = new BlurRenderPass(blurMaterial, settings)
        {
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing
        };
    }

    public override void AddRenderPasses(
        ScriptableRenderer renderer,
        ref RenderingData renderingData)
    {
        if (blurMaterial == null || blurPass == null)
            return;

        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        renderer.EnqueuePass(blurPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(blurMaterial);
        blurMaterial = null;
        blurPass = null;
    }

    private class BlurRenderPass : ScriptableRenderPass
    {
        private readonly Material material;
        private readonly BlurSettings settings;

        private class PassData
        {
            public TextureHandle source;
            public Material material;
            public float blurSize;
        }

        public BlurRenderPass(Material material, BlurSettings settings)
        {
            this.material = material;
            this.settings = settings;
        }

        public override void RecordRenderGraph(
            RenderGraph renderGraph,
            ContextContainer frameData)
        {
            float intensity = Shader.GetGlobalFloat(
                "_VestigiaBlurIntensity"
            );

            float finalBlurSize = settings.blurSize * intensity;

            if (material == null || finalBlurSize <= 0.001f)
                return;

            UniversalResourceData resources =
                frameData.Get<UniversalResourceData>();

            if (resources.isActiveTargetBackBuffer)
                return;

            TextureHandle source = resources.activeColorTexture;

            var descriptor = renderGraph.GetTextureDesc(source);
            descriptor.name = "_CameraBlurTemporary";
            descriptor.clearBuffer = false;

            TextureHandle destination =
                renderGraph.CreateTexture(descriptor);

            using (var builder =
                renderGraph.AddRasterRenderPass<PassData>(
                    "Camera Blur",
                    out var passData))
            {
                passData.source = source;
                passData.material = material;
                passData.blurSize = finalBlurSize;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(destination, 0);

                builder.SetRenderFunc(
                    (PassData data, RasterGraphContext context) =>
                    {
                        data.material.SetFloat(
                            "_BlurSize",
                            data.blurSize
                        );

                        Blitter.BlitTexture(
                            context.cmd,
                            data.source,
                            new Vector4(1, 1, 0, 0),
                            data.material,
                            0
                        );
                    }
                );
            }

            resources.cameraColor = destination;
        }
    }
}
