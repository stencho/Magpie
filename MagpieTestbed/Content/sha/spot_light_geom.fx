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

matrix IV;
matrix IVP;

float farclip;
float nearclip;

float3 cam_pos;
float3 light_pos;

//float3 CameraDirection;
float3 light_dir;
float light_angle_cos;
float light_height_question_mark; //is this actually light distance and this guy is stupid????????

float4 light_color;
float light_intensity;

float2 depth_buffer_size;
float2 light_depthmap_size;
matrix light_viewproj;

bool shadows = false;

//do some work to remove shadow maps entirely after this
//it should be pretty easy to make the application of shadows more direct, based on 
//the light depthmaps, just before the compositor pass, or even in this pass

//USE GBUFFER.BUFFER_TARGETS_DLN
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
    float4 ScreenPosition : TEXCOORD0;
    float4 Depth : TEXCOORD1;
};
struct PSO
{
    float4 Diffuse : COLOR0;
    float4 Normals : COLOR1;
    float4 Depth : COLOR2;
    float4 Lighting : COLOR3;
};


texture light_depthmap;
sampler light_depth = sampler_state
{
    texture = <light_depthmap>;
    MINFILTER = POINT;
    MAGFILTER = POINT;
    MIPFILTER = POINT;
    ADDRESSU = MIRROR;
    ADDRESSV = MIRROR;
};

float3 decode(float4 enc)
{
    return (2.0f * enc.xyz - 1.0f);
}

float4 msample(sampler Sampler, float2 UV, float2 textureSize)
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


float logzbuf(float4 xyzw)
{
    return max(1e-6, log(nearclip * xyzw.z + 1) / log(nearclip * farclip+ 1) * xyzw.w);
}


VSO MainVS(VSI input)
{
    VSO output = (VSO) 0;
    
    float4x4 wvp = mul(World, mul(View, Projection));
    
    output.Depth = ((output.Position.z - nearclip) / 15);
    output.Depth.a = 1;
    output.Position = mul(input.Position, wvp);
    output.ScreenPosition = output.Position;
    //output.Position.z = output.Depth;
    
    //output.Position = mul(output.Position, InverseViewProjection);
   // output.Position /= output.Depth.z;
    return output;
}

float depth_bias;
PSO MainPS(VSO input)
{
    PSO output = (PSO) 0;
    input.ScreenPosition.xy /= input.ScreenPosition.z;
    
    float2 UV = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1) - float2(1.0f / depth_buffer_size.xy);
    
    output.Normals = msample(NORMAL, UV, depth_buffer_size);
    output.Diffuse = tex2D(DIFFUSE, UV);
    
    float Depth = msample(DEPTH, UV, depth_buffer_size).z;
    
    output.Depth = input.Depth;
    //output.Depth = mul(output.Depth, IVP);
    //output.Depth /= output.Depth.w;
    float4 homo_pos = 1;    
    homo_pos.xyz = input.ScreenPosition.xyz;
    homo_pos.z = Depth;
    
    homo_pos = mul(homo_pos, IVP);
    homo_pos /= homo_pos.w;
    
    //if (distance(0, input.ScreenPosition) >= distance(0, output.Depth.z))
      //  clip(-1);
    
    float4 light_screen_pos = mul(homo_pos, light_viewproj);
    light_screen_pos /= light_screen_pos.w;
    
    float2 luv = 0.5 * (float2(light_screen_pos.x, -light_screen_pos.y) + 1);
    float light_z = msample(light_depth, luv, light_depthmap_size);
    float attn = 1;
    float shadow = 1;
    float cookie = 1;
    
    //output.Depth.rgb = Depth;
    float s_len = max(0.01, length(light_pos - homo_pos.xyz)) / farclip;
    shadow = (light_z * exp(-(farclip * 0.5) * (s_len - depth_bias)));
    
    
    //output.Depth.rgb = homo_pos.xyz;
    //let me see that phooooong
    float3 pixel_dir = light_pos - homo_pos.xyz; //aka L
    
    
    float d_attn = 1.0f - saturate(length(pixel_dir) - (farclip / 2));
    attn = min(d_attn, attn);    
    pixel_dir = normalize(pixel_dir);
        
    float SL = dot(pixel_dir, normalize(light_dir));
    
    output.Lighting = 1;
    
    //float3 refl = normalize(reflect(-pixel_dir, output.Normals.xyz));
    //float3 view = normalize(cam_pos - homo_pos.xyz);
    
    float NL = dot(mul(decode(output.Normals), IV), light_dir);
        
    
    if (SL <= light_angle_cos)
    {
        output.Lighting = shadow * (attn * light_intensity * (NL * light_color));
        
    }
    //else
        //clip(-1);
    
    //else
        //output.Lighting = light_intensity * light_color;
    //Calculate Final Product
    //bool z = (input.Depth.z > (output.Depth.z));
    //if (!z)
    //else
        //output.Lighting = 0;    
    

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