using Newtonsoft.Json;

namespace DebugNugetPkgDemoPkgForTrivialDemo
{
    public class SomeClass
    {
        public string SomeMethod()
        {
            string s = JsonConvert.SerializeObject(DateTime.Now);
            return s;
        }

    }
}