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

// Colors that we will use
uniform float4 color_1 = float4(0.784313725, 0.788235294, 0.262745098, 1);
uniform float4 color_2 = float4(0.490196078, 0.521568627, 0.152941176, 1);
uniform float4 color_3 = float4(0, 0.415686275, 0, 1);
uniform float4 color_4 = float4(0.015686275, 0.243137255, 0, 1);

// Color offset - changes threshold for color adjustments
uniform float offset = 0.5;

// https://github.com/HarlequinVG/shaders/blob/master/gameboy_shader/shader_files/gb_pass_0.cg
// https://www.youtube.com/watch?v=eANSPwD1SIE&ab_channel=GameDevQuickie
// https://ivanskodje.com/gameboy-shaders/
// https://docs.libretro.com/library/gambatte/
// https://csharpskolan.se/article/pixel-shaders/

// Get pixel color from screen
//float4 pixel_color = float4(texscreen(SCREEN_UV), 1);

// Function to covert a pixel color into grayscale
float4 to_grayscale(float4 pixcol)
{


    float average = (pixcol.r + pixcol.g + pixcol.b) / 3;
    return float4(average, average, average, pixcol.a);
}

// Colorizes the grayscale pixel
float4 colorize(float4 grayscale)
{

	// The color we will return
    float4 new_color;

	// Color greater than 0 in value?
    if (grayscale.r > 0)
	{
		// Set darkest color 4
        new_color = color_4;

		// Color greater than (default) 0.25 in value?
        if (grayscale.r > offset * 0.5)
		{
			// Set dark color 3
            new_color = color_3;

			// Color greater than (default) 0.50 in value?
            if (grayscale.r > offset)
			{
				// Set bright color 2
                new_color = color_2;

				// Color greater than (default) 0.75 in value?
                if (grayscale.r > offset * 1.5)
				{
					// Set brightest color 1
                    new_color = color_1;
                }
            }
        }
    }

	// Return the new color
    return new_color;
}

float4 colorizeTrans(float4 grayscale)
{

	// The color we will return
    float4 new_color;
    float alpha = grayscale.a;

	// Color greater than 0 in value?
    if (grayscale.r > 0)
    {
		// Set darkest color 4
        new_color = color_4;

		// Color greater than (default) 0.25 in value?
        if (grayscale.r > offset * 0.5)
        {
			// Set dark color 3
            new_color = color_3;

			// Color greater than (default) 0.50 in value?
            if (grayscale.r > offset)
            {
				// Set bright color 2
                new_color = color_2;

				// Color greater than (default) 0.75 in value?
                if (grayscale.r > offset * 1.5)
                {
					// Set brightest color 1
                    new_color = color_1;
                }
            }
        }
    }
    new_color.a = alpha;

	// Return the new color
    return new_color;
}

sampler2D SpriteTextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(SpriteTextureSampler, input.TextureCoordinates) * input.Color;
    //float value = (color.r + color.g + color.b)*0.333;
    //color.r = value;
    //color.g = value;
    //color.b = value;
	//if(color.a < 1)
 //   {
 //       color.r = 0;
 //       color.g = 0;
 //       color.b = 0;
 //       color.a = 0;
 //   }

	color = to_grayscale(color);
    if(color.a == 1)
    {
        color = colorize(color);
    }
    else if(color.a == 0.4f)
    {
        float alphaTemp = color.a;
        color.a = 1;
        color = colorize(color);
        color.a = 0.8f;
    }
    return color;
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};