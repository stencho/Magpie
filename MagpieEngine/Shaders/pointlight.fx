
float4x4 World;
float4x4 View;
float4x4 inverseView;
float4x4 Projection;
float4x4 InverseViewProjection;
float3 CameraPosition;
float3 LightPosition;
float LightRadius;
float4 LightColor;
float LightIntensity;
float2 GBufferTextureSize;
bool Shadows;
float DepthPrecision;
float DepthBias;
float shadowMapSize;
bool FullBright;

sampler GBuffer1 = sampler_state {
	texture = <GBuffer_Normal>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
sampler GBuffer2 = sampler_state {
	texture = <GBuffer_Depth>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
sampler ShadowMap = sampler_state {
	texture = <ShadowMapTx>;
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

struct VSI
{
	float4 Position : POSITION0;
};

struct VSO
{
	float4 Position : POSITION0;
	float4 ScreenPosition : TEXCOORD0;
};

technique Default
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}

float3 decode(float3 enc)
{
	return (2.0f * enc.xyz - 1.0f);
}

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

float4 manualSampleCUBE(sampler Sampler, float3 UVW, float3 textureSize)
{
	float3 textureSizeDiv = 1 / textureSize;

	float3 texPos = UVW * textureSize;
	float3 texPos0 = floor(texPos + 0.5f);
	float3 texPos1 = texPos0 + 1.0f;

	texPos0 = texPos0 * textureSizeDiv;
	texPos1 = texPos1 * textureSizeDiv;

	float3 blend = frac(texPos + 0.5f);

	float3 texPos000 = texPos0;
	float3 texPos001 = float3(texPos0.x, texPos0.y, texPos1.z);
	float3 texPos010 = float3(texPos0.x, texPos1.y, texPos0.z);
	float3 texPos011 = float3(texPos0.x, texPos1.y, texPos1.z);
	float3 texPos100 = float3(texPos1.x, texPos0.y, texPos0.z);
	float3 texPos101 = float3(texPos1.x, texPos0.y, texPos1.z);
	float3 texPos110 = float3(texPos1.x, texPos1.y, texPos0.z);
	float3 texPos111 = texPos1;

	float3 C000 = texCUBE(Sampler, texPos000);
	float3 C001 = texCUBE(Sampler, texPos001);
	float3 C010 = texCUBE(Sampler, texPos010);
	float3 C011 = texCUBE(Sampler, texPos011);
	float3 C100 = texCUBE(Sampler, texPos100);
	float3 C101 = texCUBE(Sampler, texPos101);
	float3 C110 = texCUBE(Sampler, texPos110);
	float3 C111 = texCUBE(Sampler, texPos111);

	float3 C = lerp(lerp(lerp(C000, C010, blend.y), lerp(C100, C110, blend.y), blend.x),
		lerp(lerp(C001, C011, blend.y), lerp(C101, C111, blend.y), blend.x),
		blend.z);

	return float4(C, 1);
}
float Epsilon = 1e-10;

float4 createLightmap(float3 Position, float3 N)
{
	//Calculate Light Vector
	float3 L = LightPosition.xyz - Position.xyz;
	//Calculate Linear Attenuation
	//float Attenuation = saturate(min(1, length(L)) / (LightRadius / 2));
	float Attenuation = saturate(1.0f - max(1, length(L)) / (LightRadius / 2));
	
	//Normalize Light Vector
	L = normalize(L);
	//Calculate Reflection vector
	float3 R = normalize(reflect(-L, N));
	//Calculate Eye vector
	float3 E = normalize(CameraPosition - Position.xyz);
	//Calculate N.L
	float NL = dot(N, L);
	//Calculate Diffuse
	float3 Diffuse = NL * LightColor.xyz;
	//Calculate Specular
	//Get Light-Z from Manually Sampled ShadowMap
	float lZ = manualSampleCUBE(ShadowMap, float3(-L.xy, L.z), shadowMapSize).r;
	//Assymetric Workaround...
	float ShadowFactor = 1;
	float sf = 1;
	//If Shadowing is on then get the Shadow Factor
	if (Shadows)
	{
		// Calculate distance to the light
		float len = min(0.01, length(LightPosition - Position)) / DepthPrecision;
		//len *= NL;
		//Calculate the Shadow Factor
		
			ShadowFactor = (lZ * exp(-(DepthPrecision * 0.46f) * ((len) - DepthBias)));
		
	}	

	float output = ShadowFactor * Attenuation * LightIntensity;

	if (output < 0.2)
		output = 0;
	else if (output > 0.2 && output < 0.4)
		output = .25;
	else if (output > 0.4 && output < 0.6)
		output = .5;
	else if (output > 0.6 && output < 0.8)
		output = .75;
	else if (output > 0.8)
		output = 1;

	if (FullBright) {
		ShadowFactor = 1;
		Attenuation = 1;
	}


	//Calculate Final Product
	return output * float4(Diffuse.rgb, 1);
}

VSO VS(VSI input)
{
	VSO output;

	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);

	output.Position = mul(viewPosition, Projection);
	output.ScreenPosition = output.Position;


	return output;
}

float4 PS(VSO input) : COLOR0
{
	input.ScreenPosition.xy /= input.ScreenPosition.w;

	float2 UV = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1) -
	float2(1.0f / GBufferTextureSize.xy);

	half4 encodedNormal = tex2D(GBuffer1, UV);
	half3 Normal = mul(decode(encodedNormal.xyz), inverseView);


	float Depth = manualSample(GBuffer2, UV, GBufferTextureSize).x;

	float4 Position = 1.0f;
	Position.xy = input.ScreenPosition.xy;
	Position.z = Depth;

	Position = mul(Position, InverseViewProjection);
	Position /= Position.w;
	
	return createLightmap(Position.xyz, Normal);
}