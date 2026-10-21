#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
=============================================================================
 HUE TOUR · BỘ NHẬP ĐỊA ĐIỂM ẨM THỰC
=============================================================================
 Đây là "prompt" được đóng gói thành công cụ dòng lệnh cho đồ án HueTour.

 Mục tiêu
 --------
 Lấy dữ liệu địa điểm ẩm thực (nhà hàng, quán cà phê, tiệm bánh, quán bar...)
 từ dự án mẫu HUEGO (github.com/hoangduong030405-art/Do_An_Khoi_Nghiep_-MST)
 rồi chuẩn hoá về đúng lược đồ của HueTour để nạp vào CSDL.

 Cách chạy
 ---------
   python scripts/import_food_places.py --source <đường dẫn tới repo mẫu>
   python scripts/import_food_places.py --source <repo mẫu> --limit 400 --strict

 Tham số
 -------
   --source   Đường dẫn tới bản clone của repo mẫu. Bắt buộc.
   --output   Thư mục ghi kết quả (mặc định: database/seed).
   --limit    Số địa điểm tối đa được nạp (mặc định: 600).
   --min-rating  Bỏ qua địa điểm có đánh giá thấp hơn mức này (mặc định: 3.5).
   --strict   Chỉ giữ địa điểm có tọa độ hợp lệ nằm trong bán kính 25km quanh
              trung tâm Huế và có tên khớp bộ lọc ẩm thực.
   --no-images Bỏ qua việc tải ảnh về wwwroot.

 Kết quả sinh ra
 ---------------
   database/seed/dia-diem-am-thuc.json   Dữ liệu chuẩn (nguồn sự thật duy nhất)
   database/DiaDiemAmThuc_Seed.sql       Lệnh INSERT cho SQL Server
   HueTour.WebApp/wwwroot/images/food/   Ảnh tải về từ nguồn của dự án mẫu

 Tính minh bạch
 --------------
 Mọi bản ghi đều mang trường "Nguồn" ghi rõ tệp dữ liệu gốc. Ảnh tải về từ
 CDN công khai của dự án mẫu, vẫn giữ nguyên đường dẫn gốc trong trường
 "ẢnhGoc" để đối chiếu giấy phép. Xem tài liệu nguồn gốc dữ liệu ở ASSETS.md.
