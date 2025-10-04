using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace ProjectABC.Engine.Scene.PostFX
{
    public class TiltShiftRendererFeature : ScriptableRendererFeature
    {
        [Serializable]
        public class Settings
        {
            public Shader shader;
            public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingPostProcessing;
        }

        private sealed class TiltShiftPass : ScriptableRenderPass
        {
            private class BindBlurPassData { }
            
            private static readonly int BlurTexId = Shader.PropertyToID("_BlurTex");
            private static readonly int TiltCenterId = Shader.PropertyToID("_TiltCenter");
            private static readonly int TiltAngleRadId = Shader.PropertyToID("_TiltAngleRad");
            private static readonly int BandHalfWidthId = Shader.PropertyToID("_BandHalfWidth");
            private static readonly int FeatherId = Shader.PropertyToID("_Feather");
            private static readonly int MaxStrengthId = Shader.PropertyToID("_MaxStrength");
            private static readonly int KernelRadiusId = Shader.PropertyToID("_KernelRadius");
            private static readonly int QualityId = Shader.PropertyToID("_Quality");
            private static readonly int AnamorphXyId = Shader.PropertyToID("_AnamorphXY");
            private static readonly int KawaseStepId = Shader.PropertyToID("_KawaseStep");
            
            private readonly Material _mat;

            public TiltShiftPass(Material mat)
            {
                _mat = mat;
                ConfigureInput(ScriptableRenderPassInput.Color);
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (_mat == null)
                {
                    return;
                }

                var stack = VolumeManager.instance.stack;
                var tiltShift = stack.GetComponent<TiltShift>();
                if (tiltShift == null || !tiltShift.IsActive())
                {
                    return;
                }

                var resourceData = frameData.Get<UniversalResourceData>();
                TextureHandle source = resourceData.activeColorTexture;

                TextureDesc fullDesc = source.GetDescriptor(renderGraph);
                fullDesc.msaaSamples = (MSAASamples)1;
                fullDesc.depthBufferBits = 0;
                
                _mat.SetVector(TiltCenterId, tiltShift.tiltCenter.value);
                _mat.SetFloat (TiltAngleRadId, tiltShift.tiltAngle.value * Mathf.Deg2Rad);
                _mat.SetFloat (BandHalfWidthId, tiltShift.bandHalfWidth.value);
                _mat.SetFloat (FeatherId, Mathf.Max(1e-4f, tiltShift.feather.value));
                _mat.SetFloat (MaxStrengthId, tiltShift.maxStrength.value);
                _mat.SetInt (KernelRadiusId, tiltShift.kernelRadius.value);
                _mat.SetInt (QualityId, tiltShift.quality.value);
                _mat.SetVector(AnamorphXyId, tiltShift.useAnamorphic.value ? tiltShift.anamorphXY.value : Vector2.one);

                int downsample = Mathf.Max(1, tiltShift.downsample.value);

                TextureDesc smallDesc = fullDesc;
                smallDesc.width = Mathf.Max(1, fullDesc.width / downsample);
                smallDesc.height = Mathf.Max(1, fullDesc.height / downsample);

                smallDesc.name = "TS_Ping";
                TextureHandle ping = renderGraph.CreateTexture(smallDesc);
                smallDesc.name = "TS_Pong";
                TextureHandle pong = renderGraph.CreateTexture(smallDesc);
                
                renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source, ping, _mat, 4), "TS Downsample");

                TextureHandle blurred;
                // Gaussian 0: H, 1: V
                if (tiltShift.method.value == (int)TiltShiftBlurMethod.Gaussian)
                {
                    
                    renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(ping, pong, _mat, 0), "TS Gauss H");
                    renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(pong, ping, _mat, 1), "TS Gauss V");
                    blurred = ping;
                }
                // 3: Kawase
                else
                {
                    int iterations = Mathf.Max(1, tiltShift.iterations.value);
                    TextureHandle from = ping;
                    TextureHandle to = pong;

                    for (int i = 0; i < iterations; i++)
                    {
                        var materialPropBlock = new MaterialPropertyBlock();
                        float step = tiltShift.kawaseStep.value + (i * 0.5f);
                        materialPropBlock.SetFloat(KawaseStepId, step);

                        var k = new RenderGraphUtils.BlitMaterialParameters(from, to, _mat, 3)
                        {
                            propertyBlock = materialPropBlock
                        };

                        renderGraph.AddBlitPass(k, $"TS Kawase {i + 1}");
                        (from, to) = (to, from);
                    }

                    blurred = from;
                }

                TextureDesc srcCopyDesc = fullDesc;
                srcCopyDesc.name = "TS_SrcCopy";
                TextureHandle srcCopy = renderGraph.CreateTexture(srcCopyDesc);
                renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source, srcCopy, _mat, 4), "TS Src Copy");

                using var builder = renderGraph.AddRasterRenderPass<BindBlurPassData>("TS Composite", out var _);
                builder.UseTexture(srcCopy, AccessFlags.Read);
                builder.UseTexture(blurred, AccessFlags.Read);
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                    
                builder.SetRenderFunc((BindBlurPassData bindBlurPassData, RasterGraphContext ctx) =>
                {
                    var materialPropBlock = new MaterialPropertyBlock();
                    materialPropBlock.SetTexture("_BlitTexture", srcCopy);
                    materialPropBlock.SetTexture("_BlurTex", blurred);
                        
                    CoreUtils.DrawFullScreen(ctx.cmd, _mat, materialPropBlock, 2);
                });
            }
        }

        public Settings settings = new();
        
        private Material _mat;
        private TiltShiftPass _pass;
        
        public override void Create()
        {
            if (settings.shader != null)
            {
                _mat = CoreUtils.CreateEngineMaterial(settings.shader);
            }

            _pass = new TiltShiftPass(_mat) { renderPassEvent = settings.injectionPoint };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var camData = renderingData.cameraData;
            if (!camData.postProcessEnabled)
            {
                return;
            }
            
            var tiltShift = VolumeManager.instance.stack.GetComponent<TiltShift>();
            bool anyOverride = tiltShift.parameters.Any(param => param.overrideState);

            if (!anyOverride)
            {
                return;
            }

            if (!tiltShift.IsActive())
            {
                return;
            }
            
            if (_mat != null)
            {
                renderer.EnqueuePass(_pass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_mat);
        }
    }
}
