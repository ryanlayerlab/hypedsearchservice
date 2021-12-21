using System;
using System.Text;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace hypedsearchservice.core
{
    public class ProteinMatcher
    {

        public List<ProteinMatch> GetProteinMatchesViaSingle(string ionCharge, double weight, double ppm_tolerance)
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
                proteinMatches.Add(proteinMatch);
            }
            return proteinMatches;
        }

        public string GenerateSqlStatement(string ionCharge, double upperBound, double lowerBound)
        {
            return "Union Select [ProteinName],[Weight],[KMerLength],[StartIndex],[EndIndex],[KMers] From ProteinMatch where IonCharge = '" + ionCharge + "' and Weight < " + upperBound.ToString() + " and Weight > " + lowerBound.ToString();
        }

        public List<ProteinMatch> GetProteinMatchesViaBulk(Input input)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var weight in input.Weights)
            {
                var lowerBound = weight - (weight * input.PPMTolerance);
                var upperBound = weight + (weight * input.PPMTolerance);
                var sqlStatement = GenerateSqlStatement(input.IonCharge, upperBound, lowerBound);
                stringBuilder.Append(sqlStatement);
            }

            var proteinMatches = new List<ProteinMatch>();
            var connectionString = "Server=tcp:layerlab.database.windows.net,1433;Initial Catalog=hypedsearch;Persist Security Info=False;User ID=ryan;Password=LayerlabPa$$word;MultipleActiveResultSets=False;TrustServerCertificate=False;Connection Timeout=0;";
            var connection = new SqlConnection(connectionString);
            var commandText = stringBuilder.ToString().Substring(6);
            var command = new SqlCommand(commandText, connection);
            command.CommandTimeout = 0;
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
