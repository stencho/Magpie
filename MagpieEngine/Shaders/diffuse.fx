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

matrix WVIT;

float FarClip = 2000;
float NearClip;

float3 tint = float4(1.0, 1.0, 1.0, 1.0);
float3 light_dir = float3(0.5, -1, 0);
bool transparent = false;
float opacity = 1.0;

float4 sky_color;
float sky_brightness;
float atmosphere = 0.02;

bool flip_texture_h;
bool flip_texture_v;

bool fn = false;
float3 atmosphere_color;

struct VSI
{
	float4 Position : POSITION0;
    float4 UV : TEXCOORD0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
    float3 Tangent : TANGENT0;
    float3 BiTangent : BINORMAL0;
};

struct VSO
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Depth : TEXCOORD1;
    float3x3 TBN : TEXCOORD2;
    float4 color : COLOR1;
    
};
struct PSO
{
    float4 Diffuse : COLOR0;
    float4 Normals : COLOR1;
    float4 Depth : COLOR2;
    float4 Lighting : COLOR3;
};








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



float Phong(float3 N)
{
    float3 R = normalize(reflect(light_dir.rgb, N));
    return dot(N, -light_dir.rgb);
}

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

float logzbuf(float z, float w)
{
    return max(1e-6, log(NearClip * z + 1) / log(NearClip * FarClip + 1) * w);
}
float logzbuf(float4 xyzw)
{
    return max(1e-6, log(NearClip * xyzw.z + 1) / log(NearClip * FarClip + 1) * xyzw.w);
}

float3 force_normal = 0;

float4 manualSample(sampler Sampler, float2 UV, float2 textureSize)
{
    float2 texelpos = textureSize * UV;
    float2 lerps = frac(texelpos);
    float texelSize = 1.0 / textureSize;
    float4 sourcevals[4];
    sourcevals[0] = tex2D(Sampler, UV);
    sourcevals[1] = tex2D(Sampler, UV + float2(texelSize, 0));
    sourcevals[2] = tex2D(Sampler, UV + float2(0, texelSize));
    sourcevals[3] = tex2D(Sampler, UV + float2(texelSize, texelSize));

    float4 interpolated = lerp(lerp(sourcevals[0], sourcevals[1], lerps.x),
		lerp(sourcevals[2], sourcevals[3], lerps.x),
		lerps.y);
    return interpolated;
}


// REGULAR RENDER PASSES
VSO MainVS(in VSI input)
{
	VSO output = (VSO)0;
	float4x4 wvp = mul(World, mul(View, Projection));

	//clip space position
	output.Position = mul(input.Position, wvp);


    //output.pos3d =  mul(input.Position, world);
    output.TexCoord = input.TexCoord;

    if (flip_texture_h > 0)
        output.TexCoord.x = 1 - output.TexCoord.x;
    if (flip_texture_v > 0)
        output.TexCoord.y = output.TexCoord.y - 1;
    
    output.Depth = 1-((output.Position.z / FarClip) / 1);
    output.Depth.a = 1;
    
    output.TBN[0] = normalize(mul(input.Tangent, (float3x3) WVIT));
    output.TBN[1] = normalize(mul(input.BiTangent, (float3x3) WVIT));    
    output.TBN[2] = normalize(mul(input.Normal, (float3x3) WVIT));
	return output;
}

matrix IVP;
PSO MainPS(VSO input)
{
    float4 rgba = tex2D(DiffuseSampler, input.TexCoord);
    
    
    half3 normal = encode(input.TBN[2]);
    //rgba.rgb *= 0.5;
    
    if (opacity >= 0 && opacity <= 1)
        rgba.a *= opacity;
        
    PSO output = (PSO) 0;
    if (rgba.a > 0.01)
    {
        
        output.Lighting.rgb = 0.5 + clamp(sky_color.rgb * 1.4, 0, 1) * (sky_brightness);
        float3 n = normal;
        
        if (fn)
            n = encode(normalize(force_normal));
        
        
        output.Normals = float4(n, 1);
         
        
            output.Lighting.rgb *= 0.4 + clamp(Phong(output.Normals.xyz), 0, 0.6);
        
        output.Lighting.a = 1;
        
        output.Depth.rgba = input.Depth.z;
        //output.Depth.xyz = mul(input.Depth.xyz, IVP);
        
        output.Depth.a = 1;
        
    //homo_pos.z = input.Depth.z;
    
        //homo_pos = mul(homo_pos, IVP);
        //homo_pos /= homo_pos.w;
        
        rgba.rgb = clamp((rgba.rgb * tint.rgb), 0, 1);
    }
    else
    {
        clip(-1);
        output.Normals = 0;
        rgba.rgb = clamp((rgba.rgb * tint.rgb), 0, 1);
    }
    
    //if (output.Depth.z < NearClip || output.Depth.z > FarClip)
        //clip(-1);
    
   // output.Depth.r = input.Depth.r;
    //float3 p = clamp(Phong(normal), 0.5, 1);
    
    
    output.Diffuse = rgba;
    
    
    return output ;
}

