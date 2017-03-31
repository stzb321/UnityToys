Shader "Unlit/CardtoonShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainColor("MainColor", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

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
				float3 normal : NORMAL0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 viewDir : NORMAL0;
				float3 normalDir : NORMAL1;
				float3 lightDir : NORMAL2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _MainColor;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.viewDir = WorldSpaceViewDir(v.vertex);
				o.lightDir = ObjSpaceLightDir(v.vertex);
				o.normalDir = UnityObjectToWorldNormal ( v.normal );
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed edge = dot(normalize(i.viewDir), i.normalDir);
				float edgeStrong = 1.0;
				if (edge < 0.4 ){
					edgeStrong = 0.0;
				}
				fixed angle = dot(normalize(i.lightDir), i.normalDir);
				float strong = 0.0;
				if (angle < 0.1){
					strong = 0.1;
				}else if(angle < 0.3){
					strong = 0.3;
				}else if(angle < 0.5){
					strong = 0.5;
				}else if(angle < 0.7){
					strong = 0.7;
				}else if(angle >= 0.7){
					strong = 1.0;
				}
				strong = strong * edgeStrong;
				col = _MainColor * strong;
				return col;
			}
			ENDCG
		}
	}
}
