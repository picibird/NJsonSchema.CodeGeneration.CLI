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

            CommandLineParser.CommandLineParser parser = new CommandLineParser.CommandLineParser()
            {
                CheckMandatoryArguments = true,
                ShowUsageOnEmptyCommandline = true
            };

            DirectoryArgument sourceDirArg = new DirectoryArgument('s', "source", "source directory with json schema files")
            {
                Optional = false,
                DirectoryMustExist = false
            };
            DirectoryArgument targetDirArg = new DirectoryArgument('t', "target", "target directory with typescript  files")
            {
                Optional = false,
                DirectoryMustExist = false
            };
            SwitchArgument showArgument = new SwitchArgument('r', "recursive", "recursive parsing from source directory", false);

            parser.Arguments.Add(sourceDirArg);
            parser.Arguments.Add(targetDirArg);
            parser.Arguments.Add(showArgument);


            try
            {
                parser.ParseCommandLine(args);
                parser.ShowParsedArguments();
                if (!parser.ParsingSucceeded)
                    return;
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
                return;
            }


            var sDirInfo = sourceDirArg.DirectoryInfo;
            var tDirInfo = targetDirArg.DirectoryInfo;
            if (!tDirInfo.Exists)
            {
                tDirInfo.Create();
            }
            //get only .json files
            var schemasFiles = sDirInfo.GetFiles().ToList().Where((f) => f.Extension.ToLower().Equals(".json")).ToList();
            System.Console.WriteLine("found {0} json schema files", schemasFiles.Count());
            foreach (FileInfo schemafile in schemasFiles)
            {
                var schemaName = Path.GetFileNameWithoutExtension(schemafile.Name);
                System.Console.WriteLine("generating {0} ", schemaName + ".ts");
                try
                {
                    var schema = JsonSchema4.FromFile(schemafile.FullName);
                    var generator = new TypeScriptGenerator(schema);
                    var typeScript = generator.GenerateFile();
                    Save(sDirInfo, tDirInfo, schemafile, typeScript, ".ts");
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine("Exception: {0} ", ex.Message);
                    System.Console.WriteLine("Overstep and continue generating remaining?");
                    if (!GetYesOrNoUserInput())
                        return;
                }
                
            }
            
        }

        private static void Save(DirectoryInfo source, DirectoryInfo target, FileInfo schema, string data, string fileExtension)
        {
            string filePath = Path.Combine(target.FullName,
                Path.GetFileNameWithoutExtension(schema.Name) + fileExtension);
            var file = new FileInfo(filePath);
            if (!file.Exists) file.Create().Close();
            using (StreamWriter sw = file.CreateText())
            {
                sw.Write(data);
            }
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
