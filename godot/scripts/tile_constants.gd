class_name TileConst extends RefCounted

# XSB grid characters (ADR-002)
const CHAR_WALL := "#"
const CHAR_FLOOR := " "
const CHAR_GOAL := "."
const CHAR_BOX := "$"
const CHAR_BOX_ON_GOAL := "*"
const CHAR_PLAYER := "@"
const CHAR_PLAYER_ON_GOAL := "+"
const CHAR_OUTSIDE := "-"  # alguns dialetos usam '-' para "fora do mapa"

# Tile kinds (estáticos no nível)
enum Tile {
	OUTSIDE = 0,   # cell vazio fora das paredes
	FLOOR = 1,
	WALL = 2,
	GOAL = 3,
}

# Cores temporárias do walking skeleton (Sprint 5 substitui por sprites)
const COLOR_BG       := Color(0.10, 0.10, 0.14)   # fundo
const COLOR_OUTSIDE  := Color(0.08, 0.08, 0.10)   # cell fora do mapa
const COLOR_FLOOR    := Color(0.20, 0.20, 0.26)
const COLOR_WALL     := Color(0.55, 0.45, 0.32)
const COLOR_WALL_HI  := Color(0.70, 0.58, 0.42)   # highlight superior
const COLOR_GOAL     := Color(0.36, 0.79, 0.34, 0.55)
const COLOR_PLAYER   := Color(0.42, 0.68, 0.93)
const COLOR_BOX      := Color(0.77, 0.60, 0.34)
const COLOR_BOX_OK   := Color(0.55, 0.85, 0.45)   # caixa em cima de alvo
