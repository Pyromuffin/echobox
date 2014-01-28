Shader "Raymarching" {
	Properties {
		
	}
	SubShader {
		Tags { "RenderType"="Transparent" }
		LOD 200
		
		Pass {
			cull front ZTest always zwrite off
			 Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

				#pragma target 5.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
			
				struct v2f {
					float4 pos : SV_POSITION;
					float3 worldPos : texcoord0;
					float4 screenPos : texcoord1;
				};
			
			
				v2f vert(appdata_base v) {
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.worldPos = mul(_Object2World, v.vertex);
					o.screenPos = ComputeScreenPos(o.pos);
					return o;
				}

				float3 Hue(float H)
				{
					float R = abs(H * 6 - 3) - 1;
					float G = 2 - abs(H * 6 - 2);
					float B = 2 - abs(H * 6 - 4);
					return saturate(float3(R,G,B));
				}

				float3 HSVtoRGB(float3 HSV)
				{
					return ((Hue(HSV.x) - 1) * HSV.y + 1) * HSV.z;
				}
				
				float3 hsv2rgb(float3 c)
				{
					float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
					float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
					return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
				}
			
				float4 screenCorner;
				float4 cameraUp;
				float4 cameraRight;
				float StepSize;
				float4 cameraWorldSize;
				float worldSize;
				 
				sampler3D Current;
				sampler2D _CameraDepthTexture;

			
				float4 frag(v2f IN) : COLOR {
				    
					float2 uv = IN.screenPos.xy/IN.screenPos.w;
				    float depth = LinearEyeDepth(tex2D(_CameraDepthTexture, uv));

				    float3 front = screenCorner.xyz + (uv.x * cameraWorldSize.x * cameraRight.xyz) + (uv.y * cameraWorldSize.y * cameraUp.xyz);
				    float3 back = IN.worldPos.xyz;
				 
				    float3 dir = normalize(back - front);
				    float4 pos = float4(front, 0);
				 
				    float4 dst = float4(0, 0, 0, 0);
				    float4 src = 0;
				 
				    float2 value = float2(0,0);
				 
				    float3 Step = dir * StepSize; 
					
					
				    for(int i = 0; i < 200; i++)
				    {
				        
					
						float4 relativePos =  ( (pos-128)	/32) + .5f;
						relativePos.w = 0;

				        value = tex3Dlod(Current, relativePos ).rg;
 
						
						if (value.r != 0) 
							value.r = abs(log(abs(value.r)));
						else 
							value.r = 0;
					
						float3 rgb = hsv2rgb(float3(value.g,1,1))  ;
						
						src = float4(rgb,value.r);

						src.a *= .001f; //reduce the alpha to have a more transparent result 
				         
				        //Front to back blending
				        // dst.rgb = dst.rgb + (1 - dst.a) * src.a * src.rgb
				        // dst.a   = dst.a   + (1 - dst.a) * src.a     
				        src.rgb *= src.a;
				        dst = (1.0f - dst.a)*src + dst;     
				     
				        //break from the loop when alpha gets high enough
				        if(dst.a >= .95)
				            break; 
					
				     
				        //advance the current position
				        pos.xyz += Step;
						
						if(length(pos - front) > depth)
							break;
				     
				        //break if the position is greater than <1, 1, 1>
				    
				    }
					
				    return float4(dst.rgb, 1-dst.a);
				}

			
			ENDCG
		}
	}
}