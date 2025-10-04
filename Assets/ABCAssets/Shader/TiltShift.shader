Shader "Hidden/URP/TiltShift"
{
    Properties { }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off Cull Off ZTest Always

        HLSLINCLUDE
        #pragma target 3.5
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        

        struct Varyings { float4 positionCS : SV_Position; };
        Varyings Vert(uint id : SV_VertexID)
        {
            Varyings o;
            // Fullscreen triangle without external include
            float2 uv = float2((id << 1) & 2, id & 2);
            o.positionCS = float4(uv * 2.0 - 1.0, 0, 1);
            return o;
        }

        // Source from RenderGraph/Blitter
        TEXTURE2D_X(_BlitTexture);
        SAMPLER(sampler_BlitTexture);
        // External blur chain texture (set as global)
        TEXTURE2D_X(_BlurTex);
        SAMPLER(sampler_BlurTex);

        CBUFFER_START(UnityPerMaterial)
        float2 _TiltCenter;
        float  _TiltAngleRad;
        float  _BandHalfWidth;
        float  _Feather;
        float  _MaxStrength;
        int    _KernelRadius;
        int    _Quality;
        float2 _AnamorphXY;
        float  _KawaseStep;
        CBUFFER_END
        
        float blurWeight(float2 uv)
        {
            float s = sin(_TiltAngleRad);
            float c = cos(_TiltAngleRad);
            float2 d = uv - _TiltCenter;
            float y = (s * d.x) + (c * d.y);
            float dist = abs(y) - max(_BandHalfWidth, 0.0);

            float aa = fwidth(y) * 1.5;
            float denom = max(_Feather, aa);
            float w = saturate(dist / max(denom, 1e-5));
            return w;
        }

        // 타깃 해상도에 독립적인 텍셀 크기 근사 (UV 미분 사용)
        float2 TexelUV(float2 uv)
        {
            // ddx/ddy는 현재 렌더타깃 해상도에 맞춰짐 (다운샘플에도 자동 적응)
            float2 dx = float2(ddx(uv.x), 0);
            float2 dy = float2(0, ddy(uv.y));
            return float2(abs(dx.x), abs(dy.y));
        }

        // --- Pass 4: Copy/Downsample ---
        float4 FragCopy(Varyings i) : SV_Target
        {
            float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
            return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
        }

        // --- Pass 0: Gaussian H ---
        float4 FragGaussH(Varyings i) : SV_Target
        {
            float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
            float2 ps = TexelUV(uv) * max(_AnamorphXY, 1e-5);
            int k = max(_KernelRadius, 1);
            int q = max(_Quality, 1);
            float3 acc = 0; float wsum = 0;
            [loop] for (int s = -k; s <= k; s += q)
            {
                float w = 1.0 - abs(s) / (k + 1.0);
                float2 uv2 = uv + float2(s, 0) * ps.x;
                acc += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv2).rgb * w;
                wsum += w;
            }
            return float4(acc / max(wsum, 1e-5), 1);
        }

        // --- Pass 1: Gaussian V ---
        float4 FragGaussV(Varyings i) : SV_Target
        {
            float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
            float2 ps = TexelUV(uv) * max(_AnamorphXY, 1e-5);
            int k = max(_KernelRadius, 1);
            int q = max(_Quality, 1);
            float3 acc = 0; float wsum = 0;
            [loop] for (int s = -k; s <= k; s += q)
            {
                float w = 1.0 - abs(s) / (k + 1.0);
                float2 uv2 = uv + float2(0, s) * ps.y;
                acc += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv2).rgb * w;
                wsum += w;
            }
            return float4(acc / max(wsum, 1e-5), 1);
        }

        // --- Pass 3: Kawase (single iteration;) ---
        float4 FragKawase(Varyings i) : SV_Target
        {
            float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
            float2 ps = TexelUV(uv);
            float2 off = _KawaseStep * ps;
            float3 c = 0;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv +  off).rgb;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + float2(-off.x,  off.y)).rgb;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + float2( off.x, -off.y)).rgb;
            c += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv -  off).rgb;
            return float4(c * 0.25, 1);
        }

        // --- Pass 2: Composite (원본 vs 블러) ---
        float4 FragComposite(Varyings i) : SV_Target
        {
            float2 uv = GetNormalizedScreenSpaceUV(i.positionCS);
            float3 src  = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv).rgb;
            float3 blur = SAMPLE_TEXTURE2D_X(_BlurTex,     sampler_BlurTex,     uv).rgb;

            float w = blurWeight(uv) * _MaxStrength;
            if (_BandHalfWidth <= 0 && _Feather <= 0)
            {
                w = _MaxStrength;
            }
            
            return float4(lerp(src, blur, saturate(w)), 1);
        }
        ENDHLSL

        // Pass 0: Gaussian H
        Pass
        {
            Name "GaussianH"
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragGaussH
            ENDHLSL
        }
        
        // Pass 1: Gaussian V
        Pass
        {
            Name "GaussianV"
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragGaussV
            ENDHLSL
        }
        
        // Pass 2: Composite
        Pass
        {
            Name "Composite"
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM #pragma vertex Vert
            #pragma fragment FragComposite
            ENDHLSL 
        }
        
        // Pass 3: Kawase
        Pass
        {
            Name "Kawase"
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragKawase
            ENDHLSL
        }
        
        // Pass 4: Copy / Downsample
        Pass
        {
            Name "CopyDownsample"
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragCopy
            ENDHLSL
        }
    }
}
