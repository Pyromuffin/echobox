Shader "Voxelize" 
{
	Properties 
	{

	}

	SubShader 
	{
		Pass
		{
			zwrite off ztest always cull off
			
			Tags { "RenderType"="Opaque" }
			LOD 200
		
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 

				// **************************************************************
				// Data structures												*
				// **************************************************************
				struct GS_INPUT
				{
					float4	pos		: POSITION;
				//	float3	normal	: NORMAL;
				//	float2  tex0	: TEXCOORD0;
				};

				struct FS_INPUT
				{
					float4	pos		: POSITION;
					//float3 col	: COLOR;
					nointerpolation float4x4  perm : texcoord0;
				};


				// **************************************************************
				// Vars															*
				// **************************************************************

				float4x4 zMVP;
				RWTexture3D<float> media;
				
				// **************************************************************
				// Shader Programs												*
				// **************************************************************

				// Vertex Shader ------------------------------------------------
				GS_INPUT VS_Main(appdata_base v)
				{
					GS_INPUT output = (GS_INPUT)0;

					output.pos =  mul(_Object2World,v.vertex);
					//output.normal = v.normal;
					//output.tex0 = float2(0, 0);

					return output;
				}



				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(3)]
				void GS_Main(triangle GS_INPUT p[3], inout TriangleStream<FS_INPUT> triStream)
				{
				
				
					//calculate normal
				
							
					float3 normal = cross( (p[1].pos.xyz - p[0].pos.xyz ), (p[2].pos.xyz - p[0].pos.xyz) );
					
					float xDot = abs( dot(normal,float3(1,0,0)));
					float yDot = abs( dot(normal,float3(0,1,0)));
					float zDot = abs( dot(normal,float3(0,0,1)));
				
					float4x4 perm;
					FS_INPUT pIn;
					if(xDot > yDot && xDot > zDot){
						//pIn.col = float3(1,0,0);
						perm = float4x4(0,1,0,0, 0,0,1,0, 1,0,0,0 ,0,0,0,1);
					}
					else if(yDot > xDot && yDot > zDot){
						//pIn.col = float3(0,1,0);
						perm = float4x4 (0,0,1,0, 1,0,0,0, 0,1,0,0 ,0,0,0,1);
					}
					else{
						//pIn.col = float3(0,0,1);
						perm = float4x4(1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1);
					}
					
					pIn.perm = perm;
	
					zMVP = mul( zMVP,perm);
					
					pIn.pos = mul(zMVP, p[0].pos);
					//pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(zMVP, p[1].pos);
					//pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos =  mul(zMVP, p[2].pos);
					//pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

				}



				// Fragment Shader -----------------------------------------------
				float4 FS_Main(FS_INPUT input) : COLOR
				{
				
					float3 unswizzled = mul(input.perm, input.pos).xyz;
					media[floor(unswizzled)] = 0;
					
					
					discard;
					return float4(0,0,0,0);
				}

			ENDCG
		}
	} 
}
