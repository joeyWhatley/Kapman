// What grows where, shared by the ground (terrain.gdshader) and the plants scattered on it
// (scatter.gdshader) so a wood on the ground always has trees in it. Needs world_height.gdshaderinc.
// Frisian coast about 800 AD: beach and dunes with marram, salt marsh, wet meadow and peat bog,
// heath on the sandy uplands, oak/beech woods on higher ground and alder carr in the wet.

float fbm3(vec2 q) {
	// Turned by a 3-4-5 triangle and scaled by whole numbers, so with offsets folded over 1280
	// cells (5 x 256) every layer still repeats exactly and re-centring never shifts the pattern.
	float s = vnoise(mat2(vec2(0.6, 0.8), vec2(-0.8, 0.6)) * q) * 0.6;
	s += vnoise(mat2(vec2(0.8, 0.6), vec2(-0.6, 0.8)) * q * 2.0 + 17.0) * 0.28;
	s += vnoise(mat2(vec2(0.6, -0.8), vec2(0.8, 0.6)) * q * 4.0 + 41.0) * 0.12;
	return s;
}

struct Cover {
	float big;      // 0..1 large-scale pattern (woods vs open)
	float mid;      // 0..1
	float fine;     // 0..1
	float woods;
	float heath;
	float dune;
	float beach;
	float marsh;
	float peat;
	float wet;      // low, wet ground (reeds, alder)
	float sea_m;    // metres from the sea
};

Cover cover_at(vec2 p, float h) {
	Cover c;
	c.big = fbm3(p / 900.0 + noise_off[6]) * 0.5 + 0.5;
	c.mid = fbm3(p / 140.0 + noise_off[7]) * 0.5 + 0.5;
	c.fine = fbm3(p / 23.0 + noise_off[8]) * 0.5 + 0.5;
	c.sea_m = sea_distance(p) * 2550.0;
	c.beach = h < 0.3 ? 0.0 : 1.0 - smoothstep(60.0, 180.0, c.sea_m);
	c.dune = (1.0 - smoothstep(1400.0, 2600.0, c.sea_m + c.mid * 600.0)) * smoothstep(2.0, 4.0, h) * (1.0 - c.beach);
	c.marsh = (1.0 - smoothstep(250.0, 900.0, c.sea_m)) * (1.0 - smoothstep(1.5, 3.5, h)) * (1.0 - c.beach);
	c.peat = smoothstep(0.64, 0.7, c.big) * (1.0 - smoothstep(2.0, 5.0, h));
	c.heath = smoothstep(14.0, 24.0, h) * smoothstep(0.64, 0.7, c.mid + c.fine * 0.1);
	c.wet = (1.0 - smoothstep(1.0, 3.0, h)) * smoothstep(0.55, 0.75, c.mid);   // hollows, not the whole lowland
	// woods: more of them on the higher sandy ground (the Veluwe, the Utrecht ridge) than in the wet lowland
	float wood_line = mix(0.54, 0.4, smoothstep(4.0, 25.0, h));
	c.woods = smoothstep(wood_line, wood_line + 0.05, c.big + c.fine * 0.1) * smoothstep(1.2, 2.5, h) * (1.0 - c.dune) * (1.0 - c.marsh) * (1.0 - c.beach) * (1.0 - c.heath * 0.8)
		* smoothstep(0.9, 2.2, h - water_level(p));     // not on river banks: keeps the water open
	return c;
}
