#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

out vec4 finalColor;

void main()
{
  float r = 0.5 ;
  vec2 p = fragTexCoord - vec2(0.5);
  finalColor = fragColor;
  if (length(p) <= 0.5){
    float s = length(p) - r;
    if (s <= 0){
      finalColor = fragColor * 1.5;
    }else{
      float t = 1- s / (0.5 - r);
      finalColor = vec4(fragColor.xyz, t);
    }
  }
  else{
   finalColor = vec4(0);
  }
}
