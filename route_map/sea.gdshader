shader_type canvas_item;
// The living ink sea. Drawn as one rectangle over the whole map (map units in, see sea_layer.gd).
//  - ink wash that deepens away from the coast (sea_distance.png: 0 = land, 1 = far out)
//  - coastal ripple lines that slowly travel outward from every shore
//  - engraved wave strokes in open water, drifting with the wind; rougher in storms
// All line widths are kept constant on screen with fwidth(), so zooming never makes them blurry.

uniform sampler2D dist_tex : filter_linear;
uniform sampler2D noise_tex : filter_linear, repeat_enable;
uniform vec2 map_size = vec2(4950.0, 3140.0);
uniform vec4 ink : source_color = vec4(0.1, 0.11, 0.13, 1.0);
uniform vec2 wind = vec2(0.7, -0.7);        // direction the wind blows toward (map coords)
uniform float wind_strength = 0.2;
uniform vec2 flow_offset = vec2(0.0);      // accumulated drift, fed by the script (no jumps)
uniform float zoom = 1.0;
uniform float dist_units = 425.0;           // map units at dist value 1.0
// Up to 4 weather systems: xy = centre (map units), z = radius, w = strength 0..1
uniform vec4 storms[4];
uniform sampler2D fog_tex : filter_linear;   // charted (1) / fogged (0)
uniform vec2 fog_size = vec2(4950.0, 3140.0);

varying vec2 p;

void vertex() {
	p = VERTEX;
}

float line_at(float phase, float width_px) {
	float w = fwidth(phase) * width_px;
	float q = abs(fract(phase + 0.5) - 0.5);
	return 1.0 - smoothstep(0.0, w, q);
}

void fragment() {
	float d = texture(dist_tex, p / map_size).r;
	float sea = smoothstep(0.004, 0.02, d);
	float du = d * dist_units;
	float t = TIME;

	// Storminess here: strongest near a weather system's centre.
	float storm = 0.0;
	for (int i = 0; i < 4; i++) {
		vec4 s = storms[i];
		if (s.z > 0.0) {
			storm = max(storm, s.w * (1.0 - smoothstep(s.z * 0.3, s.z, distance(p, s.xy))));
		}
	}

	// Depth wash: shallows stay paper, open sea takes a light ink tone.
	float wash = 0.015 + 0.075 * smoothstep(8.0, 260.0, du) + 0.05 * storm;

	// Coastal ripples: thin contour lines moving outward, fading with distance and at far zoom.
	float near = 1.0 - smoothstep(6.0, 48.0, du);
	float ripple = line_at(du / 9.0 - t * 0.22, 1.1) * near * 0.22 * smoothstep(0.45, 0.9, zoom);

	// Engraved wave strokes: stripes across the wind, broken into dashes by noise, drifting downwind.
	vec2 w = normalize(wind);
	vec2 across = vec2(-w.y, w.x);
	vec2 flow = p - flow_offset;
	float stripes = line_at(dot(flow, w) / 13.0 + texture(noise_tex, flow / 900.0).r * 2.0, 1.0);
	float dash = smoothstep(0.52 - 0.12 * storm, 0.62, texture(noise_tex, vec2(dot(flow, across) / 70.0, dot(flow, w) / 24.0)).r);
	float open_sea = smoothstep(30.0, 110.0, du);
	float waves = stripes * dash * open_sea * (0.13 + 0.25 * storm) * smoothstep(0.35, 0.75, zoom);

	// Under the fog the sea keeps only a faint wash: ripples would give the coastline away.
	float charted = texture(fog_tex, p / fog_size).r;
	COLOR = vec4(ink.rgb, sea * clamp(wash * mix(0.35, 1.0, charted) + (ripple + waves) * charted, 0.0, 1.0));
}
