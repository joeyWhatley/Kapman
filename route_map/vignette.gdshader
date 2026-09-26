shader_type canvas_item;
// Soft darkening toward the screen edges, like an old chart under lamplight.

uniform float strength = 0.16;
uniform vec4 tint : source_color = vec4(0.18, 0.16, 0.12, 1.0);

void fragment() {
	vec2 d = UV - vec2(0.5);
	d.x *= 1.35;
	float v = smoothstep(0.35, 0.85, length(d));
	COLOR = vec4(tint.rgb, v * strength);
}
