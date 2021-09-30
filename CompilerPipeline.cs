using System;

namespace Compiler
{
    public static class CompilerPipeline
    {
        public static void Evaluate(string inputFile, string outputFile)
        {
            Syntactic syntactic = new Syntactic(inputFile);
            
            syntactic.Analysis();
        }
    }
}
