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
    public static class megaproteinmatch
    {
        [FunctionName("megaproteinmatch")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, ExecutionContext context)
        {
            try
            {
                var content = await new StreamReader(req.Body).ReadToEndAsync();
                Input input = JsonConvert.DeserializeObject<Input>(content);

                var proteinMatches = GetProteinMatchesViaDatabase(input, context);
                var serializeProteinMatches = JsonConvert.SerializeObject(proteinMatches);
                return new OkObjectResult(serializeProteinMatches);
            }
            catch (Exception e)
            {
                return new ObjectResult(e.ToString());
            }
        }

        internal static string GenerateSqlStatement(string ionCharge, double upperBound, double lowerBound)
        {
            return "Union Select [ProteinName],[Weight],[KMerLength],[StartIndex],[EndIndex],[KMers] From ProteinMatch where IonCharge = '" + ionCharge + "' and Weight < " + upperBound.ToString() + " and Weight > " + lowerBound.ToString();
        }


        internal static List<ProteinMatch> GetProteinMatchesViaDatabase(Input input, ExecutionContext context)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach(var weight in input.Weights)
            {
                var lowerBound = weight - (weight * input.PPMTolerance);
                var upperBound = weight + (weight * input.PPMTolerance);
                var sqlStatement = GenerateSqlStatement(input.IonCharge, upperBound, lowerBound);
                stringBuilder.Append(sqlStatement);
            }

            var proteinMatches = new List<ProteinMatch>();
            var connectionString = "Server=tcp:layerlab.database.windows.net,1433;Initial Catalog=hypedsearch;Persist Security Info=False;User ID=ryan;Password=LayerlabPa$$word;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=0;";
            var connection = new SqlConnection(connectionString);
            var commandText = stringBuilder.ToString().Substring(6);
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
                proteinMatches.Add(proteinMatch);
            }
            return proteinMatches;
        }

    }
}
