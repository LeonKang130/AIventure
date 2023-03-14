shader_type canvas_item;
const float r_max = 0.6;
const float r_mid = 0.4;
const float r_min = 0.35;
uniform vec3 color;

void fragment() {
	float attenuation = 6.0 + 4.0 * cos(TIME * 2.0);
	float radius = length(UV - 0.5);
	COLOR = radius < r_min ? vec4(color, pow(radius / r_min, attenuation + 2.0)) :
		(radius > r_max ? vec4(0.0) :
		(radius > r_mid ? vec4(color, pow((r_max - radius) / (r_max - r_mid), attenuation)) :
		vec4(color, 1.0)));
}
