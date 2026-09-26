// Shared by the ocean, rivers and lakes so all water in the 3D world reads as one material.

uniform vec4 shallow : source_color = vec4(0.3, 0.7, 0.68, 1.0);
uniform vec4 mid : source_color = vec4(0.1, 0.45, 0.6, 1.0);
uniform vec4 deep : source_color = vec4(0.04, 0.18, 0.36, 1.0);
uniform vec4 foam : source_color = vec4(0.96, 0.96, 0.92, 1.0);

// Water colour for a depth in metres.
vec3 water_colour(float depth) {
	vec3 col = mix(shallow.rgb, mid.rgb, smoothstep(0.0, 70.0, depth));
	return mix(col, deep.rgb, smoothstep(70.0, 1500.0, depth));
}

// Final surface: faceted normal from screen derivatives, a touch of sky at grazing angles.
void water_surface(vec3 col, vec3 vertex, vec3 view, out vec3 albedo, out vec3 normal) {
	vec3 n = normalize(cross(dFdx(vertex), dFdy(vertex)));
	float fres = pow(1.0 - clamp(dot(n, view), 0.0, 1.0), 4.0);
	albedo = mix(col, vec3(0.8, 0.86, 0.9), fres * 0.35);
	normal = n;
}
