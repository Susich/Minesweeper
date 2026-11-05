import os
import math
from PIL import Image, ImageDraw, ImageFilter

os.makedirs('Assets/Sprites', exist_ok=True)

def create_cell_closed():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Outer background
    draw.rectangle([1, 1, 62, 62], fill=(23, 27, 38, 255), outline=(42, 52, 71, 255), width=1)
    # Inner subtle bevel
    draw.line([(2, 2), (61, 2)], fill=(55, 68, 92, 255), width=1)
    draw.line([(2, 2), (2, 61)], fill=(55, 68, 92, 255), width=1)
    draw.line([(61, 2), (61, 61)], fill=(15, 18, 26, 255), width=1)
    draw.line([(2, 61), (61, 61)], fill=(15, 18, 26, 255), width=1)
    
    # Center tech pip
    draw.rectangle([30, 30, 33, 33], fill=(71, 85, 105, 255))
    draw.point((31, 31), fill=(132, 148, 149, 255))
    draw.point((32, 32), fill=(132, 148, 149, 255))
    
    # Corner registration notches (1px ticks)
    draw.point((5, 5), fill=(0, 242, 254, 180))
    draw.point((58, 5), fill=(0, 242, 254, 180))
    draw.point((5, 58), fill=(0, 242, 254, 180))
    draw.point((58, 58), fill=(0, 242, 254, 180))
    
    img.save('Assets/Sprites/cell_closed.png')

def create_cell_hover():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Glowing hover background
    draw.rectangle([1, 1, 62, 62], fill=(28, 37, 56, 255), outline=(0, 242, 254, 255), width=2)
    
    # Corner reticle crosshair brackets
    # Top-Left
    draw.line([(3, 3), (9, 3)], fill=(0, 242, 254, 255), width=2)
    draw.line([(3, 3), (3, 9)], fill=(0, 242, 254, 255), width=2)
    # Top-Right
    draw.line([(54, 3), (60, 3)], fill=(0, 242, 254, 255), width=2)
    draw.line([(60, 3), (60, 9)], fill=(0, 242, 254, 255), width=2)
    # Bottom-Left
    draw.line([(3, 60), (9, 60)], fill=(0, 242, 254, 255), width=2)
    draw.line([(3, 54), (3, 60)], fill=(0, 242, 254, 255), width=2)
    # Bottom-Right
    draw.line([(54, 60), (60, 60)], fill=(0, 242, 254, 255), width=2)
    draw.line([(60, 54), (60, 60)], fill=(0, 242, 254, 255), width=2)
    
    # Center crosshair
    draw.line([(30, 32), (33, 32)], fill=(0, 242, 254, 220), width=1)
    draw.line([(32, 30), (32, 33)], fill=(0, 242, 254, 220), width=1)
    
    img.save('Assets/Sprites/cell_hover.png')

def create_cell_pressed():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Sunken dark tile
    draw.rectangle([1, 1, 62, 62], fill=(10, 14, 24, 255), outline=(20, 26, 38, 255), width=1)
    draw.line([(2, 2), (61, 2)], fill=(5, 8, 17, 255), width=1)
    draw.line([(2, 2), (2, 61)], fill=(5, 8, 17, 255), width=1)
    img.save('Assets/Sprites/cell_pressed.png')

def create_cell_revealed():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Flush recessed background
    draw.rectangle([1, 1, 62, 62], fill=(14, 18, 28, 240), outline=(30, 41, 59, 200), width=1)
    # Subtle inner corner marks
    draw.point((3, 3), fill=(40, 50, 70, 150))
    draw.point((60, 3), fill=(40, 50, 70, 150))
    draw.point((3, 60), fill=(40, 50, 70, 150))
    draw.point((60, 60), fill=(40, 50, 70, 150))
    
    img.save('Assets/Sprites/cell_revealed.png')

def create_cell_exploded():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Crimson detonation flash
    draw.rectangle([1, 1, 62, 62], fill=(147, 0, 10, 255), outline=(255, 0, 85, 255), width=2)
    # Bright inner hazard radiation
    draw.ellipse([14, 14, 49, 49], fill=(239, 68, 68, 200), outline=(255, 180, 171, 255), width=1)
    
    img.save('Assets/Sprites/cell_exploded.png')

def create_icon_flag():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Outer glow halo
    for r in range(16, 24, 2):
        draw.ellipse([32 - r, 32 - r, 32 + r, 32 + r], outline=(255, 0, 85, int(35 * (1 - r / 24))))
    
    # Base stand / pod
    draw.polygon([(20, 52), (44, 52), (37, 46), (27, 46)], fill=(58, 73, 75, 255), outline=(0, 242, 254, 255))
    # Holographic pole
    draw.line([(32, 14), (32, 47)], fill=(0, 242, 254, 255), width=2)
    draw.line([(32, 12), (32, 14)], fill=(224, 253, 255, 255), width=3)
    
    # Cyber hazard flag banner (digital pennant / diamond)
    draw.polygon([(32, 14), (49, 23), (32, 33)], fill=(255, 0, 85, 230), outline=(255, 180, 171, 255))
    # Inner energy core
    draw.polygon([(34, 19), (43, 23), (34, 27)], fill=(255, 218, 214, 255))
    
    img.save('Assets/Sprites/icon_flag.png')

