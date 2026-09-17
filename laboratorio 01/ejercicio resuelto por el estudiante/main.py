"""Laboratorio 02: ataques de colores, escudo y energía absorbida."""

import pygame
import math
import random
from pygame import mixer

from recursos import load_image, load_sound, SOUNDS_DIR


# Initialize the pygame / Create the screen
pygame.init()
screen = pygame.display.set_mode((800, 600))

# Background / Background Sound
background = load_image("background.jpg", (800, 600))
if mixer.get_init() and (SOUNDS_DIR / "background.wav").is_file():
    try:
        mixer.music.load(str(SOUNDS_DIR / "background.wav"))
        mixer.music.play(-1)
    except (pygame.error, OSError) as error:
        print(f"No se pudo reproducir la música: {error}")

# Title and icon
pygame.display.set_caption("Space Invaders - Laboratorio 02")
pygame.display.set_icon(load_image("ufo.png", (32, 32)))

# Player
PLAYER_SIZE = 40
PLAYER_HALF = PLAYER_SIZE / 2
playerImg = pygame.transform.rotate(load_image("player.png", (PLAYER_SIZE, PLAYER_SIZE)), -90)
PLAYER_SPEED = 300  # Píxeles por segundo; permite interceptar las bolas.
playerX = 90.0
playerY = 310

# Enemy: cada monstruo conserva su color al reaparecer.
COLORS = {"rojo": (255, 85, 105), "azul": (65, 170, 255)}
enemyImg = load_image("enemy.png", (64, 64))
# Colorea la silueta en memoria y conserva la transparencia del archivo original.
enemyImages = {}
for color_name, color_rgb in COLORS.items():
    colored_image = enemyImg.copy()
    for px in range(colored_image.get_width()):
        for py in range(colored_image.get_height()):
            original = enemyImg.get_at((px, py))
            brightness = max(original.r, original.g, original.b) / 255
            colored_image.set_at((px, py),
                                 (*[round(c * brightness) for c in color_rgb], original.a))
    enemyImages[color_name] = colored_image
enemies = []
waveNumber = 0
waveTime = 0.0
wavePending = 0
waveSpawnTimer = 0.0
waveRest = 0.0
backgroundOffset = 0.0
WAVE_SIZE = 6
WAVE_PATTERNS = ("Serpiente", "Arco", "Ondas cruzadas")

# Bullet: disparo normal del jugador.
bulletImg = pygame.transform.rotate(load_image("bullet.png", (8, 22)), -90)
bulletX = 0.0
bulletY = 310.0
bullet_state = "ready"
bullet_Sound = load_sound("laser.wav")
explosion_Sound = load_sound("explosion.wav")

# Enemy attacks / Shield / Special attack
enemyAttacks = []
BALL_RADIUS = 9
BALL_SPEED = 180
SHIELD_RADIUS = 26
# Incluye el destello del escudo y evita las barras de información.
PLAYER_MARGIN = SHIELD_RADIUS + 5 - PLAYER_HALF
PLAYER_MIN_X = PLAYER_MARGIN
PLAYER_MAX_X = 800 - PLAYER_SIZE - PLAYER_MARGIN
PLAYER_MIN_Y = 94 + PLAYER_MARGIN
PLAYER_MAX_Y = 572 - PLAYER_SIZE - PLAYER_MARGIN
MAX_LIVES = 3
ENERGY_REQUIRED = 5
shieldColor = "azul"
shieldEnergy = 0
lives = MAX_LIVES
damageCooldown = 0.0
specialFlash = 0.0
absorbFlash = 0.0

# Score / Game Over text
score_value = 0
font = pygame.font.Font("freesansbold.ttf", 22)
small_font = pygame.font.Font("freesansbold.ttf", 16)
over_font = pygame.font.Font("freesansbold.ttf", 56)
game_over = False


def reset_game():
    """Reinicia también bolas, energía, efectos y temporizadores."""
    global playerX, playerY, bulletX, bulletY, bullet_state, score_value, game_over
    global shieldColor, shieldEnergy, lives, damageCooldown, specialFlash, absorbFlash
    playerX = 90.0
    playerY = 310.0
    bulletX, bulletY, bullet_state = 0.0, playerY, "ready"
    score_value, game_over = 0, False
    shieldColor, shieldEnergy, lives = "azul", 0, MAX_LIVES
    damageCooldown = specialFlash = absorbFlash = 0.0
    enemyAttacks.clear()
    enemies.clear()
    global waveNumber, waveTime, wavePending, waveSpawnTimer, waveRest, backgroundOffset
    waveNumber = 0
    waveTime = waveSpawnTimer = waveRest = backgroundOffset = 0.0
    wavePending = 0
    start_wave()


