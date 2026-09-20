import random

import pygame as pg
import pytest

from movement_core import SteeringAgent, clamp_vector, seek, seek_with_arrive, wander_target


def test_clamp_vector_limits_magnitude() -> None:
    result = clamp_vector(pg.Vector2(30, 40), 10)
    assert result.length() == pytest.approx(10)


def test_seek_points_toward_target_and_limits_force() -> None:
    desired, steering = seek(pg.Vector2(0, 0), pg.Vector2(), pg.Vector2(100, 0), 20, 3)
    assert desired == pg.Vector2(20, 0)
    assert steering.length() == pytest.approx(3)


def test_arrive_reduces_desired_speed_inside_radius() -> None:
    far_desired, _ = seek_with_arrive(pg.Vector2(), pg.Vector2(), pg.Vector2(100, 0), 20, 10, 50)
    near_desired, _ = seek_with_arrive(pg.Vector2(), pg.Vector2(), pg.Vector2(10, 0), 20, 10, 50)
    assert far_desired.length() == pytest.approx(20)
    assert near_desired.length() == pytest.approx(4)


def test_zero_distance_is_safe() -> None:
    desired, steering = seek_with_arrive(pg.Vector2(5, 5), pg.Vector2(), pg.Vector2(5, 5), 20, 10, 50)
    assert desired == pg.Vector2()
    assert steering == pg.Vector2()


def test_wander_target_lies_on_projected_circle() -> None:
    target, center = wander_target(pg.Vector2(10, 20), pg.Vector2(1, 0), 90, 80, 25)
    assert center == pg.Vector2(90, 20)
    assert target.distance_to(center) == pytest.approx(25)


def test_agent_wraps_around_world() -> None:
    agent = SteeringAgent((99, 50), (5, 0), 10, 10)
    agent.acceleration = pg.Vector2()
    agent.integrate(1, 100, 100)
    assert agent.position.x == pytest.approx(4)