def create_icon_mine():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = 32, 32
    # 8 radiating detonator spikes
    for angle in range(0, 360, 45):
        rad = math.radians(angle)
        x1 = cx + math.cos(rad) * 12
        y1 = cy + math.sin(rad) * 12
        x2 = cx + math.cos(rad) * 25
        y2 = cy + math.sin(rad) * 25
        draw.line([(x1, y1), (x2, y2)], fill=(255, 0, 85, 255), width=3)
        # Spike tip
        tip_x = cx + math.cos(rad) * 26
        tip_y = cy + math.sin(rad) * 26
        draw.ellipse([tip_x - 2, tip_y - 2, tip_x + 2, tip_y + 2], fill=(255, 180, 171, 255))
        
    # Main spherical casing
    draw.ellipse([cx - 15, cy - 15, cx + 15, cy + 15], fill=(23, 27, 38, 255), outline=(0, 242, 254, 255), width=2)
    # Inner telemetry ring
    draw.ellipse([cx - 9, cy - 9, cx + 9, cy + 9], fill=(15, 19, 29, 255), outline=(245, 158, 11, 255), width=1)
    # Core detonator LED
    draw.ellipse([cx - 5, cy - 5, cx + 5, cy + 5], fill=(255, 0, 85, 255))
    draw.ellipse([cx - 2, cy - 2, cx + 2, cy + 2], fill=(255, 255, 255, 255))
    
    img.save('Assets/Sprites/icon_mine.png')

def create_icon_false_flag():
    # Base is flag
    img = Image.open('Assets/Sprites/icon_flag.png').copy()
    draw = ImageDraw.Draw(img)
    
    # Bright neon red cross
    draw.line([(14, 14), (50, 50)], fill=(255, 0, 85, 255), width=4)
    draw.line([(14, 50), (50, 14)], fill=(255, 0, 85, 255), width=4)
    # Inner bright core line
    draw.line([(14, 14), (50, 50)], fill=(255, 200, 200, 255), width=2)
    draw.line([(14, 50), (50, 14)], fill=(255, 200, 200, 255), width=2)
    
    img.save('Assets/Sprites/icon_false_flag.png')

def create_face_normal():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = 32, 32
    
    # Outer cyan glow ring
    draw.ellipse([cx - 24, cy - 24, cx + 24, cy + 24], outline=(0, 242, 254, 255), width=2)
    # Side antenna nodes
    draw.line([(4, cy), (8, cy)], fill=(0, 242, 254, 255), width=2)
    draw.line([(55, cy), (59, cy)], fill=(0, 242, 254, 255), width=2)
    
    # Eyes (glowing cyan pips)
    draw.ellipse([22, 22, 28, 28], fill=(0, 242, 254, 255))
    draw.ellipse([36, 22, 42, 28], fill=(0, 242, 254, 255))
    
    # Friendly cyber smile arc
    draw.arc([22, 26, 42, 44], start=10, end=170, fill=(0, 242, 254, 255), width=3)
    
    img.save('Assets/Sprites/face_normal.png')

def create_face_alert():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = 32, 32
    
    # Outer amber glow ring
    draw.ellipse([cx - 24, cy - 24, cx + 24, cy + 24], outline=(245, 158, 11, 255), width=2)
    draw.line([(4, cy), (8, cy)], fill=(245, 158, 11, 255), width=2)
    draw.line([(55, cy), (59, cy)], fill=(245, 158, 11, 255), width=2)
    
    # Wide eyes
    draw.ellipse([21, 21, 29, 29], fill=(245, 158, 11, 255))
    draw.ellipse([35, 21, 43, 29], fill=(245, 158, 11, 255))
    
    # 'O' mouth
    draw.ellipse([27, 34, 37, 44], outline=(245, 158, 11, 255), width=3)
    
    img.save('Assets/Sprites/face_alert.png')

def create_face_win():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = 32, 32
    
    # Outer triumphant cyan ring
    draw.ellipse([cx - 24, cy - 24, cx + 24, cy + 24], outline=(0, 255, 102, 255), width=2)
    draw.line([(4, cy), (8, cy)], fill=(0, 255, 102, 255), width=2)
    draw.line([(55, cy), (59, cy)], fill=(0, 255, 102, 255), width=2)
    
    # Cool sunglasses / cyber visor
    draw.polygon([(18, 22), (46, 22), (42, 32), (34, 32), (32, 27), (30, 32), (22, 32)], fill=(0, 242, 254, 255), outline=(224, 253, 255, 255))
    # Visor reflection stripe
    draw.line([(22, 25), (42, 25)], fill=(255, 255, 255, 200), width=1)
    
    # Wide triumphant grin
    draw.arc([22, 28, 42, 44], start=10, end=170, fill=(0, 255, 102, 255), width=3)
    
    img.save('Assets/Sprites/face_win.png')

