class_name SoccerNPC
extends Node2D

var body_color: Color = Color.WHITE
var velocity: Vector2 = Vector2.ZERO
var max_speed: float = 150.0
var max_force: float = 420.0
var radius: float = 18.0
var label_text: String = "NPC"


func _init(color: Color = Color.WHITE, npc_label: String = "NPC") -> void:
	body_color = color
	label_text = npc_label


func seek(target: Vector2, delta: float, speed_multiplier: float = 1.0) -> void:
	# SEEKING: calcula la velocidad deseada hacia el objetivo y aplica steering.
	var offset := target - position
	if offset.length_squared() < 0.01:
		velocity = velocity.move_toward(Vector2.ZERO, max_force * delta)
		return

	var desired_velocity := offset.normalized() * max_speed * speed_multiplier
	var steering := (desired_velocity - velocity).limit_length(max_force)
	velocity = (velocity + steering * delta).limit_length(max_speed * speed_multiplier)
	position += velocity * delta
	queue_redraw()


func keep_inside(field_rect: Rect2) -> void:
	var minimum := field_rect.position + Vector2(radius, radius)
	var maximum := field_rect.end - Vector2(radius, radius)
	var corrected := Vector2(
		clamp(position.x, minimum.x, maximum.x),
		clamp(position.y, minimum.y, maximum.y)
	)

	if corrected.x != position.x:
		velocity.x *= -0.45
	if corrected.y != position.y:
		velocity.y *= -0.45
	position = corrected


func _draw() -> void:
	# Sombra, cuerpo y una pequeña marca que indica hacia dónde mira el NPC.
	draw_circle(Vector2(4, 6), radius, Color(0, 0, 0, 0.28))
	draw_circle(Vector2.ZERO, radius, body_color)
	draw_arc(Vector2.ZERO, radius, 0.0, TAU, 40, Color.WHITE, 2.0)

	var facing := velocity.normalized() if velocity.length_squared() > 4.0 else Vector2.RIGHT
	draw_line(Vector2.ZERO, facing * (radius + 7.0), Color.WHITE, 3.0, true)
