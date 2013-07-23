using System;

namespace TypescriptBuilders
{
    public class Arguments
    {      
        public String ConfigPath { get; set; }

        public Arguments(string[] args)
        {           
            try
            {
                ConfigPath = args[0];
            }
            catch (Exception)
            {
            }
        }
    }
}
