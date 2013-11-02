Shader "Raymarching" {
	Properties {
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" "Queue" = "Geometry+1" }
		LOD 200
		
		Pass {
		Lighting Off Cull front ZTest Always ZWrite Off Fog { Mode Off}  
			CGPROGRAM

				#pragma target 5.0
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
			
				struct v2f {
					float4 pos : SV_POSITION;
					float3 worldPos : texcoord0;
					float4 screenPos : texcoord1;
					float2 depth : texcoord2;
				};
			
			
				v2f vert(appdata_base v) {
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.worldPos = mul(_Object2World, v.vertex);
					o.screenPos = ComputeScreenPos(o.pos);
					COMPUTE_EYEDEPTH(o.depth);
					return o;
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
				 
				    float value = 0;
				 
				    float3 Step = dir * StepSize; 
				 
				    for(int i = 0; i < 200; i++)
				    {
				        pos.w = 0;
				        value = tex3Dlod(Current, pos/worldSize).r;
				        if(value < 0)
							 src = float4(0,abs(value),0,abs(value));
						else
							src =  float4(value,0,0,value);
							    
						src.a *= .1f; //reduce the alpha to have a more transparent result 
				         
				        //Front to back blending
				        // dst.rgb = dst.rgb + (1 - dst.a) * src.a * src.rgb
				        // dst.a   = dst.a   + (1 - dst.a) * src.a     
				        src.rgb *= src.a;
				        dst = (1.0f - dst.a)*src + dst;     
				     
				        //break from the loop when alpha gets high enough
				        if(dst.a >= .95f)
				            break; 
						if(length(pos - front) > depth)
							break;
				     
				        //advance the current position
				        pos.xyz += Step;
				     
				        //break if the position is greater than <1, 1, 1>
				    
				    }
					
				    return dst;
				}
			ENDCG
		}
	}
}