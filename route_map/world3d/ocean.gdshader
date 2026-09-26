shader_type spatial;
render_mode cull_disabled, depth_draw_always, specular_schlick_ggx, world_vertex_coords;
// The sea. Gerstner waves (the same set as waves.gd, so the ship rides them), damped in shallow
// water; small ripples from a scrolling normal map; clear shallows showing the seabed (screen
// refraction + depth), deep water absorbing to teal-blue; foam on crests, on the shore and in the
// ship's wake. Painterly colours to match the land.

#include "res://route_map/world3d/world_height.gdshaderinc"
#include "res://route_map/world3d/clouds.gdshaderinc"
#include "res://route_map/world3d/toon.gdshaderinc"

instance uniform float cell = 1.0;
instance uniform float half_extent = 32.0;
instance uniform float level_bias = 0.0;

const int WAVES = 8;
uniform vec4 wave_a[8];     // dir.x, dir.y, k, amp
uniform vec2 wave_b[8];     // steepness Q, phase
uniform vec2 wind_dir = vec2(1.0, 0.0);
uniform float sea_state = 0.4;


uniform sampler2D ripple_a : hint_normal, filter_linear_mipmap, repeat_enable;
uniform sampler2D ripple_b : hint_normal, filter_linear_mipmap, repeat_enable;
uniform vec2 ripple_off_a;  // origin + time scroll, folded on the CPU
uniform vec2 ripple_off_b;
uniform sampler2D foam_tex : filter_linear_mipmap, repeat_enable;
uniform vec2 foam_off;

uniform sampler2D screen_tex : hint_screen_texture, filter_linear_mipmap;
uniform sampler2D depth_tex : hint_depth_texture, filter_linear_mipmap;

// North Sea: grey-green and cold, lighter and browner over the sandbanks
uniform vec3 deep_col : source_color = vec3(0.07, 0.15, 0.17);
uniform vec3 mid_col : source_color = vec3(0.12, 0.25, 0.25);
uniform vec3 shallow_col : source_color = vec3(0.3, 0.4, 0.34);
uniform vec3 foam_col : source_color = vec3(0.86, 0.87, 0.84);
uniform vec3 crest_col : source_color = vec3(0.16, 0.34, 0.31);    // light through thin crests

// Ship: hull footprint (no water inside it) and wake trail.
uniform vec4 ship_frame = vec4(0.0, 0.0, 1.0, 0.0);   // x, z, cos(heading), sin(heading)
uniform vec2 ship_half = vec2(0.0, 0.0);              // half length, half beam (0 = no ship)
uniform vec4 wake[24];                                // x, z, age (s), speed
uniform int wake_count = 0;

varying vec2 rest_xz;
varying float depth_m;
varying float jacobian;
varying float crest;
varying vec3 wave_n;
varying float view_dist;
varying float calm;         // 1 on rivers: no swell, no surf

void vertex() {
	vec3 w = VERTEX;
	vec2 rel = w.xz - NODE_POSITION_WORLD.xz;
	float dd = max(abs(rel.x), abs(rel.y)) / half_extent;
	float morph = smoothstep(0.72, 0.94, dd);
	vec2 odd = mod(round(w.xz / cell), 2.0);
	w.xz -= odd * cell * morph;
	vec2 p = w.xz;
	rest_xz = p;
	float level = water_level(p);
	calm = smoothstep(0.04, 0.3, level);
	depth_m = level - base_height(p);
	float shallow = smoothstep(0.2, 7.0, depth_m) * (1.0 - calm);
	w.y = level;
	vec3 disp = vec3(0.0);
	vec3 n = vec3(0.0, 1.0, 0.0);
	float jx = 1.0, jz = 1.0, jxz = 0.0;
	float hsum = 0.0, amax = 0.0;
	for (int i = 0; i < WAVES; i++) {
		vec2 dir = wave_a[i].xy;
		float k = wave_a[i].z;
		float lam = 6.2831853 / k;
		// waves shorter than a few grid squares would alias: fade them out on coarse levels
		float a = wave_a[i].w * shallow * smoothstep(cell * 3.0, cell * 6.0, lam);
		float q = wave_b[i].x;
		float th = k * dot(dir, p) + wave_b[i].y;
		float c = cos(th), s = sin(th);
		disp.x += q * a * dir.x * c;
		disp.z += q * a * dir.y * c;
		disp.y += a * s;
		float wa = k * a;
		n.x -= dir.x * wa * c;
		n.z -= dir.y * wa * c;
		n.y -= q * wa * s;
		jx -= q * wa * dir.x * dir.x * s;
		jz -= q * wa * dir.y * dir.y * s;
		jxz -= q * wa * dir.x * dir.y * s;
		hsum += a;
		amax += wave_a[i].w;
	}
	w += disp;
	w.y += level_bias;
	wave_n = normalize(n);
	jacobian = jx * jz - jxz * jxz;
	crest = amax > 0.0 ? clamp(disp.y / (amax * 0.8) * 0.5 + 0.5, 0.0, 1.0) : 0.5;
	VERTEX = w;
	view_dist = length((VIEW_MATRIX * vec4(w, 1.0)).xyz);
}

float seg_dist(vec2 p, vec2 a, vec2 b) {
	vec2 ab = b - a;
	float t = clamp(dot(p - a, ab) / max(dot(ab, ab), 1e-4), 0.0, 1.0);
	return length(p - a - ab * t);
}

