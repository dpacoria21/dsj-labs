"""Ejemplo I: Seeking con Arrive/Approach siguiendo el cursor."""

from __future__ import annotations

import argparse
import os
import random
import sys
from pathlib import Path

if "--capture" in sys.argv:
    os.environ.setdefault("SDL_VIDEODRIVER", "dummy")

import pygame as pg

from movement_core import SteeringAgent, Vector2, safe_direction

WIDTH, HEIGHT = 900, 600
FPS = 60
MAX_SPEED = 240.0
MAX_FORCE = 360.0
APPROACH_RADIUS = 120

BG = (28, 32, 38)
YELLOW = (255, 210, 60)
GREEN = (80, 220, 120)
RED = (255, 100, 100)
CYAN = (70, 220, 230)
WHITE = (235, 235, 235)
MUTED = (155, 165, 178)


def new_agent(rng: random.Random) -> SteeringAgent:
    angle = rng.uniform(0, 360)
    return SteeringAgent(
        (rng.randint(60, WIDTH - 60), rng.randint(110, HEIGHT - 60)),
        Vector2(100, 0).rotate(angle),
        MAX_SPEED,
        MAX_FORCE,
    )


def draw_arrow(surface: pg.Surface, color: tuple[int, int, int], start: Vector2, vector: Vector2, scale: float) -> None:
    end = start + vector * scale
    pg.draw.line(surface, color, start, end, 3)
    if vector.length_squared() > 1:
        direction = safe_direction(vector)
        pg.draw.line(surface, color, end, end - direction.rotate(28) * 10, 3)
        pg.draw.line(surface, color, end, end - direction.rotate(-28) * 10, 3)


def render(screen: pg.Surface, font: pg.font.Font, agents: list[SteeringAgent], target: Vector2, debug: bool) -> None:
    screen.fill(BG)
    pg.draw.circle(screen, CYAN, target, 9)
    pg.draw.circle(screen, CYAN, target, 17, 2)
    if debug:
        pg.draw.circle(screen, WHITE, target, APPROACH_RADIUS, 1)
    for agent in agents:
        heading = safe_direction(agent.velocity)
        points = [agent.position + heading * 18, agent.position + heading.rotate(135) * 13, agent.position + heading.rotate(-135) * 13]
        pg.draw.polygon(screen, YELLOW, points)
        if debug:
            draw_arrow(screen, GREEN, agent.position, agent.velocity, 0.28)
            draw_arrow(screen, RED, agent.position, agent.desired, 0.28)
    title = font.render("SEEKING + ARRIVE", True, WHITE)
    help_text = font.render("Mouse = objetivo | V = vectores | M = nuevo agente | ESPACIO = pausa | ESC = salir", True, MUTED)
    legend = font.render("Verde: velocidad actual   Rojo: velocidad deseada   Cian: objetivo", True, WHITE)
    screen.blit(title, (18, 16))
    screen.blit(help_text, (18, 44))
    screen.blit(legend, (18, HEIGHT - 34))


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", type=Path, help="Guarda una captura PNG y termina.")
    parser.add_argument("--frames", type=int, default=180)
    args = parser.parse_args()

    pg.init()
    screen = pg.display.set_mode((WIDTH, HEIGHT))
    pg.display.set_caption("LAB02 - Seeking")
    clock = pg.time.Clock()
    font = pg.font.SysFont("consolas", 18)
    rng = random.Random(7)
    agents = [new_agent(rng)]
    if args.capture:
        agents = [
            SteeringAgent((140, 170), (80, 45), MAX_SPEED, MAX_FORCE),
            SteeringAgent((170, 430), (110, -20), MAX_SPEED, MAX_FORCE),
            SteeringAgent((710, 470), (-70, -50), MAX_SPEED, MAX_FORCE),
        ]

    paused = False
    debug = True
    running = True
    frame = 0
    target = Vector2(650, 260)
    while running:
        dt = (1 / FPS) if args.capture else min(clock.tick(FPS) / 1000.0, 0.05)
        for event in pg.event.get():
            if event.type == pg.QUIT:
                running = False
            elif event.type == pg.KEYDOWN:
                if event.key == pg.K_ESCAPE:
                    running = False
                elif event.key == pg.K_SPACE:
                    paused = not paused
                elif event.key == pg.K_v:
                    debug = not debug
                elif event.key == pg.K_m:
                    agents.append(new_agent(rng))
        if not args.capture:
            target = Vector2(pg.mouse.get_pos())
        if not paused:
            for agent in agents:
                agent.apply_seek(target, APPROACH_RADIUS)
                agent.integrate(dt, WIDTH, HEIGHT)
        render(screen, font, agents, target, debug)
        pg.display.flip()
        frame += 1
        if args.capture and frame >= args.frames:
            args.capture.parent.mkdir(parents=True, exist_ok=True)
            pg.image.save(screen, args.capture)
            running = False
    pg.quit()


if __name__ == "__main__":
    main()