// INSTANCED SHADER MAIN PASSES AND SPECIFICS HERE
struct VSII
{
    float4 tex_offset_flip : TEXCOORD1;
    
    float4 r1 : TEXCOORD2;
    float4 r2 : TEXCOORD3;
    float4 r3 : TEXCOORD4;
    float4 r4 : TEXCOORD5;
    
    float4 tint : COLOR0;
    //float3 pos : TEXCOORD5;
    //float3 velocity : TEXCOORD6;
    //float2 tex_offset : TEXCOORD8;
    //float3 rot_opa_flip : TEXCOORD9;
};

float4x4 CreateMatrixFromCols(float4 c0, float4 c1, float4 c2, float4 c3)
{
    return float4x4(c0.x, c1.x, c2.x, c3.x,
		c0.y, c1.y, c2.y, c3.y,
		c0.z, c1.z, c2.z, c3.z,
		c0.w, c1.w, c2.w, c3.w);
}


VSO InstancedVS(in VSI input, in VSII input2)
{
    VSO output = (VSO) 0;
    float4x4 WorldInstance = mul(CreateMatrixFromCols(input2.r1, input2.r2, input2.r3, input2.r4), mul(View, Projection));;
    float4x4 WorldInstanceut = CreateMatrixFromCols(input2.r1, input2.r2, input2.r3, input2.r4);
    //float4 instance_pos = float4(input.Position.xyz + input2.offset, 1.0);
    
    float4x4 wvp = mul(World, mul(View, Projection));
    
    output.Position = mul(input.Position, WorldInstance);
    //output.pos3d = mul(instance_pos, WorldInstance);
    
    output.TexCoord = input.TexCoord + input2.tex_offset_flip.xy;
    if (input2.tex_offset_flip.y > 0)
        output.TexCoord.x = 1 - output.TexCoord.x;
    if (input2.tex_offset_flip.w > 0)
        output.TexCoord.y = 1 - output.TexCoord.y;
    //output.color = tex2D(DiffuseSampler, output.TexCoord);
    
    output.color = input2.tint;
    output.Depth = mul(input.Position, WorldInstance) * 0.008;
    output.Depth.a = 1;
    
    output.TBN[0] = normalize(mul(input.Tangent, (float3x3) WorldInstanceut));
    output.TBN[1] = normalize(mul(input.BiTangent, (float3x3) WorldInstanceut));
    
    output.TBN[2] = normalize(mul(input.Normal, (float3x3) WorldInstanceut));
    return output;
}


PSO InstancedPS(VSO input)
{
    float4 rgba = tex2D(DiffuseSampler, input.TexCoord) * input.color;
    
    
    half3 normal = encode(input.TBN[2]);
    //rgba.rgb *= 0.5;
    
    //if (opacity >= 0 && opacity <= 1)
       // rgba.a *= opacity;
        
    PSO output = (PSO) 0;
    if (rgba.a > 0.5)
    {
      
        output.Lighting.rgb = 0.5 + clamp(sky_color.rgb * 1.4, 0, 1) * (sky_brightness);
        float3 n = normal;
        
        if (fn)
            n = encode(force_normal);     
        
        output.Normals = float4(n, 1);
        
        //output.Lighting.rgb = 0.5;
         
        output.Lighting.rgb *= 0.8 + clamp(Phong(output.Normals.xyz), 0, 0.6);
        
        
        
        output.Lighting.a = 1;
        output.Depth.rgba = input.Depth.rgba;
        output.Depth.xyz = input.Depth.z;
        
        output.Depth.a = 1;
        
        
        rgba.rgb = clamp((rgba.rgb * tint.rgb), 0, 1);
    }
    else
    {
        //clip(-1);
        output.Normals = 0;
        rgba.rgb = clamp((rgba.rgb * tint.rgb), 0, 1);
    }
    
    //if (output.Depth.z < NearClip || output.Depth.z > FarClip)
        //clip(-1);
    
   // output.Depth.r = input.Depth.r;
    //float3 p = clamp(Phong(normal), 0.5, 1);
    
    output.Diffuse = rgba;
    
    
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

technique instanced
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL InstancedVS();
        PixelShader = compile PS_SHADERMODEL InstancedPS();
    }
};