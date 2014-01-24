Shader "TCParticles/Offscreen/Offscreen" 
{
	Properties 
	{
		_MainTex ("Texture", 2D) = "white" { }
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_InvFade ("Soft Particles Factor", Range(100.0,1000.0)) = 1.0
	}

	Category 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "TCParticles"="True" "TCOffscreen"="True"}
		
		ZWrite Off 
		Blend One One

		SubShader 
		{
			Pass 
			{	
				CGPROGRAM

				#pragma target 5.0
				#pragma multi_compile TC_BILLBOARD TC_BILLBOARD_STRETCHED TC_BILLBOARD_TAILSTRETCH TC_MESH
				#pragma multi_compile TC_ALIGNED TC_ROTATE
				#pragma multi_compile TC_COLOUR_LIFETIME TC_COLOUR_SPEED
				#pragma vertex particle_vertex
				#pragma fragment frag

				#include "TCShaderInc.cginc"

				uniform sampler2D _MainTex;
				uniform sampler2D _TCDepth;

				float2 _TCRes;

				float _InvFade;

				fixed4 frag (particle_fragment i) : COLOR
				{
					float2 suv = float2(i.pos.x, i.pos.y) / (_TCRes);
					float z = Linear01Depth(i.pos.z);
					float sz = DecodeFloatRGBA(tex2D(_TCDepth, suv).rgba);
					float fade = saturate (_InvFade * (sz - z));
					i.col.a *= fade;

					return tex2Dbias(_MainTex, float4(i.uv, 0, -3)) * i.col;
				}
				
				ENDCG
				
			}
		}
	}

	Fallback Off
}
