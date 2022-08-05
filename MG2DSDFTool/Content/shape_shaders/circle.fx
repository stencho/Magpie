#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};


sampler2D texture_inside : register(s1){	
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

sampler2D texture_border : register(s3) {	
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

sampler2D texture_outer : register(s2){	
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};

float border_size;

float4 inner_color;
float4 border_color;
float4 outer_color;

bool draw_inner;
bool draw_border;
bool draw_outer;

bool draw_distance_field;

float inner_tex_scale;
float total_scale;

static const float PI = 3.14159265f; 

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float2 tx = input.TextureCoordinates * 2 - 1;
	float d = tx.x * tx.x + tx.y * tx.y;
	
	if (draw_distance_field && d > -1) {
		return float4(1-d,1-d,1-d,1-d);
		

	} else if (draw_inner && (d <= total_scale-border_size)) {		
		return tex2D(texture_inside,  (input.TextureCoordinates - (border_size/PI/2)) / (1 - (border_size/PI)));

	}  else if (draw_border && (d < total_scale && d > total_scale-border_size)) {
		return tex2D(texture_border, input.TextureCoordinates);

	} else if (draw_outer && (d > total_scale-border_size)) {
		return tex2D(texture_outer, input.TextureCoordinates);

	}  else {
		return float4(0,0,0,0);
	}

} 

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};
