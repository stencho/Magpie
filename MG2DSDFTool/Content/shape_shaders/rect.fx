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


sampler2D inner_texture : register(s1){	
	MINFILTER = LINEAR;
	MAGFILTER = LINEAR;
	MIPFILTER = LINEAR;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
float2 inner_texture_resolution;
float inner_texture_aspect_ratio;

sampler2D border_texture : register(s2) {	
	MINFILTER = POINT;
	MAGFILTER = POINT;
	MIPFILTER = POINT;
	ADDRESSU = WRAP;
	ADDRESSV = WRAP;
};
float border_texture_aspect_ratio;

int inner_repeats;
int border_repeats;

float2 inner_offset;
float2 border_offset;

float border_size;

float2 total_size;

float4 inner_color;
float4 border_color;

bool inner_draw;
bool border_draw;

bool distance_field_draw;



static const float PI = 3.14159265f; 
float d_sq (float2 A, float2 B) {
	float2 C = A-B;
	return dot(C,C);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{	
	float d;
	float2 tx = input.TextureCoordinates * 2 - 1;

	float aspect_ratio;
	float aspect_ratio_inv;

	float2 inner_aspect;
	float2 border_aspect;

	float x_b;
	float y_b;

	bool x_greater = false;


	if (total_size.x >= total_size.y) {
		x_greater = true;

		aspect_ratio = total_size.x / total_size.y;
		aspect_ratio_inv = total_size.y / total_size.x;

		border_aspect = float2(aspect_ratio, border_texture_aspect_ratio);
		
		x_b = aspect_ratio;
		y_b = 1;
	} else {
		x_greater = false;
		aspect_ratio = total_size.x / total_size.y;
		aspect_ratio_inv = total_size.x / total_size.y;

		border_aspect = float2(border_texture_aspect_ratio, aspect_ratio);

		x_b = 1;
		y_b = aspect_ratio;	
	}

	
	if (inner_texture_resolution.x >= inner_texture_resolution.y) {	
		float ar = inner_texture_resolution.x / inner_texture_resolution.y;
		inner_aspect = float2(ar, 1) * aspect_ratio;
	} else {
		float ar = inner_texture_resolution.x / inner_texture_resolution.y;
		inner_aspect = float2(1, ar) * aspect_ratio ;	
	}

	//draw border texture or colour	
	if (border_draw 
		&& ((x_greater && (tx.x <= -1+(border_size)  || tx.x > 1-(border_size)  
		||  tx.y > 1-(border_size * aspect_ratio) || tx.y <= -1+(border_size * aspect_ratio)))

		|| (!x_greater && (tx.x <= -1+(border_size * aspect_ratio)  || tx.x > 1-(border_size * aspect_ratio)  
		||  tx.y > 1-(border_size) || tx.y <= -1+(border_size))))
		
	){
		return tex2D(border_texture, 
		(input.TextureCoordinates * border_aspect)
		* border_repeats
		+ border_offset
		) * border_color;
		
	//draw main, inside texture or color
	} else if (inner_draw && (tx.x <= 1)) {

		return tex2D(inner_texture, 
		((input.TextureCoordinates * inner_aspect) - (border_size/2)) / (1 - (border_size)) 
		 * inner_repeats 
		 + inner_offset
		) * inner_color;
			
	//in distance field debug mode, display distance field
	} else if (distance_field_draw) { 
		// this is wrong and should be a point of minimum norm sqrt distance meme but whatever it's just for testing
		float d = 1-distance(tx, float2(0,0));
		return float4(d,d,d,d);	

	//return nothing otherwise
	} else {
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
