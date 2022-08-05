#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float2 resolution;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 hundo = input.TextureCoordinates * 100;	
	float aspect = resolution.y / resolution.x;
	hundo.y *= aspect;
	
	if (fmod(hundo.y, 2) < 1) {
		if (fmod(hundo.x, 2) < 1) {
			return float4(0.75,0.75,0.75,1);	
		} else {
			return float4(0.25,0.25,0.25,1);			
		}	
	} else {
		if (!(fmod(hundo.x, 2) < 1)) {
			return float4(0.75,0.75,0.75,1);	
		} else {
			return float4(0.25,0.25,0.25,1);			
		}	
	}
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};