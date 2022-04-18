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

float3x3 WVIT;

float FarClip = 2000;
float NearClip;
float LightBias;

sampler DIFFUSE : register(s0);
sampler NORMAL : register(s1);
sampler DEPTH : register(s2);
sampler LIGHTING : register(s3);

texture DiffuseMap;
sampler DiffuseSampler = sampler_state
{
	texture = <DiffuseMap>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
	float3 Tangent : TANGENT0;
	float3 BiTangent : BINORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Depth : TEXCOORD1;
    float3x3 TBN : TEXCOORD2;
};
struct PSO
{
    float4 Diffuse : COLOR0;
    float4 Normals : COLOR1;
    float4 Depth : COLOR2;
    float4 Lighting : COLOR3;
};

half3 encode(half3 n)
{
    n = normalize(n);
    n.xyz = 0.5f * (n.xyz + 1.0f);
    return n;
}
half3 decode(half3 enc)
{
    return (2.0f * enc.xyz - 1.0f);
}

float3 ambient_light;


VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 wvp = mul(World, mul(View, Projection));
		
	output.Position = mul(input.Position, wvp);
    output.TexCoord = input.TexCoord;

		
    //output.Depth = 1-((output.Position.z / FarClip));

	output.Depth = output.Position;
    
	output.TBN[0] = normalize(mul(input.Tangent, (float3x3)WVIT));
	output.TBN[1] = normalize(mul(input.BiTangent, (float3x3)WVIT));
	output.TBN[2] = normalize(mul(input.Normal, (float3x3)WVIT));

	return output;
}

float PCF(float depth, float NdotL, float2 shadowmap_UV) {
	
	return 0.5f;
}

PSO MainPS(VertexShaderOutput input)
{
    PSO output = (PSO)0;

    float4 rgba = tex2D(DiffuseSampler, input.TexCoord);
	
    output.Depth.rgb = input.Depth.z/input.Depth.w;
	output.Depth.a = 1;

    output.Normals.rgb = encode(normalize(input.TBN[2]));
	output.Normals.a = 1;

    output.Diffuse = rgba;

	output.Lighting = float4(ambient_light,1);
		
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