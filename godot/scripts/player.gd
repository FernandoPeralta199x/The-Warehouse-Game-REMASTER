class_name Player extends Node2D

const MOVE_DURATION := 0.12

var grid_pos: Vector2i = Vector2i.ZERO :
	set(value):
		grid_pos = value
		_animate_to(grid_pos)

var tile_size: int = 96
var facing: Vector2i = Vector2i(0, 1)  # default: olhando pra baixo
var _tween: Tween

func _ready() -> void:
	position = Vector2(grid_pos.x * tile_size, grid_pos.y * tile_size)

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
	var pad := tile_size * 0.18
	var rect := Rect2(pad, pad, tile_size - 2 * pad, tile_size - 2 * pad)
	# corpo
	draw_rect(rect, TileConst.COLOR_PLAYER, true)
	# borda escura
	draw_rect(rect, TileConst.COLOR_PLAYER.darkened(0.4), false, 3.0)
	# indicador de direção (triângulo apontando facing)
	var cx := tile_size / 2.0
	var cy := tile_size / 2.0
	var r := tile_size * 0.16
	var fx: float = facing.x
	var fy: float = facing.y
	var tip := Vector2(cx + fx * r * 1.4, cy + fy * r * 1.4)
	var perp := Vector2(-fy, fx)
	var base_a := Vector2(cx + perp.x * r * 0.7, cy + perp.y * r * 0.7)
	var base_b := Vector2(cx - perp.x * r * 0.7, cy - perp.y * r * 0.7)
	draw_colored_polygon(PackedVector2Array([tip, base_a, base_b]),
		TileConst.COLOR_PLAYER.darkened(0.5))
