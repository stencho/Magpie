#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D background_texture;
Texture2D display_texture;
Texture2D gui_texture;

sampler2D background_sampler = sampler_state { Texture = <background_texture>; };
sampler2D display_sampler = sampler_state { Texture = <display_texture>; };
sampler2D gui_sampler = sampler_state { Texture = <gui_texture>; };

struct VertexShaderOutput
{
	float4 position : SV_POSITION;
	float2 tex_coords : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return tex2D(background_sampler, input.tex_coords);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};