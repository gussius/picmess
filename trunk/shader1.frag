#version 140

in vec3 lightPositionNormalized;
in vec3 normal;
out vec3 fragColor;

void main(void)
{
	float lightIntensity = 0.5 * dot(lightPositionNormalized, normal) + 1;
	fragColor = lightIntensity * vec3( 0.8, 0.5, 0.0 );
}