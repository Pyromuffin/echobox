Shader "TCParticles/Additive (soft)" 
{
	Properties 
	{
		_MainTex ("Texture", 2D) = "white" { }
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	}

	Category 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "TCParticles"="True" }
		Blend OneMinusDstColor One
		ZWrite Off 

		SubShader 
		{
			Pass 
			{
				CGPROGRAM

				#pragma target 5.0
				#pragma multi_compile TC_BILLBOARD TC_BILLBOARD_STRETCHED TC_BILLBOARD_TAILSTRETCH TC_MESH
				#pragma multi_compile TC_ALIGNED TC_ROTATE
				#pragma multi_compile TC_COLOUR_LIFETIME TC_COLOUR_SPEED
				#pragma multi_compile TC_SIZE TC_PIXEL_SIZE 
				#pragma multi_compile TC_PERSPECTIVE TC_ORTHOGRAPHIC
				#pragma multi_compile TC_UV_NORMAL TC_UV_SPRITE_ANIM
				#pragma vertex particle_vertex
				#pragma fragment frag

				#include "TCShaderInc.cginc"

				sampler2D _MainTex;

				fixed3 frag (particle_fragment i) : COLOR
				{
					fixed4 col = tex2Dbias(_MainTex, float4(i.uv, 0, -3));
					return min(float3(1.0f, 1.0, 1.0f), col.rgb * i.col.rgb * i.col.a * col.a);
				}

				ENDCG
			}
		}
	}

	Fallback Off
}
