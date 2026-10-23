"""Generate a VietQR/EMVCo payment QR from local bank details.

The script takes bank details from command-line arguments so real account
information is never stored in this tracked source file. Use it for local
testing or for assets that are excluded from Git.

Example:
    python tools/make-vietqr.py ^
        --bank-bin 970422 ^
        --account 0123456789 ^
        --holder "CONG TY VI DU" ^
        --output HueTour.WebApp/wwwroot/images/vietqr-local.png
"""

from __future__ import annotations

import argparse
from decimal import Decimal, InvalidOperation
from pathlib import Path

import segno


def tlv(tag: str, value: str) -> str:
    encoded_length = len(value)
    if encoded_length > 99:
        raise ValueError(f"TLV value for tag {tag} is too long ({encoded_length} chars)")
    return f"{tag}{encoded_length:02d}{value}"


def crc16_ccitt(data: str) -> str:
    crc = 0xFFFF
    for byte in data.encode("ascii"):
        crc ^= byte << 8
        for _ in range(8):
            crc = ((crc << 1) ^ 0x1021) & 0xFFFF if crc & 0x8000 else (crc << 1) & 0xFFFF
    return f"{crc:04X}"


def build_payload(
    bank_bin: str,
    account_number: str,
    account_holder: str,
    city: str,
    amount: Decimal | None = None,
    content: str | None = None,
) -> str:
    if not bank_bin.isdigit() or len(bank_bin) != 6:
        raise ValueError("--bank-bin must be a 6-digit NAPAS bank BIN")
    if not account_number.isdigit() or not 6 <= len(account_number) <= 19:
        raise ValueError("--account must contain 6-19 digits")
    if not account_holder.strip():
        raise ValueError("--holder is required")

    beneficiary = (
        tlv("00", bank_bin)
        + tlv("01", account_number)
        + tlv("02", "QRIBFTTA")
    )

    payload = tlv("00", "01")
    payload += tlv("01", "12" if amount is not None else "11")
    payload += tlv("38", beneficiary)
    payload += tlv("53", "704")

    if amount is not None:
        if amount <= 0:
            raise ValueError("--amount must be greater than zero")
        if amount != amount.to_integral_value():
            raise ValueError("--amount must be a whole number of VND")
        payload += tlv("54", str(int(amount)))

    payload += tlv("58", "VN")
    payload += tlv("59", account_holder.strip()[:25])
    payload += tlv("60", city.strip()[:15])

    if content:
        payload += tlv("62", tlv("08", content.strip()[:25]))

    payload += "6304"
    return payload + crc16_ccitt(payload)


def parse_tlv(payload: str) -> dict[str, str]:
    fields: dict[str, str] = {}
    index = 0
    while index < len(payload):
        tag = payload[index : index + 2]
        length = int(payload[index + 2 : index + 4])
        value = payload[index + 4 : index + 4 + length]
        fields[tag] = value
        index += 4 + length
    return fields


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--bank-bin", required=True, help="6-digit NAPAS bank BIN, e.g. 970422")
    parser.add_argument("--account", required=True, help="Beneficiary account number")
    parser.add_argument("--holder", required=True, help="Beneficiary account holder (ASCII recommended)")
    parser.add_argument("--city", default="HUE", help="Merchant city (default: HUE)")
    parser.add_argument("--amount", help="Optional whole VND amount for a dynamic QR")
    parser.add_argument("--content", help="Optional transfer content for a dynamic QR")
    parser.add_argument("--output", required=True, help="Output PNG path")
    args = parser.parse_args()

    amount = None
    if args.amount:
        try:
            amount = Decimal(args.amount)
        except InvalidOperation as exc:
            raise SystemExit("--amount must be a number") from exc

    payload = build_payload(
        bank_bin=args.bank_bin,
        account_number=args.account,
        account_holder=args.holder,
        city=args.city,
        amount=amount,
        content=args.content,
    )

    # Validate the generated structure before writing the image.
    root = parse_tlv(payload)
    if root.get("00") != "01" or root.get("53") != "704" or root.get("58") != "VN":
        raise RuntimeError("Generated payload failed basic EMVCo validation")
    expected_crc = payload[-4:]
    if crc16_ccitt(payload[:-4]) != expected_crc:
        raise RuntimeError("Generated payload failed CRC validation")
    beneficiary = parse_tlv(root["38"])
    if beneficiary.get("01") != args.account:
        raise RuntimeError("Generated payload does not contain the expected account number")

    output = Path(args.output)
    output.parent.mkdir(parents=True, exist_ok=True)
    qr = segno.make(payload, error="M", micro=False)
    qr.save(output, scale=8, border=4, dark="#111111", light="#ffffff")

    print(f"output: {output}")
    print(f"amount: {amount if amount is not None else 'not encoded (static QR)'}")
    print(f"account: {args.account[:4]}***{args.account[-3:]}")
    print(f"crc: {expected_crc}")


if __name__ == "__main__":
    main()
