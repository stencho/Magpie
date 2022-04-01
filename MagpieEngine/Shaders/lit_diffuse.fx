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

float4x4 WVIT;

float FarClip = 2000;
float NearClip;
float LightBias;
bool in_light;

bool flip_texture_h;
bool flip_texture_v;

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
    float4 UV : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float4 Depth : TEXCOORD2;
	float4 lightpos : TEXCOORD3;
	float4 WorldPos : TEXCOORD4;
    float4 color : COLOR1;
    
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

float4x4 lightWVP;
float LightClip;
float3 LightPosition;
float3 LightDirection;
float3 light_color;
float3 ambient_light;

texture shadow_map;
SamplerState ShadowMapSampler
{
	texture = <shadow_map>;
	MinFilter = anisotropic;
	MagFilter = anisotropic;
	MipFilter = anisotropic;
	AddressU = clamp;
	AddressV = clamp;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
	VertexShaderOutput output = (VertexShaderOutput)0;

	float4x4 wvp = mul(World, mul(View, Projection));
		
	output.lightpos = mul(input.Position, lightWVP);
	output.WorldPos = mul(input.Position, World);
	output.Position = mul(input.Position, wvp);
    output.TexCoord = input.UV;

    if (flip_texture_h > 0)
        output.TexCoord.x = 1 - output.TexCoord.x;
    if (flip_texture_v > 0)
        output.TexCoord.y = output.TexCoord.y - 1;
		
    output.Depth = 1-((output.Position.z / FarClip));
    output.Depth.a = 1;
    
    output.Normal = normalize(mul(input.Normal, WVIT));
	return output;
}

float PCF(float depth, float NdotL, float2 shadowmap_UV) {
	
	return 0.5f;
}

PSO MainPS(VertexShaderOutput input) : COLOR
{
    PSO output = (PSO)0;
	bool osm = true;
	float d_center = 0.5;
	float distance = 0.0;

    float4 rgba = tex2D(DiffuseSampler, input.TexCoord);
		
    output.Depth.rgb = input.Depth.r;
	output.Depth.a = 1;
    output.Normals.rgb = encode(input.Normal);
	output.Normals.a = 1;
    output.Diffuse = rgba;
	output.Lighting = float4(ambient_light,1);
		
	return output;
}

		/*
	float lpos = input.lightpos.z / input.lightpos.w;

	float2 stexcoord = mad(0.5, input.lightpos.xy / input.lightpos.w, float2(0.5, 0.5));
	stexcoord.y = 1.0f - stexcoord.y;

	float stx = tex2D(ShadowMapSampler, stexcoord.xy).x;

	if ((saturate(stexcoord.x) == stexcoord.x) && (saturate(stexcoord.y) == stexcoord.y)) {
		osm = false;  
	}

	float3 l = 1;
	
	if (in_light){
		if (stx.x < lpos - 0.00001) {
			l = ambient_light;
		} else  {	
			d_center = 1-(length(float2(0.5, 0.5)-stexcoord.xy) * 2);
			distance = 1-(length(LightPosition - input.WorldPos) / LightClip);
		
			l = ( clamp(light_color * distance , ambient_light, 1.0));
		}

		if (osm) {
			l = ambient_light;
		}
	} else {
		l = ambient_light;
	}
	
	output.Lighting.rgb = l;
	*/


technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};