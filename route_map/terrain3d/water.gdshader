shader_type spatial;
// Low-poly sea: faceted waves (flat normals from screen derivatives), coloured by the real depth,
// with foam where the water meets the shore.

uniform sampler2D height_tex : filter_linear;
uniform vec2 map_size = vec2(7316.0, 3872.0);
uniform vec4 shallow : source_color = vec4(0.42, 0.7, 0.68, 1.0);
uniform vec4 mid : source_color = vec4(0.18, 0.46, 0.58, 1.0);
uniform vec4 deep : source_color = vec4(0.08, 0.2, 0.36, 1.0);
uniform vec4 foam : source_color = vec4(0.95, 0.95, 0.9, 1.0);

varying vec3 world;

void vertex() {
	world = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
	float t = TIME;
	VERTEX.y += sin(world.x * 0.04 + t * 1.0) * 0.18 + cos(world.z * 0.047 + t * 0.8) * 0.16
		+ sin((world.x + world.z) * 0.019 + t * 0.6) * 0.22;
}

void fragment() {
	float m = texture(height_tex, world.xz / map_size).r;      // metres, negative at sea
	float depth = clamp(-m, 0.0, 4000.0);
	vec3 col = mix(shallow.rgb, mid.rgb, smoothstep(0.0, 60.0, depth));
	col = mix(col, deep.rgb, smoothstep(60.0, 1200.0, depth));
	// shore foam, gently pulsing
	float shore = 1.0 - smoothstep(0.0, 14.0, depth);
	shore *= 0.6 + 0.4 * sin(TIME * 1.5 + world.x * 0.08 + world.z * 0.05);
	col = mix(col, foam.rgb, clamp(shore, 0.0, 1.0) * 0.55);
	ALBEDO = col;
	NORMAL = normalize(cross(dFdx(VERTEX), dFdy(VERTEX)));
	ROUGHNESS = 0.25;
	METALLIC = 0.0;
	SPECULAR = 0.55;

}
