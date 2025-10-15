using CsvHelper;
using CsvHelper.Configuration;
using Enzivor.Api.Models;
using Enzivor.Api.Models.Dtos;
using Enzivor.Api.Models.Enums;
using Enzivor.Api.Services.Interfaces;
using System.Formats.Asn1;
using System.Globalization;
using System.Globalization;

namespace Enzivor.Api.Services.Implementations
{
    public class CsvPredictionReader : ICsvPredictionReader
    {
        // Ucita CSV fajl koji je generisao Python model (infer_seg_folder.py)
        // i pretvori ga u listu LandfillDto objekata.

        public List<LandfillDto> ReadPredictions(string csvPath)
        {
            if (!File.Exists(csvPath))
                throw new FileNotFoundException($"CSV fajl nije pronađen: {csvPath}");

            var predictions = new List<LandfillDto>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                TrimOptions = TrimOptions.Trim,
            };

            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Read();
                csv.ReadHeader();

                while (csv.Read())
                {
                    var imageName = csv.GetField<string>("imageName");
                    var confidence = csv.GetField<double>("confidence");
                    var polygonPx = csv.GetField<string>("polygonCoordinates");

                    var regionFromCsv = csv.GetField<string>("region");
                    var typeFromCsv = csv.GetField<string>("type") ?? csv.GetField<string>("category");

                    var dto = new LandfillDto
                    {
                        ImageName = imageName,
                        Type = typeFromCsv,
                        Confidence = confidence,
                        PolygonCoordinates = polygonPx,
                        RegionFromCsv = regionFromCsv
                    };

                    dto.ParsedRegion = ParseRegionFromString(regionFromCsv);

                    predictions.Add(dto);
                }
            }

            return predictions;
        }

        private static SerbianRegion? ParseRegionFromString(string? regionStr)
        {
            if (string.IsNullOrWhiteSpace(regionStr))
                return null;

            return regionStr.Trim().ToLowerInvariant() switch
            {
                "vojvodina" or "војводина" => SerbianRegion.Vojvodina,
                "beograd" or "београд" or "belgrade" => SerbianRegion.Belgrade,
                "sumadija" or "шумадија" or "sumadija i pomoravlje" or "шумадија и поморавље" => SerbianRegion.SumadijaPomoravlje,
                "zapadna srbija" or "западна србија" or "western serbia" => SerbianRegion.WesternSerbia,
                "istocna srbija" or "источна србија" or "eastern serbia" => SerbianRegion.EasternSerbia,
                "juzna srbija" or "јужна србија" or "southern serbia" => SerbianRegion.SouthernSerbia,
                _ => null
            };
        }
    }
}
