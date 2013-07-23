using System;
using System.Collections.Generic;

namespace TypescriptBuilders.Data
{
    public class Dependency
    {
        public String Name { get; set; }
        public String RelativePath { get; set; }
        public String IndexPage { get; set; }
        public List<string> DependsOnNames { get; set; }
        public List<string> ExcludeFolders { get; set; }
        public List<string> ExcludeNames { get; set; } 
    }
}
