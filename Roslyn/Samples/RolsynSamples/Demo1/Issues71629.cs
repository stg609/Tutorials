using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Demo1
{
    //https://github.com/dotnet/runtime/issues/71629
    public class Issues71629
    {
        public static async Task Main2(string[] args)
        {
            string code = @"dynamic eo = new ExpandoObject(); 
                        // eo.abc = 123; // comment this line will fix the issue
                        Console.WriteLine(System.AppDomain.CurrentDomain.GetAssemblies().Count());";
            ScriptOptions so = ScriptOptions.Default
                .AddImports("System", "System.Linq", "System.Dynamic")
                .AddReferences("System", "System.Core", "Microsoft.CSharp");

            var cs = CSharpScript.Create(code, so);
            var compilation = cs.GetCompilation();

            //            var synTree = CSharpSyntaxTree.ParseText(@"
            //using System;
            //using System.Linq;
            //using System.Dynamic;

            //public class Program
            //{
            //    public static void Main(string[] args)
            //    {
            //        dynamic eo = new ExpandoObject();
            //        //eo.abc = 123;
            //        Console.WriteLine(System.AppDomain.CurrentDomain.GetAssemblies().Count());
            //        eo = null;
            //    }
            //}");
            //        var  compilation =  CSharpCompilation.Create("test", new[] { synTree }, new[]{
            //                MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            //                MetadataReference.CreateFromFile(typeof(ExpandoObject).Assembly.Location),
            //                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            //                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
            //                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
            //                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly.Location),
            //                });


            using MemoryStream ms = new MemoryStream();
            var rslt = compilation.Emit(ms);

            while (true)
            {
                ms.Seek(0, SeekOrigin.Begin);
                AssemblyLoadContext lc = new AssemblyLoadContext("test", isCollectible: true);
                var ass = lc.LoadFromStream(ms);

                var typ = ass.GetType("Submission#0");
                var mem = typ.GetMethod("<Factory>", BindingFlags.Static | BindingFlags.Public);
                //var typ = ass.GetTypes().First() ;
                //var mem = typ.GetMethods().First();
                //var retTask = mem.Invoke(null, new object[] { null}) ;
                var retTask = mem.Invoke(null, new object[] { new object[2] }) as Task<object>;
                var rsltTsk = await retTask;

                lc.Unload();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                await Task.Delay(200);
            }
        }
    }
}
