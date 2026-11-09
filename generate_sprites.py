import os
import math
from PIL import Image, ImageDraw

os.makedirs('Assets/Sprites', exist_ok=True)

def supersample_and_save(draw_fn, filepath, size=(128, 128), scale=2):
    hires_size = (size[0] * scale, size[1] * scale)
    img = Image.new('RGBA', hires_size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    draw_fn(draw, hires_size[0], hires_size[1], scale)
    out = img.resize(size, Image.Resampling.LANCZOS)
    out.save(filepath)

def draw_cell_closed(d, w, h, s):
    radius = 8 * s
    d.rounded_rectangle([2*s, 2*s, w - 2*s, h - 2*s], radius=radius, fill=(30, 38, 56, 255), outline=(46, 59, 82, 255), width=2*s)
    d.line([(radius + 2*s, 4*s), (w - radius - 2*s, 4*s)], fill=(55, 70, 96, 180), width=1*s)
    cx, cy = w // 2, h // 2
    r_pip = 2.5 * s
    d.ellipse([cx - r_pip, cy - r_pip, cx + r_pip, cy + r_pip], fill=(80, 95, 120, 255))

def draw_cell_hover(d, w, h, s):
    radius = 8 * s
    d.rounded_rectangle([2*s, 2*s, w - 2*s, h - 2*s], radius=radius, fill=(44, 56, 78, 255), outline=(0, 242, 254, 255), width=3*s)
    c_len = 10 * s
    d.line([(4*s, 8*s), (4*s + c_len, 8*s)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(8*s, 4*s), (8*s, 4*s + c_len)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(w - 4*s - c_len, 8*s), (w - 4*s, 8*s)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(w - 8*s, 4*s), (w - 8*s, 4*s + c_len)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(4*s, h - 8*s), (4*s + c_len, h - 8*s)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(8*s, h - 4*s - c_len), (8*s, h - 4*s)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(w - 4*s - c_len, h - 8*s), (w - 4*s, h - 8*s)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(w - 8*s, h - 4*s - c_len), (w - 8*s, h - 4*s)], fill=(0, 242, 254, 255), width=2*s)
    cx, cy = w // 2, h // 2
    r_pip = 3 * s
    d.ellipse([cx - r_pip, cy - r_pip, cx + r_pip, cy + r_pip], fill=(0, 242, 254, 255))

def draw_cell_pressed(d, w, h, s):
    radius = 8 * s
    d.rounded_rectangle([2*s, 2*s, w - 2*s, h - 2*s], radius=radius, fill=(20, 26, 39, 255), outline=(15, 20, 30, 255), width=2*s)
    d.line([(radius, 3*s), (w - radius, 3*s)], fill=(10, 14, 22, 255), width=2*s)
    d.line([(3*s, radius), (3*s, h - radius)], fill=(10, 14, 22, 255), width=2*s)

def draw_cell_revealed(d, w, h, s):
    radius = 8 * s
    d.rounded_rectangle([2*s, 2*s, w - 2*s, h - 2*s], radius=radius, fill=(18, 24, 36, 255), outline=(26, 34, 51, 200), width=1*s)
    cx, cy = w // 2, h // 2
    d.ellipse([cx - 1.5*s, cy - 1.5*s, cx + 1.5*s, cy + 1.5*s], fill=(40, 50, 70, 140))

def draw_cell_exploded(d, w, h, s):
    radius = 8 * s
    d.rounded_rectangle([2*s, 2*s, w - 2*s, h - 2*s], radius=radius, fill=(127, 29, 29, 255), outline=(239, 68, 68, 255), width=3*s)
    cx, cy = w // 2, h // 2
    d.ellipse([cx - 24*s, cy - 24*s, cx + 24*s, cy + 24*s], fill=(220, 38, 38, 200))
    d.ellipse([cx - 14*s, cy - 14*s, cx + 14*s, cy + 14*s], fill=(254, 202, 202, 255))

def draw_icon_flag(d, w, h, s):
    cx, cy = w // 2, h // 2
    staff_x = cx - 8 * s
    d.line([(staff_x, cy - 24*s), (staff_x, cy + 22*s)], fill=(160, 175, 195, 255), width=3*s)
    d.line([(staff_x - 6*s, cy + 22*s), (staff_x + 6*s, cy + 22*s)], fill=(160, 175, 195, 255), width=3*s)
    pennant = [
        (staff_x + 2*s, cy - 22*s),
        (staff_x + 24*s, cy - 11*s),
        (staff_x + 2*s, cy)
    ]
    d.polygon(pennant, fill=(255, 75, 110, 255))
    d.ellipse([staff_x + 22*s, cy - 13*s, staff_x + 26*s, cy - 9*s], fill=(255, 200, 220, 255))

def draw_icon_mine(d, w, h, s):
    cx, cy = w // 2, h // 2
    r_core = 16 * s
    r_spike = 24 * s
    for angle_deg in range(0, 360, 45):
        rad = math.radians(angle_deg)
        x1 = cx + math.cos(rad) * (r_core - 2*s)
        y1 = cy + math.sin(rad) * (r_core - 2*s)
        x2 = cx + math.cos(rad) * r_spike
        y2 = cy + math.sin(rad) * r_spike
        d.line([(x1, y1), (x2, y2)], fill=(200, 215, 230, 255), width=3*s)
        d.ellipse([x2 - 2*s, y2 - 2*s, x2 + 2*s, y2 + 2*s], fill=(0, 242, 254, 255))
    d.ellipse([cx - r_core, cy - r_core, cx + r_core, cy + r_core], fill=(24, 30, 42, 255), outline=(130, 150, 175, 255), width=2*s)
    d.ellipse([cx - 7*s, cy - 7*s, cx + 7*s, cy + 7*s], fill=(255, 0, 85, 255))
    d.ellipse([cx - 3*s, cy - 3*s, cx + 3*s, cy + 3*s], fill=(255, 200, 200, 255))

def draw_icon_false_flag(d, w, h, s):
    draw_icon_flag(d, w, h, s)
    cx, cy = w // 2, h // 2
    d.line([(cx - 14*s, cy - 14*s), (cx + 14*s, cy + 14*s)], fill=(255, 0, 85, 255), width=4*s)
    d.line([(cx - 14*s, cy + 14*s), (cx + 14*s, cy - 14*s)], fill=(255, 0, 85, 255), width=4*s)

def draw_face_normal(d, w, h, s):
    cx, cy = w // 2, h // 2
    r_orb = 24 * s
    d.ellipse([cx - r_orb, cy - r_orb, cx + r_orb, cy + r_orb], fill=(10, 37, 48, 255), outline=(0, 242, 254, 255), width=2*s)
    d.line([(cx - r_orb - 5*s, cy), (cx - r_orb + 2*s, cy)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(cx + r_orb - 2*s, cy), (cx + r_orb + 5*s, cy)], fill=(0, 242, 254, 255), width=2*s)
    d.ellipse([cx - 9*s, cy - 8*s, cx - 4*s, cy - 3*s], fill=(0, 242, 254, 255))
    d.ellipse([cx + 4*s, cy - 8*s, cx + 9*s, cy - 3*s], fill=(0, 242, 254, 255))
    d.arc([cx - 11*s, cy - 4*s, cx + 11*s, cy + 13*s], start=20, end=160, fill=(0, 242, 254, 255), width=2*s)

def draw_face_alert(d, w, h, s):
    cx, cy = w // 2, h // 2
    r_orb = 24 * s
    d.ellipse([cx - r_orb, cy - r_orb, cx + r_orb, cy + r_orb], fill=(10, 37, 48, 255), outline=(0, 242, 254, 255), width=2*s)
    d.line([(cx - r_orb - 5*s, cy), (cx - r_orb + 2*s, cy)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(cx + r_orb - 2*s, cy), (cx + r_orb + 5*s, cy)], fill=(0, 242, 254, 255), width=2*s)
    d.ellipse([cx - 10*s, cy - 10*s, cx - 3*s, cy - 3*s], fill=(0, 242, 254, 255))
    d.ellipse([cx + 3*s, cy - 10*s, cx + 10*s, cy - 3*s], fill=(0, 242, 254, 255))
    d.ellipse([cx - 5*s, cy + 3*s, cx + 5*s, cy + 13*s], fill=(0, 242, 254, 255))

def draw_face_win(d, w, h, s):
    cx, cy = w // 2, h // 2
    r_orb = 24 * s
    d.ellipse([cx - r_orb, cy - r_orb, cx + r_orb, cy + r_orb], fill=(12, 45, 55, 255), outline=(0, 242, 254, 255), width=3*s)
    visor = [
        (cx - 14*s, cy - 9*s),
        (cx + 14*s, cy - 9*s),
        (cx + 11*s, cy - 1*s),
        (cx - 11*s, cy - 1*s)
    ]
    d.polygon(visor, fill=(0, 242, 254, 255))
    d.arc([cx - 12*s, cy - 2*s, cx + 12*s, cy + 14*s], start=15, end=165, fill=(0, 242, 254, 255), width=3*s)

def draw_face_defeat(d, w, h, s):
    cx, cy = w // 2, h // 2
    r_orb = 24 * s
    d.ellipse([cx - r_orb, cy - r_orb, cx + r_orb, cy + r_orb], fill=(45, 15, 20, 255), outline=(255, 0, 85, 255), width=3*s)
    d.line([(cx - 11*s, cy - 9*s), (cx - 5*s, cy - 3*s)], fill=(255, 0, 85, 255), width=2*s)
    d.line([(cx - 11*s, cy - 3*s), (cx - 5*s, cy - 9*s)], fill=(255, 0, 85, 255), width=2*s)
    d.line([(cx + 5*s, cy - 9*s), (cx + 11*s, cy - 3*s)], fill=(255, 0, 85, 255), width=2*s)
    d.line([(cx + 5*s, cy - 3*s), (cx + 11*s, cy - 9*s)], fill=(255, 0, 85, 255), width=2*s)
    d.line([(cx - 10*s, cy + 9*s), (cx - 4*s, cy + 6*s)], fill=(255, 0, 85, 255), width=2*s)
    d.line([(cx - 4*s, cy + 6*s), (cx + 3*s, cy + 11*s)], fill=(255, 0, 85, 255), width=2*s)
    d.line([(cx + 3*s, cy + 11*s), (cx + 10*s, cy + 7*s)], fill=(255, 0, 85, 255), width=2*s)

def draw_face_pause(d, w, h, s):
    cx, cy = w // 2, h // 2
    r_orb = 24 * s
    d.ellipse([cx - r_orb, cy - r_orb, cx + r_orb, cy + r_orb], fill=(15, 25, 40, 255), outline=(245, 158, 11, 255), width=2*s)
    d.line([(cx - 10*s, cy - 5*s), (cx - 3*s, cy - 5*s)], fill=(245, 158, 11, 255), width=2*s)
    d.line([(cx + 3*s, cy - 5*s), (cx + 10*s, cy - 5*s)], fill=(245, 158, 11, 255), width=2*s)
    d.line([(cx + 14*s, cy - 16*s), (cx + 20*s, cy - 16*s)], fill=(245, 158, 11, 255), width=1*s)
    d.line([(cx + 20*s, cy - 16*s), (cx + 14*s, cy - 10*s)], fill=(245, 158, 11, 255), width=1*s)
    d.line([(cx + 14*s, cy - 10*s), (cx + 20*s, cy - 10*s)], fill=(245, 158, 11, 255), width=1*s)
    d.line([(cx - 5*s, cy + 7*s), (cx + 5*s, cy + 7*s)], fill=(245, 158, 11, 255), width=2*s)

def draw_panel_cyber(d, w, h, s):
    radius = 16 * s
    d.rounded_rectangle([3*s, 3*s, w - 3*s, h - 3*s], radius=radius, fill=(18, 23, 34, 245), outline=(38, 49, 68, 255), width=2*s)
    d.line([(radius + 4*s, 4*s), (w - radius - 4*s, 4*s)], fill=(0, 242, 254, 90), width=1*s)

def draw_panel_danger(d, w, h, s):
    radius = 16 * s
    d.rounded_rectangle([3*s, 3*s, w - 3*s, h - 3*s], radius=radius, fill=(24, 15, 22, 248), outline=(244, 63, 94, 200), width=2*s)
    d.line([(radius + 4*s, 4*s), (w - radius - 4*s, 4*s)], fill=(244, 63, 94, 255), width=2*s)

def draw_button_cyber(d, w, h, s):
    radius = 10 * s
    d.rounded_rectangle([2*s, 2*s, w - 2*s, h - 2*s], radius=radius, fill=(22, 29, 43, 255), outline=(43, 53, 72, 255), width=2*s)

def draw_button_danger(d, w, h, s):
    radius = 10 * s
    d.rounded_rectangle([2*s, 2*s, w - 2*s, h - 2*s], radius=radius, fill=(225, 29, 72, 255), outline=(251, 113, 133, 255), width=2*s)

def draw_button_accent(d, w, h, s):
    radius = 10 * s
    d.rounded_rectangle([2*s, 2*s, w - 2*s, h - 2*s], radius=radius, fill=(0, 242, 254, 255), outline=(125, 248, 255, 255), width=2*s)

def draw_icon_restart(d, w, h, s):
    cx, cy = w // 2, h // 2
    r = 16 * s
    d.arc([cx - r, cy - r, cx + r, cy + r], start=45, end=330, fill=(0, 242, 254, 255), width=3*s)
    rad = math.radians(45)
    ax, ay = cx + math.cos(rad) * r, cy + math.sin(rad) * r
    d.polygon([(ax - 2*s, ay - 6*s), (ax + 7*s, ay + 1*s), (ax - 4*s, ay + 5*s)], fill=(0, 242, 254, 255))

def draw_icon_pause(d, w, h, s):
    cx, cy = w // 2, h // 2
    w_bar = 5 * s
    h_bar = 20 * s
    d.rectangle([cx - 8*s, cy - h_bar//2, cx - 8*s + w_bar, cy + h_bar//2], fill=(224, 253, 255, 255))
    d.rectangle([cx + 3*s, cy - h_bar//2, cx + 3*s + w_bar, cy + h_bar//2], fill=(224, 253, 255, 255))

def draw_icon_play(d, w, h, s):
    cx, cy = w // 2, h // 2
    pts = [(cx - 8*s, cy - 12*s), (cx + 10*s, cy), (cx - 8*s, cy + 12*s)]
    d.polygon(pts, fill=(0, 242, 254, 255))

def draw_icon_hint(d, w, h, s):
    cx, cy = w // 2, h // 2
    r = 14 * s
    d.ellipse([cx - r, cy - r - 4*s, cx + r, cy + r - 4*s], fill=(245, 158, 11, 255))
    d.rectangle([cx - 6*s, cy + 8*s, cx + 6*s, cy + 15*s], fill=(160, 175, 195, 255))
    d.line([(cx - 4*s, cy + 17*s), (cx + 4*s, cy + 17*s)], fill=(160, 175, 195, 255), width=2*s)

def draw_icon_sound_on(d, w, h, s):
    cx, cy = w // 2, h // 2
    d.polygon([(cx - 14*s, cy - 6*s), (cx - 7*s, cy - 6*s), (cx + 2*s, cy - 14*s), (cx + 2*s, cy + 14*s), (cx - 7*s, cy + 6*s), (cx - 14*s, cy + 6*s)], fill=(185, 202, 203, 255))
    d.arc([cx - 8*s, cy - 10*s, cx + 12*s, cy + 10*s], start=-50, end=50, fill=(0, 242, 254, 255), width=2*s)
    d.arc([cx - 12*s, cy - 16*s, cx + 18*s, cy + 16*s], start=-50, end=50, fill=(0, 242, 254, 255), width=2*s)

def draw_icon_sound_off(d, w, h, s):
    cx, cy = w // 2, h // 2
    d.polygon([(cx - 14*s, cy - 6*s), (cx - 7*s, cy - 6*s), (cx + 2*s, cy - 14*s), (cx + 2*s, cy + 14*s), (cx - 7*s, cy + 6*s), (cx - 14*s, cy + 6*s)], fill=(130, 145, 155, 255))
    d.line([(cx + 6*s, cy - 10*s), (cx + 18*s, cy + 10*s)], fill=(244, 63, 94, 255), width=3*s)
    d.line([(cx + 18*s, cy - 10*s), (cx + 6*s, cy + 10*s)], fill=(244, 63, 94, 255), width=3*s)

def draw_icon_fullscreen(d, w, h, s):
    cx, cy = w // 2, h // 2
    sz = 14 * s
    d_tick = 6 * s
    d.line([(cx - sz, cy - sz + d_tick), (cx - sz, cy - sz), (cx - sz + d_tick, cy - sz)], fill=(185, 202, 203, 255), width=2*s)
    d.line([(cx + sz - d_tick, cy - sz), (cx + sz, cy - sz), (cx + sz, cy - sz + d_tick)], fill=(185, 202, 203, 255), width=2*s)
    d.line([(cx - sz, cy + sz - d_tick), (cx - sz, cy + sz), (cx - sz + d_tick, cy + sz)], fill=(185, 202, 203, 255), width=2*s)
    d.line([(cx + sz - d_tick, cy + sz), (cx + sz, cy + sz), (cx + sz, cy + sz - d_tick)], fill=(185, 202, 203, 255), width=2*s)

def draw_icon_settings(d, w, h, s):
    cx, cy = w // 2, h // 2
    r_core = 8 * s
    r_gear = 15 * s
    for i in range(6):
        ang = math.radians(i * 60)
        x = cx + math.cos(ang) * r_gear
        y = cy + math.sin(ang) * r_gear
        d.ellipse([x - 3.5*s, y - 3.5*s, x + 3.5*s, y + 3.5*s], fill=(185, 202, 203, 255))
    d.ellipse([cx - (r_gear - 2*s), cy - (r_gear - 2*s), cx + (r_gear - 2*s), cy + (r_gear - 2*s)], fill=(185, 202, 203, 255))
    d.ellipse([cx - r_core, cy - r_core, cx + r_core, cy + r_core], fill=(11, 14, 23, 255))

def draw_icon_gamepad(d, w, h, s):
    cx, cy = w // 2, h // 2
    d.rounded_rectangle([cx - 20*s, cy - 10*s, cx + 20*s, cy + 10*s], radius=8*s, fill=(0, 242, 254, 255))
    d.line([(cx - 13*s, cy), (cx - 7*s, cy)], fill=(11, 14, 23, 255), width=2*s)
    d.line([(cx - 10*s, cy - 3*s), (cx - 10*s, cy + 3*s)], fill=(11, 14, 23, 255), width=2*s)
    d.ellipse([cx + 6*s, cy - 4*s, cx + 10*s, cy], fill=(11, 14, 23, 255))
    d.ellipse([cx + 10*s, cy, cx + 14*s, cy + 4*s], fill=(11, 14, 23, 255))

def draw_icon_warning(d, w, h, s):
    cx, cy = w // 2, h // 2
    pts = [(cx, cy - 18*s), (cx + 18*s, cy + 14*s), (cx - 18*s, cy + 14*s)]
    d.polygon(pts, fill=(244, 63, 94, 255))
    d.line([(cx, cy - 7*s), (cx, cy + 4*s)], fill=(255, 255, 255, 255), width=3*s)
    d.ellipse([cx - 1.8*s, cy + 7.2*s, cx + 1.8*s, cy + 10.8*s], fill=(255, 255, 255, 255))

def draw_icon_check(d, w, h, s):
    cx, cy = w // 2, h // 2
    d.line([(cx - 12*s, cy), (cx - 4*s, cy + 8*s), (cx + 14*s, cy - 10*s)], fill=(0, 242, 254, 255), width=4*s)

def draw_icon_close(d, w, h, s):
    cx, cy = w // 2, h // 2
    sz = 10 * s
    d.line([(cx - sz, cy - sz), (cx + sz, cy + sz)], fill=(185, 202, 203, 255), width=3*s)
    d.line([(cx - sz, cy + sz), (cx + sz, cy - sz)], fill=(185, 202, 203, 255), width=3*s)

def draw_icon_leaderboard(d, w, h, s):
    cx, cy = w // 2, h // 2
    d.rectangle([cx - 16*s, cy + 2*s, cx - 6*s, cy + 16*s], fill=(56, 189, 248, 255))
    d.rectangle([cx - 5*s, cy - 8*s, cx + 5*s, cy + 16*s], fill=(0, 242, 254, 255))
    d.rectangle([cx + 6*s, cy + 7*s, cx + 16*s, cy + 16*s], fill=(56, 189, 248, 255))

def draw_icon_rules(d, w, h, s):
    cx, cy = w // 2, h // 2
    d.rounded_rectangle([cx - 12*s, cy - 16*s, cx + 12*s, cy + 16*s], radius=3*s, fill=(22, 29, 43, 255), outline=(0, 242, 254, 255), width=2*s)
    d.line([(cx - 7*s, cy - 8*s), (cx + 7*s, cy - 8*s)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(cx - 7*s, cy - 2*s), (cx + 7*s, cy - 2*s)], fill=(0, 242, 254, 255), width=2*s)
    d.line([(cx - 7*s, cy + 4*s), (cx + 3*s, cy + 4*s)], fill=(0, 242, 254, 255), width=2*s)

if __name__ == '__main__':
    print("Generating comprehensive cyberpunk sprites...")
    supersample_and_save(draw_cell_closed, 'Assets/Sprites/cell_closed.png')
    supersample_and_save(draw_cell_hover, 'Assets/Sprites/cell_hover.png')
    supersample_and_save(draw_cell_pressed, 'Assets/Sprites/cell_pressed.png')
    supersample_and_save(draw_cell_revealed, 'Assets/Sprites/cell_revealed.png')
    supersample_and_save(draw_cell_exploded, 'Assets/Sprites/cell_exploded.png')
    supersample_and_save(draw_icon_flag, 'Assets/Sprites/icon_flag.png')
    supersample_and_save(draw_icon_mine, 'Assets/Sprites/icon_mine.png')
    supersample_and_save(draw_icon_false_flag, 'Assets/Sprites/icon_false_flag.png')
    supersample_and_save(draw_face_normal, 'Assets/Sprites/face_normal.png')
    supersample_and_save(draw_face_alert, 'Assets/Sprites/face_alert.png')
    supersample_and_save(draw_face_win, 'Assets/Sprites/face_win.png')
    supersample_and_save(draw_face_defeat, 'Assets/Sprites/face_defeat.png')
    supersample_and_save(draw_face_pause, 'Assets/Sprites/face_pause.png')
    supersample_and_save(draw_panel_cyber, 'Assets/Sprites/panel_cyber.png')
    supersample_and_save(draw_panel_danger, 'Assets/Sprites/panel_danger.png')
    supersample_and_save(draw_button_cyber, 'Assets/Sprites/button_cyber.png')
    supersample_and_save(draw_button_danger, 'Assets/Sprites/button_danger.png')
    supersample_and_save(draw_button_accent, 'Assets/Sprites/button_accent.png')

    supersample_and_save(draw_icon_restart, 'Assets/Sprites/icon_restart.png', (64, 64))
    supersample_and_save(draw_icon_pause, 'Assets/Sprites/icon_pause.png', (64, 64))
    supersample_and_save(draw_icon_play, 'Assets/Sprites/icon_play.png', (64, 64))
    supersample_and_save(draw_icon_hint, 'Assets/Sprites/icon_hint.png', (64, 64))
    supersample_and_save(draw_icon_sound_on, 'Assets/Sprites/icon_sound_on.png', (64, 64))
    supersample_and_save(draw_icon_sound_off, 'Assets/Sprites/icon_sound_off.png', (64, 64))
    supersample_and_save(draw_icon_fullscreen, 'Assets/Sprites/icon_fullscreen.png', (64, 64))
    supersample_and_save(draw_icon_settings, 'Assets/Sprites/icon_settings.png', (64, 64))
    supersample_and_save(draw_icon_gamepad, 'Assets/Sprites/icon_gamepad.png', (64, 64))
    supersample_and_save(draw_icon_warning, 'Assets/Sprites/icon_warning.png', (64, 64))
    supersample_and_save(draw_icon_check, 'Assets/Sprites/icon_check.png', (64, 64))
    supersample_and_save(draw_icon_close, 'Assets/Sprites/icon_close.png', (64, 64))
    supersample_and_save(draw_icon_leaderboard, 'Assets/Sprites/icon_leaderboard.png', (64, 64))
    supersample_and_save(draw_icon_rules, 'Assets/Sprites/icon_rules.png', (64, 64))
    print("All sprites successfully generated!")
