#version 330 core

uniform sampler2D Samplers[4];
uniform sampler2D MapSampler;
uniform sampler2D SideSampler;
uniform sampler2D BlendSampler;
uniform vec2 projectorSize;
uniform bool calibration;
uniform vec4 uvSourceRange; // [uvStart.xy, uvEnd.xy]
uniform vec4 surfaceBlends;
uniform float doSurfaceBlending;

//Input Format
in vec2 ex_TextureUVs;

//Output Format
out vec4 out_Colour;

void main()
{
  vec2 uv = ex_TextureUVs;

  float sidef = textureLod(SideSampler, uv, 0).r * 255.0;
  vec3 colour = vec3(0);
  float intensity = 1.0f;
  vec2 mapRead;
  uint side;

  ivec2 uvi = ivec2(int(uv.x * projectorSize.x),int(uv.y * projectorSize.y));
  side = uint(texelFetch(SideSampler, uvi, 0).r * 255.0);

  if (side == sidef) // same texture so can do bilinear
  {
    mapRead = textureLod(MapSampler, uv, 0).rg;
    intensity = textureLod(BlendSampler, uv, 0).r;
  }
  else
  {
    mapRead = texelFetch(MapSampler, uvi, 0).rg;
    intensity = texelFetch(BlendSampler, uvi, 0).r;
  }

  if (calibration)
    mapRead = uv;
  else
    mapRead = uvSourceRange.xy + (uvSourceRange.zw - uvSourceRange.xy) * mapRead; // remap UV

  if (mapRead.x < 0 || mapRead.y < 0 || mapRead.x > 1 || mapRead.y > 1)
  {
    out_Colour = vec4(0, 0, 0, 1);
    return;
  }

  // tried using an array here but the ati code downstairs didnt like it
  if (side == 0U) colour = textureLod(Samplers[0], mapRead, 0).rgb;
  if (side == 1U) colour = textureLod(Samplers[1], mapRead, 0).rgb;
  if (side == 2U) colour = textureLod(Samplers[2], mapRead, 0).rgb;
  if (side == 3U) colour = textureLod(Samplers[3], mapRead, 0).rgb;

  // table between-projector blending
  if (doSurfaceBlending == 1)
  {
    intensity = 1.0;
    if (uv.y > surfaceBlends.z) // bottom
      intensity = 1.0 - ((uv.y - surfaceBlends.z) / (1.0 - surfaceBlends.z));
    else if (uv.y < surfaceBlends.x) // top
      intensity = 1.0 - ((surfaceBlends.x - uv.y) / surfaceBlends.x);
    else if (uv.x > surfaceBlends.y) // right
      intensity = 1.0 - ((uv.x - surfaceBlends.y) / (1.0 - surfaceBlends.y));
    else if (uv.x < surfaceBlends.w) // left
      intensity = 1.0 - ((surfaceBlends.w - uv.x) / surfaceBlends.w);
  }
  
  out_Colour = vec4(colour * intensity, 1);
}
