//------------------------------ TEXTURE PROPERTIES ----------------------------
// This is the texture that SpriteBatch will try to set before drawing
texture ScreenTexture;

//This is the texture used to control which areas are lit / darkened.
texture LightTexture;

// Our sampler for the texture, which is just going to be pretty simple
sampler TextureSampler = sampler_state
{
	Texture = <ScreenTexture>;
};

sampler TextureSampler2 = sampler_state
{
	Texture = <LightTexture>;
};

//------------------------ PIXEL SHADER ----------------------------------------
// This pixel shader will simply look up the color of the texture at the
// requested point, and turns it into a sepia tone
float4 PixelShaderFunction(float2 TextureCoordinate : TEXCOORD0) : COLOR0
{
	float4 color = tex2D(TextureSampler, TextureCoordinate);
	float4 color2 = tex2D(TextureSampler2, TextureCoordinate);

	float4 outputColor = color * color2;
	/*outputColor.r = (color.r * 0.393) + (color.g * 0.769) + (color.b * 0.189);
	outputColor.g = (color.r * 0.349) + (color.g * 0.686) + (color.b * 0.168);
	outputColor.b = (color.r * 0.272) + (color.g * 0.534) + (color.b * 0.131);
*/
	
	return outputColor;
}

//-------------------------- TECHNIQUES ----------------------------------------
// This technique is pretty simple - only one pass, and only a pixel shader
technique Texture
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
}