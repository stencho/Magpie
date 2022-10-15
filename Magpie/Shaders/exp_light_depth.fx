float4x4 World;
float4x4 View;
float4x4 Projection;
float3 LightPosition;
float DepthPrecision;

struct VSI {
	float4 Position : POSITION0;
};

struct VSO {
	float4 Position : POSITION0;
	float4 WorldPosition : TEXCOORD0;
	float4 ViewPosition : TEXCOORD1;
};

VSO VS(VSI input) {
	VSO output;
	float4 worldPosition = mul(input.Position, World);
	float4 viewPosition = mul(worldPosition, View);
	output.Position = mul(viewPosition, Projection);
	output.WorldPosition = worldPosition;
	output.ViewPosition = output.Position;
	return output;
}
float distSquared( float3 A, float3 B )
{

    float3 C = A - B;
    return dot( C, C );

}

float4 PS(VSO input) : COLOR0 {
	float C = 0.00001;
	input.WorldPosition.xy /= input.WorldPosition.w;
	float depth = length(input.WorldPosition.xyz - LightPosition.xyz) / (DepthPrecision);
	//float depth = (input.ViewPosition.z/ DepthPrecision);
	//return (log(C * depth + 1) / log(C * DepthPrecision + 1) * depth);
	return log(depth+1);
}

technique Default {
	pass p0 {
		VertexShader = compile vs_3_0 VS();
		PixelShader = compile ps_3_0 PS();
	}
}