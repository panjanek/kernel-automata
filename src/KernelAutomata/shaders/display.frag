#version 330 core

in vec2 uv;
uniform sampler2D uStateRed;
uniform sampler2D uStateGreen;
uniform vec2 uZoomCenter;       // [0,1] texture space
uniform float uZoom;            // >1.0 = zoom in
uniform float uAspect;
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
    vec2 pos = vec2((uv.x - 0.5) / (uZoom*uAspect) + uZoomCenter.x, (uv.y - 0.5) / uZoom + uZoomCenter.y);

    float f1 = texture(uStateRed, pos).r;
    float f2 = texture(uStateGreen, pos).r;
    
    float r = amplify(f1, 2);
    float g = amplify(f2, 2);
    float b = amplify((f1+f2)/2, 3);
    
    fragColor = vec4(r, g, b ,1);

    //if (pos.x<0 || pos.x >=1 || pos.y<0 || pos.y>=1)
    //    fragColor *= 0.5;
}