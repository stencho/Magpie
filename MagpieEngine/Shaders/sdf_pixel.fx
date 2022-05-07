float2 offset;

float2 billboard_size;
float2 draw_offset = float2(0,0);
float2 draw_scale = float2(1,1);

float alpha_scissor = 0.5f;

float3 tint;

sampler2D SDFTEX : register(s0);

//Pixel Shader
float4 color;

float4 PS(float4 position : SV_Position, float4 color : COLOR0, float2 TexCoords : TEXCOORD0) : COLOR0
{
	
	float a = (tex2D(SDFTEX, TexCoords) * color).r;
	//a = smoothstep(1-alpha_scissor,alpha_scissor,a);

	if (a > alpha_scissor*0.5f){
		clip(-1);
	}
	
	return float4(tint.r, tint.g, tint.b, 1);
}//Technique

technique Default
{
	pass p0
	{
		PixelShader = compile ps_3_0 PS();
	}
}