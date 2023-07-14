#version 330

uniform mat4 MVP;
in vec3 position0;
in vec2 texcoord0;
out vec2 passTexcoord0;

void main()
{
    passTexcoord0 = texcoord0;
    gl_Position = MVP * vec4(position0, 1.0);
}
