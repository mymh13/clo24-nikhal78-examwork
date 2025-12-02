using Ticketing.Contracts.Bookings;

namespace Ticketing.Web.Helpers;

public static class TicketActivationHelper
{
    private const int DefaultValidityMinutes = 90;
    
    public static (DateTime validFrom, DateTime validTo) CalculateValidityPeriod(DateTime activationTime, int validityMinutes = DefaultValidityMinutes)
    {
        var validFrom = activationTime;
        var validTo = activationTime.AddMinutes(validityMinutes);
        return (validFrom, validTo);
    }
    
    public static bool CanActivate(Booking booking)
    {
        if (booking == null)
        {
            return false;
        }
        
        if (!TicketStatus.TicketIsValid(booking.Status))
        {
            return false;
        }
        
        return booking.Status == TicketStatus.Created;
    }
    
    public static string? ValidateActivation(Booking booking, string userId, string userRole)
    {
        if (booking == null)
        {
            return "Booking not found.";
        }
        
        if (!TicketStatus.TicketIsValid(booking.Status))
        {
            return "Invalid ticket status.";
        }
        
        if (booking.Status != TicketStatus.Created)
        {
            return $"Ticket cannot be activated. Current status: {booking.Status}. Only tickets with status '{TicketStatus.Created}' can be activated.";
        }
        
        if (userRole == "User" && booking.CustomerId != userId)
        {
            return "You can only activate your own tickets.";
        }
        
        return null;
    }
}

