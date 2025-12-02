using Ticketing.Contracts.Users;
using Ticketing.Web.Helpers;
using Xunit;

namespace Ticketing.Web.Tests.Helpers;

public class PriceCalculationHelperTests
{
    #region CalculatePriceModifier Tests

    [Fact]
    public void CalculatePriceModifier_ChildUnder12_ReturnsZero()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-10), // 10 years old
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(0.0m, result);
    }

    [Fact]
    public void CalculatePriceModifier_ChildExactly11_ReturnsZero()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-11).AddDays(-1), // Just turned 11
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(0.0m, result);
    }

    [Fact]
    public void CalculatePriceModifier_Student12to65_ReturnsHalfPrice()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-25), // 25 years old
            IsStudent = true
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(0.5m, result);
    }

    [Fact]
    public void CalculatePriceModifier_Pensioner65Plus_ReturnsHalfPrice()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-70), // 70 years old
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(0.5m, result);
    }

    [Fact]
    public void CalculatePriceModifier_PensionerExactly65_ReturnsHalfPrice()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-65), // Exactly 65
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(0.5m, result);
    }

    [Fact]
    public void CalculatePriceModifier_Standard12to65NonStudent_ReturnsFullPrice()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-30), // 30 years old
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(1.0m, result);
    }

    [Fact]
    public void CalculatePriceModifier_StandardExactly12_ReturnsFullPrice()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-12), // Exactly 12
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(1.0m, result);
    }

    [Fact]
    public void CalculatePriceModifier_StandardExactly64_ReturnsFullPrice()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = DateTime.UtcNow.AddYears(-64).AddDays(-1), // Just before 65
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(1.0m, result);
    }

    [Fact]
    public void CalculatePriceModifier_NullDateOfBirthWithStudent_ReturnsHalfPrice()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = null,
            IsStudent = true
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(0.5m, result);
    }

    [Fact]
    public void CalculatePriceModifier_NullDateOfBirthWithoutStudent_ReturnsFullPrice()
    {
        // Arrange
        var user = new User
        {
            DateOfBirth = null,
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert
        Assert.Equal(1.0m, result);
    }

    [Fact]
    public void CalculatePriceModifier_BirthdayToday_CalculatesAgeCorrectly()
    {
        // Arrange - User turning 12 today
        var today = DateTime.UtcNow.Date;
        var user = new User
        {
            DateOfBirth = today.AddYears(-12), // Birthday today, turning 12
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert - Should be 12, so full price (not child)
        Assert.Equal(1.0m, result);
    }

    [Fact]
    public void CalculatePriceModifier_BirthdayTomorrow_CalculatesAgeCorrectly()
    {
        // Arrange - User turning 12 tomorrow (still 11 today)
        var today = DateTime.UtcNow.Date;
        var user = new User
        {
            DateOfBirth = today.AddYears(-11).AddDays(-1), // Birthday tomorrow, still 11
            IsStudent = false
        };

        // Act
        var result = PriceCalculationHelper.CalculatePriceModifier(user);

        // Assert - Should still be 11, so free (child)
        Assert.Equal(0.0m, result);
    }

    #endregion

    #region CalculateTotalPrice Tests

    [Fact]
    public void CalculateTotalPrice_ZeroZones_ReturnsZero()
    {
        // Arrange
        decimal priceModifier = 1.0m;
        int numberOfZones = 0;
        decimal basePricePerZone = 20.0m;

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones, basePricePerZone);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateTotalPrice_NegativeZones_ReturnsZero()
    {
        // Arrange
        decimal priceModifier = 1.0m;
        int numberOfZones = -1;
        decimal basePricePerZone = 20.0m;

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones, basePricePerZone);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateTotalPrice_SingleZoneFullPrice_ReturnsBasePrice()
    {
        // Arrange
        decimal priceModifier = 1.0m; // Full price
        int numberOfZones = 1;
        decimal basePricePerZone = 20.0m;

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones, basePricePerZone);

        // Assert
        Assert.Equal(20.0m, result);
    }

    [Fact]
    public void CalculateTotalPrice_SingleZoneHalfPrice_ReturnsHalfBasePrice()
    {
        // Arrange
        decimal priceModifier = 0.5m; // Half price (student/pensioner)
        int numberOfZones = 1;
        decimal basePricePerZone = 20.0m;

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones, basePricePerZone);

        // Assert
        Assert.Equal(10.0m, result);
    }

    [Fact]
    public void CalculateTotalPrice_SingleZoneFree_ReturnsZero()
    {
        // Arrange
        decimal priceModifier = 0.0m; // Free (child)
        int numberOfZones = 1;
        decimal basePricePerZone = 20.0m;

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones, basePricePerZone);

        // Assert
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateTotalPrice_MultipleZonesFullPrice_ReturnsCorrectTotal()
    {
        // Arrange
        decimal priceModifier = 1.0m; // Full price
        int numberOfZones = 3;
        decimal basePricePerZone = 20.0m;

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones, basePricePerZone);

        // Assert
        Assert.Equal(60.0m, result); // 3 zones * 20 SEK * 1.0 = 60 SEK
    }

    [Fact]
    public void CalculateTotalPrice_MultipleZonesHalfPrice_ReturnsCorrectTotal()
    {
        // Arrange
        decimal priceModifier = 0.5m; // Half price
        int numberOfZones = 3;
        decimal basePricePerZone = 20.0m;

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones, basePricePerZone);

        // Assert
        Assert.Equal(30.0m, result); // 3 zones * 20 SEK * 0.5 = 30 SEK
    }

    [Fact]
    public void CalculateTotalPrice_MultipleZonesFree_ReturnsZero()
    {
        // Arrange
        decimal priceModifier = 0.0m; // Free
        int numberOfZones = 3;
        decimal basePricePerZone = 20.0m;

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones, basePricePerZone);

        // Assert
        Assert.Equal(0m, result); // 3 zones * 20 SEK * 0.0 = 0 SEK
    }

    [Fact]
    public void CalculateTotalPrice_CustomBasePrice_ReturnsCorrectTotal()
    {
        // Arrange
        decimal priceModifier = 1.0m;
        int numberOfZones = 2;
        decimal basePricePerZone = 25.0m; // Custom base price

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones, basePricePerZone);

        // Assert
        Assert.Equal(50.0m, result); // 2 zones * 25 SEK * 1.0 = 50 SEK
    }

    [Fact]
    public void CalculateTotalPrice_DefaultBasePrice_ReturnsCorrectTotal()
    {
        // Arrange
        decimal priceModifier = 1.0m;
        int numberOfZones = 2;
        // basePricePerZone not specified, should use default 20.0m

        // Act
        var result = PriceCalculationHelper.CalculateTotalPrice(priceModifier, numberOfZones);

        // Assert
        Assert.Equal(40.0m, result); // 2 zones * 20 SEK (default) * 1.0 = 40 SEK
    }

    #endregion
}

