Shader "TCParticles/Additive" 
{
	Properties 
	{
		_MainTex ("Texture", 2D) = "white" { }
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	}

	Category 
	{
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "TCParticles"="True"  "RenderType"="Transparent"}
		Blend SrcAlpha One
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


				fixed4 frag (particle_fragment i) : COLOR
				{
					return tex2Dbias(_MainTex, float4(i.uv, 0, -3)) * i.col;
				}
				
				ENDCG
				
			}
		}
	}

	Fallback Off
}
