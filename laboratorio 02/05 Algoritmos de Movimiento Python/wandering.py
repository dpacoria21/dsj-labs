"""Ejemplo II: dos variantes del movimiento Wandering."""

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
MAX_SPEED = 190.0
MAX_FORCE = 260.0
WANDER_DISTANCE = 85.0
WANDER_RADIUS = 55.0
WANDER_JITTER = 95.0

BG = (28, 32, 38)
YELLOW = (255, 210, 60)
GREEN = (80, 220, 120)
RED = (255, 100, 100)
CYAN = (70, 220, 230)
WHITE = (235, 235, 235)
MUTED = (155, 165, 178)


def new_agent(rng: random.Random, position: tuple[float, float] | None = None) -> SteeringAgent:
    position = position or (rng.randint(80, WIDTH - 80), rng.randint(110, HEIGHT - 80))
    return SteeringAgent(position, Vector2(100, 0).rotate(rng.uniform(0, 360)), MAX_SPEED, MAX_FORCE)


def draw_arrow(surface: pg.Surface, color: tuple[int, int, int], start: Vector2, vector: Vector2, scale: float) -> None:
    end = start + vector * scale
    pg.draw.line(surface, color, start, end, 3)


def render(screen: pg.Surface, font: pg.font.Font, agents: list[SteeringAgent], debug: bool) -> None:
    screen.fill(BG)
    for agent in agents:
        heading = safe_direction(agent.velocity)
        points = [agent.position + heading * 18, agent.position + heading.rotate(135) * 13, agent.position + heading.rotate(-135) * 13]
        pg.draw.polygon(screen, YELLOW, points)
        if debug:
            pg.draw.circle(screen, WHITE, agent.wander_center, int(WANDER_RADIUS), 1)
            pg.draw.line(screen, CYAN, agent.wander_center, agent.wander_target, 3)
            pg.draw.circle(screen, CYAN, agent.wander_target, 5)
            draw_arrow(screen, GREEN, agent.position, agent.velocity, 0.28)
            draw_arrow(screen, RED, agent.position, agent.desired, 0.28)
    screen.blit(font.render("WANDERING CONTINUO", True, WHITE), (18, 16))
    screen.blit(font.render("V = vectores | M = nuevo agente | ESPACIO = pausa | ESC = salir", True, MUTED), (18, 44))
    screen.blit(font.render("El objetivo cambia suavemente sobre el circulo proyectado", True, CYAN), (18, HEIGHT - 34))


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture", type=Path, help="Guarda una captura PNG y termina.")
    parser.add_argument("--frames", type=int, default=160)
    args = parser.parse_args()

    pg.init()
    screen = pg.display.set_mode((WIDTH, HEIGHT))
    pg.display.set_caption("LAB02 - Wandering")
    clock = pg.time.Clock()
    font = pg.font.SysFont("consolas", 18)
    rng = random.Random(19)
    agents = [new_agent(rng)]
    if args.capture:
        agents = [new_agent(rng, (180, 160)), new_agent(rng, (690, 170)), new_agent(rng, (220, 430)), new_agent(rng, (670, 430))]

    paused = False
    debug = True
    running = True
    frame = 0
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
        if not paused:
            for agent in agents:
                agent.apply_wander(dt, WANDER_DISTANCE, WANDER_RADIUS, WANDER_JITTER, rng)
                agent.integrate(dt, WIDTH, HEIGHT)
        render(screen, font, agents, debug)
        pg.display.flip()
        frame += 1
        if args.capture and frame >= args.frames:
            args.capture.parent.mkdir(parents=True, exist_ok=True)
            pg.image.save(screen, args.capture)
            running = False
    pg.quit()


if __name__ == "__main__":
    main()
