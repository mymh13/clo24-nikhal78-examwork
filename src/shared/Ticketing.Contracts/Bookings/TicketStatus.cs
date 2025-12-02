namespace Ticketing.Contracts.Bookings;

public static class TicketStatus
{
    public const string Created = "Created";
    
    public const string Activated = "Activated";
    
    public const string Valid = "Valid";
    
    public const string Expired = "Expired";
    
    public static bool TicketIsValid(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return false;
        }
        
        return status == Created || 
               status == Activated || 
               status == Valid || 
               status == Expired;
    }
    
    public static string[] GetAllTicketStatuses()
    {
        return new[] { Created, Activated, Valid, Expired };
    }
}