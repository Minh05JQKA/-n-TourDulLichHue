"""Generate HueTour's safe, non-payment QR demo asset."""

from pathlib import Path

import segno


PAYLOAD = (
    "HUETOUR-DEMO-QR | Ma QR minh hoa cho do an capstone, "
    "khong dung de chuyen tien | 0123456789"
)
SCALE = 6
BORDER = 4
OUTPUT = (
    Path(__file__).resolve().parents[1]
    / "HueTour.WebApp"
    / "wwwroot"
    / "images"
    / "vietqr-demo.png"
)


def main() -> None:
    qr = segno.make(PAYLOAD, error="M", micro=False)
    width, height = qr.symbol_size(scale=SCALE, border=BORDER)
    if width != height or not 280 <= width <= 320:
        raise RuntimeError(f"Unexpected demo QR size: {width}x{height}")

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    qr.save(
        OUTPUT,
        scale=SCALE,
        border=BORDER,
        dark="#111111",
        light="#ffffff",
    )

    print(f"payload: {PAYLOAD}")
    print(f"output: {OUTPUT.name}")
    print(f"size: {width}x{height}px")


if __name__ == "__main__":
    main()
