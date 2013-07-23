using System.Collections.Generic;

namespace TypescriptBuilders.Data
{
    public class Config
    {        
        public List<Dependency> Dependencies { get; set; }
        public List<DefinitionsFileConfig> Definitions { get; set; } 
    }
}
