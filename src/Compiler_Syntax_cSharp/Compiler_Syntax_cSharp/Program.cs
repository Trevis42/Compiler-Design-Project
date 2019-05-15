using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Compiler_Syntax_cSharp
{
    class Program
    {
        private static readonly string[][] precedenceTbl = new string[50][];
        private static readonly Stack<string> stack = new Stack<string>();
        public static string[] Labels { get; set; }

        private static void Main(string[] args)
        {
            string tokenTblFileName = "tokenTable.txt";
            string syntaxFileName = "syntax.txt";
            string pTblFile = "PrecTable.txt";
            string outputFilePath = Path.GetFullPath(syntaxFileName);
            string tknTablePath = @"H:\School\Sam Houston\Spring 2019\Compiler Design\Course Assignments\Compiler_Lexical_cSharp\Compiler_Lexical_cSharp\bin\Debug\netcoreapp3.0"; //needed

            if (File.Exists(outputFilePath))
            {
                File.Delete(syntaxFileName); //!resets the file due to writing by appending
            }
            string inFIle = $"{tknTablePath}\\{tokenTblFileName}";
            SyntaxCheck(ReadFileIn(inFIle), pTblFile);

            Console.WriteLine("\nPress any key to continue...");
            Console.ReadLine();
        }
        //===Y
        #region Syntax Eval Functions
        private static void SyntaxCheck(string[] fileLines, string pTbl)
        {
            //---G
            //local data
            int items = fileLines.Length;
            int attribs = 2;
            int knt = 0;

            string[][] tokens = new string[items][];
            string[] row;

            TableIn(pTbl, precedenceTbl); //initialize the table (create with empty strings in it)

            tokens = InitTable(tokens, attribs);
            var regex = new Regex(@"\s{2,}|\t", RegexOptions.Compiled);
            //---G
            //Process lines of token table
            for (int i = 0; i < fileLines.Length; i++) //reading one line at a time
            {
                string line = fileLines[i];
                line.Trim(' ');
                line.TrimStart(' ');
                line.TrimEnd(' '); //some extra trimming to make sure we dont bring in any whitespace

                var results = regex.Split(line);
                string name = results[0];
                string ident = results[1].TrimStart(' ');

                row = new string[2];
                tokens[knt] = results;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    row[0] = name;
                }
                else
                {
                    continue;
                }
                row[1] = ident;

                knt++;
            }
            GenerateQuads(tokens);
        }

        private static void GenerateQuads(string[][] tokenList)
        {
            int i = 0;
            int kount = 1;
            //bool tempTokenRead = false;
            string takes = ">";
            string catchNextToken = null;
            string quadsFileOut = "quads.txt";

            if (File.Exists(quadsFileOut))
            {
                File.Delete(quadsFileOut); //!resets the file due to writing by appending
            }

            //?scan through token list and compare current token to the next one
            stack.Push(";");
            Console.WriteLine($"stack initial: {stack.Peek()}");

            stack.Push(tokenList[0][0]);
            while (i < tokenList.Length-1)
            {
                string currToken;
                string nextToken;
                string tokenClass;
                string tokenClassNext = "";
                if (!String.IsNullOrWhiteSpace(catchNextToken))
                {
                    //this area is for dealing with the temps as they get put back on the stack
                    currToken = stack.Peek();
                    Console.WriteLine($"current Token: {currToken}");

                    tokenClass = $"{stack.Peek()}";
                    Console.WriteLine($"TokenClass: Temp");
                    i--;

                    //Next token
                    nextToken = tokenList[i + 1][0];
                    Console.WriteLine($"next Token : {nextToken}");
                    tokenClassNext = tokenList[i + 1][1];
                    Console.WriteLine($"TokenClassNext: {tokenClassNext}");
                }
                else
                {
                    //currToken = stack.Peek();
                    currToken = tokenList[i][0];
                    Console.WriteLine($"\ncurrent Token: {currToken}");
                    tokenClass = tokenList[i][1];
                    Console.WriteLine($"TokenCLass: {tokenClass}");

                    //Next
                    if (i == tokenList.Length) //if we somehow go past the index of the array
                    {
                        nextToken = ";";
                        Console.WriteLine($"next Token: {nextToken}");
                        Console.WriteLine($"TokenClassNext: _SEMICOLON");
                        stack.Push(nextToken);
                        Console.WriteLine($"End with semicolon: {stack.Peek()}");
                    }
                    else
                    {
                        nextToken = tokenList[i + 1][0];
                        Console.WriteLine($"next Token: {nextToken}");
                        tokenClassNext = tokenList[i + 1][1];
                        Console.WriteLine($"TokenClassNext: {tokenClassNext}");
                    }
                }
                catchNextToken = null;
                i++;

                //Relation checking
                string relation = CompareTokens(currToken, nextToken, tokenClass, tokenClassNext, Labels);
                if (relation.Equals("_"))
                {
                    Console.WriteLine($"{relation} := NONE");
                }
                bool relationValid;
                if (relation.Equals(takes))
                {
                    relationValid = true;
                    Console.WriteLine($"relation: {relation}");
                }
                else
                {
                    relationValid = false;
                    Console.WriteLine($"relation: {relation}");
                    stack.Push(nextToken);
                    Console.WriteLine($"Next Token pushed to stack: {nextToken}");
                }

                //!If relation equals > when pushed, pop till we find < and process taht as a quad, then push the result in stack
                while (relationValid && (!stack.Count().Equals(0) || (stack.Count().Equals(1) && stack.Peek().Equals(";"))))
                {
                    string val3 = stack.Pop();
                    string val1 = stack.Pop();
                    string val2 = stack.Pop();

                    //-========== Steps for generating quads ==========
                    //?Scan left to right pushing tokens into a stack untill the right side of the handle is discovered (">")
                    //?Scan down the stack till the left side of the handle in located ("<")
                    //?Reduce the handle placing the result in the stack (pop each item between handles and process)
                    //?Repeat steps above till input is recognized or rejected
                    string temp = $"T{kount}";
                    Reduction(val1, val2, val3, temp, quadsFileOut);
                    stack.Push(temp);
                    kount++;
                    catchNextToken = nextToken;
                    //stack.Push(nextToken);
                    relationValid = false;
                    if (stack.Count() == 0)
                    {
                        relationValid = false;
                    }
                }
            }
        }

        private static void Reduction(string val1, string val2, string val3, string temp, string quadsFileOut)
        {
            string quad = $"{val1} {val2} {val3} {temp}\n";
            Console.WriteLine($"\nQuad: {quad}");
            WriteFileOut(quad, quadsFileOut);
        }

        private static string CompareTokens(string token, string tokenNext, string tokenClass, string tokenClassNext, string[] labels)
        {
            //!need to add a check for identifies/idLits/variables/...etc.
            int i;
            int row = 0; 
            int col = 0;
            bool relationFound = false;

            while (relationFound == false)
            {
                for (i = 0; i < labels.Length; i++)
                {
                    if (token == labels[i])
                    {
                        row = i;
                        break;
                    }
                    if (tokenClass.Equals("_VAR") || tokenClass.Equals("VAR"))
                    {
                        string tkn = "<ident>";
                        row = Array.IndexOf(labels, tkn);
                        break;
                    }
                    if (tokenClass.Equals("_DIGIT"))
                    {
                        string tkn = "<integer>";
                        row = Array.IndexOf(labels, tkn);
                        break;
                    }
                }
                for (i = 0; i < labels.Length; i++)
                {
                    if (tokenNext == labels[i])
                    {
                        col = i;
                        break;
                    }
                    if (tokenClassNext.Equals("_VAR") || tokenClass.Equals("VAR"))
                    {
                        string tkn = "<ident>";
                        col = Array.IndexOf(labels, tkn);
                        break;
                    }
                    if (tokenClassNext.Equals("_DIGIT"))
                    {
                        string tkn = "<integer>";
                        col = Array.IndexOf(labels, tkn);
                        break;
                    }
                }
                relationFound = true;
            }
            string relation = precedenceTbl[row][col];
            return relation;
        }

        #endregion
        //===Y
        #region Utility Functions

        private static string[][] InitTable(string[][] tknArr, int attribs)
        {
            for (int i = 0; i < tknArr.GetLength(0); i++)
            {
                tknArr[i] = new string[attribs];
                for (int j = 0; j < attribs; j++)
                {
                    tknArr[i][j] = "";
                }
            }
            return tknArr;
        }

        #endregion
        //===Y
        #region File I/O

        private static void TableIn(string fileName, string[][] precedenceTbl)
        {
            string path = Path.GetFullPath(fileName);
            var rows = File.ReadAllLines(path).Select(l => l.Split('|').ToArray()).ToArray();
            string outFile = "table.txt";

            if (File.Exists(outFile))
            {
                File.Delete(outFile); //!resets the file since I am using appending to the file each time something writes to it.
            }
            //Then:
            bool stop = false;
            int kount = 0;
            foreach (var item in rows)
            {
                if (stop == false)  //write the labels to an array and then skip to the other lines
                {
                    Labels = item;
                    stop = true;
                    continue;
                }

                if (stop == true)
                {
                    precedenceTbl[kount] = item;

                    string output = outTable(item);
                    string outTable(string[] arr)
                    {
                        string output = "";
                        //set the output
                        foreach (var item in arr.AsSpan(..))
                        {
                            output += item + " ";
                        }
                        return output;
                    }

                    WriteFileOut(output + "\n", outFile);
                    //Console.WriteLine($"{output}{"\n"}");
                }

                kount++;
            }
        }

        private static string[] ReadFileIn(string fileName)
        {
            File.ReadAllText(fileName); //read into array
            FileInfo fileInfo = new FileInfo(fileName);
            var MAX = fileInfo.Length;
            _ = new string[MAX]; //only allocate memory here
            string[] allLines = File.ReadAllLines(fileName);

            return allLines;
        }

        private static void WriteFileOut(string output)
        {
            const string fileName = "syntaxOutput.txt"; //?change to default output
            File.AppendAllText(fileName, output);
        }

        private static void WriteFileOut(string output, string fileName)
        {
            File.AppendAllText(fileName, output);
        }

        #endregion
        //===Y
    }
}
