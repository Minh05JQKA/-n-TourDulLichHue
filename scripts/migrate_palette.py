#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
=============================================================================
 HUE TOUR · ĐỔI BẢNG MÀU GIAO DIỆN (JADE -> TÍM HOÀNG GIA & VÀNG CUNG ĐÌNH)
=============================================================================
 Ánh xạ lại mã màu trong các tệp CSS của HueTour sang bảng màu lấy cảm hứng
 từ dự án mẫu HUEGO (web-hue4u/public/css/style.css):

   --primary      #582C83   Tím Hoàng Gia
   --accent-gold  #D4AF37   Vàng Cung Đình

 Cách chạy
 ---------
   python scripts/migrate_palette.py            # áp dụng cho các tệp mặc định
   python scripts/migrate_palette.py --dry-run  # chỉ báo cáo

 Ghi chú: script chỉ thay đúng các mã màu nằm trong bảng ánh xạ, các màu
 khác giữ nguyên nên không làm hỏng bố cục.
=============================================================================
"""

from __future__ import annotations

import argparse
import sys
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parent.parent
DEFAULT_FILES = [
    REPO_ROOT / "HueTour.WebApp" / "wwwroot" / "app.css",
    REPO_ROOT / "HueTour.WebApp" / "wwwroot" / "css" / "travel-discovery.css",
]

for _stream in (sys.stdout, sys.stderr):
    if hasattr(_stream, "reconfigure"):
        try:
            _stream.reconfigure(encoding="utf-8", errors="replace")
        except (ValueError, OSError):  # pragma: no cover
            pass

# --------------------------------------------------------------------------
# Bảng ánh xạ: màu cũ (jade/teal) -> màu mới (tím hoàng gia / vàng cung đình)
# --------------------------------------------------------------------------
HEX_MAP = {
    # ---- Nền tối, chữ chính ----
    "#183a34": "#241137",
    "#153d37": "#2e1548",
    "#1c5149": "#3c1d5e",
    "#28635a": "#4a2472",
    "#286558": "#4a2472",
    "#26665b": "#48216e",
    "#173f39": "#241137",
    "#173e38": "#241137",
    "#183d37": "#231033",
    "#101a17": "#1a0e28",
    "#121c18": "#1c0f2b",
    "#14201b": "#1e1130",
    "#192520": "#221432",
    "#25332d": "#2c1a42",
    "#31473d": "#3a2358",
    "#235a50": "#3d1f63",
    "#0a2927": "#180b26",
    "#0e3733": "#1e1035",
    "#0f3d38": "#1c0f2b",
    "#185f57": "#582c83",
    "#103e3a": "#451e6b",
    "#28786e": "#7a4eab",
    "#15574f": "#582c83",
    "#134e45": "#4a2170",
    "#1c6b5f": "#6b3a9e",
    "#0f4438": "#2a1450",
    # ---- Xanh jade dùng cho trạng thái "tự nhiên" ----
    "#83c5b8": "#c4a7e7",
    "#a8d7b4": "#c6b4e6",
    # ---- Xanh lam / xanh rêu làm trạng thái phụ ----
    "#477567": "#6b3a9e",
    "#427462": "#63359a",
    "#42816b": "#5e2f92",
    "#496158": "#5b2e8e",
    "#548278": "#6b3a9e",
    "#5c9878": "#7c4bae",
    "#356c5c": "#5b2e8e",
    "#647465": "#6b7280",
    "#66736c": "#64748b",
    "#6b7770": "#64748b",
    "#73847a": "#7a8a9a",
    "#758278": "#7a8a9a",
    "#a3b2aa": "#aab4c0",
    "#9aa19c": "#a3aab2",
    # ---- Vàng / đồng ----
    "#b88b3d": "#b8860b",
    "#9a7a3e": "#9c783b",
    "#a47e3a": "#a47e3a",
    "#a88b54": "#b08844",
    "#857044": "#8a6d2f",
    "#816431": "#8a6d2f",
    "#947239": "#9c7b37",
    "#92703a": "#9a7838",
    "#6d572d": "#6e5a2e",
    "#4a442e": "#4e4632",
    "#856b3c": "#8b7142",
    "#9c783b": "#a07d3e",
    "#b38a44": "#bb9248",
    "#a27a35": "#aa8239",
    "#d4b976": "#dcc384",
    "#f0d594": "#f2dfa4",
    "#f0e2c0": "#f4e8c8",
    "#f1dfb6": "#f3e4bc",
    "#f3e5c4": "#f6e9cb",
    "#e5cc91": "#ebd79b",
    "#ddc17f": "#e7ce94",
    "#e6d3aa": "#ebdcb4",
    "#e8d5a9": "#eddcae",
    "#d9c28d": "#e3ce9b",
    # ---- Nền giấy / be ----
    "#f4eddd": "#f8f2e6",
    "#faf5e7": "#fbf7ec",
    "#f7f0df": "#faf5e8",
    "#fcf7e9": "#fcf9ee",
    "#f7f3e8": "#faf6ec",
    "#f7f5ee": "#faf8f2",
    "#f8f7f2": "#fbfaf7",
    "#fbfaf6": "#fcfbf8",
    "#f5f1e7": "#f8f5ee",
    "#f3f4ed": "#f6f7f2",
    "#ece8dc": "#efede6",
    "#e9e8dd": "#edece5",
    "#e8d8ac": "#eee1be",
    # ---- Xanh xám nhạt ----
    "#e4ebe5": "#eaedf2",
    "#e5eee9": "#ebeff4",
    "#edf5ef": "#f0f4f8",
    "#dce9e2": "#e2e9f0",
    "#e8dfc9": "#eee6d4",
    "#d8dfe6": "#e0e5ec",
    "#eef3f6": "#f1f4f8",
    "#273c50": "#2b2036",
    # ---- Dải xanh rêu còn sót lại trong app.css ----
    "#2b3d36": "#2c1a42",
    "#243832": "#28183f",
    "#17211e": "#1c1030",
    "#40594e": "#3a2358",
    "#496258": "#6b5f7a",
    "#58756b": "#6b3a9e",
    "#1d281f": "#241137",
    "#1d2b26": "#241137",
    "#19241f": "#201029",
    "#192521": "#221432",
    "#15221d": "#1c1030",
    "#8c9e97": "#9aa7b4",
    "#9cb0a7": "#a3aab8",
    "#aab8b1": "#a79bb8",
    "#d8e2dc": "#dde4ec",
    "#e4ebe6": "#e8edf2",
    "#e7f0eb": "#ebeff4",
}

# Các cặp rgba() nền jade cần đổi sang tím.
RGBA_MAP = {
    "rgba(25, 58, 52,": "rgba(58, 28, 94,",
    "rgba(25, 58, 52,": "rgba(58, 28, 94,",
    "rgba(19, 66, 61,": "rgba(58, 28, 94,",
    "rgba(24, 58, 51,": "rgba(72, 33, 110,",
    "rgba(24, 80, 73,": "rgba(74, 36, 114,",
    "rgba(26, 52, 44,": "rgba(60, 28, 96,",
    "rgba(8, 34, 31,": "rgba(24, 10, 44,",
    "rgba(12, 48, 43,": "rgba(38, 18, 62,",
    "rgba(16, 53, 49,": "rgba(52, 24, 82,",
    "rgba(28, 81, 73,": "rgba(60, 29, 94,",
    "rgba(40, 99, 90,": "rgba(74, 36, 114,",
    "rgba(66, 129, 107,": "rgba(107, 58, 158,",
    "rgba(66,129,107,": "rgba(107,58,158,",
    "rgba(47, 122, 102,": "rgba(90, 45, 135,",
    "rgba(22, 94, 78,": "rgba(70, 34, 108,",
    "rgba(24, 94, 78,": "rgba(70, 34, 108,",
    "rgba(0, 105, 92,": "rgba(107, 58, 158,",
    "rgba(19, 78, 68,": "rgba(60, 28, 94,",
    "rgba(19, 72, 65,": "rgba(88, 44, 131,",
    "rgba(16, 62, 58,": "rgba(59, 26, 92,",
    "rgba(37, 92, 84,": "rgba(76, 38, 118,",
    "rgba(50, 110, 100,": "rgba(88, 46, 130,",
    "rgba(45, 105, 95,": "rgba(84, 42, 126,",
    "rgba(33, 110, 100,": "rgba(84, 42, 126,",
    "rgba(38, 101, 90,": "rgba(74, 36, 114,",
    "rgba(30, 90, 80,": "rgba(68, 33, 104,",
    "rgba(23, 78, 69,": "rgba(62, 30, 96,",
    # ---- Nhóm bổ sung: dải jade còn sót lại trong app.css ----
    "rgba(24, 95, 87": "rgba(88, 44, 131",
    "rgba(24,95,87": "rgba(88,44,131",
    "rgba(40, 120, 110": "rgba(107, 58, 158",
    "rgba(40,120,110": "rgba(107,58,158",
    "rgba(13, 47, 43": "rgba(59, 26, 92",
    "rgba(13,47,43": "rgba(59,26,92",
    "rgba(14, 52, 48": "rgba(48, 22, 78",
    "rgba(14,52,48": "rgba(48,22,78",
    "rgba(11, 43, 39": "rgba(42, 20, 68",
    "rgba(11,43,39": "rgba(42,20,68",
    "rgba(19, 44, 41": "rgba(48, 22, 78",
    "rgba(19,44,41": "rgba(48,22,78",
    "rgba(10, 34, 32": "rgba(34, 16, 56",
    "rgba(10,34,32": "rgba(34,16,56",
    "rgba(9, 35, 31": "rgba(30, 14, 50",
    "rgba(9,35,31": "rgba(30,14,50",
    "rgba(9, 35, 32": "rgba(30, 14, 50",
    "rgba(9,35,32": "rgba(30,14,50",
    "rgba(12, 39, 35": "rgba(40, 20, 66",
    "rgba(12,39,35": "rgba(40,20,66",
    "rgba(10, 30, 26": "rgba(32, 16, 54",
    "rgba(10,30,26": "rgba(32,16,54",
    "rgba(8, 33, 31": "rgba(26, 12, 42",
    "rgba(8,33,31": "rgba(26,12,42",
    "rgba(8, 30, 28": "rgba(26, 12, 42",
    "rgba(8,30,28": "rgba(26,12,42",
    "rgba(8, 30, 26": "rgba(26, 12, 42",
    "rgba(8,30,26": "rgba(26,12,42",
    "rgba(6, 28, 25": "rgba(22, 10, 38",
    "rgba(6,28,25": "rgba(22,10,38",
    "rgba(16, 53, 49": "rgba(52, 24, 82",
    "rgba(16,53,49": "rgba(52,24,82",
    "rgba(25, 12, 27": "rgba(32, 14, 48",
    "rgba(25,12,27": "rgba(32,14,48",
    "rgba(74, 21, 75": "rgba(58, 28, 94",
    "rgba(74,21,75": "rgba(58,28,94",
    "rgba(22, 15, 25": "rgba(30, 14, 46",
    "rgba(22,15,25": "rgba(30,14,46",
    "rgba(29, 22, 32": "rgba(38, 20, 56",
    "rgba(29,22,32": "rgba(38,20,56",
    # ---- Vàng đồng cũ -> vàng cung đình ----
    "rgba(193, 139, 47": "rgba(184, 134, 11",
    "rgba(193,139,47": "rgba(184,134,11",
    "rgba(239, 209, 127": "rgba(242, 223, 164",
    "rgba(239,209,127": "rgba(242,223,164",
    "rgba(224, 184, 71": "rgba(212, 175, 55",
    "rgba(224,184,71": "rgba(212,175,55",
    "rgba(242, 207, 105": "rgba(242, 223, 164",
    "rgba(242,207,105": "rgba(242,223,164",
    "rgba(211, 167, 70": "rgba(212, 175, 55",
    "rgba(211,167,70": "rgba(212,175,55",
    "rgba(184, 133, 37": "rgba(184, 134, 11",
    "rgba(184,133,37": "rgba(184,134,11",
    "rgba(131, 197, 184": "rgba(196, 167, 231",
    "rgba(131,197,184": "rgba(196,167,231",
}


def migrate(path: Path, dry_run: bool) -> tuple[int, int]:
    text = path.read_text(encoding="utf-8")
    original = text

    for old_hex, new_hex in HEX_MAP.items():
        text = text.replace(old_hex.upper(), new_hex).replace(old_hex, new_hex)

    for old_rgba, new_rgba in RGBA_MAP.items():
        text = text.replace(old_rgba, new_rgba)

    if text == original:
        return 0, 0

    changed_hex = sum(
        original.upper().count(old.upper()) for old in HEX_MAP
    )
    changed_rgba = sum(original.count(old) for old in RGBA_MAP)

    if not dry_run:
        path.write_text(text, encoding="utf-8")

    return changed_hex, changed_rgba


def main() -> int:
    parser = argparse.ArgumentParser(description="Đổi bảng màu jade sang tím hoàng gia & vàng cung đình.")
    parser.add_argument("files", nargs="*", help="Các tệp CSS cần xử lý.")
    parser.add_argument("--dry-run", action="store_true", help="Chỉ báo cáo, không ghi tệp.")
    args = parser.parse_args()

    targets = [Path(f).resolve() for f in args.files] if args.files else DEFAULT_FILES

    for target in targets:
        if not target.exists():
            print(f"[!] Không tìm thấy: {target}")
            continue
        hex_count, rgba_count = migrate(target, args.dry_run)
        verb = "sẽ đổi" if args.dry_run else "đã đổi"
        print(f"[+] {target.name}: {verb} {hex_count} mã HEX và {rgba_count} mã RGBA")

    return 0


if __name__ == "__main__":
    sys.exit(main())
