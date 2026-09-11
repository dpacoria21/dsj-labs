"""Space Invaders: bloques y nombres como en el PDF (páginas 4-7)."""

import pygame
import math
import random
from pygame import mixer

from recursos import load_image, load_sound, SOUNDS_DIR


# Initialize the pygame
pygame.init()

# Create the screen
screen = pygame.display.set_mode((800, 600))

# Background
background = load_image("background.jpg", (800, 600))

# Background Sound
if mixer.get_init() and (SOUNDS_DIR / "background.wav").is_file():
    try:
        mixer.music.load(str(SOUNDS_DIR / "background.wav"))
        mixer.music.play(-1)
    except (pygame.error, OSError) as error:
        print(f"No se pudo reproducir background.wav: {error}")

# Title and icon
pygame.display.set_caption("Space Invaders")
icon = load_image("ufo.png", (32, 32))
pygame.display.set_icon(icon)

# Player
playerImg = load_image("player.png", (64, 64))
playerX = 370
playerY = 480
playerX_change = 0

# Enemy
enemyImg = []
enemyX = []
enemyY = []
enemyX_change = []
enemyY_change = []
num_of_enemies = 6

for i in range(num_of_enemies):
    enemyImg.append(load_image("enemy.png", (64, 64)))
    enemyX.append(random.randint(0, 735))
    enemyY.append(random.randint(50, 150))
    enemyX_change.append(1)
    enemyY_change.append(40)

# Bullet
# Ready: la bala está disponible. Fire: la bala está en movimiento.
bulletImg = load_image("bullet.png", (32, 32))
bulletX = 0
bulletY = 480
bulletX_change = 0
bulletY_change = 10
bullet_state = "ready"

# Se precargan los efectos para evitar leerlos en cada disparo.
bullet_Sound = load_sound("laser.wav")
explosion_Sound = load_sound("explosion.wav")

# Score
score_value = 0
font = pygame.font.Font("freesansbold.ttf", 32)
textX = 10
textY = 10

# Game Over text
over_font = pygame.font.Font("freesansbold.ttf", 64)
game_over = False


def show_score(x, y):
    score = font.render("Score :" + str(score_value), True, (0, 255, 0))
    screen.blit(score, (x, y))


def game_over_text():
    over_text = over_font.render("GAME OVER", True, (0, 255, 0))
    screen.blit(over_text, (200, 250))


def player(x, y):
    screen.blit(playerImg, (x, y))


def enemy(x, y, i):
    screen.blit(enemyImg[i], (x, y))


def fire_bullet(x, y):
    global bullet_state
    bullet_state = "fire"
    screen.blit(bulletImg, (x + 16, y + 10))


def isCollision(enemyX, enemyY, bulletX, bulletY):
    distance = math.sqrt(math.pow(enemyX - bulletX, 2) + math.pow(enemyY - bulletY, 2))
    return distance < 27


# Game Loop
def main():
    global playerX, playerX_change, bulletX, bulletY, bullet_state
    global score_value, game_over

    clock = pygame.time.Clock()
    running = True
    try:
        while running:
            # Limita la velocidad del ejemplo a 60 fotogramas por segundo.
            clock.tick(60)

            # RGB / Background Image
            screen.fill((0, 0, 0))
            screen.blit(background, (0, 0))

            for event in pygame.event.get():
                if event.type == pygame.QUIT:
                    running = False

                # Check whether the key pressed is right or left
                if event.type == pygame.KEYDOWN and not game_over:
                    if event.key == pygame.K_LEFT:
                        playerX_change = -1
                    if event.key == pygame.K_RIGHT:
                        playerX_change = 1
                    if event.key == pygame.K_SPACE:
                        if bullet_state == "ready":
                            if bullet_Sound:
                                bullet_Sound.play()
                            bulletX = playerX
                            fire_bullet(bulletX, bulletY)

                if event.type == pygame.KEYUP:
                    if event.key == pygame.K_LEFT or event.key == pygame.K_RIGHT:
                        keys = pygame.key.get_pressed()
                        playerX_change = int(keys[pygame.K_RIGHT]) - int(keys[pygame.K_LEFT])

                if event.type == pygame.WINDOWFOCUSLOST:
                    playerX_change = 0

            if not running:
                break

            if not game_over:
                # Checking for boundaries of spaceship
                playerX += playerX_change
                if playerX <= 0:
                    playerX = 0
                elif playerX >= 736:
                    playerX = 736

                # Enemy movement
                for i in range(num_of_enemies):
                    # Game Over
                    if enemyY[i] > 440:
                        game_over = True
                        break

                    enemyX[i] += enemyX_change[i]
                    if enemyX[i] <= 0:
                        enemyX[i] = 0
                        enemyX_change[i] = 1
                        enemyY[i] += enemyY_change[i]
                    elif enemyX[i] >= 736:
                        enemyX[i] = 736
                        enemyX_change[i] = -1
                        enemyY[i] += enemyY_change[i]

                    if enemyY[i] > 440:
                        game_over = True
                        break

                    # Collision
                    collision = bullet_state == "fire" and isCollision(
                        enemyX[i], enemyY[i], bulletX, bulletY
                    )
                    if collision:
                        if explosion_Sound:
                            explosion_Sound.play()
                        bulletY = 480
                        bullet_state = "ready"
                        score_value += 1
                        enemyX[i] = random.randint(0, 736)
                        enemyY[i] = random.randint(50, 150)

                    enemy(enemyX[i], enemyY[i], i)

                # Bullet movement
                if not game_over:
                    if bulletY <= 0:
                        bulletY = 480
                        bullet_state = "ready"
                    if bullet_state == "fire":
                        fire_bullet(bulletX, bulletY)
                        bulletY -= bulletY_change

            if game_over:
                # Borra los enemigos y conserva la pantalla final.
                screen.blit(background, (0, 0))
                game_over_text()

            player(playerX, playerY)
            show_score(textX, textY)
            pygame.display.update()
    finally:
        pygame.quit()


if __name__ == "__main__":
    main()
