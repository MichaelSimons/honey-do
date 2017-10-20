using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace NumericCombine
{
    public class NumericCombine
    {
        private static string Input1File { get; set; }
        private static string Input2File { get; set; }
        private static string OutputFile { get; set; }
        
        public static int Main(string[] args)
        {
            int result = 0;

            try
            {
                ParseArgs(args);
                ProcessFiles();
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
                string input1File = null;
                syntax.DefineParameter("input1-file", ref input1File, "The first file to parse");
                Input1File = input1File;

                string input2File = null;
                syntax.DefineParameter("input2-file", ref input2File, "The first file to parse");
                Input2File = input2File;

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

        private static void ProcessFiles()
        {
            List<char[]> outputColumns = new List<char[]>();
            
            FileStream input1FileStream = new FileStream(Input1File, FileMode.Open);
            FileStream input2FileStream = new FileStream(Input2File, FileMode.Open);
            
            using (StreamReader reader1 = new StreamReader(input1FileStream))
            using (StreamReader reader2 = new StreamReader(input2FileStream))
            using (StreamWriter outFile = new StreamWriter(OutputFile))
            {
                while (!reader1.EndOfStream)
                {
                    string[] input1 = reader1.ReadLine().Split(' ');
                    string[] input2 = reader2.ReadLine().Split(' ');
                    
                    string[] output = new string[input1.Length];
                    for (int i = 0; i < input1.Length; i ++)
                    {
                        output[i] = (int.Parse(input1[i]) + int.Parse(input2[i])).ToString();
                    }
                    
                    string outputLine = string.Join(' ', output);
                    outFile.WriteLine(outputLine);
                }
            }
        }
    }
}
