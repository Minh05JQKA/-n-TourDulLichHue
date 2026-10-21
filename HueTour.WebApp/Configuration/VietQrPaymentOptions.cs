namespace HueTour.WebApp.Configuration;

public sealed class VietQrPaymentOptions
{
    public const string SectionName = "VietQrPayment";

    public string BankCode { get; set; } = "DEMO";
    public string BankName { get; set; } = "Demo Bank";
    public string AccountHolder { get; set; } = "NGUYEN VAN A";
    public string AccountNumber { get; set; } = "0123456789";
    public string QrImagePath { get; set; } = string.Empty;
    public string TransferContentTemplate { get; set; } = "HUETOUR {BookingCode} {PhoneNumber}";

    public string FormatTransferContent(string? bookingCode, string? phoneNumber)
    {
        var content = TransferContentTemplate
            .Replace("{BookingCode}", bookingCode?.Trim() ?? string.Empty, StringComparison.Ordinal)
            .Replace("{PhoneNumber}", phoneNumber?.Trim() ?? string.Empty, StringComparison.Ordinal);

        return string.Join(' ', content.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
