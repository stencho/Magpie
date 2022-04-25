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
float3 decode(half3 enc)
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

	//output.Depth = output.Position;
    
	output.Depth.x = output.Position.z;
	output.Depth.y = output.Position.w;
	output.Depth.z = mul(mul(input.Position, World),View).z;

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
	
	/*
    output.Depth.rgb = input.Depth.z/input.Depth.w;
	output.Depth.a = 1;
	*/
	
	output.Depth.r = input.Depth.x / input.Depth.y;
	output.Depth.gba = 1;

    output.Normals.rgb = encode(normalize(input.TBN[2]));
	output.Normals.a = 1;

    output.Diffuse = rgba;

	//TODO vvvUSE ALPHA CHANNEL OF LIGHTINGvvv FOR KEEPING TRACK OF SCENE ALPHA
	// this will allow for at the very least 1 bit of alpha through obviously Lighting.w.a = 0;
	// but also with a bit of work, well, a float is a lot of bytes, it would be possible to do stuff like storing a set of bytes in a float in the alpha, representing an ID from a list of 255 possible values
	// I think this would allow up to 4 transparencies in a row before it'd break, and would allow individual IDs 0-255
	// 4 8-bit ints packed into a 32-bit float, [AAAA/AAAA][BBBB/BBBB][CCCC/CCCC][DDDD/DDDD]
	//										    0xAA,      0xBB,      0xCC,      0xDD
	output.Lighting = float4(0,0,0,1);
		
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