extends Node2D

const SoccerNPCScript = preload("res://npc.gd")

const FIELD_RECT := Rect2(70.0, 100.0, 820.0, 370.0)
const BLUE := Color("3297ff")
const RED := Color("ff5364")
const BALL_COLOR := Color("ffd43b")
const STEAL_DISTANCE := 35.0
const STEAL_COOLDOWN := 0.85

var players: Array[SoccerNPC] = []
var possession_index := 0
var ball_position := Vector2.ZERO
var steal_cooldown := 0.0
var blue_score := 0
var red_score := 0
var paused := false
var capture_directory := ""
var capture_elapsed := 0.0
var capture_count := 0
var capture_busy := false
var capture_after_steal := false

var score_label: Label
var state_label: Label


func _ready() -> void:
	create_interface()
	create_players()
	restart_round(0)
	configure_capture_mode()
	queue_redraw()


func create_players() -> void:
	var blue_player := SoccerNPCScript.new(BLUE, "AZUL")
	var red_player := SoccerNPCScript.new(RED, "ROJO")
	add_child(blue_player)
	add_child(red_player)
	players = [blue_player, red_player]

	# El perseguidor es ligeramente más rápido para que pueda recuperar la pelota.
	blue_player.max_speed = 155.0
	red_player.max_speed = 155.0


func create_interface() -> void:
	var title := Label.new()
	title.position = Vector2(70, 22)
	title.text = "NPC MOVEMENT · FÚTBOL CON SEEKING"
	title.add_theme_font_size_override("font_size", 24)
	title.add_theme_color_override("font_color", Color.WHITE)
	add_child(title)

	score_label = Label.new()
	score_label.position = Vector2(692, 26)
	score_label.add_theme_font_size_override("font_size", 20)
	score_label.add_theme_color_override("font_color", BALL_COLOR)
	add_child(score_label)

	state_label = Label.new()
	state_label.position = Vector2(70, 492)
	state_label.add_theme_font_size_override("font_size", 16)
	state_label.add_theme_color_override("font_color", Color("d7e9dd"))
	add_child(state_label)

	var help := Label.new()
	help.position = Vector2(694, 496)
	help.text = "ESPACIO: pausa   R: reiniciar"
	help.add_theme_font_size_override("font_size", 13)
	help.add_theme_color_override("font_color", Color("a8c7b2"))
	add_child(help)


func _process(delta: float) -> void:
	if paused:
		return

	steal_cooldown = maxf(0.0, steal_cooldown - delta)

	var holder := players[possession_index]
	var chaser := players[1 - possession_index]
	var attack_goal := get_attack_goal(possession_index)

	# Quien tiene la pelota busca la portería rival a velocidad moderada.
	holder.seek(attack_goal, delta, 0.72)
	update_ball_position(holder)

	# El rival aplica el mismo algoritmo Seeking directamente hacia la pelota.
	chaser.seek(ball_position, delta, 1.08)

	for player in players:
		player.keep_inside(FIELD_RECT)

	if steal_cooldown <= 0.0 and chaser.position.distance_to(ball_position) <= STEAL_DISTANCE:
		change_possession()

	check_goal()
	update_interface()
	queue_redraw()
	handle_capture_mode(delta)


func update_ball_position(holder: SoccerNPC) -> void:
	var facing := holder.velocity.normalized()
	if facing.length_squared() < 0.01:
		facing = Vector2.RIGHT if possession_index == 0 else Vector2.LEFT
	ball_position = holder.position + facing * 25.0


func change_possession() -> void:
	possession_index = 1 - possession_index
	steal_cooldown = STEAL_COOLDOWN

	var new_holder := players[possession_index]
	var escape_direction := Vector2.RIGHT if possession_index == 0 else Vector2.LEFT
	new_holder.velocity = escape_direction * new_holder.max_speed
	update_ball_position(new_holder)
	if capture_directory != "" and capture_count == 1:
		capture_after_steal = true


