"""Pruebas del nivel horizontal y del combate de colores."""
import os
os.environ['SDL_VIDEODRIVER'] = 'dummy'
os.environ['SDL_AUDIODRIVER'] = 'dummy'
import math
import unittest
from unittest.mock import patch
import main as g


class GameTests(unittest.TestCase):
    def setUp(self):
        g.reset_game()

    def hit(self, color):
        g.enemyAttacks.append(dict(x=g.playerX+20, y=g.playerY+20, vx=0, vy=0, color=color))
        g.update_attacks(0)

    def test_wave_has_two_colors_and_staggered_entry(self):
        self.assertEqual(g.enemies, [])
        g.update_waves(1/120)
        self.assertEqual(len(g.enemies), 1)
        self.assertGreater(g.enemies[0]['x'], 800)
        for _ in range(420):
            g.update_waves(1/120)
        self.assertEqual(len(g.enemies), 6)
        self.assertEqual(sum(m['color']=='rojo' for m in g.enemies), 3)
        self.assertEqual(sum(m['color']=='azul' for m in g.enemies), 3)
        self.assertEqual(g.wavePending, 0)

    def test_curves_move_left_and_stay_inside_vertical_area(self):
        for pattern in range(3):
            g.reset_game()
            g.spawn_enemy()
            g.wavePending = 0
            monster = g.enemies[0]
            monster['pattern'] = pattern
            ys = []
            for _ in range(600):
                x = monster['x']
                g.update_waves(1/120)
                self.assertLess(monster['x'], x)
                self.assertGreaterEqual(monster['y'], 94)
                self.assertLessEqual(monster['y'] + 64, 572)
                ys.append(monster['y'])
            self.assertGreater(max(ys)-min(ys), 80)

    def test_next_wave_waits_and_rotates_pattern(self):
        g.wavePending = 0
        g.update_waves(1.0)
        self.assertEqual(g.waveNumber, 1)
        g.update_waves(0.6)
        self.assertEqual(g.waveNumber, 2)
        self.assertEqual(g.wavePending, 6)
        g.update_waves(1/120)
        self.assertEqual(g.enemies[0]['pattern'], 1)

    def test_leaving_left_is_not_game_over(self):
        g.spawn_enemy()
        g.enemies[0]['x'] = -65
        g.wavePending = 0
        g.update_game(1/120, 0)
        self.assertEqual(g.enemies, [])
        self.assertFalse(g.game_over)

    def test_no_offscreen_shooting_and_attacks_aim_left(self):
        g.spawn_enemy()
        m = g.enemies[0]
        m['shoot'] = 0
        g.update_waves(1/120)
        self.assertEqual(g.enemyAttacks, [])
        m['x'] = 600
        g.update_waves(1/120)
        self.assertEqual(len(g.enemyAttacks), 1)
        ball = g.enemyAttacks[0]
        self.assertLess(ball['vx'], 0)
        self.assertEqual(ball['color'], m['color'])
        self.assertAlmostEqual(math.hypot(ball['vx'], ball['vy']), g.BALL_SPEED)

    def test_horizontal_normal_shot_hits_only_once(self):
        g.spawn_enemy()
        m=g.enemies[0]
        m.update(x=400, y=300)
        g.fire_bullet()
        self.assertEqual(g.bulletX, g.playerX + 40)
        self.assertEqual(g.bulletY + g.bulletImg.get_height()/2, g.playerY+20)
        g.bulletX, g.bulletY = 400, 320
        with patch.object(g, 'update_waves'):
            g.update_game(1/120, 0)
        self.assertEqual(g.score_value, 1)
        self.assertEqual(g.bullet_state, 'ready')
        self.assertEqual(g.enemies, [])

    def test_special_only_counts_visible_enemies(self):
        for x in (200, 500, 810):
            g.spawn_enemy()
            g.enemies[-1]['x']=x
        self.assertFalse(g.special_attack())
        g.shieldEnergy=5
        self.assertTrue(g.special_attack())
        self.assertEqual(g.score_value, 2)
        self.assertEqual(len(g.enemies), 1)
        self.assertEqual(g.shieldEnergy, 0)
        self.assertEqual(g.wavePending, 6)

    def test_absorption_switch_cap_and_damage(self):
        self.hit('azul')
        g.change_shield()
        self.hit('rojo')
        self.assertEqual(g.shieldEnergy, 2)
        self.assertEqual(g.lives, 3)
        for _ in range(8): self.hit('rojo')
        self.assertEqual(g.shieldEnergy, 5)
        self.hit('azul')
        self.hit('azul')
        self.assertEqual(g.lives, 2)
        self.assertEqual(g.shieldEnergy, 5)

    def test_contact_damage_and_death_freeze(self):
        for _ in range(3):
            g.spawn_enemy()
            g.enemies[-1].update(x=g.playerX-12, y=g.playerY-12)
            g.damageCooldown=0
            with patch.object(g, 'update_waves'):
                g.update_game(1/120, 0)
        self.assertTrue(g.game_over)
        before=(g.playerX,g.playerY,g.waveTime)
        g.update_game(0.1,1,1)
        self.assertEqual(before,(g.playerX,g.playerY,g.waveTime))
        g.reset_game()
        self.assertFalse(g.game_over)
        self.assertEqual((g.playerX,g.playerY,g.lives,g.waveNumber),(90,310,3,1))

    def test_wasd_diagonal_speed_and_bounds(self):
        x,y=g.playerX,g.playerY
        g.update_game(0.1,1,-1)
        self.assertAlmostEqual(math.hypot(g.playerX-x,g.playerY-y),30)
        for dx,dy in ((-1,-1),(1,1)):
            g.playerX=g.PLAYER_MIN_X if dx<0 else g.PLAYER_MAX_X
            g.playerY=g.PLAYER_MIN_Y if dy<0 else g.PLAYER_MAX_Y
            x,y=g.playerX,g.playerY
            g.update_game(0.1,dx,dy)
            self.assertEqual((g.playerX,g.playerY),(x,y))

    def test_projectiles_removed_at_every_edge(self):
        for x,y in ((-20,300),(820,300),(300,70),(300,600)):
            g.enemyAttacks.append(dict(x=x,y=y,vx=0,vy=0,color='rojo'))
        g.update_attacks(0)
        self.assertEqual(g.enemyAttacks,[])


if __name__ == '__main__':
    unittest.main()
