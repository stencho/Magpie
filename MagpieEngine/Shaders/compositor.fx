#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


sampler2D DiffuseSampler = sampler_state
{
	texture = <DiffuseLayer>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};
sampler2D LightSampler = sampler_state
{
	texture = <LightLayer>;
	MINFILTER = ANISOTROPIC;
	MAGFILTER = ANISOTROPIC;
	MIPFILTER = ANISOTROPIC;
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};
sampler2D DepthSampler = sampler_state
{
    texture = <DepthLayer>;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};
sampler2D NormalSampler = sampler_state
{
    texture = <NormalLayer>;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
};

struct VSI
{
	float3 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

struct VSO
{
	float4 Position : POSITION0;
	float2 UV : TEXCOORD0;
};

bool draw_2d = true;
float2 offset = float2(0, 0);

VSO MainVS(VSI input)
{
	VSO output = (VSO)0;
    output.Position = float4(input.Position + float3(offset, 0), 1);
	output.UV = input.UV;
	return output;
}

bool fog = false;

int buffer = 1;
float sky_brightness;
float3 atmosphere_color;
float eyelid = 0;

bool fullbright = false;

float3 color_lerp(float3 a, float3 b, float position)
{
    return float3(
                a.r - ((a.r - b.r) * position),
                a.g - ((a.g - b.g) * position),
                a.b - ((a.b - b.b) * position));
}

float cubicPulse( float c, float w, float x )
{
    x = abs(x - c);
    if( x>w ) return 0.0;
    x /= w;
    return 1.0 - x*x*(3.0-2.0*x);
}

float4 MainPS(VSO input) : COLOR
{
    float4 rgba = tex2D(DiffuseSampler, input.UV);
    float4 l = tex2D(LightSampler, input.UV);
    float4 n = tex2D(NormalSampler, input.UV);
    float4 d = tex2D(DepthSampler, input.UV);
	d.rgb = (1-d.rgb);

    float x = 2-(cubicPulse(0.5,2,input.UV.x));
	if (x <0.5) x = 0;
    float4 rgba_final = rgba;
    //if (d.z >=0.95) 
        //clip(-1);

    if (fog)
    {
		//
		rgba_final *= l;
		
        rgba_final = float4(
			clamp(
				color_lerp(
						rgba.rgb,
						(atmosphere_color * sky_brightness), 
						d.z),
			0, 1), 
		
		(clamp((1.8+ (d.z))*2.4, 0, 1)) * x );

    }
    
    if (eyelid > 0)
    {
       // if (input.UV.g > eyelid)
            //rgba.xyz = 0;
    }
    
    if (buffer == 0 || (fullbright && buffer == -1))
        return rgba.rgba;
    else if (buffer == -1)        
        return rgba_final;
    else if (buffer == 1)
        return n; //normals
    else if (buffer == 2)
        return d; //depth
    else if (buffer == 3)
        return l; //lighting
    else if (buffer == 4)
        return rgba_final *= l * 1.3; //final
    else
        return rgba_final *= l;
    
}

technique draw
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};