Shader "Custom/RayCursorShader"
{
    Properties
    {
        _OutlineWidth("Outline Width", Range(0, 0.4)) = 0.03
        _CenterSize("Center Size", Range(0, 0.5)) = 0.15
        _Color("Inner Color", Color) = (1,1,1,1)
        _OutlineColor("Outline Color", Color) = (0,0.44,1,1)
        _Alpha("Alpha", Range(0, 1)) = 1
        _RadialGradientIntensity("Radial Gradient Intensity", Range(0, 1)) = 1
        _RadialGradientScale("Radial Gradient Scale", Range(0, 1)) = 0.2
        _RadialGradientBackgroundOpacity("Background Opacity", Range(0, 1)) = 1
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+10" 
            "IgnoreProjector"="True" 
        }
        
        // 両面描画とZ深度の設定
        Cull Off
        ZTest LEqual
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // プロパティの変数定義
            float _RadialGradientScale;
            float _RadialGradientIntensity;
            float _RadialGradientBackgroundOpacity;
            float _OutlineWidth;
            float4 _Color;
            float4 _OutlineColor;
            float _CenterSize;
            float _Alpha;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                
                // 中心のドット
                float centerMask = 1 - saturate(dist / _CenterSize);
                
                // アウトライン
                float outlineMask = 1 - saturate((dist - _CenterSize) / _OutlineWidth);
                
                // 放射状のグラデーション
                float gradientMask = 1 - saturate(dist / _RadialGradientScale);
                gradientMask = pow(gradientMask, _RadialGradientIntensity);
                
                // 色の合成
                float4 col = _Color * centerMask;
                col += _OutlineColor * outlineMask * (1 - centerMask);
                col.a *= _Alpha * max(max(centerMask, outlineMask), 
                    gradientMask * _RadialGradientBackgroundOpacity);
                
                return col;
            }
            ENDCG
        }
    }
}
