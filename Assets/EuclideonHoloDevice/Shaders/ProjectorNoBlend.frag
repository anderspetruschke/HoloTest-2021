#version 330 core

uniform sampler2D Sampler;

//Input Format
in vec2 ex_TextureUVs;

//Output Format
out vec4 out_Colour;

void main()
{
  vec2 uv = ex_TextureUVs;
  float blackScreenPercentage = 0.1;
  vec2 middle = vec2(0.5, 0.5);
  vec2 distance = uv - middle;
  distance = (2 * abs(distance));
  float xaxis = smoothstep(0.08, 0.01, 1 - distance.x);
  float yaxis = smoothstep(0.08, 0.01, 1 - distance.y);
  float colourIntensity = 1 - max(xaxis, yaxis);
  vec4 colour = texture(Sampler, uv);
  out_Colour = vec4(colour.xyz * colourIntensity, colour.a);
}
