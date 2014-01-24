#include "UnityCG.cginc"
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

struct Particle
{
	float3 pos;
	float3 velocity;
	float startSize;
	float size;
	float life;
	float deltLife;
	float rotation;
	float mass;
};
StructuredBuffer<Particle> particles : register(cs_5_0, u[0]);

sampler2D _ColTex;
float4 emitterPos;

float glow;

#ifdef TC_MESH
	StructuredBuffer<float2> uvs;
#endif

StructuredBuffer<float3> bufPoints;


#ifndef TC_BILLBOARD
#ifndef TC_MESH
	float speedScale;
	float lengthScale;
#endif
#endif

struct particle_fragment
{
	half4 pos : SV_POSITION;
	fixed4 col : COLOR;
	half2 uv : TEXCOORD0;


    float4 projPos : TEXCOORD1;

};

#ifdef TC_BILLBOARD_STRETCHED
	StructuredBuffer<float> stretchBuffer;
#endif

#ifdef TC_BILLBOARD_TAILSTRETCH
	StructuredBuffer<float> stretchBuffer;
#endif

#ifdef TC_COLOUR_SPEED
	float maxSpeed;
#endif


float bufferOffset;
float maxParticles;
float tcOrthoSize;

#ifdef TC_UV_SPRITE_ANIM
	float _SpriteAnimWidth;
	float _SpriteAnimHeight;

	float4 _SpriteAnimUv;
#endif


uint GetId(uint id)
{
	return (id + bufferOffset) % maxParticles;
}


float4x4 TC_MATRIX_M;
float4x4 TC_MATRIX_V;

float4x4 TC_MATRIX_VP;

#ifdef TC_MESH

	particle_fragment particle_vertex(uint id : SV_VertexID, uint inst : SV_InstanceID)
	{
		inst = GetId(inst);
		particle_fragment o;

		#ifdef TC_COLOUR_SPEED
			float4 tp = float4(length(particles[inst].velocity) / maxSpeed, 0.0f, 0.0f, 0.0f);
		#else
			float4 tp = float4(particles[inst].life, 0.0f, 0.0f, 0.0f);
		#endif

		float gl = glow + 1.0f;
		o.col = tex2Dlod(_ColTex, tp).rgba * float4(gl, gl, gl, 1.0f);

		#ifdef TC_ROTATE
			float angle = particles[inst].rotation;

			float c = cos(angle);
			float s = sin(angle);

			float3x3 rotation = float3x3(float3(c, 0, s), float3(0, 1, 0), float3(-s, 0, c));

			float3 cp =  mul(bufPoints[id], rotation);
		#else
			float3 cp = bufPoints[id];
		#endif

		o.uv = uvs[id];
		float4x4 matr = mul(UNITY_MATRIX_VP, TC_MATRIX_M);

		o.pos = mul(matr, float4(particles[inst].pos + emitterPos + cp * particles[inst].size, 1.0f));

		return o;
	}		

#else

	particle_fragment particle_vertex (uint id : SV_VertexID, uint it : SV_InstanceID)
	{
		particle_fragment output;
		
		uint inst = GetId(it);


		float4x4 matr = mul(UNITY_MATRIX_VP, TC_MATRIX_M);
		
		float4 pos = mul(matr, float4(particles[inst].pos, 1.0f));
		

		#ifdef TC_COLOUR_SPEED
			float4 tp = float4(length(particles[inst].velocity) / maxSpeed, 0.0f, 0.0f, 0.0f);
		#else
			float4 tp = float4(particles[inst].life, 0.0f, 0.0f, 0.0f);
		#endif

		float gl = glow + 1.0f;
		output.col = tex2Dlod(_ColTex, tp).rgba * float4(gl, gl, gl, 1.0f);


		float mult = 1.0f;


						

		#ifdef TC_PIXEL_SIZE
		
			mult = max(1.0f / _ScreenParams.x, 1.0f / _ScreenParams.y);
			
			#ifdef TC_PERSPECTIVE
				mult *= pos.w;
			#else
				mult *= tcOrthoSize;
			#endif

		#endif

		#ifdef TC_BILLBOARD
			#ifdef TC_ROTATE
				float angle = particles[inst].rotation;

				float c = cos(angle);
				float s = sin(angle);

				float3x3 rotation = float3x3(float3(c, -s, 0), float3(s, c, 0), float3(0, 0, 1)); 

				output.pos = pos + mul(UNITY_MATRIX_P, mul(bufPoints[id] * particles[inst].size * mult, rotation));
			#else
				output.pos = pos + mul(UNITY_MATRIX_P, bufPoints[id] * particles[inst].size * mult);
			#endif	
		#elif TC_BILLBOARD_STRETCHED || TC_BILLBOARD_TAILSTRETCH
			
			half3 sv = mul((float3x3)UNITY_MATRIX_V, particles[inst].velocity) + float3(0.00001f, 0.0f, 0.0f);
			half l = length(sv);
			
			half3 vRight = particles[inst].size * sv / l  * mult;
			half3 vUp = float3(vRight.y, -vRight.x, 0.0f);

			

			output.pos = pos + mul(UNITY_MATRIX_P, float4(bufPoints[id].x * vRight * (1.0f + stretchBuffer[id] * (lengthScale + l * speedScale)) + bufPoints[id].y * vUp, 0.0f) );
		#else
			output.pos = float4(0.0f, 0.0f, 0.0f, 0.0f);
		#endif


		#ifdef TC_UV_SPRITE_ANIM
			half2 uv = bufPoints[id] + 0.5f;
			int sprite = (int)(particles[inst].life * (_SpriteAnimWidth * _SpriteAnimHeight));
			int x = sprite % (int)_SpriteAnimWidth;
			int y = sprite % (int)_SpriteAnimHeight;

			output.uv = float2(x * 1.0f / _SpriteAnimWidth, y * 1.0f / _SpriteAnimWidth) + uv * float2( 1.0f / _SpriteAnimWidth, 1.0f / _SpriteAnimWidth);
		#else
			output.uv = bufPoints[id] + 0.5f;
		#endif

		#ifdef TC_OFFSCREEN
			output.projPos = ComputeScreenPos(output.pos);
			output.projPos.z = -mul( UNITY_MATRIX_V, mul(TC_MATRIX_M, float4(particles[inst].pos, 0.0f))).z;
		#endif

		return output;
	}
#endif