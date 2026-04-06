using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthStatsLogic;

public class Program
{
    public static void Main()
    {
        // Mocking the Data from Seeder
        // partner1: Verified, Offers: 25%, 15%
        // partner2: Verified, Offers: 30%
        // partner3: Unverified, Offers: 10%

        int totalPartners = 3;
        int verifiedPartners = 2;
        
        var allOffers = new List<int> { 25, 15, 30, 10 };

        double verifiedQuality = (double)verifiedPartners / totalPartners * 100;
        double averageSavings = allOffers.Any() ? allOffers.Average() : 0;

        Console.WriteLine($"Total Partners: {totalPartners}");
        Console.WriteLine($"Verified Partners: {verifiedPartners}");
        Console.WriteLine($"Verified Quality: {verifiedQuality:F1}%");
        Console.WriteLine($"Average Savings: {averageSavings:F1}%");
        
        // Expected UI Display:
        // "Quality: 66.7%"
        // "Savings: 20.0%"
    }
}
