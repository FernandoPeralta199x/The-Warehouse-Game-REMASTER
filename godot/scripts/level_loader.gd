class_name LevelLoader extends RefCounted

# Parser XSB com frontmatter YAML opcional. Veja ADR-002.
#
# Formato esperado:
#
#   ---
#   title: "Stage 1"
#   author: "fguta"
#   par: 23
#   difficulty: 1
#   ---
#   ########
#   #@ $  .#
#   ########
#
# Caracteres da grid:
#   #  parede
#   (espaço) piso
#   .  alvo
#   $  caixa
#   *  caixa em cima de alvo
#   @  jogador
#   +  jogador em cima de alvo
#   -  fora do mapa (opcional; tratado como OUTSIDE)
#
# Limitações Sprint 1:
#  - apenas 1 fase por arquivo (XSB tradicional permite várias separadas
#    por linha em branco — pós-MVP)
#  - frontmatter YAML implementado pra subset: chave: valor escalar
#    (string com aspas opcional, int, sem listas/objetos aninhados)

static func from_file(path: String) -> LevelData:
	if not FileAccess.file_exists(path):
		var ld := LevelData.new()
		ld.parse_errors.append("File not found: %s" % path)
		return ld
	var f := FileAccess.open(path, FileAccess.READ)
	var text := f.get_as_text()
	return from_string(text, path)

static func from_string(text: String, source_hint: String = "<string>") -> LevelData:
	var data := LevelData.new()
	var lines := text.split("\n", false)

	var i := 0
	# Skip leading blanks
	while i < lines.size() and lines[i].strip_edges().is_empty():
		i += 1

	# Frontmatter
	if i < lines.size() and lines[i].strip_edges() == "---":
		i += 1
		while i < lines.size() and lines[i].strip_edges() != "---":
			_parse_frontmatter_line(lines[i], data)
			i += 1
		if i < lines.size():
			i += 1  # skip closing ---
		# skip blank lines after frontmatter
		while i < lines.size() and lines[i].strip_edges().is_empty():
			i += 1

	# Grid
	var grid_lines: Array[String] = []
	while i < lines.size():
		var line: String = lines[i]
		# trim trailing CR (Windows) but preserve leading spaces
		line = line.trim_suffix("\r")
		# blank line ends the grid (next fase, ignorada por enquanto)
		if line.strip_edges().is_empty():
			break
		grid_lines.append(line)
		i += 1

	if grid_lines.is_empty():
		data.parse_errors.append("%s: empty grid" % source_hint)
		return data

	# Compute width = max line length
	var w := 0
	for gl in grid_lines:
		if gl.length() > w:
			w = gl.length()
	data.width = w
	data.height = grid_lines.size()

	# Allocate tiles[][] defaulting to OUTSIDE
	data.tiles = []
	for y in data.height:
		var row: Array = []
		row.resize(w)
		row.fill(TileConst.Tile.OUTSIDE)
		data.tiles.append(row)

	# Walk each cell
	var player_count := 0
	for y in data.height:
		var gl: String = grid_lines[y]
		for x in gl.length():
			var ch := gl[x]
			match ch:
				TileConst.CHAR_WALL:
					data.tiles[y][x] = TileConst.Tile.WALL
				TileConst.CHAR_FLOOR:
					data.tiles[y][x] = TileConst.Tile.FLOOR
				TileConst.CHAR_GOAL:
					data.tiles[y][x] = TileConst.Tile.GOAL
					data.goals.append(Vector2i(x, y))
				TileConst.CHAR_BOX:
					data.tiles[y][x] = TileConst.Tile.FLOOR
					data.boxes_init.append(Vector2i(x, y))
				TileConst.CHAR_BOX_ON_GOAL:
					data.tiles[y][x] = TileConst.Tile.GOAL
					data.goals.append(Vector2i(x, y))
					data.boxes_init.append(Vector2i(x, y))
				TileConst.CHAR_PLAYER:
					data.tiles[y][x] = TileConst.Tile.FLOOR
					data.player_init = Vector2i(x, y)
					player_count += 1
				TileConst.CHAR_PLAYER_ON_GOAL:
					data.tiles[y][x] = TileConst.Tile.GOAL
					data.goals.append(Vector2i(x, y))
					data.player_init = Vector2i(x, y)
					player_count += 1
				TileConst.CHAR_OUTSIDE:
					data.tiles[y][x] = TileConst.Tile.OUTSIDE
				_:
					data.parse_errors.append("%s: unknown char '%s' at (%d,%d)" % [source_hint, ch, x, y])

	# Sanity checks
	if player_count == 0:
		data.parse_errors.append("%s: no player (@) found" % source_hint)
	elif player_count > 1:
		data.parse_errors.append("%s: %d players found, expected 1" % [source_hint, player_count])
	if data.boxes_init.is_empty():
		data.parse_errors.append("%s: no boxes ($) found" % source_hint)
	if data.boxes_init.size() != data.goals.size():
		data.parse_errors.append("%s: %d boxes vs %d goals (must match)" % \
			[source_hint, data.boxes_init.size(), data.goals.size()])

	return data

static func _parse_frontmatter_line(line: String, data: LevelData) -> void:
	var stripped := line.strip_edges()
	if stripped.is_empty() or stripped.begins_with("#"):
		return
	var colon := stripped.find(":")
	if colon < 0:
		return
	var key := stripped.substr(0, colon).strip_edges()
	var raw_val := stripped.substr(colon + 1).strip_edges()
	# Strip quotes
	if (raw_val.begins_with("\"") and raw_val.ends_with("\"")) \
	   or (raw_val.begins_with("'") and raw_val.ends_with("'")):
		raw_val = raw_val.substr(1, raw_val.length() - 2)
	match key:
		"title":      data.title = raw_val
		"author":     data.author = raw_val
		"par":        data.par = int(raw_val)
		"difficulty": data.difficulty = int(raw_val)
		_: pass  # ignora chaves desconhecidas (forward-compat)
