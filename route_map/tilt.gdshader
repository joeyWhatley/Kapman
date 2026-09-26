shader_type canvas_item;
// 2.5D: shows the flat chart (rendered off-screen) as a tilted plane. A homography maps each screen
// point to a point on the chart; the far edge recedes into a soft haze. Identity when tilt = 0.

uniform vec4 h1 = vec4(1.0, 0.0, 0.0, 0.0);   // a b c d
uniform vec4 h2 = vec4(1.0, 0.0, 0.0, 0.0);   // e f g h
uniform float tilt = 0.0;
uniform vec4 haze : source_color = vec4(0.94, 0.91, 0.84, 1.0);

void fragment() {
	vec2 uv = UV;
	vec3 q = vec3(h1.x * uv.x + h1.y * uv.y + h1.z,
			h1.w * uv.x + h2.x * uv.y + h2.y,
			h2.z * uv.x + h2.w * uv.y + 1.0);
	vec2 src = q.xy / q.z;
	vec4 col = texture(TEXTURE, src);
	float hz = tilt * 0.6 * pow(1.0 - uv.y, 2.2);
	COLOR = vec4(mix(col.rgb, haze.rgb, hz), 1.0);
}