=============================================================================
"""

from __future__ import annotations

import argparse
import json
import math
import re
import sys
import unicodedata
from pathlib import Path
from typing import Any, Iterable

# Windows console mặc định dùng cp1252, không in được tiếng Việt có dấu.
for _stream in (sys.stdout, sys.stderr):
    if hasattr(_stream, "reconfigure"):
        try:
            _stream.reconfigure(encoding="utf-8", errors="replace")
        except (ValueError, OSError):  # pragma: no cover
            pass

# --------------------------------------------------------------------------
# Hằng số cấu hình
# --------------------------------------------------------------------------

REPO_ROOT = Path(__file__).resolve().parent.parent
DEFAULT_OUTPUT_DIR = REPO_ROOT / "database" / "seed"
DEFAULT_IMAGE_DIR = REPO_ROOT / "HueTour.WebApp" / "wwwroot" / "images" / "food"

# Trung tâm Hoàng thành Huế + bán kính hợp lý cho các điểm ăn uống trong tỉnh
HUE_CENTER_LAT = 16.4637
HUE_CENTER_LNG = 107.5909
MAX_RADIUS_KM = 25.0

# Thẻ dữ liệu "vertical" của dự án mẫu.
VERTICAL_FOOD = "fnb"      # ăn uống
VERTICAL_CAFE = "cafe"     # quán cà phê

# ----------------------------------------------------------------------
# Danh sách loại trừ.
# Dự án mẫu gom nhiều dịch vụ khác vào nhóm "fnb", nên phải loại các mục
# không phải nơi ăn uống (bơi xe, tiệm cắt tóc, rạp chiếu phim...).
# ----------------------------------------------------------------------
EXCLUDED_SUBCATEGORIES = {
    "tiệm cắt tóc", "hiệu lý", "công ty", "spa", "bể bơi", "phòng gym",
    "bến xe buýt", "bãi đỗ xe", "rạp chiếu phim", "cầu lắc lọc bowling",
    "dịch vụ gặt tàu", "dịch vụ taxi", "dịch vụ thuê xe đạp",
    "dịch vụ thuê xe máy", "đại lý cho thuê xe máy", "điểm thu hút khách du lịch",
    "điểm dịch vụ phục vụ cơ quan", "khách sạn", "công viên", "nhà máy",
    "trung tâm", "bưu điện", "bệnh viện", "trường", "nhà tang",
    "nghĩa trang liệm", "sân vận động", "sân bay", "cảng", "nhà kho",
    "xưởng", "xí nghiệp", "cơ quan hành chính", "chợ", "siêu thị",
    "cửa hàng", "cửa hàng tạp phẩm", "tiệm tạp hóa nga", "gian hàng rong",
    "nhà hàng ăn nhanh",  # giữ lại thành mục riêng? -> loại, quán ăn nhanh đã có
}

# ----------------------------------------------------------------------
# Từ khóa nhận diện nơi ăn uống trong tên / phân loại phụ.
# Ưu tiên "chỉ giữ nếu có từ khóa" để dữ liệu sạch hơn.
# ----------------------------------------------------------------------
FOOD_KEYWORDS = [
    # Tiếng Việt
    "nhà hàng", "quán ăn", "quán cà phê", "quán cafe", "cà phê", "quán bún",
    "quán phở", "quán hải sản", "quán nhậu", "quán lẩu", "quán gà", "quán bò",
    "quán chay", "quán mì", "quán bia", "quán rượu", "quán bar", "quán trà",
    "tiệm bánh", "tiệm cắt tóc", "tiệm ăn", "tiệm hải sản", "tiệm lẩu",
    "bún bò", "bánh bèo", "bánh khoái", "bánh mì", "bánh bưởng", "bánh ít",
    "chè huế", "cơm hến", "mì Quảng", "bánh xèo", "hủ tiếu", "bánh cuốn",
    "chảo lòa", "nem lụi", "gỏi cuốn", "bánh bột lọc", "bánh đa", "bánh bá trạng",
    "cơm tấm", "cơm gà", "lẩu", "nướng", "ẩm thực", "am thuc", "món nướng",
    "đồ ăn", "phở", "bún", "chè", "cà phê", "trà", "kem", "bánh ngọt",
    "ẩm thực đường phố", "street food", "food", "cuisine",
    # Tiếng Anh
    "restaurant", "cafe", "coffee", "bakery", "bistro", "pizza", "burger",
    "buffet", "grill", "bar ", "pub", "tea ", "dessert", "noodle", "hotpot",
    "kitchen", "eatery", "lounge", "rooftop", "food court", "shophouse",
    "steakhouse", "bbq", "chicken", "seafood", "vegan", "vegetarian",
]

# Phân loại chuẩn hoá -> nhãn hiển thị tiếng Việt
CATEGORY_LABELS = {
    "restaurant": "Nhà hàng",
    "cafe": "Quán cà phê",
    "bakery": "Tiệm bánh",
    "bar": "Quán bar",
    "streetfood": "Ẩm thực đường phố",
    "fastfood": "Quán ăn nhanh",
    "seafood": "Nhà hàng hải sản",
    "hotpot": "Quán lẩu",
    "noodle": "Quán mì · bún",
    "grill": "Quán nướng",
    "dessert": "Tiệm bánh ngọt · kem",
    "vegetarian": "Nhà hàng chay",
    "other": "Điểm ăn uống",
}

# Từ khóa trong phân loại phụ -> mã phân loại chuẩn
SUBCATEGORY_TO_CATEGORY = {
    "nhà hàng": "restaurant",
    "nhà hàng gia đình": "restaurant",
    "nhà hàng việt nam": "restaurant",
    "nhà hàng món nướng": "grill",
    "nhà hàng món lẩu": "hotpot",
    "nhà hàng hải sản": "seafood",
    "nhà hàng món cá": "seafood",
    "nhà hàng món bò": "restaurant",
    "nhà hàng món thịt": "restaurant",
    "nhà hàng món chay": "vegetarian",
    "nhà hàng ăn chay": "vegetarian",
    "nhà hàng cho người ăn chay": "vegetarian",
    "nhà hàng chuyên phở bún bò huế": "noodle",
    "nhà hàng chuyên phở bắt sống": "noodle",
    "nhà hàng pizza": "restaurant",
    "nhà hàng hàn quốc": "restaurant",
    "nhà hàng nhật bản": "restaurant",
    "nhà hàng thái lan": "restaurant",
    "nhà hàng trung quốc": "restaurant",
    "nhà hàng pháp": "restaurant",
    "nhà hàng mỹ": "restaurant",
    "nhà hàng ý": "restaurant",
    "nhà hàng châu âu": "restaurant",
    "nhà hàng châu á": "restaurant",
    "nhà hàng ẩm thực đồng quê": "restaurant",
    "nhà hàng trọng miệng": "restaurant",
    "nhà hàng cơm": "restaurant",
    "nhà hàng cơm tấm": "restaurant",
    "nhà hàng ăn nhanh": "fastfood",
    "nhà hàng bánh mì": "fastfood",
    "nhà hàng bán đồ ăn mang về": "fastfood",
    "nhà hàng bún": "noodle",
    "nhà hàng sushi": "restaurant",
    "nhà hàng takoyaki": "restaurant",
    "nhà hàng mexican": "restaurant",
    "ẩm thực huế": "restaurant",
    "ẩm thực cung đình": "restaurant",
    "quán ăn": "restaurant",
    "quán ăn nhỏ": "restaurant",
    "quán nhậu": "restaurant",
    "quán mì": "noodle",
    "quán lẩu": "hotpot",
    "quán hải sản": "seafood",
    "quán gà": "restaurant",
    "quán bò": "restaurant",
    "quán chay": "vegetarian",
    "quán bún": "noodle",
    "quán phở": "noodle",
    "quán cà phê": "cafe",
    "quán cafe": "cafe",
    "quán cà phê mèo": "cafe",
    "quán cà phê espresso": "cafe",
    "quán cà phê đồng vật": "cafe",
    "quán cà phê nghệ thuật": "cafe",
    "quán cà phê hoặc quán cà phê": "cafe",
    "trà trân châu": "cafe",
    "cửa hàng bán nước quả ép": "cafe",
    "cơ sở rang cà phê": "cafe",
    "cửa hàng": "cafe",
    "quán bar": "bar",
    "quán bar cocktail": "bar",
    "quán bar ca nhạc trực tiếp": "bar",
    "quán bia": "bar",
    "quán rượu": "bar",
    "quầy giải khát": "streetfood",
    "khu ăn uống": "streetfood",
    "ẩm thực đường phố": "streetfood",
    "tiệm bánh": "bakery",
    "tiệm bánh kẹo": "bakery",
    "tiệm bánh mì": "bakery",
    "tiệm cá tấm": "seafood",
    "tiệm ăn uống": "restaurant",
    "cửa hàng bánh": "bakery",
    "cửa hàng bánh ngọt": "dessert",
    "cửa hàng kem": "dessert",
    "cửa hàng bán đồ trong miệng": "dessert",
    "cửa hàng cà phê": "cafe",
    "phòng trà": "cafe",
    "cơ sở ăn uống lưu động": "streetfood",
    "vietnamese": "restaurant",
    "local": "restaurant",
}

# Từ khoá nằm trong TÊN quán chắc chắn là dịch vụ khác ẩm thực.
NON_FOOD_NAME_PATTERNS = [
    "barbershop", "barber", "hairdo", "cắt tóc", "hiệu lý", "spa", "massage",
    "bể bơi", "gym", "yoga", "bóng đá", "cầu lắc lọc", "bowling", "rạp chiếu",
    "rạp phim", "cinema", "taxi", "gặt tàu", "thuê xe", "cho thuê xe",
    "đại lý", "bến xe", "bãi đỗ", "photocopy", "in ấn", "giặt", "laundry",
    "hiệu thuốc", "nhà thuốc", "bệnh viện", "phòng khám", "trường", "mầm non",
    "văn phòng", "công ty", "chi nhánh", "đại lý bảo hiểm", "bảo hiểm",
    "cây xăng", "trạm xăng", "petrolimex", "xe máy", "xe đạp", "ô tô",
    "bán xe", "mua bán", "sửa xe", "rửa xe", "giữ xe", "bãi giữ xe",
    "ngân hàng", "atm", "bưu điện", "viễn thông", "fpt", "vinaphone", "viettel",
    "siêu thị", "convenience", "minimart", "cửa hàng", "tiệm tạp hóa",
    "xưởng", "nhà máy", "công viên", "sân vận động", "nhà thi đấu", "nhà tang",
    "nghĩa trang liệm", "tang lễ", "giấy phép", "luật", "văn phòng công",
]

# Món ăn đặc trưng suy ra từ tên quán.
DISH_HINTS = [
    ("bun bo hue", "Bún bò Huế"), ("bun bo", "Bún bò Huế"),
    ("banh beo", "Bánh bèo"), ("banh khoai", "Bánh khoái"),
    ("nam loc", "Nậm · Lọc"), ("banh loc", "Lọc"),
    ("banh beo chien", "Bánh bèo chiên"), ("banh kho", "Bánh khoái"),
    ("com hen", "Cơm hến"), ("mi quang", "Mì Quảng"),
    ("banh mi", "Bánh mì"), ("banh xeo", "Bánh xèo"),
    ("che", "Chè Huế"), ("ca phe", "Cà phê"), ("cafe", "Cà phê"),
    ("pho", "Phở"), ("bun", "Bún"), ("noodle", "Mì"),
    ("pizza", "Pizza"), ("burger", "Burger"), ("hotpot", "Lẩu"),
    ("lau", "Lẩu"), ("seafood", "Hải sản"), ("haisan", "Hải sản"),
    ("grill", "Nướng"), ("bbq", "Nướng"), ("bakery", "Bánh"),
    ("banh ngot", "Bánh ngọt"), ("kem", "Kem"),
    ("nha hang", "Món Việt"), ("quan", "Món Việt"),
]


# --------------------------------------------------------------------------
# Tiện ích
# --------------------------------------------------------------------------

def strip_accents(text: str) -> str:
    """Bỏ dấu tiếng Việt, giữ nguyên chữ cái không dấu."""
    if not text:
        return ""
    normalized = unicodedata.normalize("NFD", text)
    return "".join(ch for ch in normalized if unicodedata.category(ch) != "Mn")


def normalize(text: str) -> str:
    return strip_accents(text or "").lower().strip()


def haversine_km(lat1: float, lng1: float, lat2: float, lng2: float) -> float:
    radius = 6371.0
    d_lat = math.radians(lat2 - lat1)
    d_lng = math.radians(lng2 - lng1)
    a = (math.sin(d_lat / 2) ** 2
         + math.cos(math.radians(lat1)) * math.cos(math.radians(lat2))
         * math.sin(d_lng / 2) ** 2)
    return radius * 2 * math.atan2(math.sqrt(a), math.sqrt(1 - a))


def clean_text(value: Any) -> str:
    if value is None:
        return ""
    text = re.sub(r"\s+", " ", str(value)).strip()
    # Loại ký tự điều khiển mà dữ liệu cào về có thể chứa.
    return "".join(ch for ch in text if ch.isprintable())


def pick_ward(address: str) -> str:
    """Lấy phường/xã từ chuỗi địa chỉ kiểu '..., Phường Vĩnh Ninh, TP Huế'."""
    if not address:
        return ""
    parts = [p.strip() for p in address.split(",")]
    for part in parts:
        low = normalize(part)
        if low.startswith("phuong ") or low.startswith("xa ") or low.startswith("thi tran"):
            return clean_text(part)
    return ""


def to_float(value: Any) -> float | None:
    try:
        if value is None or value == "":
            return None
        return float(value)
    except (TypeError, ValueError):
        return None


def to_int(value: Any) -> int | None:
    number = to_float(value)
    return int(number) if number is not None else None


def classify(subcategories: Iterable[str], name: str) -> str:
    """Đoán nhóm danh mục chuẩn hoá từ phân loại phụ + tên quán."""
    for sub in subcategories:
        key = normalize(sub)
        if key in SUBCATEGORY_TO_CATEGORY:
            return SUBCATEGORY_TO_CATEGORY[key]

    haystack = normalize(" ".join(subcategories) + " " + name)
    if any(normalize(k) in haystack for k in ("nha hang", "restaurant")):
        return "restaurant"
    if any(normalize(k) in haystack for k in ("cafe", "ca phe", "coffee")):
        return "cafe"
    if any(normalize(k) in haystack for k in ("bakery", "tiem banh")):
        return "bakery"
    if any(normalize(k) in haystack for k in ("quan bar", "quan bia", "quan ruou", "cocktail")):
        return "bar"
    if any(normalize(k) in haystack for k in ("am thuc", "am thuc hue", "mon ", "com ")):
        return "restaurant"
    if any(normalize(k) in haystack for k in ("lau", "hotpot")):
        return "hotpot"
    if any(normalize(k) in haystack for k in ("hai san", "seafood")):
        return "seafood"
    if any(normalize(k) in haystack for k in ("noodles", "pho ", "bun ")):
        return "noodle"
    if any(normalize(k) in haystack for k in ("banh ngot", "kem ", "dessert")):
        return "dessert"
    if any(normalize(k) in haystack for k in ("chay", "vegan", "vegetarian")):
        return "vegetarian"
    if any(normalize(k) in haystack for k in ("nướng", "nuong", "grill", "bbq")):
        return "grill"
    # Tên kiểu "36 Quán", "A Bở Quán", "Bánh Bèo Bà Đỏ" vẫn là nơi ăn uống.
    if "quan" in haystack:
        return "restaurant"
    return "other"


def is_non_food_name(name: str) -> bool:
    haystack = normalize(name)
    return any(normalize(pattern) in haystack for pattern in NON_FOOD_NAME_PATTERNS)


def looks_like_food(subcategories: Iterable[str], name: str) -> bool:
    haystack = normalize(" ".join(subcategories) + " " + name)
    return any(normalize(keyword) in haystack for keyword in FOOD_KEYWORDS)


def detect_dish(name: str, subcategories: Iterable[str]) -> str:
    haystack = normalize(name + " " + " ".join(subcategories))
    for token, dish in DISH_HINTS:
        if token in haystack:
            return dish
    return ""


def build_description(name: str, category: str, dish: str, ward: str) -> str:
    label = CATEGORY_LABELS.get(category, CATEGORY_LABELS["other"])
    pieces = [f"{label} tại Huế."]
    if dish:
        pieces.append(f"Điểm nhấn ẩm thực: {dish}.")
    if ward:
        pieces.append(f"Địa chỉ thuộc khu vực {ward}.")
    pieces.append("Thông tin giá, giờ mở cửa và món tiêu biểu chưa được kiểm chứng.")
    return " ".join(pieces)


# --------------------------------------------------------------------------
# Đọc nguồn dữ liệu
# --------------------------------------------------------------------------

def load_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def collect_candidates(source_repo: Path) -> list[dict[str, Any]]:
    """Gộp dữ liệu từ places_all.json và clean_46_places.json của repo mẫu."""
    data_dir = source_repo / "backend" / "data"
    candidates: list[dict[str, Any]] = []

    places_all = data_dir / "places_all.json"
    if places_all.exists():
        for record in load_json(places_all):
            if record.get("vertical") not in (VERTICAL_FOOD, VERTICAL_CAFE):
                continue
            candidates.append({
                "slug": clean_text(record.get("id")),
                "vertical": clean_text(record.get("vertical")),
                "name": clean_text(record.get("name_vi") or record.get("name")),
                "address": clean_text(record.get("address_vi") or record.get("address")),
                "latitude": to_float(record.get("lat")),
                "longitude": to_float(record.get("lng")),
                "phone": clean_text(record.get("phone")),
                "rating": to_float(record.get("googleRating")),
                "reviewCount": to_int(record.get("googleReviewCount")),
                "image": clean_text(record.get("coverPhotoUrl")),
                "subcategories": [clean_text(s) for s in (record.get("subcategory") or [])],
                "source": "places_all.json",
                "description": "",
                "ticketPrice": None,
            })

    clean_places = data_dir / "clean_46_places.json"
    if clean_places.exists():
        for record in load_json(clean_places):
            code = (record.get("categoryCode") or "").upper()
            if code not in ("CUISINE", "CAFE"):
                continue
            candidates.append({
                "slug": clean_text(record.get("id")),
                "vertical": VERTICAL_FOOD if code == "CUISINE" else VERTICAL_CAFE,
                "name": clean_text(record.get("name")),
                "address": clean_text(record.get("address")),
                "latitude": to_float(record.get("latitude")),
                "longitude": to_float(record.get("longitude")),
                "phone": "",
                "rating": None,
                "reviewCount": None,
                "image": clean_text(record.get("thumbnailUrl")),
                "subcategories": [clean_text(record.get("categoryName"))],
                "source": "clean_46_places.json",
                "description": clean_text(record.get("descriptionVi")),
                "ticketPrice": to_float(record.get("ticketPriceVnd")),
            })

    return candidates


# --------------------------------------------------------------------------
# Chuẩn hoá + lọc
# --------------------------------------------------------------------------

def normalize_candidates(
    candidates: list[dict[str, Any]],
    *,
    limit: int,
    min_rating: float,
    min_reviews: int,
    strict: bool,
    per_category: int,
) -> list[dict[str, Any]]:
    """
    Lọc và chuẩn hoá danh sách địa điểm ẩm thực.

    Chiến lược chọn: gom toàn bộ ứng viên hợp lệ theo nhóm danh mục, lấy tối đa
    `per_category` bản ghi cho mỗi nhóm (bảo đảm không bị chiếm hết chỗ bởi
    nhà hàng), sau đó lấp các khoảng trống còn lại từ những ứng viên có điểm
    đánh giá cao nhất.
    """
    seen_slugs: set[str] = set()
    seen_names: set[str] = set()
    buckets: dict[str, list[dict[str, Any]]] = {}

    # Ưu tiên bản ghi có mô tả và giá (clean_46) đứng trước.
    candidates.sort(key=lambda item: (0 if item.get("description") else 1,))

    for item in candidates:
        name = item["name"]
        if not name:
            continue

        slug = item["slug"] or re.sub(r"[^a-z0-9]+", "-", normalize(name)).strip("-")
        if not slug or slug in seen_slugs:
            continue

        name_key = normalize(name)
        if name_key in seen_names:
            continue

        subcategories = item["subcategories"]
        if any(normalize(sub) in EXCLUDED_SUBCATEGORIES for sub in subcategories):
            continue
        if is_non_food_name(name):
            continue

        vertical = item.get("vertical") or ""
        curated = bool(item.get("description"))

        # Bản ghi biên tập tay (clean_46) luôn giữ lại.
        if not curated and not looks_like_food(subcategories, name):
            if vertical not in (VERTICAL_FOOD, VERTICAL_CAFE):
                continue

        lat, lng = item["latitude"], item["longitude"]
        if lat is None or lng is None or not (-90 <= lat <= 90) or not (-180 <= lng <= 180):
            if strict:
                continue
            lat = lng = None
        elif strict and haversine_km(HUE_CENTER_LAT, HUE_CENTER_LNG, lat, lng) > MAX_RADIUS_KM:
            continue

        rating = item["rating"]
        reviews = item["reviewCount"] or 0
        if not curated:
            if rating is None:
                continue
            if rating < min_rating:
                continue
            # Đánh giá từ 1-2 người dùng thường là nhiễu, trừ khi điểm rất cao.
            if reviews < min_reviews and rating < 4.8:
                continue

        category = classify(subcategories, name)
        if category == "other":
            continue

        # Ưu tiên quán cà phê cho nhóm vertical "cafe" nếu phân loại phụ không rõ.
        if vertical == VERTICAL_CAFE and category == "restaurant" and not any(
            normalize(sub) in SUBCATEGORY_TO_CATEGORY for sub in subcategories
        ):
            category = "cafe"

        ward = pick_ward(item["address"])
        dish = detect_dish(name, subcategories)
        description = item["description"] or build_description(name, category, dish, ward)

        price = item["ticketPrice"]
        average_price = int(price) if price else None

        seen_slugs.add(slug)
        seen_names.add(name_key)
        buckets.setdefault(category, []).append({
            "MaDiaDiem": 0,
            "Slug": slug,
            "TenDiaDiem": name,
            "DanhMuc": CATEGORY_LABELS.get(category, CATEGORY_LABELS["other"]),
            "MaDanhMuc": category,
            "MonDacTrung": dish,
            "MoTa": description,
            "DiaChi": item["address"],
            "PhuongXa": ward,
            "ToaDoLat": lat,
            "ToaDoLng": lng,
            "SoDienThoai": item["phone"],
            "GiaThamKhao": average_price,
            "DiemDanhGia": rating,
            "SoDanhGia": reviews or None,
            "AnhMinhHoa": "",
            "AnhGoc": item["image"],
            "Nguon": item["source"],
            "DuocDoiChieu": curated,
            "_sort": (0 if curated else 1, -(rating or 0), -reviews),
        })

    # Vòng 1: mỗi danh mục lấy tối đa per_category bản ghi tốt nhất.
    selected: list[dict[str, Any]] = []
    for category in sorted(buckets):
        bucket = sorted(buckets[category], key=lambda r: r["_sort"])
        selected.extend(bucket[:per_category])

    # Vòng 2: lấp nốt khoảng trống còn lại theo điểm đánh giá giảm dần.
    if len(selected) < limit:
        taken = {record["Slug"] for record in selected}
        leftovers = [
            record
            for bucket in buckets.values()
            for record in bucket
            if record["Slug"] not in taken
        ]
        leftovers.sort(key=lambda r: r["_sort"])
        selected.extend(leftovers[: max(0, limit - len(selected))])

    selected = selected[:limit]
    for index, record in enumerate(selected, start=1):
        record["MaDiaDiem"] = index
        record.pop("_sort", None)

    return selected


# --------------------------------------------------------------------------
# Sinh tệp kết quả
# --------------------------------------------------------------------------

def slugify_image_name(slug: str) -> str:
    return re.sub(r"[^a-z0-9._-]+", "-", slug.lower()).strip("-") or "food-place"


def write_json(records: list[dict[str, Any]], output_dir: Path) -> Path:
    output_dir.mkdir(parents=True, exist_ok=True)
    target = output_dir / "dia-diem-am-thuc.json"
    payload = {
        "NguonDuLieu": "Dự án mẫu HUEGO (Do_An_Khoi_Nghiep_-MST) - backend/data",
        "PhienBan": 1,
        "SoLuong": len(records),
        "GhiChu": (
            "Dữ liệu được lọc từ places_all.json (vertical=fnb|cafe) và bổ sung "
            "mô tả từ clean_46_places.json. Ảnh lấy từ CDN công khai của dự án mẫu, "
            "giữ nguyên đường dẫn gốc trong AnhGoc. Không dùng cho mục đích thương mại."
        ),
        "DiaDiem": records,
    }
    with target.open("w", encoding="utf-8") as handle:
        json.dump(payload, handle, ensure_ascii=False, indent=2)
    return target


def sql_escape(value: Any) -> str:
    if value is None:
        return "NULL"
    if isinstance(value, bool):
        return "1" if value else "0"
    if isinstance(value, (int, float)):
        return str(value)
    text = str(value).replace("'", "''")
    # Every string column is NVARCHAR; prefix literals to keep Vietnamese text intact in SQL Server.
    return f"N'{text}'"


def write_sql(records: list[dict[str, Any]], output_dir: Path) -> Path:
    output_dir.mkdir(parents=True, exist_ok=True)
    target = output_dir / "DiaDiemAmThuc_Seed.sql"

    lines = [
        "-- =====================================================================",
        "-- HUE TOUR · SEED ĐỊA ĐIỂM ẨM THỰC (Sinh tự động)",
        "-- =====================================================================",
        "-- Nguồn : Dự án mẫu HUEGO - backend/data (places_all.json, clean_46_places.json)",
        "-- Lệnh  : python scripts/import_food_places.py --source <repo mẫu>",
        f"-- Số bản ghi: {len(records)}",
        "-- Chạy sau khi đã tạo bảng dbo.DiaDiemAmThuc trong HueTour_SchemaAndData.sql",
        "-- =====================================================================",
        "SET NOCOUNT ON;",
        "SET IDENTITY_INSERT dbo.DiaDiemAmThuc ON;",
        "GO",
        "",
        "DELETE FROM dbo.DiaDiemAmThuc;",
        "",
    ]

    for record in records:
        cols = (
            "MaDiaDiem", "Slug", "TenDiaDiem", "DanhMuc", "MaDanhMuc", "MonDacTrung",
            "MoTa", "DiaChi", "PhuongXa", "ToaDoLat", "ToaDoLng", "SoDienThoai",
            "GiaThamKhao", "DiemDanhGia", "SoDanhGia", "AnhMinhHoa", "AnhGoc",
            "Nguon", "DuocDoiChieu",
        )
        values = ", ".join(sql_escape(record.get(col)) for col in cols)
        lines.append(
            "INSERT INTO dbo.DiaDiemAmThuc (" + ", ".join(cols) + ") VALUES (" + values + ");"
        )

    lines += [
        "SET IDENTITY_INSERT dbo.DiaDiemAmThuc OFF;",
        "GO",
        "PRINT N'Da nạp " + str(len(records)) + " địa điểm ẩm thực.';",
        "GO",
        "",
    ]
    target.write_text("\n".join(lines), encoding="utf-8")
    return target


# --------------------------------------------------------------------------
# Tải ảnh minh hoạ
# --------------------------------------------------------------------------

def download_images(records: list[dict[str, Any]], image_dir: Path, limit: int) -> int:
    try:
        import urllib.request
    except ImportError:  # pragma: no cover
        print("[!] Không có urllib.request, bỏ qua tải ảnh.")
        return 0

    image_dir.mkdir(parents=True, exist_ok=True)
    saved = 0
    for record in records[:limit]:
        remote = record.get("AnhGoc") or ""
        if not remote.startswith("http"):
            continue
        extension = ".webp" if ".webp" in remote.lower() else ".jpg"
        filename = slugify_image_name(record["Slug"]) + extension
        target = image_dir / filename
        if target.exists():
            record["AnhMinhHoa"] = f"/images/food/{filename}"
            continue
        try:
            request = urllib.request.Request(
                remote, headers={"User-Agent": "HueTour-Capstone/1.0 (educational import)"}
            )
            with urllib.request.urlopen(request, timeout=20) as response:
                payload = response.read()
            if len(payload) < 1024:
                continue
            target.write_bytes(payload)
            record["AnhMinhHoa"] = f"/images/food/{filename}"
            saved += 1
            print(f"    + {filename} ({len(payload) // 1024} KB)")
        except Exception as exc:  # noqa: BLE001 - ghi nhận và bỏ qua từng ảnh lỗi
            print(f"    ! Không tải được {remote}: {exc}")
    return saved


# --------------------------------------------------------------------------
# Main
# --------------------------------------------------------------------------

def main() -> int:
    parser = argparse.ArgumentParser(
        description="Nhập địa điểm ẩm thực từ dự án mẫu HUEGO vào HueTour.",
    )
    parser.add_argument("--source", required=True, help="Đường dẫn tới repo mẫu đã clone.")
    parser.add_argument("--output", default=str(DEFAULT_OUTPUT_DIR), help="Thư mục ghi JSON + SQL.")
    parser.add_argument("--image-dir", default=str(DEFAULT_IMAGE_DIR), help="Thư mục lưu ảnh.")
    parser.add_argument("--limit", type=int, default=600, help="Số địa điểm tối đa.")
    parser.add_argument("--min-rating", type=float, default=3.8, help="Điểm đánh giá tối thiểu.")
    parser.add_argument("--min-reviews", type=int, default=8, help="Số đánh giá tối thiểu.")
    parser.add_argument(
        "--per-category", type=int, default=160,
        help="Số bản ghi tối đa cho mỗi nhóm danh mục (giữ cân bằng).",
    )
    parser.add_argument("--image-limit", type=int, default=60, help="Số ảnh tải về.")
    parser.add_argument(
        "--strict", action="store_true", default=True,
        help="Chỉ giữ địa điểm trong bán kính 25km quanh Huế (mặc định: bật).",
    )
    parser.add_argument(
        "--no-strict", dest="strict", action="store_false",
        help="Giữ cả địa điểm ngoài bán kính 25km quanh Huế.",
    )
    parser.add_argument("--no-images", action="store_true", help="Không tải ảnh.")
    args = parser.parse_args()

    source_repo = Path(args.source).expanduser().resolve()
    if not (source_repo / "backend" / "data").exists():
        print(f"[x] Không tìm thấy backend/data trong: {source_repo}")
        print("    Hãy clone repo mẫu trước:")
        print("    git clone https://github.com/hoangduong030405-art/Do_An_Khoi_Nghiep_-MST.git")
        return 1

    print(f"[*] Đọc dữ liệu từ: {source_repo / 'backend' / 'data'}")
    candidates = collect_candidates(source_repo)
    print(f"[*] Số bản ghi ứng viên (fnb + cafe + clean_46): {len(candidates)}")

    records = normalize_candidates(
        candidates,
        limit=args.limit,
        min_rating=args.min_rating,
        min_reviews=args.min_reviews,
        strict=args.strict,
        per_category=args.per_category,
    )
    print(f"[*] Sau khi lọc: {len(records)} địa điểm ẩm thực hợp lệ.")

    if not args.no_images:
        print(f"[*] Đang tải tối đa {args.image_limit} ảnh minh hoạ...")
        download_images(records, Path(args.image_dir), args.image_limit)

    output_dir = Path(args.output).resolve()
    json_path = write_json(records, output_dir)
    sql_path = write_sql(records, output_dir)
    print(f"[+] Đã ghi: {json_path}")
    print(f"[+] Đã ghi: {sql_path}")

    by_category: dict[str, int] = {}
    for record in records:
        by_category[record["DanhMuc"]] = by_category.get(record["DanhMuc"], 0) + 1
    print("[*] Phân bố theo danh mục:")
    for category, count in sorted(by_category.items(), key=lambda kv: -kv[1]):
        print(f"      {category}: {count}")

    return 0


if __name__ == "__main__":
    sys.exit(main())
