extends PanelContainer
## Small music player for the corner. Plays every .ogg / .mp3 / .wav in res://music, in order.
## Layout lives in music_player.tscn; _build() only runs if the nodes are missing.

const Style := preload("res://route_map/style.gd")
const MUSIC_DIR := "res://music"

var _player: AudioStreamPlayer
var _tracks: Array = []
var _index := 0
var _title: Label
var _play: Button
var _playing := false


func _ready() -> void:
	if get_node_or_null("Body") == null:
		_build()
	_bind()
	_scan()


func _build() -> void:
	name = "MusicPlayer"
	var player := AudioStreamPlayer.new()
	player.name = "Player"
	player.volume_db = linear_to_db(0.7)
	add_child(player)
	var v := VBoxContainer.new()
	v.name = "Body"
	v.add_theme_constant_override("separation", 6)
	add_child(v)
	var title := Label.new()
	title.name = "Title"
	title.add_theme_font_size_override("font_size", 12)
	title.add_theme_color_override("font_color", Style.INK_SOFT)
	title.clip_text = true
	title.custom_minimum_size = Vector2(200, 0)
	title.mouse_filter = Control.MOUSE_FILTER_PASS
	v.add_child(title)
	var h := HBoxContainer.new()
	h.name = "Controls"
	h.add_theme_constant_override("separation", 4)
	v.add_child(h)
	for pair in [["Prev", "|◀", "Previous"], ["Play", "▶", "Play / pause"], ["Next", "▶|", "Next"]]:
		var b := Button.new()
		b.name = pair[0]
		b.text = pair[1]
		b.tooltip_text = pair[2]
		b.custom_minimum_size = Vector2(34, 26)
		h.add_child(b)
	var vol := HSlider.new()
	vol.name = "Volume"
	vol.min_value = 0.0
	vol.max_value = 1.0
	vol.step = 0.01
	vol.value = 0.7
	vol.custom_minimum_size = Vector2(80, 0)
	vol.size_flags_vertical = Control.SIZE_SHRINK_CENTER
	vol.tooltip_text = "Volume"
	h.add_child(vol)


func _bind() -> void:
	_player = $Player
	_title = $Body/Title
	_play = $Body/Controls/Play
	_player.finished.connect(next)
	$Body/Controls/Prev.pressed.connect(prev)
	_play.pressed.connect(toggle)
	$Body/Controls/Next.pressed.connect(next)
	var vol: HSlider = $Body/Controls/Volume
	vol.value_changed.connect(func(x): _player.volume_db = linear_to_db(maxf(x, 0.001)))
	_player.volume_db = linear_to_db(maxf(vol.value, 0.001))


func _scan() -> void:
	_tracks.clear()
	var seen := {}
	for f in DirAccess.get_files_at(MUSIC_DIR):
		var file := f.trim_suffix(".import").trim_suffix(".remap")
		if file.get_extension().to_lower() in ["ogg", "mp3", "wav"] and not seen.has(file):
			seen[file] = true
			_tracks.append(MUSIC_DIR.path_join(file))
	_tracks.sort()
	_show()


func _show() -> void:
	if _tracks.is_empty():
		_title.text = "No music yet"
		_title.tooltip_text = "Put .ogg or .mp3 files in the project's music folder"
		_play.disabled = true
		return
	_title.text = _tracks[_index].get_file().get_basename().replace("_", " ")
	_play.text = "❚❚" if _playing else "▶"


func _start() -> void:
	var stream = load(_tracks[_index])
	if stream is AudioStreamOggVorbis or stream is AudioStreamMP3:
		stream.loop = false
	_player.stream = stream
	_player.play()
	_playing = true
	_show()


func toggle() -> void:
	if _tracks.is_empty():
		return
	if _playing:
		_player.stream_paused = true
		_playing = false
	elif _player.stream != null and _player.stream_paused:
		_player.stream_paused = false
		_playing = true
	else:
		_start()
	_show()


func next() -> void:
	if _tracks.is_empty():
		return
	_index = (_index + 1) % _tracks.size()
	_player.stream_paused = false
	_start()


func prev() -> void:
	if _tracks.is_empty():
		return
	_index = (_index - 1 + _tracks.size()) % _tracks.size()
	_player.stream_paused = false
	_start()
