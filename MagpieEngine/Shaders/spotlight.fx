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
matrix IVP;
matrix LVP;

float3 LightPosition;
float3 LightDirection;
float LightAngle;
float LightClip;

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

//float4 Phong(float3 Position, float3 N, float radialAttenuation,float SpecularIntensity, float SpecularPower) {
	//Calculate Light vector
	//float3 L = LightPosition.xyz - Position.xyz;

	//Calculate height Attenuation
	//float heightAttenuation = 1.0f - saturate(length(L) - (LightHeight / 2));

	//Calculate total Attenuation
	//float Attenuation = min(radialAttenuation, heightAttenuation);

	//Now Normalize the Light
	//L = normalize(L);

	//Calculate L.S
	//float SL = dot(L, S);

	//No asymmetrical returns in HLSL, so work around with this
	//float4 Shading = 0;

	//If this pixel is in the SpotLights Cone
	//if(SL <= LightAngleCos) {
		//Calculate Reflection Vector
	//	float3 R = normalize(reflect(-L, N));

		//Calculate Eye Vector
	//	float3 E = normalize(CameraPosition - Position.xyz);

		//Calculate N.L
	//	float NL = dot(N, L);

		//Calculate Diffuse
	//	float3 Diffuse = NL * LightColor.xyz;

		//Calculate Specular
		//float Specular = SpecularIntensity * pow(saturate(dot(R, E)), SpecularPower);

		//Calculate Final Product
	//	Shading = Attenuation * LightIntensity * float4(Diffuse.rgb, 1);
	//}
	//Return Shading Value
	//return Shading;
//}

PSO MainPS(VertexShaderOutput input) : COLOR
{
	input.ScreenPosition.xy /= input.ScreenPosition.w;

	float d = tex2D(DEPTH,input.ScreenPosition.xy);
	PSO output = (PSO)1;

	float4 p = 1.0f;
	p.xy = input.ScreenPosition.xy;
	p.z = d;

	p = mul(p, IVP);
	p/=p.w;

	float3 L = LightPosition.xyz - p.xyz;

	L = normalize(L);
	float SL = dot(L,LightDirection);

	float shading = 0;

	if (SL <= LightAngle) {
		shading = 1;
	} else {
		shading = 0;
	}

	output.Diffuse = float4(0,0,0,0);
	output.Normals = float4(0,0,0,0);
	output.Depth = float4(d,d,d,1);
	output.Lighting = float4(shading,shading,shading,1);
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