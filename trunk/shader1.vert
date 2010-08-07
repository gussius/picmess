#version 140

uniform mat4 modelview_matrix; 
uniform mat4 projection_matrix;

in vec3 vertex_position;
in vec3 vertex_normal;
in vec3 lightPosition;

out vec3 normal;
out vec3 lightPositionNormalized;
 
void main(void)
{
	lightPositionNormalized = normalize(lightPosition);
	normal = ( modelview_matrix * vec4( vertex_normal, 0 ) ).xyz;
	gl_Position = projection_matrix * modelview_matrix * vec4( vertex_position, 1 );
}