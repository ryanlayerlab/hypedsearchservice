using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using hypedsearchservice.core;


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
                var matcher = new ProteinMatcher();
                var proteinMatches = matcher.GetProteinMatchesViaSingle(ion_charge, weight, ppm_tolerance);
                var serializeProteinMatches = JsonConvert.SerializeObject(proteinMatches);
                return new OkObjectResult(serializeProteinMatches);
            }
            catch(Exception e)
            {
                return new ObjectResult(e.ToString());
            }

        }
    }
}

