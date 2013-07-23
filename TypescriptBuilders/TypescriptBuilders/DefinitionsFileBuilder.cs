using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TypescriptBuilders.Data;

namespace TypescriptBuilders
{
    class DefinitionsFileBuilder
    {
        private static string ConvertToFullPath(string relativePath)
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), relativePath);
        }

        private static string DelimHeader(string delineator)
        {
            return String.Format("// --------{0}--------", delineator);
        }

        private static string DelimFooter(string delineator)
        {
            return String.Format("// --------END {0}--------", delineator);
        }

        private static string Wrap(string text, string delineator)
        {
            return DelimHeader(delineator) + text + DelimFooter(delineator);
        }

        public static void BuildLocalDefFile(DefinitionsFileConfig config)
        {
            var allDTs = ConvertToFullPath(Path.Combine(config.RootFolder, config.NameOfDefFile));
            
            if (!File.Exists(allDTs))
            {
                Console.WriteLine("Cannot find {0}, creating emtpy", allDTs);
                File.Create(allDTs).Close();
            }

            var angularFolders = config.SubFolders;

            var text = File.ReadAllText(allDTs);

            foreach (var folderWithFiles in angularFolders)
            {
                var delineatorName = folderWithFiles.ToUpperInvariant();

                var fullPath = ConvertToFullPath(Path.Combine(config.RootFolder, folderWithFiles));

                if (!Directory.Exists(fullPath))
                {
                    Console.WriteLine("Not testing {0} since it doesn't exist", folderWithFiles);
                    continue;
                }

                var files = Directory.EnumerateFiles(fullPath, "*.ts", SearchOption.AllDirectories).ToList();

                if (files.Count > 0)
                {

                    if (!text.Contains(DelimHeader(delineatorName)))
                    {
                        Console.WriteLine("--> Creating block {0}", folderWithFiles); 
                        text += Environment.NewLine + Wrap(Environment.NewLine, delineatorName) + Environment.NewLine;
                    }

                    var relativePath = new Uri(ConvertToFullPath(fullPath));

                    var tsFiles = Environment.NewLine + 
                                      files.Select(file => FormatTypeScriptReference(relativePath, file))
                                           .Aggregate((acc, i) => acc + Environment.NewLine + i) + Environment.NewLine;


                    var regex = new Regex(Wrap(".*", delineatorName), RegexOptions.Singleline);

                    text = regex.Replace(text, Wrap(tsFiles, delineatorName));


                    Console.WriteLine("Updated block {0}", folderWithFiles);                    
                }
            }

            File.WriteAllText(allDTs, text);            
        }

        private static string FormatTypeScriptReference(Uri relativePath, string file)
        {
            return String.Format("/// <reference path=\"./{0}\"/>",
                                 relativePath.MakeRelativeUri(new Uri(ConvertToFullPath(file))));
        }
    }
}
