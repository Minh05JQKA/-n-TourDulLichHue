#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
=============================================================================
 HUE TOUR · BỘ TỐI ƯU ẢNH
=============================================================================
 Nén và resize ảnh trong wwwroot để trang tải nhanh hơn, giữ nguyên tên tệp.

 Cách chạy
 ---------
   python scripts/optimize_images.py HueTour.WebApp/wwwroot/images/food
   python scripts/optimize_images.py HueTour.WebApp/wwwroot/images --max-width 1600 --quality 82

 Tham số
 -------
   --max-width   Chiều rộng tối đa (mặc định 1400 px).
   --quality     Chất lượng JPEG/WebP (mặc định 82).
   --min-bytes   Bỏ qua tệp nhỏ hơn ngưỡng này (mặc định 60 KB).
   --dry-run     Chỉ báo cáo, không ghi tệp.
=============================================================================
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

try:
    from PIL import Image
except ImportError:  # pragma: no cover
    print("[x] Chưa cài Pillow. Chạy: python -m pip install Pillow")
    sys.exit(1)

for _stream in (sys.stdout, sys.stderr):
    if hasattr(_stream, "reconfigure"):
        try:
            _stream.reconfigure(encoding="utf-8", errors="replace")
        except (ValueError, OSError):  # pragma: no cover
            pass

SUPPORTED = {".jpg", ".jpeg", ".png", ".webp"}


def optimize_file(path: Path, max_width: int, quality: int, dry_run: bool) -> tuple[int, int]:
    before = path.stat().st_size
    try:
        with Image.open(path) as image:
            image.load()
            if image.mode in ("RGBA", "LA", "P"):
                background = Image.new("RGB", image.size, (255, 255, 255))
                image = image.convert("RGBA")
                background.paste(image, mask=image.split()[-1])
                image = background
            else:
                image = image.convert("RGB")

            if image.width > max_width:
                ratio = max_width / image.width
                image = image.resize((max_width, max(1, int(image.height * ratio))), Image.LANCZOS)

            suffix = path.suffix.lower()
            save_format = "WEBP" if suffix == ".webp" else "JPEG"
            options = {"quality": quality, "optimize": True}
            if save_format == "JPEG":
                options["progressive"] = True

            if dry_run:
                return before, before

            temp = path.with_suffix(path.suffix + ".tmp")
            image.save(temp, format=save_format, **options)
            temp.replace(path)
    except Exception as exc:  # noqa: BLE001 - bỏ qua tệp lỗi
        print(f"    ! {path.name}: {exc}")
        return before, before

    after = path.stat().st_size
    return before, after


def main() -> int:
    parser = argparse.ArgumentParser(description="Nén ảnh trong thư mục wwwroot.")
    parser.add_argument("directories", nargs="+", help="Các thư mục cần tối ưu.")
    parser.add_argument("--max-width", type=int, default=1400, help="Chiều rộng tối đa (px).")
    parser.add_argument("--quality", type=int, default=82, help="Chất lượng nén.")
    parser.add_argument("--min-bytes", type=int, default=60 * 1024, help="Bỏ qua tệp nhỏ hơn ngưỡng.")
    parser.add_argument("--dry-run", action="store_true", help="Chỉ báo cáo.")
    args = parser.parse_args()

    total_before = 0
    total_after = 0
    touched = 0

    for directory in args.directories:
        folder = Path(directory).expanduser().resolve()
        if not folder.is_dir():
            print(f"[!] Không tìm thấy thư mục: {folder}")
            continue

        for path in sorted(folder.rglob("*")):
            if not path.is_file() or path.suffix.lower() not in SUPPORTED:
                continue
            if path.stat().st_size < args.min_bytes:
                continue

            before, after = optimize_file(path, args.max_width, args.quality, args.dry_run)
            total_before += before
            total_after += after
            touched += 1
            saved = before - after
            if saved > 0:
                print(f"    {path.name:<52} {before // 1024:>6} KB -> {after // 1024:>5} KB  (-{saved // 1024} KB)")

    if touched:
        saved = total_before - total_after
        percent = (saved / total_before * 100) if total_before else 0
        verb = "sẽ nén" if args.dry_run else "đã nén"
        print(f"[+] {verb} {touched} ảnh: {total_before // 1024} KB -> {total_after // 1024} KB (tiết kiệm {percent:.1f}%)")

    return 0


if __name__ == "__main__":
    sys.exit(main())
