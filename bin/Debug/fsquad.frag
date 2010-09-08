#version 130
precision mediump float;

in vec2 textureCoord;
out vec4 color;

void main()
{
   vec4 color1 = texture2D(t,textureCoord);
   color = color1;
}