import os
import math
from PIL import Image, ImageDraw, ImageFilter

os.makedirs('Assets/Sprites', exist_ok=True)

def create_cell_closed():
    # 64x64 high-res cyber cell
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Outer background - dark cyber slate
    draw.rounded_rectangle([1, 1, 62, 62], radius=4, fill=(38, 42, 53, 255), outline=(58, 73, 75, 255), width=1)
    
    # Subtle inner bevel highlight (top and left)
    draw.line([(4, 3), (60, 3)], fill=(65, 78, 100, 200), width=1)
    draw.line([(3, 4), (3, 60)], fill=(65, 78, 100, 200), width=1)
    # Subtle inner bevel shadow (bottom and right)
    draw.line([(4, 61), (60, 61)], fill=(20, 24, 33, 220), width=1)
    draw.line([(61, 4), (61, 60)], fill=(20, 24, 33, 220), width=1)
    
    # Center tech pip (subtle 3x3 square)
    draw.rectangle([30, 30, 33, 33], fill=(80, 95, 115, 255))
    draw.point((31, 31), fill=(132, 148, 149, 255))
    draw.point((32, 32), fill=(132, 148, 149, 255))
    
    # 4 Corner registration micro-ticks in neon cyan
    draw.point((5, 5), fill=(0, 242, 254, 220))
    draw.point((58, 5), fill=(0, 242, 254, 220))
    draw.point((5, 58), fill=(0, 242, 254, 220))
    draw.point((58, 58), fill=(0, 242, 254, 220))
    
    img.save('Assets/Sprites/cell_closed.png')

def create_cell_hover():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Brightened surface with neon cyan border
    draw.rounded_rectangle([1, 1, 62, 62], radius=4, fill=(49, 53, 64, 255), outline=(0, 242, 254, 255), width=2)
    
    # Corner reticle crosshair brackets in bright cyan
    # Top-Left
    draw.line([(3, 3), (11, 3)], fill=(0, 242, 254, 255), width=2)
    draw.line([(3, 3), (3, 11)], fill=(0, 242, 254, 255), width=2)
    # Top-Right
    draw.line([(52, 3), (60, 3)], fill=(0, 242, 254, 255), width=2)
    draw.line([(60, 3), (60, 11)], fill=(0, 242, 254, 255), width=2)
    # Bottom-Left
    draw.line([(3, 60), (11, 60)], fill=(0, 242, 254, 255), width=2)
    draw.line([(3, 52), (3, 60)], fill=(0, 242, 254, 255), width=2)
    # Bottom-Right
    draw.line([(52, 60), (60, 60)], fill=(0, 242, 254, 255), width=2)
    draw.line([(60, 52), (60, 60)], fill=(0, 242, 254, 255), width=2)
    
    # Center crosshair pip
    draw.line([(29, 32), (34, 32)], fill=(0, 242, 254, 240), width=1)
    draw.line([(32, 29), (32, 34)], fill=(0, 242, 254, 240), width=1)
    
    img.save('Assets/Sprites/cell_hover.png')

def create_cell_pressed():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Deep sunken dark tile
    draw.rounded_rectangle([1, 1, 62, 62], radius=4, fill=(23, 27, 38, 255), outline=(15, 18, 26, 255), width=1)
    draw.line([(3, 3), (60, 3)], fill=(10, 12, 18, 255), width=1)
    draw.line([(3, 3), (3, 60)], fill=(10, 12, 18, 255), width=1)
    img.save('Assets/Sprites/cell_pressed.png')

def create_cell_revealed():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Flush recessed background
    draw.rounded_rectangle([1, 1, 62, 62], radius=4, fill=(23, 27, 38, 255), outline=(35, 43, 56, 180), width=1)
    # Very faint center pip
    draw.point((32, 32), fill=(58, 73, 75, 120))
    
    img.save('Assets/Sprites/cell_revealed.png')

def create_cell_exploded():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Crimson detonation flash
    draw.rounded_rectangle([1, 1, 62, 62], radius=4, fill=(147, 0, 10, 255), outline=(255, 0, 85, 255), width=2)
    # Bright inner hazard radiation
    draw.ellipse([14, 14, 49, 49], fill=(239, 68, 68, 220), outline=(255, 180, 171, 255), width=1)
    draw.ellipse([22, 22, 41, 41], fill=(255, 200, 200, 255))
    
    img.save('Assets/Sprites/cell_exploded.png')

