#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

matrix World;
matrix View;
matrix Projection;

sampler DIFFUSE : register(s0);
sampler NORMAL : register(s1);
sampler DEPTH : register(s2);
sampler LIGHTING : register(s3);

struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 ScreenPosition : TEXCOORD0;
	float4 Color : COLOR0;
};

struct PSO
{
    float4 Diffuse : COLOR0;
    float4 Normals : COLOR1;
    float4 Depth : COLOR2;
    float4 Lighting : COLOR3;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;
	
	float4x4 wvp = mul(World, mul(View, Projection));
	output.Position = mul(input.Position, wvp);
	output.ScreenPosition = output.Position;
	output.Color = float4(1,1,1,1);

	return output;
}

PSO MainPS(VertexShaderOutput input) : COLOR
{
	input.ScreenPosition.xy /= input.ScreenPosition.w;
	PSO output = (PSO)1;
	float d = tex2D(DEPTH,input.ScreenPosition.xy);
	output.Diffuse = float4(0,0,0,0);
	output.Normals = float4(0,0,0,0);
	output.Depth = float4(d,d,d,1);
	output.Lighting = float4(1,1,1,1);
	return output;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};