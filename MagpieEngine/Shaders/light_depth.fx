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

float FarClip;
float NearClip;

//USE GBUFFER.BUFFER_TARGETS
sampler DIFFUSE : register(s0);
sampler NORMAL : register(s1);
sampler DEPTH : register(s2);
sampler LIGHTING : register(s3);


sampler dsampler = sampler_state
{
    texture = <diffuse>;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};


struct VSI
{
    float4 Position : POSITION0;
    float4 UV : TEXCOORD0;
};

struct VSO
{
    float4 Position : POSITION;
    float4 UV : TEXCOORD0;
    float4 Depth : TEXCOORD1;
    float3 pos3d : TEXCOORD6;
};
struct PSO
{
    float4 Depth : COLOR0;
};

VSO MainVS(in VSI input)
{
    VSO output = (VSO) 0;
    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.pos3d = input.Position.xyz;
    //output.Depth.xyzw = input.Position.xyzw;
    output.Depth = mul(input.Position, wvp) * 0.008;
    //MAKE THIS EXPONENTIAL
    //output.Depth = max(0.01f, length())
    output.Depth.a = 1;
    //output.Depth.a = 1;
    output.UV = input.UV;
    return output;
}

float logzbuf(float4 xyzw)
{
    return max(1e-6, log(NearClip * xyzw.z + 1) / log(NearClip * FarClip + 1) * xyzw.w);
}
float depth_bias;
float3 light_pos;
PSO MainPS(VSO input) : COLOR
{
    PSO output = (PSO) 0;
    input.Position /= input.Position.w;
    float4x4 wvp = mul(World, mul(View, Projection));
    float a = tex2D(dsampler, input.UV.xy).a;
    if (a < .5)
        clip(-1);
   // output.Depth.xyzw = input.Depth.xyzw;// * (FarClip * 0.02);
    float d = max(0.01, length(light_pos - input.pos3d)) / depth_bias;
    output.Depth = exp((depth_bias * 0.5) * d);
    output.Depth.a = 1;
    //output.Depth.g = 0;
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