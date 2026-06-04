class_name Box extends Node2D

const MOVE_DURATION := 0.12

var grid_pos: Vector2i = Vector2i.ZERO :
	set(value):
		grid_pos = value
		_animate_to(grid_pos)

var tile_size: int = 96
var on_goal: bool = false :
	set(value):
		if on_goal != value:
			on_goal = value
			queue_redraw()

var _tween: Tween

func _ready() -> void:
	position = _grid_to_pixel(grid_pos)

func snap_to(pos: Vector2i) -> void:
	grid_pos = pos
	position = _grid_to_pixel(pos)
	if _tween: _tween.kill()

func _animate_to(pos: Vector2i) -> void:
	var target := _grid_to_pixel(pos)
	if _tween: _tween.kill()
	_tween = create_tween().set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	_tween.tween_property(self, "position", target, MOVE_DURATION)

func _grid_to_pixel(pos: Vector2i) -> Vector2:
	return Vector2(pos.x * tile_size, pos.y * tile_size)

func _draw() -> void:
	var pad := tile_size * 0.10
	var rect := Rect2(pad, pad, tile_size - 2 * pad, tile_size - 2 * pad)
	var fill := TileConst.COLOR_BOX_OK if on_goal else TileConst.COLOR_BOX
	draw_rect(rect, fill, true)
	draw_rect(rect, fill.darkened(0.4), false, 4.0)
	# detalhe central — "x" tipo amarra de caixa de madeira
	var c := tile_size / 2.0
	var s := tile_size * 0.18
	var line_color := fill.darkened(0.55)
	draw_line(Vector2(c - s, c - s), Vector2(c + s, c + s), line_color, 3.0)
	draw_line(Vector2(c - s, c + s), Vector2(c + s, c - s), line_color, 3.0)