void fragment() {
	vec2 p = rest_xz;
	// The shoreline is decided per pixel from the 50 m data, not by where coarse mesh squares
	// happen to cross the water, so it stays smooth at every distance.
	float level_px = water_level(p);
	float depth_px = level_px - base_height(p);
	if (depth_px < -0.03) {
		discard;
	}
	// No water inside the hull.
	if (ship_half.x > 0.0) {
		vec2 r = p - ship_frame.xy;
		vec2 sl = vec2(r.x * ship_frame.z + r.y * ship_frame.w, -r.x * ship_frame.w + r.y * ship_frame.z);
		// the waterline: full beam amidships, narrowing to the raked posts
		float along = abs(sl.x) / (ship_half.x * 0.86);
		if (along < 1.0 && abs(sl.y) < ship_half.y * 0.88 * (1.0 - pow(along, 2.6))) {
			discard;
		}
	}
	// Ripples: two scrolling normal maps, fading with distance so the far sea stays calm.
	float fade = 1.0 - smoothstep(200.0, 2500.0, view_dist);
	vec3 ra = texture(ripple_a, p / 38.0 + ripple_off_a).xyz * 2.0 - 1.0;
	vec3 rb = texture(ripple_b, p / 11.0 + ripple_off_b).xyz * 2.0 - 1.0;
	vec2 rip = (ra.xy * 0.3 + rb.xy * 0.18) * (0.3 + sea_state) * fade;
	vec3 n_world = normalize(wave_n + vec3(rip.x, 0.0, rip.y));
	NORMAL = normalize((VIEW_MATRIX * vec4(n_world, 0.0)).xyz);

	// How much water lies between this surface and whatever is behind it.
	vec2 refr_uv = SCREEN_UV + n_world.xz * 0.025 * (1.0 - smoothstep(20.0, 400.0, view_dist));
	float scene_depth = textureLod(depth_tex, refr_uv, 0.0).r;
	vec4 behind = INV_PROJECTION_MATRIX * vec4(refr_uv * 2.0 - 1.0, scene_depth, 1.0);
	float behind_dist = -behind.z / behind.w;
	if (behind_dist < -VERTEX.z) {        // the refracted pixel is in front of the water: don't bend
		refr_uv = SCREEN_UV;
		scene_depth = textureLod(depth_tex, refr_uv, 0.0).r;
		behind = INV_PROJECTION_MATRIX * vec4(refr_uv * 2.0 - 1.0, scene_depth, 1.0);
		behind_dist = -behind.z / behind.w;
	}
	float thickness = max(behind_dist + VERTEX.z, 0.0);
	thickness = min(thickness, max(depth_px, 0.0) * 3.0 + 1.0);    // the far view: use the charted depth
	vec3 seen = textureLod(screen_tex, refr_uv, 0.0).rgb;
	// painted depth bands (soft-edged), like washes on the chart
	vec3 body = mix(mid_col, deep_col, smoothstep(14.0, 18.0, depth_px) * 0.6 + smoothstep(45.0, 60.0, depth_px) * 0.4);
	body = mix(shallow_col, body, smoothstep(2.0, 3.0, depth_px) * 0.6 + smoothstep(6.0, 8.0, depth_px) * 0.4);
	float clear = exp(-thickness / 2.6);
	vec3 col = mix(body, seen * mix(vec3(0.75, 0.9, 0.88), vec3(1.0), clear), clear);
	col = mix(col, crest_col, smoothstep(0.55, 1.0, crest) * 0.35 * (1.0 - clear));

	// Foam: breaking crests, the surf line, and the wake.
	float f_tex = texture(foam_tex, p / 9.0 + foam_off).r;
	float f_big = texture(foam_tex, p / 31.0 - foam_off * 0.6).r;
	float foam = smoothstep(0.62, 0.2, jacobian) * smoothstep(0.35, 0.65, f_tex) * (0.4 + sea_state);
	float surf = (1.0 - smoothstep(0.2, 1.6, depth_px)) * (1.0 - calm);
	surf *= smoothstep(0.35, 0.6, f_tex * 0.6 + f_big * 0.6 + 0.25 * sin(TIME * 1.4 + depth_px * 5.0));
	foam = max(foam, surf);
	for (int i = 0; i < 23; i++) {
		if (i + 1 >= wake_count) break;
		vec4 a = wake[i];
		vec4 b = wake[i + 1];
		float age = a.z;
		float width = 1.6 + age * 0.55;
		float d = seg_dist(p, a.xy, b.xy);
		float strength = (1.0 - smoothstep(0.0, 26.0, age)) * clamp(a.w / 2.5, 0.2, 1.0);
		// a V of turbulence behind the hull: brightest at the edges of the widening trail
		float edge = smoothstep(width, width * 0.6, d) * (0.5 + 0.5 * smoothstep(width * 0.2, width * 0.8, d));
		foam = max(foam, edge * strength * smoothstep(0.25, 0.55, f_tex + 0.25));
	}
	foam = smoothstep(0.35, 0.5, foam);                                   // crisp, painted foam
	// the chart's water-lines: thin ink contours running parallel to the shore
	float shore_m = sea_distance(p) * 2550.0;
	float wl = 0.0;
	for (int k = 0; k < 3; k++) {
		float at = 70.0 + float(k) * float(k + 1) * 60.0;
		wl = max(wl, (1.0 - smoothstep(0.0, 4.0 + view_dist * 0.002, abs(shore_m - at))) * (1.0 - float(k) * 0.25));
	}
	wl *= (1.0 - calm) * (1.0 - smoothstep(1500.0, 9000.0, view_dist)) * smoothstep(0.3, 1.0, depth_px);
	col = mix(col, col * 0.62, wl * 0.55);
	col = mix(col, foam_col, clamp(foam, 0.0, 1.0));

	ALBEDO = col * mix(1.0, cloud_shade(p), 0.8);
	ROUGHNESS = mix(0.1, 0.6, clamp(foam, 0.0, 1.0));
	SPECULAR = 0.5;
	METALLIC = 0.0;
	ALPHA = 1.0;
}
