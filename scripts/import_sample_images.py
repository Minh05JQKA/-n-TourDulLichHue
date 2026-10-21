#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
=============================================================================
 HUE TOUR · NHẬP ẢNH TỪ DỰ ÁN MẪU (HUEGO) + GHI CÔNG NGUỒN
=============================================================================
 Chép ảnh hero và ảnh điểm đến (POI) từ dự án mẫu về wwwroot/images/,
 đồng thời nén lại cho nhẹ và tự động ghi công nguồn vào ASSETS.md.

 Cách chạy
 ---------
   python scripts/import_sample_images.py --source "C:\\path\\to\\Do_An_Khoi_Nghiep"
   python scripts/import_sample_images.py --source "..." --dry-run

 Lưu ý bản quyền
 ----------------
 Ảnh trong dự án mẫu HUEGO được dự án đó thu thập từ nhiều nguồn và bản
 README của họ tự cảnh báo "chưa xác minh được giấy phép". Vì vậy:
   * chỉ dùng cho mục đích học tập / demo bản capstone,
   * mọi tệp đều được ghi rõ nguồn trong ASSETS.md,
   * không được mô tả là ảnh Creative Commons nếu chưa xác minh.
=============================================================================
"""

from __future__ import annotations

import argparse
import re
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

REPO_ROOT = Path(__file__).resolve().parent.parent
WWWROOT = REPO_ROOT / "HueTour.WebApp" / "wwwroot"
IMAGES = WWWROOT / "images"
ASSETS_MD = REPO_ROOT / "ASSETS.md"

BLOCK_START = "<!-- HUEGO_SAMPLE_IMAGES_START -->"
BLOCK_END = "<!-- HUEGO_SAMPLE_IMAGES_END -->"

# --------------------------------------------------------------------------
# Nhóm ảnh và nhãn hiển thị
# --------------------------------------------------------------------------
HERO_IMAGES = [
    ("am-thuc-cung-dinh-dan-gian.jpg", "Ẩm thực cung đình và dân gian Huế (ảnh bìa trang mẫu)"),
    ("ca-phe-song-huong.jpg", "Cà phê bên bờ sông Hương (ảnh bìa trang mẫu)"),
    ("di-san-trieu-nguyen.jpg", "Di sản triều Nguyễn (ảnh bìa trang mẫu)"),
    ("chua-co-xu-hue.jpg", "Chốn thiền môn và chùa cổ Huế (ảnh bìa trang mẫu)"),
]

POI_IMAGES = [
    ("cau-ngoi-thanh-toan.jpg", "Cầu Ngói Thanh Toàn"),
    ("cau-trang-tien.jpg", "Cầu Tràng Tiền"),
    ("cho-dong-ba.jpg", "Chợ Đông Ba"),
    ("chua-thien-mu.jpg", "Chùa Thiên Mụ"),
    ("cung-an-dinh.jpg", "Cung An Đình"),
    ("cuu-dinh.jpg", "Cửa Đình"),
    ("dai-noi-hue.jpg", "Đại Nội (Hoàng thành Huế)"),
    ("dien-hon-chen.jpg", "Điện Hòn Chén"),
    ("dien-thai-hoa.jpg", "Điện Thái Hòa"),
    ("doi-vong-canh.jpg", "Đồi Vọng Cảnh"),
    ("ho-tinh-tam.jpg", "Hồ Tịnh Tâm"),
    ("lang-khai-dinh.jpg", "Lăng Khải Định"),
    ("lang-minh-mang.jpg", "Lăng Minh Mạng"),
    ("lang-tu-duc.jpg", "Lăng Tự Đức"),
    ("ngo-mon.jpg", "Ngọ Môn"),
    ("pha-tam-giang.jpg", "Phá Tam Giang"),
    ("song-huong.jpg", "Sông Hương"),
]

# Ảnh dựng dùng làm ảnh đại diện khi một địa điểm không có ảnh riêng.
FOOD_FALLBACKS = [
    ("fallback-cafe.jpg", "ca-phe-song-huong.jpg", "Ẩm thực Huế — ảnh đại diện nhóm quán cà phê"),
    ("fallback-restaurant.jpg", "am-thuc-cung-dinh-dan-gian.jpg", "Ẩm thực Huế — ảnh đại diện nhóm nhà hàng"),
    ("fallback-bakery.jpg", "cho-dong-ba.jpg", "Ẩm thực Huế — ảnh đại diện nhóm tiệm bánh"),
    ("fallback-food.jpg", "am-thuc-cung-dinh-dan-gian.jpg", "Ẩm thực Huế — ảnh đại diện nhóm món ăn"),
]

SOURCE_REPO = "https://github.com/hoangduong030405-art/Do_An_Khoi_Nghiep_-MST.git"
SOURCE_NOTE = ("Dự án mẫu HUEGO (`backend/` + `web-hue4u/public/`); "
               "ảnh do người tập hợp, giấy phép chưa được xác minh — dùng cho mục đích học tập.")


def optimize(src: Path, dest: Path, max_width: int, quality: int, dry_run: bool) -> int:
    dest.parent.mkdir(parents=True, exist_ok=True)
    with Image.open(src) as image:
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

        if dry_run:
            return 0

        image.save(dest, format="JPEG", quality=quality, optimize=True, progressive=True)

    return dest.stat().st_size


def update_assets_md(rows: list[str]) -> None:
    block = "\n".join([
        BLOCK_START,
        "### Ảnh nhập từ dự án mẫu HUEGO (dùng cho học tập / demo capstone)",
        "",
        "| Tệp | Nội dung | Tác giả / giấy phép | Nguồn |",
        "|---|---|---|---|",
        *rows,
        "",
        f"> {SOURCE_NOTE}",
        f"> Kho nguồn: [{SOURCE_REPO}]({SOURCE_REPO})",
        BLOCK_END,
    ])

    if ASSETS_MD.exists():
        text = ASSETS_MD.read_text(encoding="utf-8")
    else:
        text = "# Ghi công ảnh\n"

    pattern = re.compile(
        re.escape(BLOCK_START) + r".*?" + re.escape(BLOCK_END),
        re.DOTALL,
    )
    if pattern.search(text):
        text = pattern.sub(block, text)
    else:
        text = text.rstrip() + "\n\n" + block + "\n"

    ASSETS_MD.write_text(text, encoding="utf-8")


def main() -> int:
    parser = argparse.ArgumentParser(description="Nhập ảnh từ dự án mẫu HUEGO và ghi công nguồn.")
    parser.add_argument("--source", required=True, help="Thư mục gốc của repo mẫu HUEGO.")
    parser.add_argument("--hero-width", type=int, default=1920)
    parser.add_argument("--poi-width", type=int, default=1400)
    parser.add_argument("--quality", type=int, default=82)
    parser.add_argument("--dry-run", action="store_true")
    args = parser.parse_args()

    source_root = Path(args.source).expanduser().resolve()
    public = source_root / "web-hue4u" / "public"
    hero_src = public / "img" / "hero"
    poi_src = public / "images" / "pois"

    if not public.exists():
        print(f"[x] Không tìm thấy {public}. Kiểm tra lại tham số --source.")
        return 1

    rows: list[str] = []
    copied = 0
    saved_bytes = 0

    for folder, items, width, sub in (
        ("hero", HERO_IMAGES, args.hero_width, "img/hero"),
        ("poi", POI_IMAGES, args.poi_width, "images/pois"),
    ):
        src_dir = hero_src if folder == "hero" else poi_src
        for filename, label in items:
            src = src_dir / filename
            if not src.exists():
                print(f"    ! thiếu tệp nguồn: {src}")
                continue

            dest = IMAGES / folder / filename
            size = optimize(src, dest, width, args.quality, args.dry_run)
            copied += 1
            saved_bytes += size

            rows.append(
                f"| `{folder}/{filename}` | {label} | Người tập hợp trong dự án mẫu; giấy phép chưa xác minh | "
                f"[Repo mẫu HUEGO]({SOURCE_REPO}) — `{sub}/{filename}` |"
            )

    # Ảnh đại diện cho các địa điểm ẩm thực chưa có ảnh riêng.
    for filename, origin, label in FOOD_FALLBACKS:
        origin_path = next(
            (candidate for candidate in (
                IMAGES / "hero" / origin,
                IMAGES / "poi" / origin,
            ) if candidate.exists()),
            None,
        )
        if origin_path is None:
            print(f"    ! thiếu ảnh gốc cho ảnh đại diện: {origin}")
            continue

        dest = IMAGES / "food" / filename
        size = optimize(origin_path, dest, 1200, args.quality, args.dry_run)
        copied += 1
        saved_bytes += size

        rows.append(
            f"| `food/{filename}` | {label} | Ảnh dẫn xuất từ ảnh mẫu `{origin}`; giấy phép chưa xác minh | "
            f"[Repo mẫu HUEGO]({SOURCE_REPO}) — `img/hero/{origin}` |"
        )

    if not args.dry_run:
        update_assets_md(rows)

    verb = "sẽ chép" if args.dry_run else "đã chép"
    print(f"[+] {verb} {copied} ảnh (tổng {saved_bytes // 1024} KB sau nén)")
    print(f"[+] Ghi công nguồn trong: {ASSETS_MD.name}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
