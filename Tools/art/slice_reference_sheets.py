#!/usr/bin/env python3
"""Fatia as folhas de sprites de REFERENCIA em sprites transparentes de jogo.

Personagens: grade 4x4 (FRONT/BACK/LEFT/RIGHT x IDLE/WALK1-3) + retrato grande.
Empilhadeiras: vistas FRONT/BACK/LEFT/RIGHT + arte isométrica grande (hero).

Pipeline por folha:
 1. cor de fundo = mediana dos pixels de borda (cinza escuro com ruído);
 2. máscara de fundo por distância de cor + flood-fill a partir das bordas
    (preserva cinzas internos da roupa) + bolsões internos ~idênticos ao fundo;
 3. componentes conectados (com fechamento morfológico para unir partes soltas);
 4. filtro de rótulos de texto (área/altura mínimas);
 5. personagens: maior componente = retrato; os 16 restantes viram grade por
    clusters de centróide (4 linhas x 4 colunas);
 6. canvas uniforme por personagem, âncora bottom-center (baseline dos pés).
"""
from __future__ import annotations

import sys
from pathlib import Path

import numpy as np
from PIL import Image
from scipy import ndimage

ROOT = Path(__file__).resolve().parents[2]
REF = ROOT / "REFERENCIA"
OUT = ROOT / "Tools" / "art" / "sliced"

BG_DIST_MAIN = 30.0     # distância p/ máscara de fundo (ruído do cinza)
BG_DIST_POCKET = 18.0   # bolsões internos: só se quase idênticos ao fundo
MIN_AREA = 4000         # descarta rótulos de texto ("FRONT", "WALK 1", ...)
MIN_HEIGHT = 70
CLOSE_PX = 7            # raio de fechamento p/ unir partes soltas (chapéu etc.)


def load_rgb(path: Path) -> np.ndarray:
    return np.asarray(Image.open(path).convert("RGB"), dtype=np.float32)


def background_mask(rgb: np.ndarray) -> tuple[np.ndarray, np.ndarray]:
    h, w, _ = rgb.shape
    border = np.concatenate([
        rgb[0], rgb[-1], rgb[:, 0], rgb[:, -1],
    ])
    bg = np.median(border, axis=0)
    dist = np.sqrt(((rgb - bg) ** 2).sum(axis=2))
    bg_colored = dist < BG_DIST_MAIN

    # flood-fill do fundo externo a partir das bordas
    outside = np.zeros((h, w), dtype=bool)
    seed = np.zeros((h, w), dtype=bool)
    seed[0, :] = seed[-1, :] = True
    seed[:, 0] = seed[:, -1] = True
    seed &= bg_colored
    lbl, _ = ndimage.label(bg_colored)
    outside_ids = np.unique(lbl[seed])
    outside_ids = outside_ids[outside_ids != 0]
    outside = np.isin(lbl, outside_ids)

    # bolsões internos (entre pernas etc.): só vira fundo se ~idêntico ao bg
    pockets = bg_colored & ~outside
    if pockets.any():
        plbl, pn = ndimage.label(pockets)
        for pid in range(1, pn + 1):
            sel = plbl == pid
            if dist[sel].mean() < BG_DIST_POCKET:
                outside |= sel

    return outside, dist


def components(fg: np.ndarray, close_px: int = CLOSE_PX):
    closed = ndimage.binary_closing(
        fg, structure=np.ones((close_px, close_px), dtype=bool))
    lbl, n = ndimage.label(closed)
    boxes = ndimage.find_objects(lbl)
    comps = []
    for i, sl in enumerate(boxes, start=1):
        if sl is None:
            continue
        region = (lbl[sl] == i) & fg[sl]
        area = int(region.sum())
        h = sl[0].stop - sl[0].start
        if area < MIN_AREA or h < MIN_HEIGHT:
            continue
        comps.append({
            "slice": sl,
            "mask": region,
            "area": area,
            "cy": (sl[0].start + sl[0].stop) / 2,
            "cx": (sl[1].start + sl[1].stop) / 2,
        })
    return comps


def split_merged(fg: np.ndarray, comps: list, expected: int) -> list:
    """Se componentes se fundiram (ex.: hero encostando numa vista), re-divide
    os maiores usando fechamento menor até atingir a contagem esperada."""
    close = CLOSE_PX
    while len(comps) < expected and close > 1:
        close -= 2
        comps = components(fg, close_px=close)
    return comps


def cut_sprite(rgb: np.ndarray, comp: dict) -> Image.Image:
    sl, mask = comp["slice"], comp["mask"]
    crop = rgb[sl].astype(np.uint8)
    alpha = (mask * 255).astype(np.uint8)
    rgba = np.dstack([crop, alpha])
    return Image.fromarray(rgba, "RGBA")


