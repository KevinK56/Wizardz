namespace Wizardz.Shared.Models;

public class WizardUnit
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = "🧙‍♂️";
    public double BaseCost { get; set; }
    public double CostMultiplier { get; set; } = 1.15;
    public double BaseMps { get; set; } // Mana per second
    public int Count { get; set; }

    public double GetCostForNext(int quantity = 1)
    {
        if (quantity <= 1)
        {
            return Math.Floor(BaseCost * Math.Pow(CostMultiplier, Count));
        }

        // Geometric series sum: S = a * (r^n - 1) / (r - 1)
        double a = BaseCost * Math.Pow(CostMultiplier, Count);
        double r = CostMultiplier;
        double sum = a * (Math.Pow(r, quantity) - 1.0) / (r - 1.0);
        return Math.Floor(sum);
    }

    public int GetMaxAffordable(double availableMana)
    {
        if (availableMana < GetCostForNext(1))
            return 0;

        // n = floor( log( (Mana * (r-1)/a) + 1 ) / log(r) )
        double a = BaseCost * Math.Pow(CostMultiplier, Count);
        double r = CostMultiplier;
        double maxN = Math.Log((availableMana * (r - 1.0) / a) + 1.0) / Math.Log(r);
        return Math.Max(1, (int)Math.Floor(maxN));
    }

    public double GetTotalMps(double unitMultiplier = 1.0, double globalMultiplier = 1.0)
    {
        return Count * BaseMps * unitMultiplier * globalMultiplier;
    }
}
