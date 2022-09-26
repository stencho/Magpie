float3 LightDirection = float3(0,-1,0);

float3 LightColor = float3(1,1,1);
float3 AmbientUnlit = float3(0.5,0.5,0.5);

float LightIntensity = 1;

Matrix InverseView;

sampler NORMAL : register(s0) = sampler_state {
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};
sampler DEPTH : register(s1) = sampler_state {
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	
	ADDRESSU = CLAMP;
	ADDRESSV = CLAMP;
};


struct VSI {
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};
struct VSO {
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
	float3 pos : TEXCOORD1;
};

//Pixel Shader Out
struct PSO
{
    float4 Lighting : COLOR0;
};

//Vertex Shader
VSO VS(VSI input)
{
	VSO output = (VSO)0;

	output.Position = input.Position;
	output.pos = input.Position;
	output.TexCoord = input.TexCoord;
		
	return output;
}

half3 encode(half3 n)
{
	n = normalize(n);
	n.xyz = 0.5f * (n.xyz + 1.0f);
	return n;
}
float3 decode(float3 enc) {
	return (2.0f * enc.xyz- 1.0f);
}

float4 decode(float4 enc) {
	return (2.0f * enc.xyzw- 1.0f);
}

bool fog = true;
float fog_start;
float3 camera_pos;
float FarClip;

PSO PS(VSO input)
{
	PSO output = (PSO)0;
	
	float3 Normal = tex2D(NORMAL,input.TexCoord).rgb;
	float4 dNormal = mul(decode(Normal), InverseView);

	float Depth = tex2D(DEPTH,input.TexCoord).r;

	float NL = (dot(-dNormal , normalize(LightDirection)) * 1) + 0.1;	  

	if (Depth == 1)
		clip(-1);
		
	output.Lighting.rgb = ((LightColor * LightIntensity) * 0.5) + (NL * saturate((LightColor * LightIntensity)) / 2) ;

	output.Lighting.a = 1;
	if (fog && Depth > fog_start) {
		//output.Lighting.a = Depth - fog_start;
	} 
	
	if (fog && Depth >= .999) {
		output.Lighting.a = 0;
	}

	return output;
}

technique Default
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}