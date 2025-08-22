Shader "Neon/Object/ObjectIngameShader"
{
    Properties
    {
        //[Header(Stencil)]
        //[Space(10)]
        //_Stencil ("Stencil ID", Range(0,255)) = 0
        //_ReadMask ("ReadMask", Range(0,255)) = 255
        //_WriteMask ("WriteMask", Range(0,255)) = 255
        //[Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Int) = 3
        //[Enum(UnityEngine.Rendering.StencilOp)] _StencilPass ("Stencil Pass", Int) = 0
        //[Enum(UnityEngine.Rendering.StencilOp)] _StencilFail ("Stencil Fail", Int) = 0
        //[Enum(UnityEngine.Rendering.StencilOp)] _StencilZFail ("Stencil ZFail", Int) = 0

        //[Space(20)]
        [Header(Ztest)]
        [Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Int) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4

        [Space(10)]
        [Header(Color)]
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("BaseColor Texture", 2D) = "white" {}


        [Header(Normal)]
        [Toggle]_UseNormalTex ("Use Normal Texture", float) = 0
        [NoScaleOffset]_NormalTex ("Normal Texture", 2D) = "bump" {}

        [Header(Shadow)]
        [Space(10)]
        _ReceiveShadowAmount ("Receive Shadow Amount", Range(0,1)) = 1
        _ShadowZOffset ("Shadow Z Offset", Range(-0.02,0.02)) = 0

        [Space(10)]
        _ShadowThreshold ("Shadow Position", Range(0,1)) = 0.5
        _ShadowSmooth ("Shadow Transition Softness", Range(0,0.5)) = 0.05

        [Space(10)]
        _ShadowColor ("Shadow Color", Color) = (0.5,0.5,0.5,1)
        [Toggle] _USESHADOWCOLORTEXTURE ("Use Shadow Color Texture", float) = 0
        [NoScaleOffset] _ShadowColorTexture ("Shadow Color Texture", 2D) = "white"{}
        _ShadowColorTextureAmount ("Shadow Color Texture Amount", Range(0,1)) = 0.5

        [Space(20)]
        [Header(Matcap)]
        [Space(10)]
        [Toggle] _UseMatcap("Use Matcap", float) = 0
        [HDR] _MatcapColor ("Matcap Color", Color) = (1,1,1,1)
        _MatcapTexture ("Matcap Texture", 2D) = "black" {}
        _MatcapMixAmount ("Matcap Mix Amount", Range(0,1)) = 0
        _MatcapAddAmount ("Matcap Add Amount", Range(0,1)) = 0

        [Space(30)]
        [NoScaleOffset] _ILMMaskTex ("ILM Mask Texture (R:Glossiness, G:Specular Mask, B:Emissive, A: Matcap) ", 2D) = "black"{}

        [Header(Specular)]
        [Space(10)]
        _SpecularColor ("Specular HighLight Color", Color) = (1,1,1,1)
        _SpecularPower ("Specular Power", float) = 32
        _SpecularThreshold ("Specular Threshold", Range(0,1)) = 0.5
        _SpecularSmothness ("Specular Smoothness", Range(0,0.5)) = 0.15
        _SpecularIntensity ("Specular Intensity", Range(0,10)) = 0
     
        [Space(20)]
        [Header(Emissive)]
        [Space(10)]
        _EmissiveColor ("Emissive Color", Color) = (1,1,1,1)
        _EmissivePower ("Emissive Power", float) = 1
        _EmissiveIntensity ("Emissive Intensity", float) = 1

        [Space(20)]
        [Header(RimLight_VertexColorB)]
        [Space(10)]
        [Toggle]_UseRimLight ("Use Rim Light", float) = 1
        [Enum(Rim, 0,ShadowReflectionRim,1 )]_RimMode ("Rim Mode", float) = 1
        [Toggle]_ApplyLightDirection ("Apply LightDirection", float) = 1
        
        _RimColor ("Rim Light Color", Color) = (1,1,1,1)
        _RimApplyMainTextureColor ("RimApplyMainTextureColor", Range(0, 1)) = 0
        _RimThreshold ("Rim Width", Range(0,1)) = 0.2
        _RimSmooth("Rim Smooth", Range(0, 0.5)) = 0.1
        _RimIntensity ("Rim Intensity", Range(0,5)) = 0
        [Toggle]_DistanceRimAttenuation("Distance Rim Attenuation", float) = 0
        
        [Space(20)]
        [Header(Outline)]
        [Space(10)]
        [Toggle] _UseBakedNormal ("Use Baked Normal (Channal is TEXCROOD7)", float) = 0
        _OutlineWidth("Outline Width", float) = 0.1
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 0)
        _UsePerspectiveCorrect("Use Perspective Correct", Range(0,1)) = 0.2
        _ApplyMainTextureColor("Apply MainTexture Color", Range(0,1)) = 0
        //[Toggle] _OnlyVertexColorViewMode ("Only Vertex Color View Mode (Debug)", float) = 0
        _ZOffset("Outline Z Offset", Range(-1, 1)) = 0
        [Toggle] _UseVertexColorAtten("Use Vertex Color (R: Outline Width )", float) = 0

        //[HideInInspector] MyCharacterOutlineColor("MyCharacterOutlineColor", Color) = (0,0,0,0)
        //[HideInInspector] EnemyCharacterOutlineColor("EnemyCharacterOutlineColor", Color) = (0,0,0,0)

    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry+0"
        }
        LOD 100

        Pass
        {

            Name "Universal Forward"
            Tags 
            { 
                "LightMode" = "UniversalForward"
            }
            Cull Back
            ZTest [_ZTest]
            ZWrite [_ZWrite] 

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            
             // GPU Instancing
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            // -------------------------------------
            // Universal Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _LIGHT_LAYERS
            #pragma multi_compile_fragment _ _LIGHT_COOKIES


            //option
            #pragma shader_feature_local _ _USENORMALTEX_ON

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            TEXTURE2D(_ILMMaskTex);    SAMPLER(sampler_ILMMaskTex);
            TEXTURE2D(_ShadowColorTexture);     SAMPLER(sampler_ShadowColorTexture);
            #if _USEMATCAP_ON
                TEXTURE2D(_MatcapTexture);     SAMPLER(sampler_MatcapTexture);
            #endif
            #if _USENORMALTEX_ON
                TEXTURE2D(_NormalTex);   SAMPLER(sampler_NormalTex);  
            #endif
            CBUFFER_START(UnityPerMaterial)    

            half4 _MainTex_ST;
            half4 _ILMMaskTex_ST;
            half4 _ShadowColorTexture_ST;

            float4 _Color;
            float _ReceiveShadowAmount;
            float4 _ShadowColor;
            float _ShadowThreshold;
            float _ShadowSmooth;
            float _ShadowColorTextureAmount;


            float4 _SpecularColor;
            float _SpecularPower;
            float _SpecularIntensity;
            float _SpecularSmothness, _SpecularThreshold;

            float _UseRimLight;
            float _ApplyLightDirection;
            float _RimMode;
            float4 _RimColor;
            float _RimApplyMainTextureColor;
            float _RimThreshold;
            float _RimSmooth;
            float _RimIntensity;
            float _DistanceRimAttenuation;

            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _ApplyMainTextureColor;
            float _UsePerspectiveCorrect;
            
            float _ZOffset, _ShadowZOffset;

            float _MatcapMixAmount, _MatcapAddAmount;
            float4 _MatcapColor;

            float _UseVertexColorAtten;

            float4 _EmissiveColor;
            float _EmissivePower, _EmissiveIntensity;

            CBUFFER_END
         
            struct appdata
            {
                float4 color : COLOR0;
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID                                
            };


            struct v2f
            {
                float4 color : COLOR0;
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL; 
                float2 texcoord : TEXCOORD0;
                float fogCoord  : TEXCOORD1;
                float4 shadowCoord : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float3 worldPos : TEXCOORD4;
                float distanceCamera : TEXCOORD5;
                float3 tangent : TEXCOORD6;
                float3 biTangent : TEXCOORD7;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };





            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.color = v.color;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);          
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.fogCoord = ComputeFogFactor(o.vertex.z);
                float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
                o.worldPos = worldPos;
                o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
                o.shadowCoord = float4(0,0,0,0);
                o.distanceCamera = distance(_WorldSpaceCameraPos.xyz, worldPos.xyz);
                o.tangent = TransformObjectToWorldDir(v.tangent.xyz);
                o.biTangent = cross(o.normal, o.tangent) * v.tangent.w;

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {

                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                i.shadowCoord = TransformWorldToShadowCoord(i.worldPos);
                Light mainLight = GetMainLight(i.shadowCoord);

                i.color = lerp(1, i.color, _UseVertexColorAtten);

                
                uint meshRenderingLayers = GetMeshRenderingLightLayer();


                float4 MainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord)* _Color;
                float4 Mask = SAMPLE_TEXTURE2D(_ILMMaskTex, sampler_ILMMaskTex, i.texcoord);

                
                //source
                #if _USENORMALTEX_ON
                    float3 NormalTS = UnpackNormal( SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.texcoord));
                    float3x3 tbnMatrix = float3x3( normalize(i.tangent.xyz), normalize(i.biTangent), normalize(i.normal) );
                    i.normal = normalize( mul ( NormalTS, tbnMatrix) );
                #else
                    i.normal = normalize(i.normal);
                #endif
                
                float ndotl = dot(mainLight.direction, i.normal);    
                float ndotv = dot(i.normal, normalize(i.viewDir) );  
                float halfLambert = ndotl * 0.5 + 0.5;

                //rim
                float rimLight =  smoothstep( (1-_RimThreshold) - _RimSmooth, (1-_RimThreshold) + _RimSmooth, max(0, 1 - ndotv )) * lerp(_RimIntensity,saturate(_RimIntensity),_RimMode)  ;
                rimLight = lerp( rimLight, rimLight * (1 / (i.distanceCamera + 0.001) ), _DistanceRimAttenuation);
                rimLight *= lerp(0,1,_UseRimLight);

                //shadow
                float smoothedShadow = saturate(smoothstep(_ShadowThreshold - _ShadowSmooth, _ShadowThreshold + _ShadowSmooth , halfLambert) + lerp(0,rimLight,_RimMode)) * saturate(lerp(1,mainLight.shadowAttenuation,_ReceiveShadowAmount) + lerp(0,rimLight,_RimMode));
                float3 ColorShadow;


                #if _USESHADOWCOLORTEXTURE_ON
                    float4 ShadowColorTexture = SAMPLE_TEXTURE2D(_ShadowColorTexture, sampler_ShadowColorTexture, i.texcoord);// * _Color;
                    
                    ColorShadow = lerp( lerp( _ShadowColor.rgb, ShadowColorTexture.xyz, _ShadowColorTextureAmount ) , 1 , smoothedShadow) ;
                #else
                    ColorShadow = lerp( _ShadowColor.rgb, 1 , smoothedShadow) ;
                #endif
                
                //combine
                float4 c = float4(1,1,1,MainTex.a);
                
                float3 directDiffuse;

                if (IsMatchingLightLayer(mainLight.layerMask, meshRenderingLayers))
                {
                    directDiffuse = MainTex.rgb * mainLight.color * ColorShadow ;
                }else
                {
                    directDiffuse = 0;
                }

                
                float3 sh = SampleSH(float3(0,1,0)) ;
                c.rgb = directDiffuse + MainTex.rgb * sh ;

                #if _USEMATCAP_ON
                    // Matcap
                    float3 viewNormal = mul((float3x3)UNITY_MATRIX_V, i.normal);
                    float2 matcapUV = viewNormal.xy * 0.5 + 0.5;
                    float3 matcapColor = SAMPLE_TEXTURE2D(_MatcapTexture,sampler_MatcapTexture, matcapUV).xyz * _MatcapColor.rgb;

                    float3 repColor = lerp(c.rgb, matcapColor, _MatcapMixAmount * Mask.a);
                    float3 addColor = lerp(0, matcapColor, _MatcapAddAmount * Mask.a);
                    c.rgb = repColor + addColor;
                #endif

                //Rim
                float3 limDefault = lerp(1, smoothedShadow, _ApplyLightDirection) * _RimColor.rgb;
                c.rgb += lerp( rimLight * lerp(limDefault, MainTex.rgb, _RimApplyMainTextureColor) , 0 , _RimMode);
                
                #ifdef _ADDITIONAL_LIGHTS
                    uint pixelLightCount = GetAdditionalLightsCount();
                    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                    {
                        Light light = GetAdditionalLight(lightIndex, i.worldPos);

                        if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                        {
                            c.rgb += c.rgb * LightingLambert(light.color * saturate(light.distanceAttenuation) * light.shadowAttenuation , light.direction , i.normal)  ;
                        }
                    }
                #endif 

                //Emissive
                c.rgb += pow(abs(Mask.b), _EmissivePower) * _EmissiveColor.rgb * _EmissiveIntensity;

                //Specular
                float3 halfVector = normalize(mainLight.direction + i.viewDir);
                float ndoth = dot(i.normal, halfVector);
                float specular =  smoothstep(_SpecularThreshold - _SpecularSmothness, _SpecularThreshold +_SpecularSmothness, pow(max(0,ndoth), _SpecularPower * Mask.r +0.001) )  ;
                
                float3 specularColor = specular * _SpecularColor.rgb * _SpecularIntensity * mainLight.color * Mask.g;
                c.rgb += specularColor;
                c.rgb = MixFog(c.rgb, i.fogCoord);

                //c.rgb = lerp (c.rgb, i.color.rgb, _OnlyVertexColorViewMode);
                

                return c;    
            }

            ENDHLSL
        }

                

        Pass
        {
        Name "ShadowCaster"

        Tags{"LightMode" = "ShadowCaster"}

            Cull Back

            HLSLPROGRAM

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

           // GPU Instancing
            #pragma multi_compile_instancing
          
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);


            CBUFFER_START(UnityPerMaterial)    

            half4 _MainTex_ST;
            half4 _ILMMaskTex_ST;
            half4 _ShadowColorTexture_ST;

            float4 _Color;
            float _ReceiveShadowAmount;
            float4 _ShadowColor;
            float _ShadowThreshold;
            float _ShadowSmooth;
            float _ShadowColorTextureAmount;


            float4 _SpecularColor;
            float _SpecularPower;
            float _SpecularIntensity;
            float _SpecularSmothness, _SpecularThreshold;

            float _UseRimLight;
            float _ApplyLightDirection;
            float _RimMode;
            float4 _RimColor;
            float _RimApplyMainTextureColor;
            float _RimThreshold;
            float _RimSmooth;
            float _RimIntensity;
            float _DistanceRimAttenuation;

            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _ApplyMainTextureColor;
            float _UsePerspectiveCorrect;
            
            float _ZOffset, _ShadowZOffset;

            float _MatcapMixAmount, _MatcapAddAmount;
            float4 _MatcapColor;

            float _UseVertexColorAtten;

            float4 _EmissiveColor;
            float _EmissivePower, _EmissiveIntensity;

            CBUFFER_END

            struct VertexInput
            {          
            float4 vertex : POSITION;
            float4 normal : NORMAL;

            UNITY_VERTEX_INPUT_INSTANCE_ID  
            };
          
            struct VertexOutput
            {          
            float4 vertex : SV_POSITION;
          
            UNITY_VERTEX_INPUT_INSTANCE_ID          
            
  
            };

            VertexOutput ShadowPassVertex(VertexInput v)
            {
               VertexOutput o;
               UNITY_SETUP_INSTANCE_ID(v);
               UNITY_TRANSFER_INSTANCE_ID(v, o);
            // UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);                             
           
              float3 worldPos = TransformObjectToWorld(v.vertex.xyz) ;
              float3 viewDirWS = normalize(_WorldSpaceCameraPos.xyz - worldPos.xyz);
              float3 positionWS = worldPos - viewDirWS * _ShadowZOffset; 

              float3 normalWS   = TransformObjectToWorldNormal(v.normal.xyz);
         
              float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _MainLightPosition.xyz));
              
              o.vertex = positionCS;
             
              return o;
            }

            half4 ShadowPassFragment(VertexOutput i) : SV_TARGET
            {  
                UNITY_SETUP_INSTANCE_ID(i);
         //     UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                return 0;
            }

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0

            Cull Back

            HLSLPROGRAM
          
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
  
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag
              
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
              
            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);


            CBUFFER_START(UnityPerMaterial)    

            half4 _MainTex_ST;
            half4 _ILMMaskTex_ST;
            half4 _ShadowColorTexture_ST;

            float4 _Color;
            float _ReceiveShadowAmount;
            float4 _ShadowColor;
            float _ShadowThreshold;
            float _ShadowSmooth;
            float _ShadowColorTextureAmount;


            float4 _SpecularColor;
            float _SpecularPower;
            float _SpecularIntensity;
            float _SpecularSmothness, _SpecularThreshold;

            float _UseRimLight;
            float _ApplyLightDirection;
            float _RimMode;
            float4 _RimColor;
            float _RimApplyMainTextureColor;
            float _RimThreshold;
            float _RimSmooth;
            float _RimIntensity;
            float _DistanceRimAttenuation;

            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _ApplyMainTextureColor;
            float _UsePerspectiveCorrect;
            
            float _ZOffset, _ShadowZOffset;

            float _MatcapMixAmount, _MatcapAddAmount;
            float4 _MatcapColor;

            float _UseVertexColorAtten;

            float4 _EmissiveColor;
            float _EmissivePower, _EmissiveIntensity;

            CBUFFER_END
              
            struct VertexInput
            {
                float4 vertex : POSITION;                  
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

                struct VertexOutput
                {          
                float4 vertex : SV_POSITION;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID          
                
                
                };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
              // UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = TransformWorldToHClip(TransformObjectToWorld(v.vertex.xyz));

                return o;
            }

            half4 frag(VertexOutput IN) : SV_TARGET
            {                  
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags
            {
                "LightMode" = "Outline"
            }

            Stencil
            {
                Ref [_Stencil]
                ReadMask [_ReadMask]
                WriteMask [_WriteMask]
                Comp [_StencilComp]
                Pass [_StencilPass]
                Fail [_StencilFail]
                ZFail [_StencilZFail]
            }

           
            // Render State
            Blend One Zero, One Zero
            Cull Front
            //ZTest LEqual
            ZWrite On
            // ColorMask: <None>
            AlphaToMask On
            
        
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)    

            half4 _MainTex_ST;
            half4 _ILMMaskTex_ST;
            half4 _ShadowColorTexture_ST;

            float4 _Color;
            float _ReceiveShadowAmount;
            float4 _ShadowColor;
            float _ShadowThreshold;
            float _ShadowSmooth;
            float _ShadowColorTextureAmount;


            float4 _SpecularColor;
            float _SpecularPower;
            float _SpecularIntensity;
            float _SpecularSmothness, _SpecularThreshold;

            float _UseRimLight;
            float _ApplyLightDirection;
            float  _RimMode;
            float4 _RimColor;
            float _RimApplyMainTextureColor;
            float _RimThreshold;
            float _RimSmooth;
            float _RimIntensity;
            float _DistanceRimAttenuation;

            
            float _OutlineWidth;
            float4 _OutlineColor;
            float _ApplyMainTextureColor;
            float _UsePerspectiveCorrect;
            
            float _ZOffset, _ShadowZOffset;

            float _MatcapMixAmount, _MatcapAddAmount;
            float4 _MatcapColor;

            float _UseVertexColorAtten;

            float4 _EmissiveColor;
            float _EmissivePower, _EmissiveIntensity;

            CBUFFER_END

            // local option : Color
            //#pragma multi_compile_local _ _USEMYCHARACTEROUTLINECOLOR_ON

            struct appdata
            {
                float4 color : COLOR0;
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 tangent : TANGENT;

                #ifdef _USEBAKEDNORMAL_ON
                    float3 normal : TEXCOORD7;
                #else
                    float3 normal : NORMAL;
                #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID                                
            };


            struct v2f
            {
                float4 color : COLOR0;
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float fogCoord  : TEXCOORD1;
                
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.color = v.color;
                v.color = lerp(1, v.color, _UseVertexColorAtten);

                //for ugc editor
                _ZOffset *= (1 - unity_OrthoParams.w);

                float3 positionWS = TransformObjectToWorld(v.vertex.xyz  + (v.normal * v.color.r * 0.01 * _OutlineWidth * lerp(1, TransformObjectToHClip(v.vertex.xyz).a ,_UsePerspectiveCorrect)) );

                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - positionWS);
                
                o.vertex = TransformWorldToHClip(positionWS - normalize( viewDir) * _ZOffset  )   ;
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.fogCoord = ComputeFogFactor(o.vertex.z);          

                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                float4 col = float4(1, 1, 1, 1);

                float4 MainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                
                col.rgb = lerp(_OutlineColor.rgb, MainTex.rgb, _ApplyMainTextureColor);
                col.rgb = MixFog(col.rgb, i.fogCoord);

                return col;
            }
            ENDHLSL
        }
    }
}
