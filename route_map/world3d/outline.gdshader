shader_type spatial;
render_mode unshaded, depth_draw_never, depth_test_disabled, cull_disabled, fog_disabled, shadows_disabled;
// Ink outlines: a full-screen pass (a quad under the camera) that finds silhouettes and creases
// from the depth and normal buffers and draws them in the chart's iron-gall ink. Lines fade with
// distance so far terrain stays painterly and the view hands over cleanly to the 2D chart.

uniform sampler2D depth_tex : hint_depth_texture, filter_nearest;
uniform sampler2D normal_tex : hint_normal_roughness_texture, filter_nearest;
uniform vec4 ink : source_color = vec4(0.13, 0.1, 0.08, 0.85);
uniform float thickness = 2.0;          // pixels of the (supersampled) render
uniform float depth_edge = 0.045;       // relative depth jump that counts as a silhouette
uniform float normal_edge = 0.5;        // normal change that counts as a crease
uniform float fade_near = 250.0;        // metres: full-strength lines up to here...
uniform float fade_far = 2800.0;        // ...gone by here

void vertex() {
	POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

float lin_depth(vec2 uv, mat4 inv_proj) {
	float d = textureLod(depth_tex, uv, 0.0).r;
	vec4 v = inv_proj * vec4(uv * 2.0 - 1.0, d, 1.0);
	return -v.z / v.w;
}

vec3 nrm(vec2 uv) {
	return textureLod(normal_tex, uv, 0.0).xyz * 2.0 - 1.0;
}

void fragment() {
	vec2 px = thickness / VIEWPORT_SIZE;
	float dc = lin_depth(SCREEN_UV, INV_PROJECTION_MATRIX);
	if (dc > fade_far || dc < 0.0) {
		discard;
	}
	vec3 nc = nrm(SCREEN_UV);
	float edge = 0.0;
	vec2 offs[4] = vec2[](vec2(px.x, 0.0), vec2(-px.x, 0.0), vec2(0.0, px.y), vec2(0.0, -px.y));
	for (int i = 0; i < 4; i++) {
		vec2 uv = SCREEN_UV + offs[i];
		if (uv.x < 0.0 || uv.y < 0.0 || uv.x > 1.0 || uv.y > 1.0) {
			continue;
		}
		float dn = lin_depth(uv, INV_PROJECTION_MATRIX);
		// only the nearer side of a silhouette draws the line
		if (dn > fade_far * 4.0) {
			// against the sky: outline nearby things (mast, trees on a ridge), not the far horizon
			edge = max(edge, 1.0 - smoothstep(fade_near, fade_far * 0.5, dc));
			continue;
		}
		float dj = (dn - dc) / max(dc, 0.1);
		edge = max(edge, smoothstep(depth_edge, depth_edge * 2.0, dj));
		float nd = 1.0 - dot(nc, nrm(uv));
		edge = max(edge, smoothstep(normal_edge, normal_edge + 0.25, nd) * 0.7);
	}
	float fade = 1.0 - smoothstep(fade_near, fade_far, dc);
	ALBEDO = ink.rgb;
	ALPHA = edge * fade * ink.a;
}
