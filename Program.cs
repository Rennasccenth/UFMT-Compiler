using System;
using static System.IO.Directory;

namespace Compiler
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            const string inputFile = "input.txt";
            const string outputFile = "output.txt";
            CompilerPipeline.Evaluate(inputFile, outputFile);
        }
    }
}
