class_name LevelData extends RefCounted

# Metadata (frontmatter YAML opcional)
var title: String = "Untitled"
var author: String = ""
var par: int = 0
var difficulty: int = 1

# Dimensions
var width: int = 0
var height: int = 0

# Static map: tiles[y][x] -> TileConst.Tile
var tiles: Array = []

# Dynamic state at load
var goals: Array = []                  # Array[Vector2i]
var boxes_init: Array = []             # Array[Vector2i] — posição inicial das caixas
var player_init: Vector2i = Vector2i(-1, -1)

# Diagnostics
var parse_errors: Array[String] = []

func is_valid() -> bool:
	return parse_errors.is_empty() \
		and width > 0 and height > 0 \
		and player_init != Vector2i(-1, -1) \
		and boxes_init.size() > 0 \
		and goals.size() == boxes_init.size()

func tile_at(pos: Vector2i) -> int:
	if pos.x < 0 or pos.y < 0 or pos.x >= width or pos.y >= height:
		return TileConst.Tile.OUTSIDE
	return tiles[pos.y][pos.x]

func is_walkable(pos: Vector2i) -> bool:
	var t := tile_at(pos)
	return t == TileConst.Tile.FLOOR or t == TileConst.Tile.GOAL
