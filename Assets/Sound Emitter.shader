Shader "Sound Emitter" {
	Properties {
		_Emit ("Sound Emission", Vector) = (0,0,0,0)
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
			
				struct v2f {
					float4 pos : SV_POSITION;
					
				};
			
				v2f vert(appdata_base v) {
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					return o;
				}
			
				float4 _Emit;
			
				float4 frag(v2f IN) : COLOR {
					half4 c = _Emit;
					return c;
				}
			ENDCG
		}
	}
}