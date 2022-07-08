using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Demo1
{
    //https://github.com/dotnet/runtime/issues/71629
    public class TestMemoryCache
    {
        public class ContextWrapper
        {
            public object Context { get; set; }
        }

        public static async Task Main(string[] args)
        {
            string code = @"
return SomeMethod(Context); // use to get the return value and the variables like CSharpScript.Run
object SomeMethod(dynamic context)
{
    // actual code == start
    dynamic eo = new ExpandoObject(); 
    //eo.abc = 123; // comment out this line to solve the problem
    //context.Append(""aaa""); // comment out this line to solve the problem

    Console.WriteLine(System.AppDomain.CurrentDomain.GetAssemblies().Count());
    return 1;
    // == end


    return null;
}";

            ScriptOptions so = ScriptOptions.Default
                .AddImports("System", "System.Linq", "System.Dynamic")
                .AddReferences("System", "System.Core", "Microsoft.CSharp");

            var cs = CSharpScript.Create(code, so, typeof(ContextWrapper));
            var compilation = cs.GetCompilation();

            IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
            int i = 0;
            while (i < 25)
            {
                object[] rslt = cache.GetOrCreate("a", itm =>
                
                {
                    Console.WriteLine("create new");
                    itm.SetAbsoluteExpiration(TimeSpan.FromSeconds(2));
                    itm.RegisterPostEvictionCallback((object key, object value, EvictionReason reason, object state) =>
                    {
                        //var d = (AssemblyLoadContext)value;
                        //d.Unload();
                        var d = (object[])value;
                        ((AssemblyLoadContext)d.First()).Unload();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();

                        Console.WriteLine("posted");
                    });

                    using MemoryStream ms = new MemoryStream();
                    var rslt = compilation.Emit(ms);


                    ms.Seek(0, SeekOrigin.Begin);
                    AssemblyLoadContext lc = new AssemblyLoadContext("test", isCollectible: true);
                    var ass = lc.LoadFromStream(ms);

                    var typ = ass.GetType("Submission#0");
                    var mem = typ.GetMethod("<Factory>", BindingFlags.Static | BindingFlags.Public);
                    
                    var strongTypeDelegate = (Func<object[], Task<object>>)Delegate.CreateDelegate(typeof(Func<object[], Task<object>>), null, mem);

                    return new object[] { lc, strongTypeDelegate };

                });

                //Console.WriteLine(loadContext.Assemblies.Count());
                //var context = new ContextWrapper { Context = new StringBuilder() };
                //var rslt = await del(new object[2] { context, null });
                //del = null;

                await Task.Delay(500);
                i++;

            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Console.WriteLine("===" + System.AppDomain.CurrentDomain.GetAssemblies().Count());

            Console.ReadLine();
        }
    }
}