def create_icon_flag():
    img = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # Outer glow halo
    for r in range(16, 26, 2):
        draw.ellipse([32 - r, 32 - r, 32 + r, 32 + r], outline=(255, 0, 85, int(40 * (1 - r / 26))))
    
    # Base stand / pod
    draw.polygon([(18, 52), (46, 52), (39, 46), (25, 46)], fill=(58, 73, 75, 255), outline=(0, 242, 254, 255))
    # Holographic pole
    draw.line([(32, 13), (32, 47)], fill=(0, 242, 254, 255), width=2)
    draw.line([(32, 11), (32, 13)], fill=(224, 253, 255, 255), width=3)
    
    # Cyber hazard flag banner (digital pennant)
    draw.polygon([(32, 13), (52, 23), (32, 33)], fill=(255, 0, 85, 255), outline=(255, 180, 171, 255))
    # Inner glowing energy core
    draw.polygon([(34, 18), (45, 23), (34, 28)], fill=(255, 230, 230, 255))
    # Top beacon dot
    draw.ellipse([30, 9, 34, 13], fill=(0, 242, 254, 255))
    
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
        x2 = cx + math.cos(rad) * 26
        y2 = cy + math.sin(rad) * 26
        draw.line([(x1, y1), (x2, y2)], fill=(255, 0, 85, 255), width=3)
        # Spike tip
        tip_x = cx + math.cos(rad) * 27
        tip_y = cy + math.sin(rad) * 27
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
    draw.line([(12, 12), (52, 52)], fill=(255, 0, 85, 255), width=4)
    draw.line([(12, 52), (52, 12)], fill=(255, 0, 85, 255), width=4)
    # Inner bright core line
    draw.line([(12, 12), (52, 52)], fill=(255, 220, 220, 255), width=2)
    draw.line([(12, 52), (52, 12)], fill=(255, 220, 220, 255), width=2)
    
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
    
    # Outer triumphant green ring
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
    draw.line([(21, 21), (29, 29)], fill=(239, 68, 68, 255), width=2)
    draw.line([(21, 29), (29, 21)], fill=(239, 68, 68, 255), width=2)
    draw.line([(35, 21), (43, 29)], fill=(239, 68, 68, 255), width=2)
    draw.line([(35, 29), (43, 21)], fill=(239, 68, 68, 255), width=2)
    
    # Glitch frown
    draw.arc([22, 36, 42, 50], start=190, end=350, fill=(239, 68, 68, 255), width=3)
    
    img.save('Assets/Sprites/face_defeat.png')

def create_panels_and_buttons():
    # 9-sliceable cyber panel (64x64, border 12px)
    panel = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(panel)
    d.rounded_rectangle([2, 2, 61, 61], radius=6, fill=(10, 14, 24, 245), outline=(58, 73, 75, 120), width=1)
    
    # Corner registration brackets in neon cyan
    d.line([(2, 2), (10, 2)], fill=(0, 242, 254, 255), width=2)
    d.line([(2, 2), (2, 10)], fill=(0, 242, 254, 255), width=2)
    d.line([(53, 2), (61, 2)], fill=(0, 242, 254, 255), width=2)
    d.line([(61, 2), (61, 10)], fill=(0, 242, 254, 255), width=2)
    d.line([(2, 61), (10, 61)], fill=(0, 242, 254, 255), width=2)
    d.line([(2, 53), (2, 61)], fill=(0, 242, 254, 255), width=2)
    d.line([(53, 61), (61, 61)], fill=(0, 242, 254, 255), width=2)
    d.line([(61, 53), (61, 61)], fill=(0, 242, 254, 255), width=2)
    panel.save('Assets/Sprites/panel_cyber.png')
    
    # 9-sliceable danger panel
    panel_danger = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(panel_danger)
    d.rounded_rectangle([2, 2, 61, 61], radius=6, fill=(20, 10, 14, 245), outline=(255, 0, 85, 140), width=1)
    d.line([(2, 2), (10, 2)], fill=(255, 0, 85, 255), width=2)
    d.line([(2, 2), (2, 10)], fill=(255, 0, 85, 255), width=2)
    d.line([(53, 2), (61, 2)], fill=(255, 0, 85, 255), width=2)
    d.line([(61, 2), (61, 10)], fill=(255, 0, 85, 255), width=2)
    d.line([(2, 61), (10, 61)], fill=(255, 0, 85, 255), width=2)
    d.line([(2, 53), (2, 61)], fill=(255, 0, 85, 255), width=2)
    d.line([(53, 61), (61, 61)], fill=(255, 0, 85, 255), width=2)
    d.line([(61, 53), (61, 61)], fill=(255, 0, 85, 255), width=2)
    panel_danger.save('Assets/Sprites/panel_danger.png')
    
    # 9-sliceable cyber button (64x64, border 8px)
    btn_cyber = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(btn_cyber)
    d.rounded_rectangle([1, 1, 62, 62], radius=4, fill=(28, 31, 42, 255), outline=(58, 73, 75, 200), width=1)
    d.line([(2, 2), (2, 61)], fill=(0, 242, 254, 255), width=2) # Neon cyan left accent edge
    btn_cyber.save('Assets/Sprites/button_cyber.png')
    
    # 9-sliceable danger button
    btn_danger = Image.new('RGBA', (64, 64), (0, 0, 0, 0))
    d = ImageDraw.Draw(btn_danger)
    d.rounded_rectangle([1, 1, 62, 62], radius=4, fill=(40, 15, 20, 255), outline=(255, 0, 85, 200), width=1)
    d.line([(2, 2), (2, 61)], fill=(255, 0, 85, 255), width=2) # Neon red left accent edge
    btn_danger.save('Assets/Sprites/button_danger.png')

if __name__ == '__main__':
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
