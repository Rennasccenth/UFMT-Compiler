using System;
using Compiler.Extensions;
using static System.IO.Directory;

namespace Compiler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            const string inputFile = "input.txt";
            string outputFile = EnvironmentExt.GetContentPath() + "output.txt";
            
            CompilerPipeline.Evaluate(inputFile, outputFile);

            Console.WriteLine($"\n Output file saved on {outputFile}");
        }
    }
}
