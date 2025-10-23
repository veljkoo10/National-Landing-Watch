namespace Enzivor.Api.Models.Static
{
    public sealed record RegionDefinition(
        int Id,
        string Key,
        string Name,
        int Population,
        int AreaKm2
    );

    public static class RegionDefinitions
    {
        public static readonly List<RegionDefinition> All = new()
        {
            new(1, "vojvodina",           "Vojvodina",               1_740_000, 21_614),
            new(2, "beograd",             "Beograd",                 1_200_000,   3_234),
            new(3, "zapadnasrbija",       "Zapadna Srbija",           1_500_000, 26_000),
            new(4, "sumadijaipomoravlje", "Šumadija i Pomoravlje",    1_000_000, 13_000),
            new(5, "istocnasrbija",       "Istočna Srbija",            800_000, 19_000),
            new(6, "juznasrbija",         "Južna Srbija",              900_000, 15_000),
            new(7, "kosovoimetohija",     "Kosovo i Metohija",       1_600_000, 10_887)
        };

        public static Dictionary<string, int> PopulationByName =>
            All.ToDictionary(r => r.Name, r => r.Population);

        public static Dictionary<string, double> AreaByName =>
            All.ToDictionary(r => r.Name, r => (double)r.AreaKm2);

        public static RegionDefinition? GetByKey(string key) =>
            All.FirstOrDefault(r => r.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        public static RegionDefinition? GetByName(string name) =>
            All.FirstOrDefault(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}