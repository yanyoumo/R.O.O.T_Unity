Shader "ROOT/AlertIcon" {
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Shutter ("_Shutter", Range(0,1)) = 1.0

        _ShutterUpperLimit ("_ShutterUpperLimit", Range(0,1)) = 1.0
        _ShutterLowerLimit ("_ShutterLowerLimit", Range(0,1)) = 0.0
    }
    SubShader {
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed _Shutter;
            fixed _ShutterUpperLimit;
            fixed _ShutterLowerLimit;

            struct v2f {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_full v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv=v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D (_MainTex, i.uv);
                fixed3 color=fixed3(1,1,1);
                //fixed shutter=pow(_Shutter,2.2f);
                fixed shutter=lerp(_ShutterLowerLimit,_ShutterUpperLimit,_Shutter);
                color*=c.rgb;
                if(i.uv.y>shutter){
                    color=fixed3(1,0,0);
                }
                clip(c.a-0.5);
                return fixed4(color.rgb,1);
            }
            ENDCG

        }
    }
}