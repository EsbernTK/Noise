Shader "Custom/WhiteNoise"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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
			float fract(float num) {
				int wN = num;
				float f = num - wN;
				return abs(f);
			}
			float WhiteNoise(float seed) {
				float seed2 = (seed + 1.42345556)*545345.341534525634563;
				float co = cos(seed2);
				float si = sin(seed2);
				float fc = fract(co);
				float fs = fract(si);
				return ((((fract(fc*fc*fs*fs*44543453.58793245983456982345*co*si)))));
			}
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
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				float n = (WhiteNoise(i.uv[0]) + WhiteNoise(i.uv[1]))/2;
				fixed4 col = float4(n, n, n, 1);
				// just invert the colors
				//fixed4 col = fixed4(i.uv,1, 1);
				return col;
			}
			ENDCG
		}
	}
}
