shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture;
uniform float ambient = 0.1;
uniform float exponent = 3.0;
uniform bool left_top_lit = false;
uniform bool left_bottom_lit = false;
uniform bool right_top_lit = false;
uniform bool right_bottom_lit = false;
uniform bool player_lit = true;
uniform vec2 player_pos = vec2(0.5);
uniform float player_sight_radius = 0.3;
uniform float light_effect_radius = 0.3;

const vec2 left_top_pos = vec2(192.0 / 1024.0);
const vec2 left_bottom_pos = vec2(192.0 / 1024.0, 1.0 - 192.0 / 1024.0);
const vec2 right_top_pos = 1.0 - left_bottom_pos;
const vec2 right_bottom_pos = 1.0 - left_top_pos;

void fragment() {
		float mask = ambient + (player_lit ? pow(max(1.0 - length(SCREEN_UV.xy - player_pos) / player_sight_radius, 0.0), exponent) : 0.0);
		mask += left_top_lit ? pow(max(1.0 - length(SCREEN_UV.xy - left_top_pos) / light_effect_radius, 0.0), exponent) : 0.0;
		mask += left_bottom_lit ? pow(max(1.0 - length(SCREEN_UV.xy - left_bottom_pos) / light_effect_radius, 0.0), exponent) : 0.0;
		mask += right_top_lit ? pow(max(1.0 - length(SCREEN_UV.xy - right_top_pos) / light_effect_radius, 0.0), exponent) : 0.0;
		mask += right_bottom_lit ? pow(max(1.0 - length(SCREEN_UV.xy - right_bottom_pos) / light_effect_radius, 0.0), exponent) : 0.0;
		mask = min(0.8, mask);
        COLOR = vec4(texture(screen_texture, SCREEN_UV).rgb * mask, 1.0);
}