def create_face_defeat():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    cx, cy = 32, 32
    
    # Outer critical red ring
    draw.ellipse([cx - 24, cy - 24, cx + 24, cy + 24], outline=(239, 68, 68, 255), width=2)
    draw.line([(4, cy), (8, cy)], fill=(239, 68, 68, 255), width=2)
    draw.line([(55, cy), (59, cy)], fill=(239, 68, 68, 255), width=2)
    
    # X eyes
    # Left eye X
    draw.line([(21, 21), (29, 29)], fill=(239, 68, 68, 255), width=2)
    draw.line([(21, 29), (29, 21)], fill=(239, 68, 68, 255), width=2)
    # Right eye X
    draw.line([(35, 21), (43, 29)], fill=(239, 68, 68, 255), width=2)
    draw.line([(35, 29), (43, 21)], fill=(239, 68, 68, 255), width=2)
    
    # Glitch frown
    draw.arc([22, 36, 42, 50], start=190, end=350, fill=(239, 68, 68, 255), width=3)
    
    img.save('Assets/Sprites/face_defeat.png')

def create_panels_and_buttons():
    # 9-sliceable cyber panel (64x64, border 12px)
    panel = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(panel)
    d.rectangle([2, 2, 61, 61], fill=(15, 23, 42, 240), outline=(0, 242, 254, 80), width=1)
    # Corner registration marks
    d.line([(2, 2), (8, 2)], fill=(0, 242, 254, 255), width=2)
    d.line([(2, 2), (2, 8)], fill=(0, 242, 254, 255), width=2)
    d.line([(55, 2), (61, 2)], fill=(0, 242, 254, 255), width=2)
    d.line([(61, 2), (61, 8)], fill=(0, 242, 254, 255), width=2)
    d.line([(2, 61), (8, 61)], fill=(0, 242, 254, 255), width=2)
    d.line([(2, 55), (2, 61)], fill=(0, 242, 254, 255), width=2)
    d.line([(55, 61), (61, 61)], fill=(0, 242, 254, 255), width=2)
    d.line([(61, 55), (61, 61)], fill=(0, 242, 254, 255), width=2)
    panel.save('Assets/Sprites/panel_cyber.png')
    
    # Red danger panel
    panel_red = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(panel_red)
    d.rectangle([2, 2, 61, 61], fill=(20, 10, 16, 245), outline=(255, 0, 85, 120), width=1)
    d.line([(2, 2), (8, 2)], fill=(255, 0, 85, 255), width=2)
    d.line([(2, 2), (2, 8)], fill=(255, 0, 85, 255), width=2)
    d.line([(55, 2), (61, 2)], fill=(255, 0, 85, 255), width=2)
    d.line([(61, 2), (61, 8)], fill=(255, 0, 85, 255), width=2)
    d.line([(2, 61), (8, 61)], fill=(255, 0, 85, 255), width=2)
    d.line([(2, 55), (2, 61)], fill=(255, 0, 85, 255), width=2)
    d.line([(55, 61), (61, 61)], fill=(255, 0, 85, 255), width=2)
    d.line([(61, 55), (61, 61)], fill=(255, 0, 85, 255), width=2)
    panel_red.save('Assets/Sprites/panel_danger.png')
    
    # Cyber Button
    btn = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(btn)
    d.rectangle([2, 2, 61, 61], fill=(23, 27, 38, 250), outline=(0, 242, 254, 200), width=1)
    d.line([(2, 2), (6, 2)], fill=(0, 242, 254, 255), width=2)
    d.line([(2, 2), (2, 6)], fill=(0, 242, 254, 255), width=2)
    d.line([(57, 61), (61, 61)], fill=(0, 242, 254, 255), width=2)
    d.line([(61, 57), (61, 61)], fill=(0, 242, 254, 255), width=2)
    btn.save('Assets/Sprites/button_cyber.png')

    # Cyber Danger Button
    btn_danger = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(btn_danger)
    d.rectangle([2, 2, 61, 61], fill=(45, 15, 22, 250), outline=(255, 0, 85, 200), width=1)
    d.line([(2, 2), (6, 2)], fill=(255, 0, 85, 255), width=2)
    d.line([(2, 2), (2, 6)], fill=(255, 0, 85, 255), width=2)
    d.line([(57, 61), (61, 61)], fill=(255, 0, 85, 255), width=2)
    d.line([(61, 57), (61, 61)], fill=(255, 0, 85, 255), width=2)
    btn_danger.save('Assets/Sprites/button_danger.png')

create_cell_closed()
create_cell_hover()
create_cell_pressed()
create_cell_revealed()
create_cell_exploded()
create_icon_flag()
create_icon_mine()
create_icon_false_flag()
create_face_normal()
create_face_alert()
create_face_win()
create_face_defeat()
create_panels_and_buttons()
print("Successfully generated all cyber UI textures!")
