Shader "Hidden/OldFilmEffect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_HoloTex ("HoloTex", 2D) = "white" {}
		_HoloAmount("HoloAmount", Range(0.0, 1.0)) = 1.0
		_DustTex ("DustTex", 2D) = "white" {}
		_DustAmount("DustAmount", Range(0.0, 1.0)) = 1.0
		_LinesTex ("LinesTex", 2D) = "white" {}
		_LinesAmount("LinesAmount", Range(0.0, 1.0)) = 1.0
		_LinesXSpeed("LinesXSpeed", Range(0.0, 1.0)) = 1.0
		_LinesYSpeed("LinesYSpeed", Range(0.0, 1.0)) = 1.0
		_RandomVal("RandomVal", Range(-1.0, 1.0)) = 1.0
		_EffectAmount("EffectAmount", Range(0.0, 1.0)) = 1.0
		_AddTiveColor("AddTiveColor", Color) = (1,1,1,1)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _HoloTex;
			float _HoloAmount;
			sampler2D _DustTex;
			float _DustAmount;
			sampler2D _LinesTex;
			float _LinesAmount;
			float _LinesXSpeed;
			float _LinesYSpeed;
			float _RandomVal;
			fixed4 _AddTiveColor;
			float _EffectAmount;

			fixed4 frag (v2f i) : SV_Target
			{
				half2 renderTexUV = half2(i.uv.x, i.uv.y + (_RandomVal * _SinTime.z * 0.005));
				fixed4 col = tex2D(_MainTex, renderTexUV);
				fixed lum = dot(fixed3(0.299, 0.587, 0.114), col.rgb);
				fixed4 final = lum + lerp(_AddTiveColor, _AddTiveColor + fixed4(0.1f, 0.1f, 0.1f, 0.1f), _RandomVal);

				half2 LineUV = half2(i.uv.x + (_RandomVal * _Time.x * _LinesXSpeed), i.uv.y + (_Time.x * _LinesYSpeed));

				fixed4 HoloCol = tex2D(_HoloTex, i.uv);
				fixed4 DustCol = tex2D(_DustTex, i.uv);
				fixed4 LinesCol = tex2D(_LinesTex, LineUV);
				final = lerp(final, final * HoloCol, _HoloAmount);
				final = lerp(final, final * DustCol, _DustAmount);
				final = lerp(final, final * LinesCol, _LinesAmount);
				final = lerp(col, final, _EffectAmount);

				return final;
			}
			ENDCG
		}
	}
}
