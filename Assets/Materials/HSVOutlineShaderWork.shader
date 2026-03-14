Shader "Universal Render Pipeline/2D/Sprite-Lit-Default-HSV-Outline"
{
    Properties
    {
        [MainTexture]_MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        [MainColor] _Color("Tint", Color) = (1,1,1,1)
        
        _Hue("Hue Shift", Range(-1,1)) = 0
        _Saturation("Saturation", Range(0,2)) = 1
        _Value("Value (Brightness)", Range(0,2)) = 1
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.1)) = 0.02

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

            Pass
            {
                Tags { "LightMode" = "Universal2D" }

                HLSLPROGRAM
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                #pragma vertex CombinedShapeLightVertex
                #pragma fragment CombinedShapeLightFragment // Используем только один фрагментный шейдер!

                #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
                #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
                #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
                #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
                #pragma multi_compile _ DEBUG_DISPLAY

                struct Attributes
                {
                    float3 positionOS   : POSITION;
                    float4 color        : COLOR;
                    float2  uv          : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct Varyings
                {
                    float4  positionCS  : SV_POSITION;
                    float4  color       : COLOR;
                    float2  uv          : TEXCOORD0;
                    half2   lightingUV  : TEXCOORD1;
                    #if defined(DEBUG_DISPLAY)
                    float3  positionWS  : TEXCOORD2;
                    #endif
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);
                TEXTURE2D(_MaskTex);
                SAMPLER(sampler_MaskTex);
                half4 _MainTex_ST;
                float4 _Color;
                half4 _RendererColor;

                float _Hue, _Saturation, _Value;
                float4 _OutlineColor;
                float _OutlineThickness;

                #if USE_SHAPE_LIGHT_TYPE_0
                SHAPE_LIGHT(0)
                #endif

                #if USE_SHAPE_LIGHT_TYPE_1
                SHAPE_LIGHT(1)
                #endif

                #if USE_SHAPE_LIGHT_TYPE_2
                SHAPE_LIGHT(2)
                #endif

                #if USE_SHAPE_LIGHT_TYPE_3
                SHAPE_LIGHT(3)
                #endif

                Varyings CombinedShapeLightVertex(Attributes v)
                {
                    Varyings o = (Varyings)0;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    o.positionCS = TransformObjectToHClip(v.positionOS);
                    #if defined(DEBUG_DISPLAY)
                    o.positionWS = TransformObjectToWorld(v.positionOS);
                    #endif
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);

                    o.color = v.color * _Color * _RendererColor;
                    return o;
                }

                #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

                // Функции конверсии RGB <-> HSV
                float3 RGBtoHSV(float3 c)
                {
                    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
                    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
                    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
                    float d = q.x - min(q.w, q.y);
                    float e = 1.0e-10;
                    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
                }

                float3 HSVtoRGB(float3 hsv)
                {
                    float3 rgb = clamp(abs(frac(hsv.x + float3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0) - 1.0, 0.0, 1.0);
                    return hsv.z * lerp(float3(1.0, 1.0, 1.0), rgb, hsv.y);
                }

                half4 CombinedShapeLightFragment(Varyings i) : SV_Target
                {
                    // Получаем основной цвет спрайта
                    half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                    // Конвертируем в HSV
                    float3 hsv = RGBtoHSV(main.rgb);
                    hsv.x = frac(hsv.x + _Hue);
                    hsv.y = clamp(hsv.y * _Saturation, 0.0, 1.0);
                    hsv.z *= _Value;

                    // Возвращаем обратно в RGB
                    main.rgb = HSVtoRGB(hsv);

                    // Применяем маску
                    const float4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                    SurfaceData2D surfaceData;
                    InputData2D inputData;
                    

                    // Инициализируем данные для освещения
                    InitializeSurfaceData(main.rgb, main.a, mask, surfaceData);
                    InitializeInputData(i.uv, i.lightingUV, inputData);

                    // Возвращаем окончательный цвет после освещения
                    return CombinedShapeLightShared(surfaceData, inputData);
                }

                ENDHLSL
            }

            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
    
                struct appdata_t
                {
                    float4 vertex : POSITION;
                    float2 texcoord : TEXCOORD0;
                    float3 normal : NORMAL;
                    float4 color : COLOR;
                };
    
                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float2 texcoord : TEXCOORD0;
                    float3 normal : TEXCOORD1;
                    float4 color : COLOR;
                };
    
                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _OutlineColor;
                float _OutlineThickness;
                float4 _TintColor;
                float _TintAlpha;
    
                v2f vert(appdata_t v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    o.normal = v.normal;
                    o.color = v.color;
                    return o;
                }
    
                fixed4 frag(v2f i) : SV_Target
                {
                    float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                    float3 normal = normalize(i.normal);
                    float diffuse = max(0, dot(normal, lightDir));
    
                    float2 offset[8] = {
                        float2(-1, -1), float2(-1, 0), float2(-1, 1),
                        float2(0, -1), float2(0, 1),
                        float2(1, -1), float2(1, 0), float2(1, 1)
                    };
    
                    float4 original = tex2D(_MainTex, i.texcoord) * i.color;
                    float4 outlineColor = _OutlineColor;
                    float thickness = _OutlineThickness;
    
                    float alpha = original.a;
    
                    for (int j = 0; j < 8; j++)
                    {
                        float2 samplePos = i.texcoord + offset[j] * thickness;
                        alpha = max(alpha, tex2D(_MainTex, samplePos).a);
                    }
    
                    if (original.a == 0 && alpha > 0)
                    {
                        return outlineColor;
                    }
    
                    float4 tintedColor = _TintColor;
                    tintedColor.a = _TintAlpha * original.a;
    
                    float4 finalColor = lerp(original, tintedColor, _TintAlpha);
                    finalColor.rgb *= diffuse;
    
                    // Если альфа спрайта равна 0, шейдер возвращает прозрачность
                    if (original.a == 0)
                    {
                        return float4(0, 0, 0, 0);
                    }
    
                    return finalColor;
                }
                ENDCG
            }
        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 tangent      : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                half4   color           : COLOR;
                float2  uv              : TEXCOORD0;
                half3   normalWS        : TEXCOORD1;
                half3   tangentWS       : TEXCOORD2;
                half3   bitangentWS     : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            half4 _NormalMap_ST;  // Is this the right way to do this?

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = TRANSFORM_TEX(attributes.uv, _NormalMap);
                o.color = attributes.color;
                o.normalWS = -GetViewForwardDir();
                o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 NormalsRenderingFragment(Varyings i) : SV_Target
            {
                const half4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                const half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));

                return NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
            }
            ENDHLSL
        }
       
        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float4  color           : COLOR;
                float2  uv              : TEXCOORD0;
                #if defined(DEBUG_DISPLAY)
                float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;
            half4 _RendererColor;

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
                o.color = attributes.color * _Color * _RendererColor;
                return o;
            }
            

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                #if defined(DEBUG_DISPLAY)
                SurfaceData2D surfaceData;
                InputData2D inputData;
                half4 debugColor = 0;

                InitializeSurfaceData(mainTex.rgb, mainTex.a, surfaceData);
                InitializeInputData(i.uv, inputData);
                SETUP_DEBUG_DATA_2D(inputData, i.positionWS);

                if(CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                {
                    return debugColor;
                }
                #endif

                return mainTex;
            }
            ENDHLSL
        }

    }

    Fallback "Sprites/Default"
}
