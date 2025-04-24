using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

namespace Cousework_2_COMP1551
{
    class Program
    {
        private static string lastInputString = null;
        private static string lastProcessorTypeString = null;
        private static int? lastInputNumber = null;
        private static string lastSuccessfulEncodedResult = null;

        private static string databaseConnectionString = "Server=localhost;Database=courseworkcomp1551;Uid=root;Pwd=4Enix172005;";

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to our Encode Program");
            Console.WriteLine("/~~~~~~COMP1551~~~~~~/");
            Console.WriteLine("/AUTHOR: Pham Minh Triet/");
            bool running = true;
            while (running)
            {
                Console.WriteLine("\n--- Main Menu ---");
                Console.WriteLine("1. Choose an algorithm to encode");
                Console.WriteLine("2. View Encoding History");
                Console.WriteLine("3. View Saved Results");
                Console.WriteLine("4. Save Current Result");
                Console.WriteLine("5. Exit");

                Console.Write("Choose an option: ");
                string mainChoice = Console.ReadLine();

                switch (mainChoice)
                {
                    case "1":
                        Console.WriteLine("\n--- Choose Encoding Algorithm ---");
                        Console.WriteLine("1. Caesar Cipher");
                        Console.WriteLine("2. Atbash Cipher");
                        Console.WriteLine("3. Swap Adjacent Characters");
                        Console.Write("Choose an algorithm: ");
                        string algorithmChoice = Console.ReadLine();
                        ProcessString(algorithmChoice);
                        break;

                    case "2":
                        ViewEncodingHistory();
                        break;

                    case "3":
                        ViewSavedResults();
                        break;

                    case "4":
                        SaveCurrentResult();
                        break;

                    case "5":
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Invalid main menu choice. Please try again.");
                        break;
                }
            }
            Console.WriteLine("Goodbye!");
        }

