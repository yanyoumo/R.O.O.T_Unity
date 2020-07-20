Shader "Custom/FontShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        //_MainTex_ST("_MainTex_ST", Vector) = (1,1,1,1)
        _Digit("_Digit",Range(0,9))=0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _MainTex;
        float4 _MainTex_ST_Tmp;
        float _Digit;
        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            _MainTex_ST_Tmp=fixed4(0.23,0.34,0,0.33);
            if(_Digit<1){
                _MainTex_ST_Tmp=fixed4(0.23,0.34,0,0.33);
            }else if(_Digit<2){
                _MainTex_ST_Tmp=fixed4(0.23,0.34,0,0.33);
            }else if(_Digit<3){
                _MainTex_ST_Tmp=fixed4(0.23,0.34,0,0);
            }else if(_Digit<4){
                _MainTex_ST_Tmp=fixed4(0.23,0.34,0,0);
            }else if(_Digit<5){
                _MainTex_ST_Tmp=fixed4(0.23,0.34,0,0);
            }
            //fixed4 tex= tex2D (_MainTex, IN.uv_MainTex*_MainTex_ST_Tmp.xy+_MainTex_ST_Tmp.zw);
            fixed4 tex= tex2D (_MainTex, IN.uv_MainTex);
            fixed4 c = tex * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = 0;
            o.Smoothness = 0.0;
            o.Alpha = tex.r;
            clip(o.Alpha -0.5);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
