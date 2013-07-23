using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TypescriptBuilders.Data;

namespace TypescriptBuilders
{
    public class DependencyBuilder
    { 
        private Dictionary<string, List<string>> _filesByDependency = new Dictionary<string, List<string>>();
 
        public void Run(Config config)
        {           
            
            PopulateFiles(config);

            foreach (var dependency in config.Dependencies.Where(d => !String.IsNullOrEmpty(d.IndexPage)))
            {
                var files = FilesForDependency(config, dependency);

                if (files != null)
                {
                    var indexPath = Path.Combine(dependency.RelativePath, dependency.IndexPage);

                    BuildDefaultAspx(indexPath, files);

                    Console.WriteLine("Done adding script blocks to {0}", indexPath);
                }
            }

        }

        private List<string> ExcludeDependency(List<string> source, Dependency dependency)
        {
            var excluded = source;
            if (dependency.ExcludeFolders != null)
            {
                excluded = excluded.Where(f => !dependency.ExcludeFolders.Any(f.Contains)).ToList();
            }

            if (dependency.ExcludeNames != null && dependency.ExcludeNames.Count > 0)
            {
                excluded = excluded.Where(f => !dependency.ExcludeNames.Any(f.Contains)).ToList();
            }
            return excluded;
        } 

        private List<string> FilesForDependency(Config config, Dependency dependency)
        {
            var dep = FlattenDependencies(config, dependency);
            dep.Remove(dependency.Name);
            dep.Sort(new TypeScriptSorter());
            
            var dependentFiles = (from d in dep
                                     where _filesByDependency.ContainsKey(d)
                                     let files = _filesByDependency[d]                                     
                                     select files)
                                     .SelectMany(i =>
                                         {
                                             i.Sort(new TypeScriptSorter());
                                             return i;
                                         }).ToList();

            _filesByDependency[dependency.Name].Sort(new TypeScriptSorter());
            dependentFiles = dependentFiles.Concat(_filesByDependency[dependency.Name]).ToList();

            dependentFiles = ExcludeDependency(dependentFiles, dependency);

            var localUri = new Uri(Path.Combine(dependency.RelativePath, dependency.IndexPage));

            dependentFiles = dependentFiles.Select(f => localUri.MakeRelativeUri(new Uri(f)).ToString()).ToList();

            return dependentFiles.Distinct().ToList();
        }

        private List<string> FlattenDependencies(Config config, Dependency dependency)
        {
            var l = new List<string>();

            l.Add(dependency.Name);

            if (dependency.DependsOnNames == null)
            {
                return l;
            }

            foreach (var d in dependency.DependsOnNames)
            {
                var dependent = config.Dependencies.FirstOrDefault(x => x.Name == d);

                l.AddRange(FlattenDependencies(config, dependent));
            }

            return l.Distinct().ToList();
        } 

        private void PopulateFiles(Config config)
        {
            foreach (var dependency in config.Dependencies)
            {
                if (String.IsNullOrEmpty(dependency.Name) || String.IsNullOrEmpty(dependency.RelativePath))
                {
                    continue;
                }

                dependency.RelativePath = Path.GetFullPath(new Uri(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                                                       dependency.RelativePath)).LocalPath);

                _filesByDependency[dependency.Name] = Directory.EnumerateFiles(dependency.RelativePath, "*.js", SearchOption.AllDirectories).ToList();

                _filesByDependency[dependency.Name] = ExcludeDependency(_filesByDependency[dependency.Name], dependency);
            }
        }

        private static void BuildDefaultAspx(string indexPage, List<string> unorderedFiles)
        {           
            UpdateIndexPage(indexPage, unorderedFiles);
        }

        private static void UpdateIndexPage(string defaultAspx, List<string> includes)
        {
            if (!File.Exists(defaultAspx))
            {
                Console.WriteLine("{0} doesn't exist, not updating", defaultAspx);
                return;
            }
            var text = File.ReadAllText(defaultAspx);

            Func<String, string> wrap = input => String.Format("<!-- GENERATED LOCAL DATA OUT START -->{0}<!-- GENERATED LOCAL DATA OUT END", input);

            var regex = new Regex(wrap(".*"), RegexOptions.Singleline);

            var scriptBlock = includes.Select(WrapAsScriptWithTabs).Aggregate((a, i) => a + i).Trim().TrimEnd(new[] { ',' });

            var wrapped = regex.Replace(text, wrap(Environment.NewLine + Tabs() + String.Format("{0}", scriptBlock) + Environment.NewLine + Tabs()));

            File.WriteAllText(defaultAspx, wrapped);
        }

        private static String WrapAsScriptWithTabs(string file)
        {
            return String.Format(Tabs() + "{0}{1}", WrapAsScriptBlock(file), Environment.NewLine).Replace("\\", "/");
        }

        private static string WrapAsScriptBlock(string trimemdFile)
        {
            return String.Format(@"<script src=""{0}""></script>", trimemdFile);
        }

        private static String Tabs(int count = 3)
        {
            return Enumerable.Range(0, count).Select(i => "   ").Aggregate((acc, i) => acc + i);
        }
    }
}