func check_goal() -> void:
	var holder := players[possession_index]
	var scored := false

	if possession_index == 0 and holder.position.x >= FIELD_RECT.end.x - 18.0:
		blue_score += 1
		scored = true
	elif possession_index == 1 and holder.position.x <= FIELD_RECT.position.x + 18.0:
		red_score += 1
		scored = true

	if scored:
		restart_round(1 - possession_index)


func restart_round(starting_player: int = 0) -> void:
	possession_index = starting_player
	steal_cooldown = 1.0
	players[0].position = Vector2(360, 285)
	players[1].position = Vector2(600, 285)
	players[0].velocity = Vector2.RIGHT * 40.0
	players[1].velocity = Vector2.LEFT * 40.0
	update_ball_position(players[possession_index])
	update_interface()


func get_attack_goal(player_index: int) -> Vector2:
	if player_index == 0:
		return Vector2(FIELD_RECT.end.x + 35.0, FIELD_RECT.get_center().y)
	return Vector2(FIELD_RECT.position.x - 35.0, FIELD_RECT.get_center().y)


func update_interface() -> void:
	score_label.text = "AZUL %d  ·  %d ROJO" % [blue_score, red_score]
	var owner_name := "AZUL" if possession_index == 0 else "ROJO"
	state_label.text = "Pelota: %s  |  El rival persigue la pelota con Seeking" % owner_name


func configure_capture_mode() -> void:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--capture-dir="):
			capture_directory = argument.trim_prefix("--capture-dir=")
			DirAccess.make_dir_recursive_absolute(capture_directory)


func handle_capture_mode(delta: float) -> void:
	if capture_directory == "" or capture_busy:
		return

	capture_elapsed += delta
	if capture_count == 0 and capture_elapsed >= 0.35:
		capture_busy = true
		capture_frame("01_posesion_inicial.png")
	elif capture_count == 1 and capture_after_steal:
		capture_busy = true
		capture_after_steal = false
		capture_frame("02_robo_de_pelota.png")


func capture_frame(file_name: String) -> void:
	await RenderingServer.frame_post_draw
	var image := get_viewport().get_texture().get_image()
	var output_path := capture_directory.path_join(file_name)
	var error := image.save_png(output_path)
	if error != OK:
		push_error("No se pudo guardar la captura: %s" % output_path)
	capture_count += 1
	capture_busy = false
	if capture_count >= 2:
		get_tree().quit()


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		if event.keycode == KEY_SPACE:
			paused = not paused
			state_label.text = "PAUSA" if paused else state_label.text
		elif event.keycode == KEY_R:
			blue_score = 0
			red_score = 0
			restart_round(0)


func _draw() -> void:
	# Cancha y líneas principales.
	draw_rect(FIELD_RECT, Color("176b3a"), true)
	draw_rect(FIELD_RECT, Color("d7f4dd"), false, 4.0)
	draw_line(
		Vector2(FIELD_RECT.get_center().x, FIELD_RECT.position.y),
		Vector2(FIELD_RECT.get_center().x, FIELD_RECT.end.y),
		Color("d7f4dd"), 3.0
	)
	draw_arc(FIELD_RECT.get_center(), 58.0, 0.0, TAU, 64, Color("d7f4dd"), 3.0)
	draw_circle(FIELD_RECT.get_center(), 4.0, Color("d7f4dd"))

	# Porterías.
	var left_goal := Rect2(FIELD_RECT.position.x - 18.0, FIELD_RECT.get_center().y - 62.0, 18.0, 124.0)
	var right_goal := Rect2(FIELD_RECT.end.x, FIELD_RECT.get_center().y - 62.0, 18.0, 124.0)
	draw_rect(left_goal, Color("d7f4dd"), false, 3.0)
	draw_rect(right_goal, Color("d7f4dd"), false, 3.0)

	# Pelota: punto más pequeño que los personajes.
	draw_circle(ball_position + Vector2(3, 4), 9.0, Color(0, 0, 0, 0.3))
	draw_circle(ball_position, 8.0, BALL_COLOR)
	draw_arc(ball_position, 8.0, 0.0, TAU, 24, Color.WHITE, 1.5)
