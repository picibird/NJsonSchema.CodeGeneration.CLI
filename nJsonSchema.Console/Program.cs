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

            var schemasFiles = sDirInfo.GetFiles().ToList().Where((f) =>
            {
                return f.Extension.ToLower().Equals(".json");
            });

            foreach (FileInfo schemafile in schemasFiles)
            {
                var schema = JsonSchema4.FromFile(schemafile.FullName);
                var generator = new TypeScriptGenerator(schema);
                var typeScript = generator.GenerateFile();
                Save(sDirInfo, tDirInfo, schemafile, typeScript, ".ts");
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
    }
}
