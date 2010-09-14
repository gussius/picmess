#version 130
precision mediump float;

uniform sampler2D background;
uniform sampler2D foreground;
in vec2 textureCoord;
out vec4 fragColor;

void main()
{
   vec4 bgColor = texture2D(background, textureCoord);
   vec4 fgColor = texture2D(foreground, textureCoord);
   fragColor = mix( bgColor, fgColor, fgColor.a );
}