using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public static class CompilerPipeline
    {
        public static void Evaluate(string inputFile, string outputFile)
        {
            var lexAnalyzer = new LexAnalyzer(inputFile);
            
            do
            {
                var token = lexAnalyzer.NextToken();
                if (token is null) break;
                Console.WriteLine(token.ToString());
            } while (true);
        }
    }
}