def start_wave():
    global waveNumber, wavePending, waveSpawnTimer, waveTime
    waveNumber += 1
    wavePending = WAVE_SIZE
    waveSpawnTimer = waveTime = 0.0


def spawn_enemy():
    index = WAVE_SIZE - wavePending
    # Grupos de tres rojos y tres azules; el primer color alterna por oleada.
    color = "rojo" if (index // 3 + waveNumber) % 2 else "azul"
    enemies.append({"x": 810.0, "y": 300.0, "age": 0.0, "color": color,
                    "pattern": (waveNumber - 1) % len(WAVE_PATTERNS),
                    "phase": index * 0.38, "lane": -1 if index % 2 else 1,
                    "speed": min(155, 100 + (waveNumber - 1) * 5),
                    "shoot": random.uniform(0.65, 1.2)})


def update_waves(dt):
    global wavePending, waveSpawnTimer, waveTime, waveRest
    waveTime += dt
    if wavePending:
        waveSpawnTimer -= dt
        if waveSpawnTimer <= 0:
            spawn_enemy()
            wavePending -= 1
            waveSpawnTimer += 0.65
    for monster in enemies[:]:
        monster["age"] += dt
        monster["x"] -= monster["speed"] * dt
        t = monster["age"]
        if monster["pattern"] == 0:
            monster["y"] = 300 + 125 * math.sin(t * 1.65 + monster["phase"])
        elif monster["pattern"] == 1:
            monster["y"] = 300 + monster["lane"] * 155 * math.sin(min(1, t / 8) * math.pi)
        else:
            monster["y"] = 300 + monster["lane"] * (100 * math.sin(t * 2.1) + 45 * math.sin(t * 0.8))
        if monster["x"] < -64:
            enemies.remove(monster)
            continue
        # Los enemigos no disparan mientras están fuera del escenario.
        if 0 <= monster["x"] <= 736:
            monster["shoot"] -= dt
            if monster["shoot"] <= 0:
                fire_enemy_attack(monster)
                monster["shoot"] = random.uniform(1.3, 2.2)
    if wavePending == 0 and not enemies:
        waveRest += dt
        if waveRest >= 1.5:
            waveRest = 0.0
            start_wave()


def show_score(x, y):
    screen.blit(font.render(f"Puntaje: {score_value}", True, (230, 240, 255)), (x, y))


def game_over_text():
    overlay = pygame.Surface((800, 600), pygame.SRCALPHA)
    overlay.fill((5, 8, 20, 210))
    screen.blit(overlay, (0, 0))
    for text, face, y in [("GAME OVER", over_font, 235),
                          (f"Puntaje final: {score_value}", font, 310),
                          ("R: volver a jugar   |   Esc: salir", small_font, 355)]:
        rendered = face.render(text, True, (240, 245, 255))
        screen.blit(rendered, rendered.get_rect(center=(400, y)))


def player(x, y):
    # El contorno coloreado sigue siendo visible durante la protección por daño.
    color = COLORS[shieldColor]
    center = (round(x + PLAYER_HALF), round(y + PLAYER_HALF))
    pygame.draw.circle(screen, color, center, SHIELD_RADIUS, 3)
    if absorbFlash > 0:
        pygame.draw.circle(screen, (235, 255, 255), center, SHIELD_RADIUS + 5, 2)
    if damageCooldown <= 0 or int(damageCooldown * 12) % 2 == 0:
        screen.blit(playerImg, (x, y))


def enemy(monster):
    # Solo el monstruo lleva color; no tiene escudo.
    screen.blit(enemyImages[monster["color"]], (monster["x"], monster["y"]))


def fire_bullet():
    global bulletX, bulletY, bullet_state
    if game_over or bullet_state != "ready":
        return
    bulletX, bulletY = playerX + PLAYER_SIZE, playerY + (PLAYER_SIZE - bulletImg.get_height()) / 2
    bullet_state = "fire"
    if bullet_Sound:
        bullet_Sound.play()


def isCollision(x1, y1, x2, y2, radius):
    return math.hypot(x1 - x2, y1 - y2) <= radius


def change_shield():
    global shieldColor
    if not game_over:
        shieldColor = "rojo" if shieldColor == "azul" else "azul"


def fire_enemy_attack(monster):
    # Apunta hacia el jugador al disparar; la bola no lo persigue después.
    dx = min(-1.0, playerX + PLAYER_HALF - monster["x"])
    dy = playerY + PLAYER_HALF - (monster["y"] + 32)
    angle = max(-math.pi / 3, min(math.pi / 3, math.atan2(dy, -dx)))
    angle += math.radians(random.choice((-18, 0, 18)))
    enemyAttacks.append({"x": monster["x"], "y": monster["y"] + 32,
                         "vx": -math.cos(angle) * BALL_SPEED,
                         "vy": math.sin(angle) * BALL_SPEED,
                         "color": monster["color"]})


def special_attack():
    """Consume cinco absorciones y libera una descarga en toda la pantalla."""
    global shieldEnergy, score_value, specialFlash, bullet_state
    if game_over or shieldEnergy < ENERGY_REQUIRED:
        return False
    shieldEnergy = 0
    specialFlash = 0.45
    visible = [m for m in enemies if -64 < m["x"] < 800]
    score_value += len(visible)
    for monster in visible:
        enemies.remove(monster)
    enemyAttacks.clear()
    bullet_state = "ready"
    if explosion_Sound:
        explosion_Sound.play()
    return True


def update_attacks(dt):
    global shieldEnergy, lives, damageCooldown, absorbFlash, game_over
    for ball in enemyAttacks[:]:
        ball["x"] += ball["vx"] * dt
        ball["y"] += ball["vy"] * dt
        if isCollision(ball["x"], ball["y"], playerX + PLAYER_HALF, playerY + PLAYER_HALF,
                       SHIELD_RADIUS + BALL_RADIUS):
            enemyAttacks.remove(ball)
            if ball["color"] == shieldColor:
                shieldEnergy = min(ENERGY_REQUIRED, shieldEnergy + 1)
                absorbFlash = 0.18
            elif damageCooldown <= 0:
                lives -= 1
                damageCooldown = 1.0
                if explosion_Sound:
                    explosion_Sound.play()
                if lives <= 0:
                    game_over = True
                    break
        elif (ball["y"] - BALL_RADIUS > 572 or ball["y"] + BALL_RADIUS < 94
              or ball["x"] + BALL_RADIUS < 0 or ball["x"] - BALL_RADIUS > 800):
            enemyAttacks.remove(ball)


def update_game(dt, direction_x, direction_y=0):
    global playerX, playerY, bulletX, bulletY, bullet_state, score_value, game_over
    global damageCooldown, specialFlash, absorbFlash, backgroundOffset, lives
    if game_over:
        return
    backgroundOffset = (backgroundOffset + 40 * dt) % 800
    damageCooldown = max(0.0, damageCooldown - dt)
    specialFlash = max(0.0, specialFlash - dt)
    absorbFlash = max(0.0, absorbFlash - dt)

    # Checking for boundaries of spaceship
    # Normalizar impide que el movimiento diagonal sea más rápido.
    direction = pygame.Vector2(direction_x, direction_y)
    if direction.length_squared() > 1:
        direction = direction.normalize()
    playerX = max(PLAYER_MIN_X, min(PLAYER_MAX_X, playerX + direction.x * PLAYER_SPEED * dt))
    playerY = max(PLAYER_MIN_Y, min(PLAYER_MAX_Y, playerY + direction.y * PLAYER_SPEED * dt))

    # Oleadas de derecha a izquierda, con trayectorias curvas.
    update_waves(dt)

    # Bullet movement / Collision / Score: el jugador dispara a la derecha.
    if bullet_state == "fire":
        bulletX += 600 * dt
        bullet_rect = pygame.Rect(round(bulletX), round(bulletY), *bulletImg.get_size())
        for monster in enemies[:]:
            if bullet_rect.colliderect(pygame.Rect(round(monster["x"]), round(monster["y"]), 64, 64)):
                bullet_state = "ready"
                score_value += 1
                enemies.remove(monster)
                if explosion_Sound:
                    explosion_Sound.play()
                break
        if bulletX > 800:
            bullet_state = "ready"

    # Chocar con un monstruo daña; solo sus bolas pueden absorberse.
    for monster in enemies[:]:
        if isCollision(playerX + PLAYER_HALF, playerY + PLAYER_HALF,
                       monster["x"] + 32, monster["y"] + 32, PLAYER_HALF + 23):
            enemies.remove(monster)
            if damageCooldown <= 0:
                lives -= 1
                damageCooldown = 1.0
                if explosion_Sound:
                    explosion_Sound.play()
                if lives <= 0:
                    game_over = True
                    return

    # Colored attacks / Absorption / Damage
    update_attacks(dt)


def draw_game():
    screen.blit(background, (-round(backgroundOffset), 0))
    screen.blit(background, (800 - round(backgroundOffset), 0))
    for monster in enemies:
        enemy(monster)
    for ball in enemyAttacks:
        center = (round(ball["x"]), round(ball["y"]))
        pygame.draw.circle(screen, COLORS[ball["color"]], center, BALL_RADIUS)
        pygame.draw.circle(screen, (250, 250, 255), center, BALL_RADIUS, 1)
    if bullet_state == "fire":
        screen.blit(bulletImg, (bulletX, bulletY))
    player(playerX, playerY)

    if specialFlash > 0:
        effect = pygame.Surface((800, 600), pygame.SRCALPHA)
        effect.fill((*COLORS[shieldColor], int(110 * specialFlash / 0.45)))
        screen.blit(effect, (0, 0))
        radius = int((1 - specialFlash / 0.45) * 900) + 40
        pygame.draw.circle(screen, (225, 245, 255), (round(playerX + PLAYER_HALF), round(playerY + PLAYER_HALF)), radius, 5)

    # Información y controles siempre visibles.
    pygame.draw.rect(screen, (9, 15, 31), (0, 0, 800, 94))
    show_score(18, 12)
    wave_label = small_font.render(f"Oleada {waveNumber}", True, (235, 245, 255))
    screen.blit(wave_label, (680, 16))
    screen.blit(font.render(f"Vidas: {lives}", True, (240, 245, 255)), (240, 12))
    screen.blit(font.render(f"Escudo: {shieldColor.upper()}", True, COLORS[shieldColor]), (415, 12))
    energy_text = "ESPECIAL LISTO [X]" if shieldEnergy == ENERGY_REQUIRED else f"Energía: {shieldEnergy}/{ENERGY_REQUIRED}"
    screen.blit(small_font.render(energy_text, True, (235, 245, 255)), (18, 52))
    for i in range(ENERGY_REQUIRED):
        pygame.draw.rect(screen, COLORS[shieldColor] if i < shieldEnergy else (40, 50, 70),
                         (210 + i * 34, 51, 28, 18), border_radius=4)
    hint = small_font.render(f"Ruta: {WAVE_PATTERNS[(waveNumber - 1) % 3]}", True, (185, 200, 220))
    screen.blit(hint, (415, 53))
    pygame.draw.rect(screen, (9, 15, 31), (0, 572, 800, 28))
    hint = small_font.render("WASD: mover   Espacio: disparar   C: escudo   X: especial   Esc: salir", True, (220, 230, 245))
    screen.blit(hint, hint.get_rect(center=(400, 586)))
    if game_over:
        game_over_text()


# Game Loop
def main():
    reset_game()
    clock = pygame.time.Clock()
    running = True
    accumulator = 0.0
    step = 1 / 120
    try:
        while running:
            accumulator += min(clock.tick(60) / 1000, 0.1)
            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    running = False
                elif event.type == pygame.KEYDOWN:
                    if event.key == pygame.K_ESCAPE:
                        running = False
                    elif event.key == pygame.K_r and game_over:
                        reset_game()
                        accumulator = 0.0
                    elif event.key == pygame.K_SPACE:
                        fire_bullet()
                    elif event.key == pygame.K_c:
                        change_shield()
                    elif event.key == pygame.K_x:
                        special_attack()
            if not running:
                break
            keys = pygame.key.get_pressed()
            direction_x = int(keys[pygame.K_d] or keys[pygame.K_RIGHT]) - int(keys[pygame.K_a] or keys[pygame.K_LEFT])
            direction_y = int(keys[pygame.K_s] or keys[pygame.K_DOWN]) - int(keys[pygame.K_w] or keys[pygame.K_UP])
            # Pasos cortos: las bolas no atraviesan el escudo entre fotogramas.
            while accumulator >= step:
                update_game(step, direction_x, direction_y)
                accumulator -= step
            draw_game()
            pygame.display.update()
    finally:
        pygame.quit()


if __name__ == "__main__":
    main()
