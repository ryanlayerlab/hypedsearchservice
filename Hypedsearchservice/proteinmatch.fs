module proteinmatch

open System.IO
open System.Net
open Microsoft.Extensions.Logging
open Microsoft.Azure.Functions.Worker.Http
open Microsoft.Azure.Functions.Worker
open Newtonsoft.Json

type ProteinMatch = {ProteinName:string; Weight:float; StartIndex:int; EndIndex:int}
type WeightProteinMatches = {Weight:float; ProteinMatchs:ProteinMatch seq}
type AllWeightProteinMatches = WeightProteinMatches seq

let getWeightProteinMatch weight kmerLength (context: FunctionContext) =
    let fileName = "all_weight_protein_matches_kmer" + kmerLength.ToString() + ".json"
    let assemblyPath = context.FunctionDefinition.PathToAssembly
    let directoryPath = assemblyPath.Substring(0, assemblyPath.LastIndexOf('\\')).TrimEnd()
    let filePath = System.IO.Path.Combine(directoryPath, fileName)
    let json = File.ReadAllText(filePath)
    let contents = JsonConvert.DeserializeObject<AllWeightProteinMatches> json
    let found = contents|> Seq.filter(fun x -> x.Weight = weight)
    JsonConvert.SerializeObject found

[<Function "proteinmatch">]
let run ([<HttpTrigger(AuthorizationLevel.Anonymous, "GET")>] req: HttpRequestData, executionContext: FunctionContext) =
    let queryString = req.Url.Query
    let tokens = System.Web.HttpUtility.ParseQueryString(queryString)
    let kmerLength = tokens.Get("kmer_length")
    let weight = System.Double.Parse(tokens.Get("weight"))
    let res = req.CreateResponse(HttpStatusCode.OK)
    res.Headers.Add("Content-Type", "text/plain; charset=utf-8")
    let response = getWeightProteinMatch weight kmerLength executionContext
    res.WriteString(response);
    res

