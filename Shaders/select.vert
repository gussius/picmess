#version 130
precision mediump float;

uniform mat4 modelview_matrix; 
uniform mat4 projection_matrix;
in vec3 vertex_position;
 
void main(void)
{
	gl_Position = projection_matrix * modelview_matrix * vec4( vertex_position, 1 );
}