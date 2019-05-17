
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Compiler_Lexical_cSharp
{
    class Lexical
    {
        //?I probably should have used more class variables
        public static string[] wordsArr = new string[50];
        public static int wordKnt = 0;
        public static string[][] tknArray; //This was here as a test for something that isnt working and would make code worse to program

        //This is where all the keywords and classifications are stored
        #region Enums and Dictionaries
        private enum Classifications
        {
            //PARSER CLASSIFICATIONS
            NOT_DEFINED,
            KEYWRD_CLASS,
            KEYWRD_PROCEDURE,
            KEYWRD_CALL,
            KEYWRD_VAR,
            KEYWRD_CONST,
            KEYWRD_ODD,
            //LOGIC
            KEYWRD_WHILE,
            KEYWRD_DO,
            KEYWRD_IF,
            KEYWRD_FOR,
            KEYWRD_THEN,
            KEYWRD_ELSE,
            //BRACES
            LEFTBRACK,
            RIGHTBRACK,
            LEFTPARHEN,
            RIGHTPARHEN,
            //MISC
            _VAR,
            _ASSIGN,
            _IDLIT,
            _INTEGER,
            _LETTER,
            _DIGIT,
            _SPECIAL_CHAR,
            _COMMENT,
            _COMMA,
            _PERIOD,
            _SEMICOLON,
            _LCOMMENT,
            _RCOMMENT,
            _WHTSPACE,
            _WHTSPACE_TAB,
            //OPPERATIONS, ETC.
            _mOP,
            _addOP,
            _relOP,
        }

        private enum Delimiters : int
        {
            LCurly = '{',
            RCurly = '}',
            LParhen = '(',
            RParhen = ')',
            SemiCol = ';',
            Comma = ',',
            Period = '.',
            NewLine = '\n'
        }

        private enum MOp : int
        {
            Divide = '/',
            Mult = '*',
            Mod = '%'
        }

        private static readonly Dictionary<string, Classifications> Tokens = new Dictionary<string, Classifications>()
        {   //Keywords, and Not found
            { "not found", Classifications.NOT_DEFINED }, { "CLASS", Classifications.KEYWRD_CLASS }, { "CONST", Classifications.KEYWRD_CONST }, { "ODD", Classifications.KEYWRD_ODD },
            { "VAR", Classifications.KEYWRD_VAR }, { "IF", Classifications.KEYWRD_IF }, { "ELSE", Classifications.KEYWRD_IF }, { "THEN", Classifications.KEYWRD_THEN },
            { "WHILE", Classifications.KEYWRD_WHILE }, { "FOR", Classifications.KEYWRD_FOR }, { "DO", Classifications.KEYWRD_DO },
            { "PROCEDURE", Classifications.KEYWRD_PROCEDURE }, { "CALL", Classifications.KEYWRD_CALL },
            //Other
            { "_VAR", Classifications._VAR }, { @"^[0-9]$", Classifications._DIGIT }, { @"^[a-zA-Z]$", Classifications._LETTER },
            { "{", Classifications.LEFTBRACK }, { "}", Classifications.RIGHTBRACK }, { "(", Classifications.LEFTPARHEN }, { ")", Classifications.RIGHTPARHEN },
            {"=", Classifications._ASSIGN }, { ";", Classifications._SEMICOLON}, {",", Classifications._COMMA}, { ".", Classifications._PERIOD },
            //Comments and Whitespace
            { "/*", Classifications._LCOMMENT }, { "*/", Classifications._RCOMMENT }, { "//", Classifications._COMMENT },
            { "    ", Classifications._WHTSPACE_TAB}, {" ", Classifications._WHTSPACE}, {@"[\t]", Classifications._WHTSPACE_TAB},
            //relation Operators
            { ">=", Classifications._relOP}, { "<=", Classifications._relOP }, { "!=", Classifications._relOP }, { "!", Classifications._relOP },
            { ">", Classifications._relOP}, { "<", Classifications._relOP }, { "==", Classifications._relOP },
            //Multiple Operators and Add Operators
            { "%=", Classifications._mOP }, { "%", Classifications._mOP },
            { "*=", Classifications._mOP }, { "/=", Classifications._mOP}, { "*", Classifications._mOP }, { "/", Classifications._mOP },
            { "+=", Classifications._addOP }, { "-=", Classifications._addOP }, { "+", Classifications._addOP }, { "-", Classifications._addOP }
        };

        #endregion

        //Bread and butter methods for this lab
        #region Lexical Stuff
        private static void TokensToFile(string word, string value)
        {

            if (value == "not found")
            {
                if (Regex.IsMatch(word, @"^[a-zA-Z0-9]+$"))
                {
                    if (Regex.IsMatch(word, @"^[a-zA-Z]+$"))
                    {
                        WriteFileOut($"{word,-28}\t{Tokens["_VAR"]}\n");
                        Console.WriteLine($"{word,-28}\t{Tokens["_VAR"]}");
                    }
                    if (Regex.IsMatch(word, @"^[0-9]+$"))
                    {
                        WriteFileOut($"{word,-28}\t{Tokens[@"^[0-9]$"]}\n");
                        Console.WriteLine($"{word,-28}\t{Tokens[@"^[0-9]$"]}");
                    }
                    //WriteFileOut($"{word, -28}\t{Tokens["_VAR"]}\n");
                    //Console.WriteLine($"{word, -28}\t{Tokens["_VAR"]}");
                }
                else if (Regex.IsMatch(word, @"^\s+$"))
                {
                    WriteFileOut($"{word, -28}\t{Tokens[@"\s+"]}\n");
                    Console.WriteLine($"{word, -28}\t{Tokens[@"\s+"]}");
                }
                else
                {
                    WriteFileOut($"{word, -28}\t{Tokens["not found"]}\n");
                    Console.WriteLine($"{word, -28}\t{Tokens["not found"]}");
                }
            }
            else
            {
                WriteFileOut($"{word, -28}\t{value}\n");
                Console.WriteLine($"{word, -28}\t{value}");
            }
        }

        private static void AssignTokens(string word)
        {
            Dictionary<string, Classifications>.KeyCollection tknKeys = Tokens.Keys;
            //Dictionary<string, Classifications>.ValueCollection tknVals = Tokens.Values;
            int kount = 0;
            foreach (var key in tknKeys)
            {
                kount++;
                if (word.Equals(key)) //tokens.ContainsKey(word)
                {
                    TokensToFile(word, Tokens[key].ToString());
                    break;
                }
                if (kount >= Tokens.Count)
                {
                    if (string.IsNullOrWhiteSpace(word))
                    {
                        continue;
                    }
                    TokensToFile(word, "not found");
                }
            } 
        }

        private static void Scanner(string[] arr)
        {
            var word = "";
            int lineKnt = 0;
            //wordKnt = 0;
            
            //tknArray = new string[arr.Length][];
            foreach (var line in arr) //?reading one line at a time
            {
                //wordKnt = 0;
                foreach (var c in line) //?read each character in the line, concatenate to a string until it matches a token
                {
                    //?if not whitespace
                    if (!char.IsWhiteSpace(c))
                    {
                        //?and not delimiter
                        if (!c.Equals((char)Delimiters.LCurly) && !c.Equals((char)Delimiters.RCurly)
                        && !c.Equals((char)Delimiters.LParhen) && !c.Equals((char)Delimiters.RParhen)
                        && !c.Equals((char)Delimiters.SemiCol) && !c.Equals((char)Delimiters.Comma)
                        && !c.Equals((char)Delimiters.NewLine) && !c.Equals((char)Delimiters.Period))
                        {
                            //?add character to word
                            word += c;
                        }
                        else
                        {
                            //?process word before a delimiter
                            AssignTokens(word);
                            //wordsArr[wordKnt] = word; //!+Can comment these out as they are not working properly
                            //wordKnt++;                //!+Can comment these out as they are not working properly
                            word = "";
                        }
                    }
                    else
                    {
                        //?checkwhite space and process word before it
                        AssignTokens(word);
                        //wordsArr[wordKnt] = word; //!+Can comment these out as they are not working properly
                        //wordKnt++;                //!+Can comment these out as they are not working properly
                        word = "";
                    }
                    //?check if is whitespace
                    if (char.IsWhiteSpace(c))
                    {
                        //?set word to empty and then skip whitespace
                        word = "";
                        //wordKnt--;
                        //AssignTokens(word); //this was used for when I was processing white space in token table
                        continue;
                    }
                    //?check if its a delimiter
                    if (c.Equals((char)Delimiters.LCurly) || c.Equals((char)Delimiters.RCurly)
                        || c.Equals((char)Delimiters.LParhen) || c.Equals((char)Delimiters.RParhen)
                        || c.Equals((char)Delimiters.SemiCol) || c.Equals((char)Delimiters.Comma)
                        || c.Equals((char)Delimiters.NewLine) || c.Equals((char)Delimiters.Period))
                    {
                        //?add delimiter character to word and process
                        word = "";
                        word += c;
                        AssignTokens(word);
                        //wordsArr[wordKnt] = word; //!+Can comment these out as they are not working properly
                        //wordKnt++;                //!+Can comment these out as they are not working properly
                        word = "";
                    }
                }
                //string[] wrdArray = new string[wordKnt];
                //for (int i = 0; i < wordKnt; i++)
                //{
                //    wrdArray[i] = wordsArr[i];
                //}
                //tknArray[lineKnt] = wrdArray;
                lineKnt++;
            }
            //OutputArray(tknArray);
            Console.WriteLine($"Press any key to continue...");
            Console.ReadLine();
        }

        private static void SymbolTable(string[] arr)
        {
            //create symbol table below....
            Console.Clear();
            //---G
            //local data
            int items = arr.Length; //update based on the total number tokens or states.
            int attribs = 4;
            int knt = 0;
            int state = 0;
            var word = "~placeholder~";

            string[][] symbolTable = new string[items][];
            symbolTable = InitTable(symbolTable, attribs, word);
            var regex = new Regex(@"\s{2,}|\t");
            //var regex = new Regex(@"^\s+|\t", RegexOptions.Compiled);
            //---G

            //Process lines of token table and 
            //!+_run through state machine_ (<this part not implemented yet)
            //***B
            foreach (var line in arr) //reading one line at a time
            {
                line.Trim(' '); //might need this... not sure yet
                line.TrimStart(' '); //not sure if I need this one (since I want to get the white space
                line.TrimEnd(' ');

                var results = regex.Split(line);
                string name = results[0];
                string ident = results[1].TrimStart(' ');

                //CheckDualSymbols(line); //like comments //, /* or ==, etc...

                //!+Code for symboltable...
                //!+requires a switch statement w/ state machine States
                //===G
                /*************************************************************************
                 * Place Symbol table below, Once each item is found, find its value
                 * and add it to row[]. (use code below, possibly move output code below
                 * to a new method)
                 * Then create its address on memory and its segment.
                 *************************************************************************/
                //!make switchstatement here...
                //?switch by state and check key that matches that state, then go to next state.

                //?make switch based on state
                //?should I make it switch on the keywords, or some other way
                switch (state)
                {
                    case 0:
                        //Console.WriteLine($"State {state}: {word}");
                        if (CheckMatch(word))
                        {
                            state = 1;
                        }
                        break;
                    case 1:
                        //--out put name of program (variable imediately after CLASS
                        //Console.WriteLine($"State {state}: {word}");
                        break;
                    case 2: //do something
                    default:
                        break;
                }


                string[] row = symbolTable[knt];
                if (!CheckWhiteSpace(name))
                {
                    row[0] = name;
                }
                else
                {
                    continue;
                }

                row[1] = ident;

                string output = "";
                
                //set the output
                foreach (var item in row.AsSpan(..))
                {
                    output += item + ", ";
                }

                knt++;
                //===G
                //trim and print to text file
                string fileName = "symboltable.txt";
                output = output.TrimEnd(' ');
                output = output.TrimEnd(',');
                output += "\n";
                Console.Write($"Current item (trimmed): {output}");
                WriteFileOut(output, fileName); //print to a new file
            }
            //***B
        }

        #endregion

        //Utility mehtods like reading and wrting files, manipulation of arrays, etc.
        #region Utility Methods

        private static void OutputArray(string[] arr)
        {
            string output = "";
            for (int i = 0; i < arr.Length; i++)
            {
                //output = $"[{i}]: {arr[i]}";
                output = $"{arr[i]} ";
                Console.Write($"{output.Trim()}");
            }
        }

        private static void OutputArray(string[][] tokenArray)
        {
            //string output = "";
            foreach (string[] arrs in tokenArray)
            {
                OutputArray(arrs);
                Console.WriteLine();
            }
            //WriteFileOut(output, "tokenArray.txt");
        }

        private static bool CheckMatch(string word)
        {
            foreach (var key in Tokens.Keys)
            {
                if (word == key)
                {
                    return true;
                }
            }
            return false;
        }

        private static string[][] InitTable(string[][] symbolTable, int attribs, string word)
        {
            for (int i = 0; i < symbolTable.GetLength(0); i++)
            {
                symbolTable[i] = new string[attribs];
                for (int j = 0; j < attribs; j++)
                {
                    symbolTable[i][j] = word;
                }
            }
            return symbolTable;
        }

        private static bool CheckWhiteSpace(string element)
        {
            foreach (var c in element)
            {
                if (c.Equals(' '))
                {
                    return true;
                }
            }

            if (string.IsNullOrEmpty(element))
            {
                return true;
            }

            return false;
        }

        private static string[] ReadFileIn(string fileName)
        {
            //FileStream fileStream = new FileStream("file.txt", FileMode.Open);
            //using (StreamReader reader = new StreamReader(fileStream))
            //{
            //    string line = reader.ReadLine();
            //}

            //or maybe...
            File.ReadAllText(fileName); //read into array then process array??
            FileInfo fileInfo = new FileInfo(fileName);
            var MAX = fileInfo.Length;
            _ = new string[MAX]; //only allocate memory here
            string[] allLines = File.ReadAllLines(fileName);

            return allLines;
        }

        private static void WriteFileOut(string output)
        {
            const string fileName = "tokenTable.txt";
            File.AppendAllText(fileName, output);
        }

        private static void WriteFileOut(string output, string fileName)
        {
            File.AppendAllText(fileName, output);
        }

        #endregion

        private static void Main(string[] args)
        {
            string tokenTblFileName = "tokenTable.txt";
            string symTbleFileName = "symboltable.txt";
            string tknArrayFileName = "tokenArray.txt";
            string outputFilePath = Path.GetFullPath(tokenTblFileName);
            string symbleTableFilePath = Path.GetFullPath(symTbleFileName);
            string tknArrayFilePath = Path.GetFullPath(symTbleFileName);
            //Console.WriteLine($"GetFullPath('{outFileName}') returns '{outputFilePath}'"); //!for debug only (checking for correct path of file)

            if (File.Exists(outputFilePath))
            {
                File.Delete(tokenTblFileName); //!resets the file since I am using appending to the file each time something writes to it.
            }

            if (File.Exists(symbleTableFilePath))
            {
                File.Delete(symTbleFileName); //!resets the file since I am using appending to the file each time something writes to it.
            }

            if (File.Exists(tknArrayFilePath))
            {
                File.Delete(tknArrayFileName); //!resets the file since I am using appending to the file each time something writes to it.
            }

            const string fileName = "testCode.txt";
            Scanner(ReadFileIn(fileName));
            SymbolTable(ReadFileIn(tokenTblFileName));
            Console.ReadLine();
        }
    }
}
