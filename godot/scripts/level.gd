class_name Level extends Node2D

const TILE_SIZE := 96
const PLAYER_SCENE := preload("res://scenes/entities/player.tscn")
const BOX_SCENE := preload("res://scenes/entities/box.tscn")

signal level_complete

@export_file("*.xsb") var level_path: String = "res://levels/new/00_tutorial.xsb"

var data: LevelData
var player: Player
var boxes: Array[Box] = []
var goal_set: Dictionary = {}  # Vector2i -> true
var moves: int = 0
var won: bool = false
var input_locked: bool = false

@onready var entities_node: Node2D = $Entities
@onready var hud_label: Label = $"../HUD/Info"
@onready var win_panel: Control = $"../HUD/WinPanel"
@onready var win_label: Label = $"../HUD/WinPanel/VBox/Title"

func _ready() -> void:
	win_panel.visible = false
	load_level_from_path(level_path)

func load_level_from_path(path: String) -> void:
	var ld := LevelLoader.from_file(path)
	if not ld.is_valid():
		push_error("Invalid level '%s': %s" % [path, ", ".join(ld.parse_errors)])
		hud_label.text = "ERRO: %s" % ", ".join(ld.parse_errors)
		return
	_install_level(ld)

func _install_level(ld: LevelData) -> void:
	data = ld
	won = false
	moves = 0
	goal_set.clear()
	for g in data.goals:
		goal_set[g] = true

	# Limpa entidades anteriores
	for child in entities_node.get_children():
		child.queue_free()
	boxes.clear()

	# Spawn jogador
	player = PLAYER_SCENE.instantiate()
	player.tile_size = TILE_SIZE
	entities_node.add_child(player)
	player.snap_to(data.player_init)

	# Spawn caixas
	for bp in data.boxes_init:
		var b := BOX_SCENE.instantiate()
		b.tile_size = TILE_SIZE
		entities_node.add_child(b)
		b.snap_to(bp)
		b.on_goal = goal_set.has(bp)
		boxes.append(b)

	# Centraliza câmera (offset do level node)
	var level_px_w := data.width * TILE_SIZE
	var level_px_h := data.height * TILE_SIZE
	var viewport := get_viewport_rect().size
	position = Vector2(
		(viewport.x - level_px_w) / 2.0,
		(viewport.y - level_px_h) / 2.0
	)

	_update_hud()
	queue_redraw()

func _unhandled_input(event: InputEvent) -> void:
	if won or input_locked or data == null:
		return

	if event.is_action_pressed("reset_level"):
		_install_level(data)  # reset re-instala mesmo data
		return

	var dir := Vector2i.ZERO
	if event.is_action_pressed("move_up"):    dir = Vector2i(0, -1)
	elif event.is_action_pressed("move_down"):  dir = Vector2i(0, 1)
	elif event.is_action_pressed("move_left"):  dir = Vector2i(-1, 0)
	elif event.is_action_pressed("move_right"): dir = Vector2i(1, 0)

	if dir != Vector2i.ZERO:
		_try_move(dir)

func _try_move(dir: Vector2i) -> void:
	var target := player.grid_pos + dir
	player.facing = dir

	# Parede ou fora do mapa: bloqueia
	if not data.is_walkable(target):
		player.queue_redraw()
		return

	# Tem caixa no destino?
	var box_idx := _box_at(target)
	if box_idx >= 0:
		var beyond := target + dir
		# Caixa não pode ser empurrada pra parede/outra caixa
		if not data.is_walkable(beyond): return
		if _box_at(beyond) >= 0: return
		var b: Box = boxes[box_idx]
		b.grid_pos = beyond
		b.on_goal = goal_set.has(beyond)

	player.grid_pos = target
	player.queue_redraw()
	moves += 1
	_update_hud()
	_check_win()

func _box_at(pos: Vector2i) -> int:
	for i in boxes.size():
		if boxes[i].grid_pos == pos:
			return i
	return -1

func _check_win() -> void:
	for b in boxes:
		if not goal_set.has(b.grid_pos):
			return
	_on_win()

func _on_win() -> void:
	won = true
	var par_txt := " (par %d)" % data.par if data.par > 0 else ""
	win_label.text = "FASE COMPLETA!\n%d movimentos%s" % [moves, par_txt]
	win_panel.visible = true

func _update_hud() -> void:
	hud_label.text = "%s — %d movimentos   [Z=desfazer (TODO)] [R=reiniciar] [ESC=menu]" \
		% [data.title, moves]

func _draw() -> void:
	if data == null:
		return
	for y in data.height:
		for x in data.width:
			var t: int = data.tiles[y][x]
			var rect := Rect2(x * TILE_SIZE, y * TILE_SIZE, TILE_SIZE, TILE_SIZE)
			match t:
				TileConst.Tile.OUTSIDE:
					draw_rect(rect, TileConst.COLOR_OUTSIDE, true)
				TileConst.Tile.FLOOR:
					draw_rect(rect, TileConst.COLOR_FLOOR, true)
					_draw_floor_grout(rect)
				TileConst.Tile.GOAL:
					draw_rect(rect, TileConst.COLOR_FLOOR, true)
					_draw_floor_grout(rect)
					# overlay verde semi-transparente do alvo
					var pad := TILE_SIZE * 0.18
					var goal_rect := Rect2(
						rect.position.x + pad, rect.position.y + pad,
						rect.size.x - 2 * pad, rect.size.y - 2 * pad)
					draw_rect(goal_rect, TileConst.COLOR_GOAL, true)
					draw_rect(goal_rect, TileConst.COLOR_GOAL.darkened(0.3),
						false, 2.0)
				TileConst.Tile.WALL:
					draw_rect(rect, TileConst.COLOR_WALL, true)
					# faixa superior mais clara, ilusão de relevo
					var hi_rect := Rect2(rect.position.x, rect.position.y,
						rect.size.x, rect.size.y * 0.22)
					draw_rect(hi_rect, TileConst.COLOR_WALL_HI, true)
					draw_rect(rect, TileConst.COLOR_WALL.darkened(0.6),
						false, 2.0)

func _draw_floor_grout(rect: Rect2) -> void:
	# linha sutil pra dar grid visual sem virar xadrez
	var col := TileConst.COLOR_FLOOR.darkened(0.18)
	draw_line(Vector2(rect.position.x, rect.position.y + rect.size.y),
		Vector2(rect.position.x + rect.size.x, rect.position.y + rect.size.y),
		col, 1.0)
	draw_line(Vector2(rect.position.x + rect.size.x, rect.position.y),
		Vector2(rect.position.x + rect.size.x, rect.position.y + rect.size.y),
		col, 1.0)
