float4x4 World;
float4x4 View;
float4x4 InverseView;
float4x4 Projection;
float4x4 InverseViewProjection;

float3 CameraPosition;
float4x4 LightViewProjection;
float4x4 LightProjection;
float3 LightPosition;
float4 LightColor;
float LightIntensity;
float3 LightDirection;
float LightAngleCos;
float LightClip;
float2 GBufferTextureSize;
bool Shadows;
float shadowMapSize;
float DepthBias;

sampler DEPTH : register(s0) = sampler_state {
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;

	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};

sampler NORMAL : register(s1) = sampler_state {
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};

sampler COOKIE: register(s2)= sampler_state {
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};

sampler SHADOW : register(s3);

struct VSI {
	float4 Position : POSITION0;
};

struct VSO {
	float4 Position : POSITION0;
	float4 ScreenPosition : TEXCOORD0;
};

VSO VS(VSI input) {
	VSO output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.ScreenPosition = output.Position;
	return output;
}

float4 manualSample(sampler Sampler, float2 UV, float2 textureSize) {
	float2 texelpos = textureSize * UV;
	float2 lerps = frac(texelpos);
	float texelSize = 1.0 / textureSize;
	float4 sourcevals[4];

	sourcevals[0] = tex2D(Sampler, UV);
	sourcevals[1] = tex2D(Sampler, UV + float2(texelSize, 0));
	sourcevals[2] = tex2D(Sampler, UV + float2(0, texelSize));
	sourcevals[3] = tex2D(Sampler, UV + float2(texelSize, texelSize));

	float4 interpolated = lerp(lerp(sourcevals[0], sourcevals[1], lerps.x),
	lerp(sourcevals[2], sourcevals[3], lerps.x ), lerps.y);
	return interpolated;
}

float4 Phong(float3 Position, float3 N, float radialAttenuation,float SpecularIntensity, float SpecularPower) {
	float3 L = Position.xyz - LightPosition.xyz;

	float heightAttenuation = 1  - (length(L) / (LightClip));
	float Attenuation = min(radialAttenuation, heightAttenuation);

	L = normalize(L);

	float SL = dot(L, LightDirection);

	float4 Shading = 0;
	if (SL >= LightAngleCos && (distance(Position.xyz, LightPosition.xyz)) <= LightClip) {
		float NL = dot(-N, L);
		float3 Diffuse = NL * LightColor.xyz;
		Shading = float4(Diffuse.rgb, 1) * Attenuation;
	}

	return Shading;
}


float3 decode(float3 enc) {
	return (2.0f * enc.xyz- 1.0f);
}

float RGBADecode(float4 value) {
	const float4 bits = float4(1.0 / (256.0 * 256.0 * 256.0), 1.0 / (256.0 * 256.0), 1.0 / 256.0, 1);
	return dot(value.xyzw , bits);
}
float4 PS(VSO input) : COLOR0 {
	input.ScreenPosition.xy /= input.ScreenPosition.w;

	float2 UV = 0.5f * (float2(input.ScreenPosition.x, -input.ScreenPosition.y) + 1.0f);
	
	float4 encodedNormal = tex2D(NORMAL,UV);
	float3 Normal = mul(decode(encodedNormal.xyz), InverseView);
		
	float Depth = tex2D(DEPTH,UV).r;

	float4 Position = 1.0f;
	Position.xy = input.ScreenPosition.xy;
	Position.z = Depth;
	Position = mul(Position, InverseViewProjection);
	Position /= Position.w;
	
	float3 L = Position.xyz - LightPosition.xyz;
	float Ll = length(L);

	float4 LightScreenPos = mul(Position, LightViewProjection);

	float4 LSPcookie = LightScreenPos / (Ll);
	LightScreenPos /= Ll;
	
	float2 LUV = 0.5 * (float2(LightScreenPos.x, -LightScreenPos.y) + 1);
	float2 LUVcookie =  (0.5 * (float2(LSPcookie.x, -LSPcookie.y) + 1));

	float lZ = tex2D(SHADOW, LUV);

	float Attenuation = tex2D(COOKIE, LUVcookie.xy).r;

	float ShadowFactor = 1;
	if(Shadows) {
		float len = max(0.001f, distance(LightPosition, Position) / LightClip);
		ShadowFactor = (lZ * exp(-(LightClip * 0.5f) * (len - DepthBias))); 
		if (ShadowFactor == 0) 
			ShadowFactor = 1;
	}

	return Phong(Position.xyz, Normal, Attenuation, 1, 1) * saturate(ShadowFactor);
	//return float4(Position.xyz, 1) * s;
}

technique Default {
	pass p0 {
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}