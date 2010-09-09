#version 130
precision mediump float;

const vec2 madd=vec2(0.5,-0.5);
in vec3 vertex_position;
out vec2 textureCoord;


void main()
{
   textureCoord = vertex_position.xy*madd+madd; // scale vertex attribute to [0-1] range
   gl_Position = vec4( vertex_position.xy, -1.0, 1.0 );
}