using System.Text;
using System.Text.Json;
using QRCoder;
using Ticketing.Contracts.Bookings;

namespace Ticketing.Web.Helpers;

public static class QrCodeHelper
{
    public static string? GenerateQrCode(Booking booking)
    {
        if (booking.Status != TicketStatus.Activated || booking.ActivatedAt == null)
        {
            return null;
        }

        var qrData = new
        {
            bookingId = booking.Id,
            customerId = booking.CustomerId,
            activatedAt = booking.ActivatedAt.Value.ToString("O"), // ISO 8601 format
            validFrom = booking.ValidFrom?.ToString("O"),
            validTo = booking.ValidTo?.ToString("O"),
            status = booking.Status,
            version = "1.0"
        };

        var jsonData = JsonSerializer.Serialize(qrData);
        
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(jsonData, QRCodeGenerator.ECCLevel.Q);
        
        // Convert to PNG image
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20); // 20 pixels per module
        
        // Convert to base64 string
        return Convert.ToBase64String(qrCodeBytes);
    }

    public static string GetQrCodeDataUrl(string? base64QrCode)
    {
        if (string.IsNullOrEmpty(base64QrCode))
        {
            return string.Empty;
        }

        return $"data:image/png;base64,{base64QrCode}";
    }
}

