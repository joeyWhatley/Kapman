extends RefCounted
## Sea state: a set of Gerstner waves shaped by the wind (a Pierson–Moskowitz-like spread of
## wavelengths around the peak for that wind speed). The same maths runs here (for the ship's
## buoyancy) and in ocean.gdshader (for the surface), so the ship floats on the waves you see.
##
## Positions are local metres around the floating origin. Each wave's phase includes the origin
## offset and time, folded into [0, TAU) here in 64-bit, so the shader never sees huge numbers.

const G := 9.81
const COUNT := 8
## Relative wavelengths and share of the height, longest first.
const SPREAD := [1.0, 0.78, 0.61, 0.47, 0.36, 0.27, 0.2, 0.15]
const SHARE := [0.34, 0.22, 0.14, 0.1, 0.07, 0.055, 0.045, 0.03]
const ANGLES := [0.0, 0.42, -0.36, 0.78, -0.7, 0.2, -1.05, 1.15]   # radians off the wind

var wind_dir := Vector2(1, 0)      # the direction the wind blows toward (x = east, y = south)
var wind_speed := 8.0              # m/s
var choppiness := 0.75             # 0 = round swells, 1 = sharp crests

var dirs := PackedVector2Array()
var k := PackedFloat32Array()        # wavenumber (2π/λ)
var amp := PackedFloat32Array()      # metres
var omega := PackedFloat32Array()
var steep := PackedFloat32Array()    # Gerstner Q (horizontal pinch)
var phase := PackedFloat32Array()    # current phase offset per wave, [0, TAU)


func _init() -> void:
	configure(wind_dir, wind_speed)


## Rebuild the spectrum for a wind. Called when the weather changes (not every frame).
func configure(dir: Vector2, speed: float) -> void:
	wind_dir = dir.normalized() if dir.length() > 0.001 else Vector2(1, 0)
	wind_speed = maxf(speed, 0.5)
	# Peak wavelength and significant height for a fully developed sea at this wind speed.
	var peak := clampf(0.834 * wind_speed * wind_speed, 4.0, 240.0)
	var hs := clampf(0.21 * wind_speed * wind_speed / G, 0.05, 9.0)
	dirs.resize(COUNT)
	k.resize(COUNT)
	amp.resize(COUNT)
	omega.resize(COUNT)
	steep.resize(COUNT)
	phase.resize(COUNT)
	for i in COUNT:
		var lam: float = peak * SPREAD[i]
		dirs[i] = wind_dir.rotated(ANGLES[i])
		k[i] = TAU / lam
		amp[i] = hs * 0.5 * SHARE[i] / 0.4 * 0.5
		omega[i] = sqrt(G * k[i])
		steep[i] = choppiness / maxf(k[i] * amp[i] * COUNT, 0.001) * 0.9
		steep[i] = minf(steep[i], 1.0 / maxf(k[i] * amp[i] * COUNT, 0.001))


## Advance to absolute time t (seconds) with the floating origin at origin_m (absolute metres, 64-bit).
func update(t: float, origin_x: float, origin_z: float) -> void:
	for i in COUNT:
		var p := k[i] * (dirs[i].x * origin_x + dirs[i].y * origin_z) - omega[i] * t
		phase[i] = fposmod(p, TAU)


## Surface displacement at a local rest position (x, z): Vector3(dx, height, dz).
func displace(x: float, z: float) -> Vector3:
	var d := Vector3.ZERO
	for i in COUNT:
		var th := k[i] * (dirs[i].x * x + dirs[i].y * z) + phase[i]
		var c := cos(th)
		var qa := steep[i] * amp[i]
		d.x += qa * dirs[i].x * c
		d.z += qa * dirs[i].y * c
		d.y += amp[i] * sin(th)
	return d


## Water height at a local point (inverts the horizontal pinch with a few fixed-point steps).
func height_at(x: float, z: float) -> float:
	var px := x
	var pz := z
	var d := Vector3.ZERO
	for it in 3:
		d = displace(px, pz)
		px = x - d.x
		pz = z - d.z
	return displace(px, pz).y


func significant_height() -> float:
	return clampf(0.21 * wind_speed * wind_speed / G, 0.05, 9.0)


## Shader uniforms: arrays of vec4 (dir.x, dir.y, k, amp) and vec2 (steep, phase).
func push(mat: ShaderMaterial) -> void:
	var a := PackedVector4Array()
	var b := PackedVector2Array()
	for i in COUNT:
		a.append(Vector4(dirs[i].x, dirs[i].y, k[i], amp[i]))
		b.append(Vector2(steep[i], phase[i]))
	mat.set_shader_parameter("wave_a", a)
	mat.set_shader_parameter("wave_b", b)
	mat.set_shader_parameter("wind_dir", wind_dir)
	mat.set_shader_parameter("sea_state", clampf(wind_speed / 20.0, 0.0, 1.0))
