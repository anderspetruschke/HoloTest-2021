#version 330

uniform sampler2D diffuseMap0;

in vec2 passTexcoord0;

out vec4 fragColor;

void main()
{
    vec3 color = texture(diffuseMap0, passTexcoord0).rgb;
    fragColor = vec4(color, 1);
}
