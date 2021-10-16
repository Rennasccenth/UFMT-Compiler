using System;
using System.IO;

namespace Compiler
{
    public static class CompilerPipeline
    {
        public static void Evaluate(string inputFile, string outputFile)
        {
            Syntactic syntactic = new Syntactic(inputFile);
            
            File.WriteAllLines(outputFile, syntactic.Analysis().ToArray());
        }
    }
}
