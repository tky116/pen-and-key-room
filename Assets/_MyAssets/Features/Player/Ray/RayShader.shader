Shader "Custom/RayShader"
{
    Properties
    {
        // レイの開始点と終点の色を定義
        _Color0 ("Start Color", Color) = (1,1,1,0.8)  // 開始点: 白、やや透明
        _Color1 ("End Color", Color) = (1,1,1,0)      // 終点: 完全に透明
    }
    SubShader
    {
        // 透明なオブジェクトの設定
        Tags 
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
        }
        LOD 100
        
        // 透明度のための設定
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // 頂点シェーダーへの入力構造体
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            // フラグメントシェーダーへの入力構造体
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // プロパティの変数定義
            uniform float4 _Color0;
            uniform float4 _Color1;

            // 頂点シェーダー
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                // UVをZ座標に基づいて設定（レイの長さに沿ったグラデーション用）
                o.uv = v.vertex.zz * 2.0f;
                return o;
            }
            
            // フラグメントシェーダー
            fixed4 frag (v2f i) : SV_Target
            {
                // 開始色から終了色へのグラデーションを計算
                fixed4 col = lerp(_Color0, _Color1, clamp(i.uv.x, 0.0f, 1.0f) * 1.5f);
                return col;
            }
            ENDCG
        }
    }
}
