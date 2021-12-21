using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
 
namespace hypedsearchservice
{

    public static class proteinmatch
    {
        //http://hypedsearchservice.azurewebsites.net/api/proteinmatch?ion_charge=B&weight=218.07&ppm_tolerance=.01
        [FunctionName("proteinmatch")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, ExecutionContext context)
        {
            try
            {
                var ion_charge = req.Query["ion_charge"];
                var weight = Double.Parse(req.Query["weight"]);
                var ppm_tolerance = Double.Parse(req.Query["ppm_tolerance"]);
                var proteinMatches = GetProteinMatchesViaDatabase(ion_charge, weight, ppm_tolerance, context);
                var serializeProteinMatches = JsonConvert.SerializeObject(proteinMatches);
                return new OkObjectResult(serializeProteinMatches);
            }
            catch(Exception e)
            {
                return new ObjectResult(e.ToString());
            }

        }

        internal static List<ProteinMatch> GetProteinMatchesViaDatabase(string ionCharge, double weight, double ppm_tolerance, ExecutionContext context)
        {
            var lowerBound = weight - (weight * ppm_tolerance);
            var upperBound = weight + (weight * ppm_tolerance);
            var proteinMatches = new List<ProteinMatch>();
            var connectionString = "Server=tcp:layerlab.database.windows.net,1433;Initial Catalog=hypedsearch;Persist Security Info=False;User ID=ryan;Password=LayerlabPa$$word;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=0;";
            var connection = new SqlConnection(connectionString);
            var stringBuilder = new StringBuilder();
            stringBuilder.Append("Select [ProteinName],[Weight],[KMerLength],[StartIndex],[EndIndex],[KMers] ");
            stringBuilder.Append("from ProteinMatch where IonCharge = '");
            stringBuilder.Append(ionCharge);
            stringBuilder.Append("' and weight < ");
            stringBuilder.Append(upperBound);
            stringBuilder.Append(" and weight > ");
            stringBuilder.Append(lowerBound);
            var commandText = stringBuilder.ToString();
            var command = new SqlCommand(commandText, connection);
            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var proteinMatch = new ProteinMatch();
                proteinMatch.ProteinName = reader[0].ToString();
                proteinMatch.Weight = Double.Parse(reader[1].ToString());
                proteinMatch.KMerLength = Int32.Parse(reader[2].ToString());
                proteinMatch.StartIndex = Int32.Parse(reader[3].ToString());
                proteinMatch.EndIndex = Int32.Parse(reader[4].ToString());
                proteinMatch.KMers = reader[5].ToString();
            }
            return proteinMatches;
        }
    }
}

