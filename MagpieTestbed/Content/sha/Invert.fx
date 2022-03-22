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

struct VSI
{
    float4 Position : POSITION0;
};

    struct VSO
{
    float4 Position : SV_POSITION;
    float2 tx : TEXCOORD0;
};
struct PSO
{
    float4 Diffuse : COLOR0;
    float4 Normals : COLOR1;
    float4 Depth : COLOR2;
    float4 Lighting : COLOR3;
};

VSO MainVS(VSI input)
{
    VSO output = (VSO) 0;
    float4x4 wvp = mul(World, mul(View, Projection));
    output.Position = mul(input.Position, wvp);
    output.tx = output.Position.xy;
    return output;
}

PSO MainPS(VSO input)
{
    PSO output = (PSO) 0;
    output.Normals = tex2D(NORMAL, input.tx);
    output.Depth = tex2D(DEPTH, input.tx);
    output.Lighting = tex2D(LIGHTING, input.tx);
        
    output.Diffuse.rgb = float3(1, 1, 1) - tex2D(DIFFUSE, input.tx);
    
    output.Diffuse.a = 1;
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