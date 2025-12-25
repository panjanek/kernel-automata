#version 330 core

in vec2 uv;
uniform sampler2D uStateRed;
uniform sampler2D uStateGreen;
out vec4 fragColor;

float amplify(float x, int pow)
{
    float a = 1;
    for(int i=0; i<pow; i++)
        a = a * (1-x);

    return 1-a;
}

void main()
{
    float f1 = texture(uStateRed, uv).r;
    float f2 = texture(uStateGreen, uv).r;
    
    float r = amplify(f1, 2);
    float g = amplify(f2, 2);
    float b = amplify((f1+f2)/2, 3);
    
    fragColor = vec4(r, g, b ,1);
}