extends Control

const GAME_SCENE := "res://scenes/game.tscn"

@onready var play_btn: Button = $VBox/PlayButton
@onready var quit_btn: Button = $VBox/QuitButton

func _ready() -> void:
	play_btn.pressed.connect(_on_play)
	quit_btn.pressed.connect(_on_quit)
	play_btn.grab_focus()

func _on_play() -> void:
	get_tree().change_scene_to_file(GAME_SCENE)

func _on_quit() -> void:
	get_tree().quit()

func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and event.keycode == KEY_ESCAPE:
		_on_quit()
