float4x4 World;
float4x4 View;
float4x4 Projection;
float3 LightPosition;
//This is for modulating the Light's Depth Precision
float DepthPrecision;//Input Structure
float4 instanceTransform;

struct VSI
{
	float4 Position : POSITION0;
};
//Output Structure
struct VSO
{
	float4 Position : POSITION;
	float4 WorldPosition : TEXCOORD0;
	float depth : TEXCOORD1;
};

//Vertex Shader
VSO VS(VSI input)
{
	//Initialize Output
	VSO output;
	//Transform Position
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	//Pass World Position
	output.WorldPosition = worldPosition;
	output.depth = output.WorldPosition.z /= output.WorldPosition.w;
	//Return Output
	return output;
}
//Pixel Shader
float4 PS(VSO input) : COLOR0
{

	float depth = (length(LightPosition - input.depth)/DepthPrecision);

	//Return Exponential of Depth
	return float4(depth, depth, depth, 1);
}
//Technique
technique Default
{
	pass p0
	{
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}