#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
Texture2D gridTexture;

uniform float brightness = 1.13;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

sampler2D GridTextureSampler = sampler_state
{
    Texture = <gridTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 gridColor = tex2D(GridTextureSampler, input.TextureCoordinates) * input.Color;
    float4 pixel_color = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;
    //float4 color = float4(1 - pixel_color.r, 1 - pixel_color.g, 1 - pixel_color.b, pixel_color.a);
    //if (gridColor.a < 0.5)
    //{
    //    color = float4(0, 1, 1, 0.5f); // (red, green, blue, alpha-transparency)
    //}
    //else
    //{ // Since it is NOT dark, we adjust the pixel color brightness
    //    color = pixel_color * brightness;
    //}
    //color = gridColor;

    return gridColor;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};