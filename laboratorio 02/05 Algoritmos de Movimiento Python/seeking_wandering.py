"""Ejemplo III: guardia que alterna entre WANDER y SEEK/ARRIVE."""

from __future__ import annotations

import argparse
import os
import random
import sys
from pathlib import Path

if "--capture-dir" in sys.argv:
    os.environ.setdefault("SDL_VIDEODRIVER", "dummy")

import pygame as pg

from movement_core import SteeringAgent, Vector2, safe_direction

WIDTH, HEIGHT = 900, 600
FPS = 60
MAX_SPEED = 220.0
MAX_FORCE = 420.0
DETECTION_RADIUS = 190.0
SLOW_RADIUS = 90.0
WANDER_DISTANCE = 85.0
WANDER_RADIUS = 55.0
WANDER_JITTER = 95.0

BG = (28, 32, 38)
NPC = (255, 210, 60)
PLAYER = (80, 190, 255)
GREEN = (80, 220, 120)
RED = (255, 100, 100)
CYAN = (70, 220, 230)
WHITE = (235, 235, 235)
MUTED = (155, 165, 178)


class Guard(SteeringAgent):
    def __init__(self) -> None:
        super().__init__((WIDTH * 0.35, HEIGHT * 0.5), (90, -20), MAX_SPEED, MAX_FORCE)
        self.state = "WANDER"

    def update(self, dt: float, player: Vector2, rng: random.Random) -> None:
        if self.position.distance_to(player) <= DETECTION_RADIUS:
            self.state = "SEEK"
            self.apply_seek(player, SLOW_RADIUS)
        else:
            self.state = "WANDER"
            self.apply_wander(dt, WANDER_DISTANCE, WANDER_RADIUS, WANDER_JITTER, rng)
        self.integrate(dt, WIDTH, HEIGHT)


def render(screen: pg.Surface, font: pg.font.Font, guard: Guard, player: Vector2, debug: bool) -> None:
    screen.fill(BG)
    pg.draw.circle(screen, PLAYER, player, 10)
    pg.draw.circle(screen, PLAYER, player, 18, 2)
    pg.draw.circle(screen, NPC, guard.position, 14)
    heading = safe_direction(guard.velocity)
    pg.draw.line(screen, WHITE, guard.position, guard.position + heading * 22, 3)
    if debug:
        pg.draw.circle(screen, WHITE, guard.position, int(DETECTION_RADIUS), 1)
        pg.draw.line(screen, GREEN, guard.position, guard.position + guard.velocity * 0.35, 3)
        pg.draw.line(screen, RED, guard.position, guard.position + guard.desired * 0.35, 3)
        if guard.state == "WANDER":
            pg.draw.circle(screen, CYAN, guard.wander_center, int(WANDER_RADIUS), 1)
            pg.draw.circle(screen, CYAN, guard.wander_target, 5)
            pg.draw.line(screen, CYAN, guard.wander_center, guard.wander_target, 2)
        else:
            pg.draw.circle(screen, RED, player, int(SLOW_RADIUS), 1)
    state_color = RED if guard.state == "SEEK" else CYAN
    screen.blit(font.render(f"Estado: {guard.state}", True, state_color), (18, 16))
    screen.blit(font.render("Mouse = jugador | V = debug | ESC = salir", True, WHITE), (18, 44))
    screen.blit(font.render("Circulo blanco: deteccion | Circulo rojo: llegada suave", True, MUTED), (18, HEIGHT - 34))


def capture_state(screen: pg.Surface, font: pg.font.Font, out_dir: Path, seek_state: bool) -> None:
    rng = random.Random(31 if seek_state else 29)
    guard = Guard()
    player = Vector2(430, 330) if seek_state else Vector2(760, 170)
    for _ in range(90):
        guard.update(1 / FPS, player, rng)
    render(screen, font, guard, player, True)
    pg.display.flip()
    filename = "04_combinado_seek.png" if seek_state else "03_combinado_wander.png"
    pg.image.save(screen, out_dir / filename)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--capture-dir", type=Path, help="Guarda capturas de los estados WANDER y SEEK.")
    args = parser.parse_args()

    pg.init()
    screen = pg.display.set_mode((WIDTH, HEIGHT))
    pg.display.set_caption("LAB02 - Seeking + Wandering")
    clock = pg.time.Clock()
    font = pg.font.SysFont("consolas", 18)
    if args.capture_dir:
        args.capture_dir.mkdir(parents=True, exist_ok=True)
        capture_state(screen, font, args.capture_dir, False)
        capture_state(screen, font, args.capture_dir, True)
        pg.quit()
        return

    rng = random.Random()
    guard = Guard()
    debug = True
    running = True
    while running:
        dt = min(clock.tick(FPS) / 1000.0, 0.05)
        for event in pg.event.get():
            if event.type == pg.QUIT:
                running = False
            elif event.type == pg.KEYDOWN:
                if event.key == pg.K_ESCAPE:
                    running = False
                elif event.key == pg.K_v:
                    debug = not debug
        player = Vector2(pg.mouse.get_pos())
        guard.update(dt, player, rng)
        render(screen, font, guard, player, debug)
        pg.display.set_caption(f"LAB02 Seeking + Wandering | {clock.get_fps():.1f} FPS")
        pg.display.flip()
    pg.quit()


if __name__ == "__main__":
    main()