def paste_on_canvas(sprite: Image.Image, canvas_w: int, canvas_h: int) -> Image.Image:
    canvas = Image.new("RGBA", (canvas_w, canvas_h), (0, 0, 0, 0))
    x = (canvas_w - sprite.width) // 2
    y = canvas_h - sprite.height  # baseline: pés (com sombra) no fundo do canvas
    canvas.paste(sprite, (x, y), sprite)
    return canvas


def slice_character(sheet: Path, out_dir: Path, prefix: str) -> list[str]:
    rgb = load_rgb(sheet)
    outside, _ = background_mask(rgb)
    comps = components(~outside)
    if len(comps) < 17:
        raise SystemExit(f"{sheet.name}: esperava >=17 componentes, achei {len(comps)}")

    comps.sort(key=lambda c: -c["area"])
    portrait = comps[0]
    frames = comps[1:17]

    # grade: 4 linhas (y) x 4 colunas (x)
    ys = sorted(c["cy"] for c in frames)
    xs = sorted(c["cx"] for c in frames)
    row_centers = [np.mean(ys[i * 4:(i + 1) * 4]) for i in range(4)]
    col_centers = [np.mean(xs[i * 4:(i + 1) * 4]) for i in range(4)]

    grid: dict[tuple[int, int], dict] = {}
    for c in frames:
        r = int(np.argmin([abs(c["cy"] - rc) for rc in row_centers]))
        col = int(np.argmin([abs(c["cx"] - cc) for cc in col_centers]))
        if (r, col) in grid:
            raise SystemExit(f"{sheet.name}: célula duplicada ({r},{col})")
        grid[(r, col)] = c

    rows = ["Idle", "Walk1", "Walk2", "Walk3"]
    cols = ["Down", "Up", "Left", "Right"]  # FRONT, BACK, LEFT, RIGHT

    sprites = {}
    for (r, col), comp in grid.items():
        sprites[f"{rows[r]}{cols[col]}"] = cut_sprite(rgb, comp)

    canvas_w = max(s.width for s in sprites.values()) + 4
    canvas_h = max(s.height for s in sprites.values()) + 2

    out_dir.mkdir(parents=True, exist_ok=True)
    written = []
    for name, spr in sprites.items():
        p = out_dir / f"{prefix}_{name}.png"
        paste_on_canvas(spr, canvas_w, canvas_h).save(p)
        written.append(str(p.relative_to(ROOT)))

    pimg = cut_sprite(rgb, portrait)
    p = out_dir / f"{prefix}_Portrait.png"
    pimg.save(p)
    written.append(str(p.relative_to(ROOT)))
    return written


def slice_forklift(sheet: Path, out_dir: Path, prefix: str) -> list[str]:
    rgb = load_rgb(sheet)
    outside, _ = background_mask(rgb)
    fg = ~outside
    comps = components(fg)
    comps = split_merged(fg, comps, expected=5)
    if len(comps) < 5:
        raise SystemExit(f"{sheet.name}: esperava >=5 componentes, achei {len(comps)}")

    comps.sort(key=lambda c: -c["area"])
    hero = comps[0]
    views = sorted(comps[1:5], key=lambda c: (c["cy"], c["cx"]))
    # linha de cima: FRONT (esq), BACK (dir); depois LEFT; depois RIGHT
    top = sorted(views[:2], key=lambda c: c["cx"])
    named = {
        "Front": top[0],
        "Back": top[1],
        "Left": views[2],
        "Right": views[3],
    }

    out_dir.mkdir(parents=True, exist_ok=True)
    written = []
    for name, comp in named.items():
        p = out_dir / f"{prefix}_{name}.png"
        cut_sprite(rgb, comp).save(p)
        written.append(str(p.relative_to(ROOT)))
    p = out_dir / f"{prefix}_Hero.png"
    cut_sprite(rgb, hero).save(p)
    written.append(str(p.relative_to(ROOT)))
    return written


def main() -> int:
    jobs = [
        ("char", REF / "Personagens/John Miller/Character 1 - John (.png", OUT / "Characters/John", "John"),
        ("char", REF / "Personagens/Duda/Character 1 - Duda .png", OUT / "Characters/Duda", "Duda"),
        ("char", REF / "Personagens/Robert Hayes/Character 1 - Robert .png", OUT / "Characters/Robert", "Robert"),
    ]
    car_dirs = sorted((REF / "Empilhadeira").iterdir())
    for i, d in enumerate([d for d in car_dirs if d.is_dir()], start=1):
        first = sorted(d.glob("*.png"))[0]
        jobs.append(("fork", first, OUT / "Forklift", f"Car{i}"))

    total = []
    for kind, sheet, out_dir, prefix in jobs:
        fn = slice_character if kind == "char" else slice_forklift
        written = fn(sheet, out_dir, prefix)
        total.extend(written)
        print(f"{prefix}: {len(written)} sprites  <- {sheet.name}")
    print(f"total: {len(total)} sprites em {OUT.relative_to(ROOT)}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
