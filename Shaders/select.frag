#version 130
precision mediump float;

uniform vec4 surfaceColor;
out vec3 fragColor;

void main(void)
{
	fragColor = vec3(surfaceColor);
}