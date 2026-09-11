"""Carga de imágenes y sonidos; mantiene los recursos provisionales como respaldo."""

from pathlib import Path
import pygame

BASE_DIR = Path(__file__).resolve().parent
IMAGES_DIR = BASE_DIR / "src" / "imagenes"
SOUNDS_DIR = BASE_DIR / "src" / "sonidos"


def load_image(name, size):
    """Carga el recurso del usuario o dibuja una figura provisional."""
    path = IMAGES_DIR / name
    if path.is_file():
        try:
            return pygame.transform.smoothscale(
                pygame.image.load(str(path)).convert_alpha(), size
            )
        except (pygame.error, OSError) as error:
            print(f"No se pudo cargar {path.name}: {error}. Se usará una figura provisional.")

    surface = pygame.Surface(size, pygame.SRCALPHA)
    w, h = size
    if name == "background.jpg":
        surface.fill((18, 12, 38))
    elif name == "bullet.png":
        pygame.draw.ellipse(surface, (255, 235, 130), (12, 4, 8, 24))
    elif name == "enemy.png":
        pygame.draw.rect(surface, (255, 170, 30), (8, 20, 48, 28))
        for x in (16, 40):
            pygame.draw.rect(surface, (18, 12, 38), (x, 28, 8, 8))
        pygame.draw.line(surface, (255, 170, 30), (8, 12), (20, 24), 4)
        pygame.draw.line(surface, (255, 170, 30), (56, 12), (44, 24), 4)
    else:
        pygame.draw.polygon(surface, (120, 210, 255),
                            [(w // 2, 2), (w - 4, h - 6), (w // 2, h - 16), (4, h - 6)])
    return surface


def load_sound(name):
    path = SOUNDS_DIR / name
    if pygame.mixer.get_init() and path.is_file():
        try:
            return pygame.mixer.Sound(str(path))
        except (pygame.error, OSError) as error:
            print(f"No se pudo cargar {name}: {error}")
    return None


