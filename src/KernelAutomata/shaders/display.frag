#version 330 core

in vec2 uv;
uniform sampler2D uStateRed;
uniform sampler2D uStateGreen;
out vec4 fragColor;

void main()
{
    vec4 red = texture(uStateRed, uv);
    vec4 green = texture(uStateGreen, uv);
    fragColor = vec4(red.r,green.r,0,1);
}