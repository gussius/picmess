#version 130
precision mediump float;

uniform sampler2D hud;
in vec2 textureCoord;
out vec4 fragColor;

void main()
{
   vec4 color1 = texture2D(hud,textureCoord);
   fragColor = vec4 ( color1 );
}