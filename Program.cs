using TKSE_FishClassification_MLModel_ConsoleApp_CPU;
using Microsoft.ML.Data;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO;

namespace TKSE_FishClassification_MLModel_ConsoleApp_CPU
{

    class Program
    {
        static string connectionString = "Data Source=db1.tkse.lk;Initial Catalog=Area51;User ID=Area51;Password=D@e5XGnULj;";

        static byte[] GetLatestImageDataFromDatabase()
        {

            

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT TOP 1 ImageData FROM tblImages ORDER BY DateModified DESC", connection))
                {
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        return (byte[])result;
                    }
                    else
                    {
                        throw new Exception("Image data not found in the database.");
                    }
                }
            }

        }

        static void Main(string[] args)
        {
            try
            {
                // Create single instance of sample data from first line of dataset for model input
                byte[] imageBytes = GetLatestImageDataFromDatabase();

                TKSE_FishClassification_MLModel.ModelInput sampleData = new TKSE_FishClassification_MLModel.ModelInput()
                {
                    ImageSource = imageBytes,
                };

                // Make a single prediction on the sample data and print results.
                var sortedScoresWithLabel = TKSE_FishClassification_MLModel.PredictAllLabels(sampleData);
                Console.WriteLine($"{"Class",-40}{"Score",-20}");
                Console.WriteLine($"{"-----",-40}{"-----",-20}");

                foreach (var score in sortedScoresWithLabel)
                {
                    if (score.Value > 0.9)
                    {
                        Console.WriteLine($"{score.Key,-40}{score.Value,-20}");
                        Console.WriteLine($"\nFish Type is = {score.Key}");
                    }
                    System.Threading.Thread.Sleep(5000);


                }
                


            }
            catch
            {



            }
        }

    }

}







