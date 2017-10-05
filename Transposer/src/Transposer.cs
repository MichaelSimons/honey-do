using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Text.RegularExpressions;

namespace Transposer
{
    public class Transposer
    {
        private static string InputFile { get; set; }
        private static string OutputFile { get; set; }
        
        public static int Main(string[] args)
        {
            int result = 0;

            try
            {
                ParseArgs(args);
                TransposeFile();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                result = 1;
            }

            return result;
        }

        private static void ParseArgs(string[] args)
        {
            ArgumentSyntax argSyntax = ArgumentSyntax.Parse(args, syntax =>
            {
                string inputFile = null;
                syntax.DefineParameter("input-file", ref inputFile, "The file to parse and transpose");
                InputFile = inputFile;

                string outputFile = null;
                syntax.DefineParameter("output-file", ref outputFile, "The file to write the transposed content to");
                OutputFile = outputFile;
            });

            // Workaround for https://github.com/dotnet/corefxlab/issues/1689
            foreach (Argument arg in argSyntax.GetActiveArguments())
            {
                if (arg.IsParameter && !arg.IsSpecified)
                {
                    Console.Error.WriteLine($"error: `{arg.Name}` must be specified.");
                    Environment.Exit(1);
                }
            }
        }

        private static void TransposeFile()
        {
            List<char[]> outputColumns = new List<char[]>();
            
            FileStream inputFileStream = new FileStream(InputFile, FileMode.Open);
            using (StreamReader reader = new StreamReader(inputFileStream))
            {
                int startingColumn = -1;

                while (!reader.EndOfStream)
                {
                    string inputLine = reader.ReadLine();
                    if (inputLine.StartsWith('#'))
                    {
                        continue;
                    }

                    string[] cells = inputLine.Split("\t");

                    if (startingColumn == -1)
                    {
                        startingColumn = GetStartingColumn(cells);
                    }

                    char[] column = TransposeRow(cells, startingColumn);
                    outputColumns.Add(column);
                }
            }
            
            WriteOutputFile(outputColumns);
        }

        private static int GetStartingColumn(string[] cells)
        {
            Regex regex = new Regex("^[01..]/[01..].*");

            for (int i = 0; i < cells.Length; i++)
            {
                if (regex.IsMatch(cells[i]))
                {
                    return i;
                }
            }

            throw new InvalidOperationException("Unable to find column of data to transpose");
        }

        private static char[] TransposeRow(string[] cells, int startingColumn)
        {
            int numOutputRows = (cells.Length - startingColumn) * 2;
            char[] column = new char[numOutputRows];
            int rowIndex = 0;

            for (int i = startingColumn; i < cells.Length; i++)
            {
                column[rowIndex++] = cells[i][0];
                column[rowIndex++] = cells[i][2];
            }

            return column;
        }

        private static void WriteOutputFile(List<char[]> outputColumns)
        {
            using (StreamWriter file = new StreamWriter(OutputFile))
            {
                for (int i = 0; i < outputColumns[0].Length; i++)
                {
                    for (int j = 0; j < outputColumns.Count; j++)
                    {
                        file.Write(outputColumns[j][i]);
                    }

                    file.WriteLine();
                }
            }
        }
    }
}
