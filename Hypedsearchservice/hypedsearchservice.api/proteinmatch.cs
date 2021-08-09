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
 
namespace hypedsearchservice
{

    public class IonCharge
    {
        public string Case { get; set; }
    }

    public class ProteinMatch
    {
        public string ProteinName { get; set; }
        public double Weight { get; set; }
        public int KMerLength { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    public class Root
    {
        public IonCharge IonCharge { get; set; }
        public double Weight { get; set; }
        public List<ProteinMatch> ProteinMatchs { get; set; }
    }

    public static class proteinmatch
    {
        //https://hypedsearchservice.azurewebsites.net/api/proteinmatch?ion_charge=B&weight=218.07
        [FunctionName("proteinmatch")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req, ExecutionContext context)
        {
            try
            {
                var ion_charge = req.Query["ion_charge"];
                var weight = Double.Parse(req.Query["weight"]);
                var proteinMatches = GetProteinMatchesViaDatabase(ion_charge, weight, context);
                var serializeProteinMatches = JsonConvert.SerializeObject(proteinMatches);
                return new OkObjectResult(serializeProteinMatches);
            }
            catch(Exception e)
            {
                return new ObjectResult(e.ToString());
            }

        }

        internal static List<ProteinMatch> GetProteinMatchesViaFileSystem(string ionCharge, double weight, ExecutionContext context)
        {
            var fileName = "all_weight_protein_matches_" + ionCharge.ToString() + ".json";
            var assemblyPath = context.FunctionDirectory;
            var rootPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\')).TrimEnd();
            var directoryPath = System.IO.Path.Combine(rootPath, "data");
            var filePath = System.IO.Path.Combine(directoryPath, fileName);
            var json = File.ReadAllText(filePath);
            var contents = JsonConvert.DeserializeObject<Root[]>(json);
            var filteredContents = contents[0].ProteinMatchs.FindAll(x => x.Weight == weight);
            return filteredContents;
        }

        internal static List<ProteinMatch> GetProteinMatchesViaDatabase(string ionCharge, double weight, ExecutionContext context)
        {
            var proteinMatches = new List<ProteinMatch>();
            var connectionString = "xxx";
            var connection = new SqlConnection(connectionString);
            var commandText = "Select [ProteinName], [KMerLength], [Weight], [StartIndex],[EndIndex] from ProteinMatch where IonCharge = '" + ionCharge + "' and weight = " + weight.ToString();
            var command = new SqlCommand(commandText, connection);
            connection.Open();
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var proteinMatch = new ProteinMatch();
                proteinMatch.ProteinName = reader[0].ToString();
                proteinMatch.KMerLength = Int32.Parse(reader[1].ToString());
                proteinMatch.Weight = Double.Parse(reader[2].ToString());
                proteinMatch.StartIndex = Int32.Parse(reader[3].ToString());
                proteinMatch.EndIndex = Int32.Parse(reader[4].ToString());
                proteinMatches.Add(proteinMatch);
            }
            return proteinMatches;
        }
    }
}
