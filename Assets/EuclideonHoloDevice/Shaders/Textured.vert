#version 330 core

//Input Format
layout(location = 0) in vec3 position0;
layout(location = 1) in vec2 texcoord0;

//Ouput Format
out vec2 ex_TextureUVs;

uniform mat4 MVP;

void main()
{
  gl_Position = MVP * vec4(position0, 1);
  ex_TextureUVs = texcoord0;
}
