"""Funciones compartidas para los algoritmos de movimiento del Laboratorio 02."""

from __future__ import annotations

import random

import pygame as pg

Vector2 = pg.math.Vector2


def clamp_vector(vector: Vector2, max_length: float) -> Vector2:
    """Devuelve una copia cuya magnitud no supera ``max_length``."""
    result = Vector2(vector)
    if result.length_squared() > max_length * max_length:
        result.scale_to_length(max_length)
    return result


def safe_direction(vector: Vector2, fallback: Vector2 | None = None) -> Vector2:
    """Normaliza sin lanzar una excepción cuando el vector tiene magnitud cero."""
    if vector.length_squared() > 1e-9:
        return vector.normalize()
    return Vector2(fallback or (1, 0))


def seek(
    position: Vector2,
    velocity: Vector2,
    target: Vector2,
    max_speed: float,
    max_force: float,
) -> tuple[Vector2, Vector2]:
    """Calcula velocidad deseada y steering para perseguir un objetivo."""
    offset = Vector2(target) - position
    desired = safe_direction(offset, (0, 0)) * max_speed if offset else Vector2()
    steering = clamp_vector(desired - velocity, max_force)
    return desired, steering


def seek_with_arrive(
    position: Vector2,
    velocity: Vector2,
    target: Vector2,
    max_speed: float,
    max_force: float,
    slow_radius: float,
) -> tuple[Vector2, Vector2]:
    """Seeking con desaceleración progresiva dentro de ``slow_radius``."""
    offset = Vector2(target) - position
    distance = offset.length()
    if distance < 1e-6:
        desired = Vector2()
    else:
        speed = max_speed * min(1.0, distance / slow_radius)
        desired = offset.normalize() * speed
    steering = clamp_vector(desired - velocity, max_force)
    return desired, steering


def wander_target(
    position: Vector2,
    velocity: Vector2,
    angle: float,
    ring_distance: float,
    ring_radius: float,
) -> tuple[Vector2, Vector2]:
    """Coloca un objetivo sobre un círculo proyectado delante del agente."""
    forward = safe_direction(velocity)
    center = position + forward * ring_distance
    displacement = Vector2(ring_radius, 0).rotate(angle)
    return center + displacement, center


class SteeringAgent:
    """Agente autónomo pequeño usado por los tres ejemplos."""

    def __init__(
        self,
        position: tuple[float, float],
        velocity: tuple[float, float],
        max_speed: float,
        max_force: float,
    ) -> None:
        self.position = Vector2(position)
        self.velocity = Vector2(velocity)
        self.acceleration = Vector2()
        self.desired = Vector2()
        self.max_speed = max_speed
        self.max_force = max_force
        self.wander_angle = random.uniform(0, 360)
        self.wander_target = self.position.copy()
        self.wander_center = self.position.copy()

    def apply_seek(self, target: Vector2, slow_radius: float | None = None) -> None:
        if slow_radius is None:
            self.desired, self.acceleration = seek(
                self.position,
                self.velocity,
                target,
                self.max_speed,
                self.max_force,
            )
        else:
            self.desired, self.acceleration = seek_with_arrive(
                self.position,
                self.velocity,
                target,
                self.max_speed,
                self.max_force,
                slow_radius,
            )

    def apply_wander(
        self,
        dt: float,
        ring_distance: float,
        ring_radius: float,
        jitter: float,
        rng: random.Random,
    ) -> None:
        self.wander_angle += rng.uniform(-jitter, jitter) * dt
        self.wander_target, self.wander_center = wander_target(
            self.position,
            self.velocity,
            self.wander_angle,
            ring_distance,
            ring_radius,
        )
        self.apply_seek(self.wander_target)

    def integrate(self, dt: float, width: int, height: int) -> None:
        self.velocity += self.acceleration * dt
        self.velocity = clamp_vector(self.velocity, self.max_speed)
        self.position += self.velocity * dt
        self.position.x %= width
        self.position.y %= height
