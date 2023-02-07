using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlToCsv
{
       class Program
       {
              static void Main(string[] args)
              {
                     GetData();
              }

              private static void GetData ()
              {
                     string queryString = "select * from table;";
                     string connectionString = @"Server= myServer; Database=myDb; User Id=myUser; Password= myPW;";
                     using (SqlConnection sc = new SqlConnection(connectionString))
                     {
                           SqlCommand command = new SqlCommand(queryString, sc);
                           sc.Open();
                           SqlDataReader reader = command.ExecuteReader();
                           try
                           {
                                  string myfilePath = @"D:\gcp\";
                                  StringBuilder sb = new StringBuilder();
                                  StreamWriter sw = new StreamWriter(myfilePath + "my_file.csv");

                                  //Get All column 
                                  var columnNames = Enumerable.Range(0, reader.FieldCount)
                                                                           .Select(reader.GetName) //OR .Select("\""+  reader.GetName"\"") 
                                                                           .ToList();

                                  //Create column headers
                                  sb.Append(string.Join(",", columnNames));

                                  //Append Line
                                  sb.AppendLine();

                                  while (reader.Read())
                                  {
                                         for (int i = 0; i < reader.FieldCount; i++)
                                         {
                                                string value = reader[i].ToString();
                                                value = value.Replace("\n", "").Replace("\t", " ").Replace(@"""","");
                                                if (value.Contains(","))
                                                       value = "\"" + value + "\"";

                                                sb.Append(value.Replace(Environment.NewLine, " ") + ",");
                                         }
                                         sb.Length--; // Remove the last comma
                                         sb.AppendLine();
                                  }
                                  sw.Write(sb.ToString());
                                  sw.Close();
                                  sw.Close();
                           }
                           catch (Exception ex)
                           {
                                  Console.WriteLine(ex.Message.ToString());
                                  Console.ReadLine();
                           }
                           finally
                           {
                                  reader.Close();
                           }
                     }
              }
       }
}
