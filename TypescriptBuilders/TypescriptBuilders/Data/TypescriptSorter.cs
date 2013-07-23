using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypescriptBuilders.Data
{
    public class TypeScriptSorter : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            var source = x.ToLowerInvariant();
            var target = y.ToLowerInvariant();

            var types = new List<string> { "base", "data", "model", "services", "interceptor", "controllers", "directives", "filters", "app", "tests", "unittests" };

            var sourceType = types.IndexOf(types.FirstOrDefault(source.Contains));
            var targetType = types.IndexOf(types.FirstOrDefault(target.Contains));

            return sourceType.CompareTo(targetType);
        }
    }
}
