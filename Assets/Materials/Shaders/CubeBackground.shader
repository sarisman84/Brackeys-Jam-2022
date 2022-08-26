Shader "Unlit/CubeBackround"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 pos : TEXCOORD0;
                float3 camPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                //calculate the position in clip space to render the object
                o.position = UnityObjectToClipPos(v.vertex);

                o.pos = mul(unity_ObjectToWorld, v.vertex);//local vertex position to world space
                o.camPos = _WorldSpaceCameraPos;//world space camera position
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.pos - i.camPos);
                //return float4(dir, 1.0f);
                
                float3 absDir = abs(dir);
                float3 face = float3(absDir.x > absDir.y && absDir.x > absDir.z, 
                                     absDir.y > absDir.x && absDir.y > absDir.z,
                                     absDir.z > absDir.x && absDir.z > absDir.y);
                int d  = 0*face.x + 1*face.y + 2*face.z;
                int o1 = 1*face.x + 2*face.y + 0*face.z;
                int o2 = 2*face.x + 0*face.y + 1*face.z;
            
                float a1 = atan(dir[o1] / absDir[d]);
                float a2 = atan(dir[o2] / absDir[d]);
                float2 uv = float2(a1, a2) / 0.7853f;//angle from rad to [-1, 1]    (divide by 0.25*pi -> on quarter turn)
                uv = 0.5f*(uv+1.0f);

                fixed4 col = tex2D(_MainTex, uv * _MainTex_ST.xy);
                return col * _Color * _Intensity;
            }
            ENDCG
        }
    }
}
