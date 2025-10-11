using Enzivor.Api.Dtos;
using System.Formats.Asn1;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Enzivor.Api.Models;
using System.Globalization;

namespace Enzivor.Api.Services
{
    public class CsvPredictionReader
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
                // citamo red po red i mapiramo kolone
                while (csv.Read())
                {
                    var imageName = csv.GetField<string>("image_name");
                    var confidence = csv.GetField<double>("confidence");
                    var polygonPx = csv.GetField<string>("polygon_px");

                    predictions.Add(new LandfillDto
                    {
                        ImageName = imageName,
                        Confidence = confidence,
                        PolygonCoordinates = polygonPx,
                        // ostala polja cemo popuniti kasnije (kad izracunamo povrsinu i koordinate)
                    });
                }
            }

            return predictions;
        }
    }
}
