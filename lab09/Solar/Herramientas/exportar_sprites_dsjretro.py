"""Copia los sprites de DSJRetro (proyecto del curso, Godot) a Assets/DSJRetro/Sprites del proyecto Solar.

Godot colorea la cancha con `modulate` y los uniformes con el shader player_palette.gdshader. Unity no usa
esos recursos, así que el script aplica los mismos colores y guarda imágenes listas para usar.

Uso: python exportar_sprites_dsjretro.py <carpeta de djs-retro-arcade-game>
"""
import sys
from pathlib import Path

from PIL import Image

GAME = Path(sys.argv[1] if len(sys.argv) > 1 else "../../../djs-retro-arcade-game")
OUT = Path(__file__).resolve().parent.parent / "Assets" / "DSJRetro" / "Sprites"
ART = GAME / "assets" / "art"

# Colores de scenes/main.tscn (Background/grass, pattern y lines).
GRASS = (0.45490196, 0.85882354, 0.11372549)
PATTERN = (0.28627452, 0.57254905, 0.2784314)
LINES = (0.972549, 1.0, 0.92156863)

# Filas de la paleta usadas en scenes/match/match.tscn: humanos = 1, CPU = 8; piel clara = 0.
TEAM_ROWS = {"JugadorP1": 1, "JugadorCPU": 8}
SKIN_ROW = 0


def tinted(path: Path, color) -> Image.Image:
    image = Image.open(path).convert("RGBA")
    layer = Image.new("RGBA", image.size, tuple(round(c * 255) for c in color) + (255,))
    layer.putalpha(image.getchannel("A"))
    return layer


def pitch() -> Image.Image:
    backgrounds = ART / "backgrounds"
    image = tinted(backgrounds / "pitch-background.png", GRASS)
    image.alpha_composite(tinted(backgrounds / "pitch-pattern.png", PATTERN))
    image.alpha_composite(tinted(backgrounds / "pitch-lines.png", LINES))
    return image


def palette_rows(path: Path):
    image = Image.open(path).convert("RGB")
    width, height = image.size
    return [[image.getpixel((x, y)) for x in range(width)] for y in range(height)]


def recolor(sheet: Image.Image, team_row: int) -> Image.Image:
    """Lo mismo que player_palette.gdshader: la fila 0 de cada paleta son los colores originales."""
    teams = palette_rows(ART / "palettes" / "teams-color-palette.png")
    skins = palette_rows(ART / "palettes" / "skin-colors-palette.png")
    mapping = dict(zip(teams[0], teams[team_row]))
    mapping.update(zip(skins[0], skins[SKIN_ROW]))
    result = sheet.copy()
    pixels = result.load()
    for y in range(result.height):
        for x in range(result.width):
            r, g, b, a = pixels[x, y]
            if a and (r, g, b) in mapping:
                pixels[x, y] = mapping[(r, g, b)] + (a,)
    return result


def net(size: int = 32, spacing: int = 4) -> Image.Image:
    image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    for i in range(0, size, spacing):
        for j in range(size):
            image.putpixel((i, j), (255, 255, 255, 200))
            image.putpixel((j, i), (255, 255, 255, 200))
    return image


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    pitch().save(OUT / "Cancha.png")
    sheet = Image.open(ART / "characters" / "soccer-player.png").convert("RGBA")
    for name, row in TEAM_ROWS.items():
        recolor(sheet, row).save(OUT / f"{name}.png")
    copies = {
        "Balon.png": ART / "props" / "soccer-ball.png",
        "Sombra.png": ART / "characters" / "player-shadow.png",
        "Indicador1P.png": ART / "props" / "1p.png",
        "IndicadorCPU.png": ART / "props" / "cpu.png",
        "Gol.png": ART / "ui" / "goal.png",
    }
    for name, source in copies.items():
        Image.open(source).convert("RGBA").save(OUT / name)
    net().save(OUT / "Red.png")
    print("Sprites exportados en", OUT)


if __name__ == "__main__":
    main()
