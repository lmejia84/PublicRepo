using System;
using System.IO;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;

class Program
{
    static void Main(string[] args)
    {
        string my_project = "fleet-parser-330316";
        string path = @"C:\GCP\csharp_file.csv";
        string service_account = @"service_account.json";
        string sql_query = "SELECT * FROM `fleet-parser-330316.luistest.futbol_tabla_posiciones` ORDER BY posicion;";

        GoogleCredential credential = GoogleCredential.FromFile(service_account);
        BigQueryClient client = BigQueryClient.Create(my_project, credential);
        BigQueryResults results = client.ExecuteQuery(sql_query, parameters: null);

        using (StreamWriter writer = new StreamWriter(path, false, Encoding.UTF8))
        {
            //Get the column headers row and add to file before looping through all the row values
            var header = new string[results.Schema.Fields.Count];
            for (int i = 0; i < results.Schema.Fields.Count; i++)
            {
                header[i] = results.Schema.Fields[i].Name;
            }
            string headerLine = string.Join(",", header);
            writer.WriteLine(headerLine);

            //Loop through all rows from the query results and write to csv file
            foreach (BigQueryRow row in results)
            {
                var values = new object[row.Schema.Fields.Count];
                for (int i = 0; i < row.Schema.Fields.Count; i++)
                {
                    values[i] = row[i].ToString();
                }
                string line = string.Join(",", values);
                writer.WriteLine(line);
            }
        }
        Console.WriteLine("Query results saved to " + path);
        Console.ReadLine();
    }
}
