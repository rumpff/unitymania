Shader "BitShiftProductions/Noises"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white"
		_NoiseScale("Noise Scale",Float) = 5.0
		_Strength("Noise Strength",Float) = 1.0
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" }

		Pass
	{
		Stencil{
		Ref 2  //Customize this value
		Comp Equal //Customize the compare function
		Pass Keep
	}

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

	sampler2D _MainTex;
	float _NoiseScale;
	float _Strength;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	float random(float2 p)
	{
		return frac(sin(dot(p.xy,float2(_Time.y,65.115)))*2773.8856);
	}
	fixed4 frag(v2f i) : SV_Target
	{
		float2 normUV = i.uv;
		normUV *= _NoiseScale; // Scale the coordinate system by 10
		float2 ipos = floor(normUV);  // get the integer coords

									  // Assign a random value based on the integer coord
		float rand = random(ipos);
		fixed4 col = fixed4(rand,rand,rand,1.0);
		col = lerp(tex2D(_MainTex,i.uv),col,_Strength);
		return col;
	}

		ENDCG
	}
	}
}
