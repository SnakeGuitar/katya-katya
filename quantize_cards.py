"""
quantize_cards.py — palette-quantize card art to a tiny color count (the deliberate
"N64 / posterized" look) and shrink PNG size hard.

Uses Pillow's FASTOCTREE quantizer, which (unlike MEDIANCUT) preserves the alpha
channel, so transparent card art stays transparent. Banding is the intended effect at
16-20 colors; pass --dither to trade some banding for noise if a card looks too flat.

Originals are overwritten in place (git is your backup — `git checkout -- <path>` to
revert). Use --dry-run first to preview the savings.

Examples:
    python quantize_cards.py KatyaKatya.Client/KatyaKatya.Client/Resources/Images/Cards
    python quantize_cards.py Resources/Images/Cards --colors 16 --dry-run
    python quantize_cards.py card-reverse.png --colors 20 --dither
"""

import argparse
import os
import sys

from PIL import Image


def iter_pngs(paths):
    """Yield every .png under the given files/directories (dirs walked recursively)."""
    for p in paths:
        if os.path.isdir(p):
            for root, _, files in os.walk(p):
                for name in files:
                    if name.lower().endswith(".png"):
                        yield os.path.join(root, name)
        elif os.path.isfile(p) and p.lower().endswith(".png"):
            yield p
        else:
            print(f"  skip (not a .png or not found): {p}")


def quantize(path, colors, dither, dry_run):
    before = os.path.getsize(path)
    try:
        img = Image.open(path)
        img = img.convert("RGBA")  # normalize so FASTOCTREE keeps transparency
        dither_mode = Image.Dither.FLOYDSTEINBERG if dither else Image.Dither.NONE
        quantized = img.quantize(
            colors=colors,
            method=Image.Quantize.FASTOCTREE,
            dither=dither_mode,
        )

        if dry_run:
            # Encode to memory to measure size without touching the file.
            import io
            buf = io.BytesIO()
            quantized.save(buf, format="PNG", optimize=True)
            after = buf.tell()
        else:
            quantized.save(path, format="PNG", optimize=True)
            after = os.path.getsize(path)
    except Exception as exc:  # noqa: BLE001 - report and continue
        print(f"  FAILED {path}: {exc}")
        return 0, 0

    pct = (1 - after / before) * 100 if before else 0
    print(f"  {os.path.relpath(path)}: {before/1024:,.0f} KB -> {after/1024:,.0f} KB ({pct:.0f}% smaller)")
    return before, after


def main(argv=None):
    parser = argparse.ArgumentParser(description="Palette-quantize PNG card art to a tiny color count.")
    parser.add_argument("paths", nargs="+", help="PNG files or directories to process (dirs are recursive).")
    parser.add_argument("--colors", type=int, default=20, help="Palette size, 2-256 (default: 20).")
    parser.add_argument("--dither", action="store_true", help="Floyd-Steinberg dithering (reduces banding, adds noise).")
    parser.add_argument("--dry-run", action="store_true", help="Report savings without overwriting files.")
    args = parser.parse_args(argv)

    if not 2 <= args.colors <= 256:
        parser.error("--colors must be between 2 and 256")

    files = list(iter_pngs(args.paths))
    if not files:
        print("No .png files found.")
        return 1

    mode = "DRY RUN -- no files written" if args.dry_run else "overwriting in place (revert with: git checkout -- <path>)"
    print(f"Quantizing {len(files)} PNG(s) to {args.colors} colors, dither={'on' if args.dither else 'off'}.")
    print(mode + "\n")

    total_before = total_after = 0
    for path in files:
        b, a = quantize(path, args.colors, args.dither, args.dry_run)
        total_before += b
        total_after += a

    if total_before:
        pct = (1 - total_after / total_before) * 100
        print(f"\nTotal: {total_before/1024:,.0f} KB -> {total_after/1024:,.0f} KB ({pct:.0f}% smaller across {len(files)} files)")
    return 0


if __name__ == "__main__":
    sys.exit(main())
