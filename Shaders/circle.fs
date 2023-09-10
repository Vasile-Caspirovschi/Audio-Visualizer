#version 330

in vec2 fragTexCoord;
in vec4 fragColor;
in vec3 vertexNormal;
in vec3 vertexTangent;

uniform mat4 matView;
uniform mat4 matProjection;
uniform mat4 matModel;
uniform mat3 matNormal;
uniform vec4 colDiffuse;
uniform sampler2D texture0;

out vec4 finalColor;

void main()
{
  float r = 0.1;
  vec2 p = fragTexCoord - vec2(0.5);
  finalColor = fragColor;
  if (length(p) <= 0.5){
    float s = length(p) - r;
    if (s <= 0){
      finalColor = fragColor * 1.5;
    }else{
      float t = 1-  s / (0.5 - r);
      finalColor = vec4(fragColor.xyz, t*t*t);
    }
  }
  else{
   finalColor = vec4(0);
  }
}
