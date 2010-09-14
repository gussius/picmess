#version 130
precision mediump float;

const vec2 correction = vec2( 0.5,-0.5 );
uniform int startTime, currentTime, retracted;
in vec3 vertex_position;
out vec2 textureCoord;

void main()
{
   float offset = 0;
   int t = currentTime - startTime;
   if ( t < 1100 )
	{
	   if ( retracted == 1 )
		offset = 0.03 * sqrt(t);
	   else
		offset = 0.03 * sqrt(1100) - 0.03 * sqrt(t);
	}
   else
	{
	   if ( retracted == 1 )
		offset = 0.6;
	   else
		offset = 0;
	}
   float x = vertex_position.x;
   float y = vertex_position.y + offset;
   textureCoord = vertex_position.xy * correction + correction;
   gl_Position = vec4( x, y, -1.0, 1.0 );
}