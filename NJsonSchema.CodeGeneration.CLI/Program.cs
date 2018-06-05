using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using NJsonSchema.CodeGeneration.TypeScript;

namespace nJsonSchema.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).Wait();
        }

        static async Task MainAsync(string[] args)
        {
            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser()
            {
                CheckMandatoryArguments = true,
                ShowUsageOnEmptyCommandline = true
            };

            DirectoryArgument schemaDirArg = new DirectoryArgument('s', "schema", "json schema source directory")
            {
                Optional = false,
                DirectoryMustExist = true
            };
            DirectoryArgument typeScriptDirArg = new DirectoryArgument('t', "typescript", "typescript target directory")
            {
                Optional = true,
                DirectoryMustExist = false
            };
            DirectoryArgument cSharpDirArg = new DirectoryArgument('c', "csharp", "c# target directory")
            {
                Optional = true,
                DirectoryMustExist = false
            };

            SwitchArgument interactive = new SwitchArgument('i', "interactive", "interactive yes/no to continue", false);
            SwitchArgument recursive = new SwitchArgument('r', "recursive", "recursive parsing from source directory", false);

            parser.Arguments.Add(schemaDirArg);
            parser.Arguments.Add(typeScriptDirArg);
            parser.Arguments.Add(cSharpDirArg);
            //parser.Arguments.Add(interactive);
            //parser.Arguments.Add(recursive);


            try
            {
                parser.ParseCommandLine(args);
                //parser.ShowParsedArguments();
                if (!parser.ParsingSucceeded)
                    return;
            }
            catch (Exception e)
            {
                System.Console.WriteLine("Exception parsing arguments:");
                System.Console.WriteLine(e.Message);
                parser.PrintUsage(System.Console.Out);
                System.Console.WriteLine("press any key to continue");
                System.Console.ReadKey();
                return;
            }


            if (typeScriptDirArg.DirectoryInfo == null &&
                cSharpDirArg.DirectoryInfo == null)
            {
                System.Console.WriteLine("TypeScript and/or C# target directory missing");
                System.Console.WriteLine("press any key to continue");
                System.Console.ReadKey();
                return;
            }

            var sDirInfo = schemaDirArg.DirectoryInfo;
            var tDirInfo = typeScriptDirArg.DirectoryInfo;
            var cDirInfo = cSharpDirArg.DirectoryInfo;
            if (tDirInfo != null && !tDirInfo.Exists)
            {
                tDirInfo.Create();
            }
            if (cDirInfo != null && !cDirInfo.Exists)
            {
                cDirInfo.Create();
            }



            //get only .json files
            var schemasFiles = sDirInfo.GetFiles().ToList().Where((f) => f.Extension.ToLower().Equals(".json")).ToList();
            System.Console.WriteLine("found {0} json schema files", schemasFiles.Count());
            foreach (FileInfo schemafile in schemasFiles)
            {
                System.Console.WriteLine("generating from {0} to", schemafile.Name);
                try
                {
                    var schema = await JsonSchema4.FromFileAsync(schemafile.FullName);
                    //typescript
                    if (tDirInfo != null)
                    {

                        var generator = new TypeScriptGenerator(schema);
                        var typeScript = generator.GenerateFile();
                        Save(sDirInfo, tDirInfo, schemafile, typeScript, ".ts");
                    }
                    //c#
                    if (cDirInfo != null)
                    {
                        var generator = new CSharpGenerator(schema);
                        var cSharp = generator.GenerateFile();
                        Save(sDirInfo, cDirInfo, schemafile, cSharp, ".cs");
                    }

                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Exception: {0} ", ex.Message);
                    System.Console.WriteLine("Overstep and continue generating remaining?");
                    if (!GetYesOrNoUserInput())
                        return;
                }

            }
            //exit
            System.Console.WriteLine("Done!");
            System.Console.WriteLine("Press any key to exit.");
            System.Console.ReadKey();

        }

        private static void Save(DirectoryInfo source, DirectoryInfo target, FileInfo schema, string data, string fileExtension)
        {
            string fileName = Path.GetFileNameWithoutExtension(schema.Name);
            fileName = fileName.Replace(".schema", "");
            string filePath = Path.Combine(target.FullName, fileName + fileExtension);
            var file = new FileInfo(filePath);
            if (!file.Exists) file.Create().Close();
            using (StreamWriter sw = file.CreateText())
            {
                sw.Write(data);
            }
            System.Console.WriteLine(filePath);
        }

        static bool GetYesOrNoUserInput()
        {
            ConsoleKey response; // Creates a variable to hold the user's response.

            do
            {
                while (System.Console.KeyAvailable) // Flushes the input queue.
                    System.Console.ReadKey();

                System.Console.Write("Y or N? "); // Asks the user to answer with 'Y' or 'N'.
                response = System.Console.ReadKey().Key; // Gets the user's response.
                System.Console.WriteLine(); // Breaks the line.
            } while (response != ConsoleKey.Y && response != ConsoleKey.N); // If the user did not respond with a 'Y' or an 'N', repeat the loop.

            /* 
             * Return true if the user responded with 'Y', otherwise false.
             * 
             * We know the response was either 'Y' or 'N', so we can assume 
             * the response is 'N' if it is not 'Y'.
             */
            return response == ConsoleKey.Y;
        }
    }
}
