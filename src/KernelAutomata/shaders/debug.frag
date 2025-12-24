#version 330 core

in vec2 uv;
uniform sampler2D uState;
out vec4 fragColor;

void main()
{
    vec4 color = texture(uState, uv);
    //fragColor = vec4(color.r / float(512*128),0,0,1);
    //fragColor = vec4(color.r*1000,0,0,1);
    fragColor = vec4(color.r,0,0,1);
}