#version 330 core

in vec2 uv;
uniform sampler2D uState;
out vec4 fragColor;

void main()
{
    vec4 color = texture(uState, uv);
    //fragColor = color;
    fragColor = vec4(color.r*1000,0,0,1);
}