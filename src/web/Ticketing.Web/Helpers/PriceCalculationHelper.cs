using Ticketing.Contracts.Users;

namespace Ticketing.Web.Helpers;

public static class PriceCalculationHelper
{
    /// <summary>
    /// Calculates the price modifier based on user's age and student status.
    /// Returns: 0.0 for children (<12), 0.5 for students or pensioners (65+), 1.0 for standard (12-65).
    /// </summary>
    public static decimal CalculatePriceModifier(User user)
    {
        if (user.DateOfBirth == null)
        {
            // If no date of birth, assume standard pricing
            return user.IsStudent ? 0.5m : 1.0m;
        }

        var today = DateTime.UtcNow.Date;
        var birthDate = user.DateOfBirth.Value.Date;
        var age = today.Year - birthDate.Year;
        
        // Adjust age if birthday hasn't occurred this year
        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        // Child (<12): Free
        if (age < 12)
        {
            return 0.0m;
        }

        // Pensioner (65+): Half price
        if (age >= 65)
        {
            return 0.5m;
        }

        // Student (12-65): Half price
        if (user.IsStudent)
        {
            return 0.5m;
        }

        // Standard (12-65, non-student): Full price
        return 1.0m;
    }

    /// <summary>
    /// Calculates the total price for a ticket based on price modifier and number of zones.
    /// Base price per zone is configurable (default: 25 SEK).
    /// </summary>
    public static decimal CalculateTotalPrice(decimal priceModifier, int numberOfZones, decimal basePricePerZone = 25.0m)
    {
        if (numberOfZones <= 0)
        {
            return 0m;
        }

        var basePrice = basePricePerZone * numberOfZones;
        return basePrice * priceModifier;
    }

    /// <summary>
    /// Gets a human-readable description of the price modifier category.
    /// </summary>
    public static string GetPriceModifierDescription(decimal priceModifier, bool isStudent)
    {
        if (priceModifier == 0.0m)
        {
            return "Child (Free)";
        }
        
        if (priceModifier == 0.5m)
        {
            return isStudent ? "Student (50% discount)" : "Pensioner (50% discount)";
        }
        
        return "Standard (Full price)";
    }
}