        static void ProcessString(string choice)
        {
            StringProcessingBase processor = null;
            string inputString = null;
            int? inputNumber = null;
            string processorTypeString = "";
            string encodedResult = null;

            bool validInputString = false;
            while (!validInputString)
            {
                Console.WriteLine("Enter an uppercase string (max 40 characters):");
                string input = Console.ReadLine();
                if (string.IsNullOrEmpty(input) || input.Length > 40 || !input.All(char.IsUpper))
                {
                    Console.WriteLine($"Invalid input '{input}'. Please ensure the string is not empty, has a maximum length of 40 characters, and contains only uppercase letters.");
                    continue;
                }
                inputString = input;
                validInputString = true;
            }

            if (choice == "1")
            {
                bool validInputNumber = false;
                int number = 0;
                while (!validInputNumber)
                {
                    Console.WriteLine("Enter an integer value between -25 and 25:");
                    string numberInput = Console.ReadLine();
                    if (int.TryParse(numberInput, out number))
                    {
                        if (number >= -25 && number <= 25)
                        {
                            inputNumber = number;
                            validInputNumber = true;
                        }
                        else
                        {
                            Console.WriteLine($"Invalid input '{numberInput}'. Please enter a number between -25 and 25.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid number format. Please enter an integer.");
                    }
                }
            }

            try
            {
                switch (choice)
                {
                    case "1":
                        processor = new CaesarProcessor();
                        if (inputNumber.HasValue)
                        {
                            processor.InputNumber = inputNumber.Value;
                        }
                        processorTypeString = "CaesarCipher";
                        break;
                    case "2":
                        processor = new AtbashProcessor();
                        processorTypeString = "AtbashCipher";
                        break;
                    case "3":
                        processor = new AdjacentSwapProcessor();
                        processorTypeString = "AdjacentSwap";
                        break;
                    default:
                        Console.WriteLine("Invalid algorithm choice.");
                        return;
                }

                processor.InputString = inputString;

                processor.Encode();
                encodedResult = processor.Print();

                lastInputString = inputString;
                lastProcessorTypeString = processorTypeString;
                lastInputNumber = inputNumber;
                lastSuccessfulEncodedResult = encodedResult;

                Console.WriteLine($"Encoded string: {encodedResult}");
                Console.WriteLine($"Sorted input string: {processor.Sort()}");
                Console.WriteLine($"Input ASCII codes: [{string.Join(", ", processor.InputCode())}]");
                Console.WriteLine($"Output ASCII codes: [{string.Join(", ", processor.OutputCode())}]");

                using (MySqlConnection connection = new MySqlConnection(databaseConnectionString))
                {
                    try
                    {
                        connection.Open();
                        string logQuery = "INSERT INTO processinglog (Timestamp, InputString, ProcessorType, Parameter, OutputString) " +
                                           "VALUES (@timestamp, @input, @processorType, @parameter, @output)";
                        using (MySqlCommand logCmd = new MySqlCommand(logQuery, connection))
                        {
                            logCmd.Parameters.AddWithValue("@timestamp", DateTime.Now);
                            logCmd.Parameters.AddWithValue("@input", inputString);
                            logCmd.Parameters.AddWithValue("@processorType", processorTypeString);
                            logCmd.Parameters.AddWithValue("@parameter", (object)inputNumber ?? DBNull.Value);
                            logCmd.Parameters.AddWithValue("@output", encodedResult);


                            logCmd.ExecuteNonQuery();
                            Console.WriteLine("Encoding operation logged to database (processinglog).");
                        }

                        string statsQuery = "INSERT INTO processorstats (ProcessorType, UsageCount) " +
                                            "VALUES (@processorType, 1) " +
                                            "ON DUPLICATE KEY UPDATE UsageCount = UsageCount + 1";
                        using (MySqlCommand statsCmd = new MySqlCommand(statsQuery, connection))
                        {
                            statsCmd.Parameters.AddWithValue("@processorType", processorTypeString);
                            statsCmd.ExecuteNonQuery();
                            Console.WriteLine($"Usage stats updated for {processorTypeString} (processorstats).");
                        }
                    }
                    catch (Exception dbEx)
                    {
                        Console.WriteLine($"Error performing database operations: {dbEx.Message}");
                    }
                }

            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred during processing: {ex.Message}");
            }
        }

        static void ViewEncodingHistory()
        {
            Console.WriteLine("\n--- Encoding History ---");
            using (MySqlConnection connection = new MySqlConnection(databaseConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT Timestamp, InputString, ProcessorType, Parameter, OutputString FROM processinglog ORDER BY Timestamp DESC";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("No encoding history found.");
                                return;
                            }

                            Console.WriteLine("{0,-22} {1,-15} {2,-15} {3,-10} {4,-20}", "Timestamp", "Input", "Processor", "Param", "Output");
                            Console.WriteLine(new string('-', 82));

                            while (reader.Read())
                            {
                                DateTime timestamp = reader.GetDateTime("Timestamp");
                                string input = reader.GetString("InputString");
                                string processorType = reader.GetString("ProcessorType");
                                int? parameter = null;
                                object dbValue = reader["Parameter"]; 
                                if (dbValue != DBNull.Value) 
                                {
                                    string parameterString = Convert.ToString(dbValue); 
                                    if (int.TryParse(parameterString, out int parsedParameter)) 
                                    {
                                        parameter = parsedParameter;
                                    }
                                }
                                string output = reader.GetString("OutputString");

                                Console.WriteLine("{0,-22} {1,-15} {2,-15} {3,-10} {4,-20}",
                                                  timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                                                  input,
                                                  processorType,
                                                  parameter.HasValue ? parameter.ToString() : "N/A",
                                                  output);
                            }
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"Error retrieving encoding history: {dbEx.Message}");
                }
            }
        }

        static void ViewSavedResults()
        {
            Console.WriteLine("\n--- Saved Results ---");
            using (MySqlConnection connection = new MySqlConnection(databaseConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT ResultName, InputString, ProcessorType, Parameter, OutputString FROM savedresults ORDER BY ResultName";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                Console.WriteLine("No saved results found. Use Option 4 to save a result.");
                                return;
                            }

                            Console.WriteLine("{0,-20} {1,-15} {2,-15} {3,-10} {4,-20}", "Name", "Input", "Processor", "Param", "Output");
                            Console.WriteLine(new string('-', 80));

                            while (reader.Read())
                            {
                                string resultName = reader.GetString("ResultName");
                                string input = reader.GetString("InputString");
                                string processorType = reader.GetString("ProcessorType");
                                int? parameter = null;
                                object dbValue = reader["Parameter"]; 
                                if (dbValue != DBNull.Value) 
                                {
                                    string parameterString = Convert.ToString(dbValue);
                                    if (int.TryParse(parameterString, out int parsedParameter))
                                    {
                                        parameter = parsedParameter;
                                    }
                                }
                                string output = reader.GetString("OutputString");

                                Console.WriteLine("{0,-20} {1,-15} {2,-15} {3,-10} {4,-20}",
                                                  resultName,
                                                  input,
                                                  processorType,
                                                  parameter.HasValue ? parameter.ToString() : "N/A",
                                                  output);
                            }
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"Error retrieving saved results: {dbEx.Message}");
                }
            }
        }

        static void SaveCurrentResult()
        {
            if (lastSuccessfulEncodedResult == null)
            {
                Console.WriteLine("No result to save yet. Please encode a string successfully first (Option 1).");
                return;
            }

            Console.Write("Enter a name for this saved result: ");
            string resultName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(resultName))
            {
                Console.WriteLine("Result name cannot be empty. Please try again.");
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(databaseConnectionString))
            {
                try
                {
                    connection.Open();
                    string query = "INSERT INTO savedresults (ResultName, InputString, ProcessorType, Parameter, OutputString) " +
                                   "VALUES (@resultName, @input, @processorType, @parameter, @output)";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@resultName", resultName);
                        cmd.Parameters.AddWithValue("@input", lastInputString);
                        cmd.Parameters.AddWithValue("@processorType", lastProcessorTypeString);
                        cmd.Parameters.AddWithValue("@parameter", (object)lastInputNumber ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@output", lastSuccessfulEncodedResult);

                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"Result '{resultName}' saved successfully!");
                    }
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine($"Error saving result to database: {dbEx.Message}");
                }
            }
        }
    }
}