float4 ColorModifier;

struct PSInput
{
    float4 Color : COLOR0;
	float2 Coord : TEXCOORD0;
};

float4 ps_main(PSInput Input) : COLOR0
{
	float4 final;
	
	final.r = Input.Color.r * ColorModifier.x;
	final.g = Input.Color.g * ColorModifier.y;
	final.b = Input.Color.b * ColorModifier.z;
	final.a = Input.Color.a * ColorModifier.w;

   return (final);
}

technique SetMeshColor
{
    pass p0
    {
        VertexShader 		= null;
        PixelShader  		= compile ps_2_0 ps_main();
	}
}