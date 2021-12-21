using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using hypedsearchservice.core;


namespace hypedsearchservice
{
    public static class megaproteinmatch
    {
        [FunctionName("megaproteinmatch")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, ExecutionContext context)
        {
            try
            {
                var content = await new StreamReader(req.Body).ReadToEndAsync();
                Input input = JsonConvert.DeserializeObject<Input>(content);
                var matcher = new ProteinMatcher();
                var proteinMatches = matcher.GetProteinMatchesViaBulk(input);
                var serializeProteinMatches = JsonConvert.SerializeObject(proteinMatches);
                return new OkObjectResult(serializeProteinMatches);
            }
            catch (Exception e)
            {
                return new ObjectResult(e.ToString());
            }
        }


    }
}
