Shader "Custom/DepthShader"
{
    Properties
    {
        _DepthMultiplier ("Depth Multiplier", Float) = 1.0
        _DepthPower ("Depth Power", float) = 0.3
        _DepthAdd ("Depth Add", float) = 0.0
        [Toggle] _UseEffect ("Invert Color", float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _CameraDepthTexture;
            float _DepthMultiplier;
            float _DepthPower;
            float _DepthAdd;
            float _UseEffect;

            v2f vert (appdata_t v) //issue, the way the shader is implemented shows mirror image upside down
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                // o.uv = o.pos.xy * 0.5 + 0.5;
                o.uv = float2(o.pos.x * 0.5 + 0.5, 1.0 - (o.pos.y * 0.5 + 0.5)); //fix
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Read depth from depth texture
                float depth = tex2D(_CameraDepthTexture, i.uv).r;
                // Convert depth to brightness (closer = brighter)
                // float brightness = pow(((_DepthMultiplier) * depth ), _DepthPower)+ _DepthAdd;
                float brightness = (depth - 0.6)/5.4;
                if(_UseEffect < 0.5){
                     brightness =  1.0 - clamp(brightness, 0.0, 1);// not to let values exceed 1
                } else {
                     brightness = clamp(brightness, 0.0, 1); 
                }
               
                return fixed4(brightness, brightness, brightness, 1.0); // rgba I think
            }
            ENDCG
        }
    }
}
