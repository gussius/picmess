#version 130

precision mediump float;

uniform vec4 surfaceColor;

in vec3 lightPositionNormalized;
in vec3 normal;

out vec3 fragColor;

void main(void)
{
	float lightIntensity = 0.5 * dot(lightPositionNormalized, normal) + 1;
	fragColor = vec3(surfaceColor);
}