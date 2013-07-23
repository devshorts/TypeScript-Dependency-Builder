using System;
using System.IO;
using Newtonsoft.Json;
using TypescriptBuilders.Data;

namespace TypescriptBuilders
{      

    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var argsParse = new Arguments(args);

            var configPath = argsParse.ConfigPath ?? "depends.json";

            var jsonText = File.ReadAllText(configPath).Replace(@"\", @"\\");

            var config = JsonConvert.DeserializeObject<Config>(jsonText);

            Console.WriteLine("Running dependency builders");
            Console.WriteLine();

            new DependencyBuilder().Run(config);            


            foreach (var defConfig in config.Definitions)
            {
                Console.WriteLine();
                Console.WriteLine("========= Build out {1} {0} ==========", defConfig.RootFolder, defConfig.NameOfDefFile);
                Console.WriteLine();

                DefinitionsFileBuilder.BuildLocalDefFile(defConfig);
            }

            Console.WriteLine();
            Console.WriteLine("======== Generated! ==================");
        }
    }
}
