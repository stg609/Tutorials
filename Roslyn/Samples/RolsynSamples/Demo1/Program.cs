using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CodingSeb.ExpressionEvaluator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace Demo1
{

    public class Program
    {
        ExpressionEvaluator _evaluator = new ExpressionEvaluator();

        public static async Task Main(string[] args)
        {
            Program p = new Program();

            //await TestAssemblyUnload();

            await p.TestCreateFileUnloadAssemblyAsync();

            //Console.WriteLine("Hello World!");
            //var ret1 = await Evalute1Plus1Async();
            //Console.WriteLine(ret1);

            //var ret2 = await Evalute1Plus1_ComplexAsync();
            //Console.WriteLine(ret2);

            //await RunWithExplictUsingAsync();

            //await RunWithAsyncAwaitAsync();

            //await RunWithAsyncAwaitHttpReqAsync();

            //await RunWithImportsAsync();

            //await RunExpandoObjectAsync();

            //await RunExtensionMethodsAsync();

            //await RunCustomMethodsFromOtherAssemblyAsync();

            //await RunCustomMethodsWithParamsAsync();

            //await ContinueRunCustomMethodsWithParamsAsync();

            //await CreateScriptThenRunCustomMethodsWithParamsAsync();

            //await p.CodeBlockContinueWithExpressionAsync();

            //await RunExpandoObjectWithGlobalsAsync();

            //await RunWithGlobalExpandoAsync();

            //p.BenchmarkUsingExpressionEvaluator();

            //BenchmarkRunner.Run<Program>();

            await Task.CompletedTask;

            Console.ReadLine();
        }

        static Script<object> script1;
        /// <summary>
        /// 演示直接通过 EvaluateAsync 来执行简单的场景, 这种方式，RunAsync 创建的 assembly 直接加载当当前 Context 无法释放，内存占用多
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public async Task TestAssemblyUnload()
        {
            // 截至目前 2022/6/30 roslyn 尚不支持 unload assembly context
            if (script1 == null)
            {
                var sc = ScriptOptions.Default.WithOptimizationLevel(OptimizationLevel.Release)
                .AddReferences(typeof(object).Assembly)
                        .AddReferences(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly)
                        .AddReferences(typeof(System.Linq.Enumerable).Assembly)
                        .AddReferences(typeof(System.Collections.Generic.List<>).Assembly)
                        .AddImports("System", "System.Collections.Generic", "System.Linq");

                script1 = CSharpScript.Create(
                    @"  
                        //List<byte[]> arr = new List<byte[]>();
                        //for(var i=0;i<100000;i++)
                        //{
                        //    arr.Add(new byte[4096]);
                        //}
            Console.WriteLine(System.AppDomain.CurrentDomain.GetAssemblies().Count());", sc);
                //System.Console.WriteLine($"Loaded assemblies: {System.AppDomain.CurrentDomain.GetAssemblies().Count()}");
            }

            //while(true)
            await script1.RunAsync();
            // 为了与 TestCreateFileUnloadAssemblyAsync 进行对比，所以都进行一次 Collect
            //GC.Collect();
            //await Task.Delay(200);
        }


        static byte[] assemblyBinaryContent;

        /// <summary>
        /// 测试通过先编译成 dll，然后再另外一个 可以unload 的 Context 中进行加载和运行，assembly 可以释放，内存占用小
        /// </summary>
        /// <returns></returns>
        [Benchmark]
        public async Task TestCreateFileUnloadAssemblyAsync()
        {
            // 现随便执行一个脚本，这样后续的执行就会快一个数量级，如果没有这个，那么后面 sw 执行后需要 3000 ms，如果加了这个，则约 300ms
            await CSharpScript.Create("1.ToString()").RunAsync();
            MethodInfo mem = null;
            Func<object[], Task<object>> stronglyTypedDelegate = null;

            if (assemblyBinaryContent == null)
            {
                string scripts = @"
//            List<byte[]> arr = new List<byte[]>();
//for(var i=0;i<100000;i++)
//{
//arr.Add(new byte[4096]);
//}
            Console.WriteLine(System.AppDomain.CurrentDomain.GetAssemblies().Count());        
            Console.WriteLine(@in.a);        
";

                var defaultMetadataReferences = ImmutableArray.Create(new MetadataReference[]
                {
                    MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(System.AppDomain).Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                });
                string[] defaultUsings = new[] { "System", "System.Linq", "System.Collections.Generic" };


                var so = ScriptOptions.Default
                    .AddImports(defaultUsings)
                    .AddReferences(
                        "System",
                        "System.Core",
                        "System.Data",
                        "System.Data.DataSetExtensions",
                        "System.Runtime",
                        "System.Xml",
                        "System.Xml.Linq",
                        "System.Net.Http",
                        "Microsoft.CSharp");

                var roslynScript = CSharpScript.Create(scripts, so, typeof(GlobalType));
                var compilation = roslynScript.GetCompilation();

                compilation = compilation.WithOptions(compilation.Options
                    .WithOptimizationLevel(OptimizationLevel.Release)
                    .WithOutputKind(OutputKind.DynamicallyLinkedLibrary));

                using (var assemblyStream = new MemoryStream())
                {
                    var result = compilation.Emit(assemblyStream);
                    if (!result.Success)
                    {
                        var errors = string.Join(Environment.NewLine, result.Diagnostics.Select(x => x));
                        throw new Exception("Compilation errors: " + Environment.NewLine + errors);
                    }

                    assemblyBinaryContent = assemblyStream.ToArray();

                    var entryPoint = compilation.GetEntryPoint(CancellationToken.None);
                }
            }

            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                sw.Restart();
                await Exec();
                sw.Stop();
                Console.WriteLine("Elapsed:" + sw.ElapsedMilliseconds);

                await Task.Delay(200);
            }


            async Task Exec()
            {
                CollectibleAssemblyLoadContext context = new CollectibleAssemblyLoadContext();
                using MemoryStream ms2 = new MemoryStream(assemblyBinaryContent);

                var ass = context.LoadFromStream(ms2);

                if (stronglyTypedDelegate == null)
                {
                    var typ = ass.GetType("Submission#0");
                    mem = typ.GetMethod("<Factory>", BindingFlags.Static | BindingFlags.Public);
                    stronglyTypedDelegate = (Func<object[], Task<object>>)Delegate.CreateDelegate(typeof(Func<object[], Task<object>>), null, mem);
                }

                //var rslt = await stronglyTypedDelegate(new object[2]);
                var ctx = new GlobalType();
                ctx.@in.a = "!23";

                var retTask = mem.Invoke(null, new object[] { new object[2] {ctx, null } }) as Task<object>;
                var rslt = await retTask;

                // 卸载是异步的
                context.Unload();

                // 回收会触发 context 卸载
                //GC.Collect();
            }
        }

        public class GlobalType
        {
            private NullableExpandoObject _in = new NullableExpandoObject();
            public dynamic @in
            {
                get
                {
                    return _in;
                }
            }
        }
        public class NullableExpandoObject : DynamicObject
        {
            private Dictionary<string, object> _dic = new Dictionary<string, object>();

            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                bool found = _dic.TryGetValue(binder.Name, out result);
                if (!found)
                {
                    result = null;
                }
                return true;

            }

            public override bool TrySetMember(SetMemberBinder binder, object value)
            {
                AddProperty(binder.Name, value);
                return true;
            }

            public void AddProperty(string name, object value)
            {
                _dic[name] = value;
            }

            public object this[string propertyName]
            {
                get
                {
                    return _dic[propertyName];
                }
                set
                {
                    AddProperty(propertyName, value);
                }
            }
        }

        static async Task<object> Evalute1Plus1Async()
        {
            return await CSharpScript.EvaluateAsync("1+1");
        }


        static async Task<object> Evalute1Plus1_ComplexAsync()
        {
            // var a = 1+1 由于是个代码块，所以必须使用分号
            // Evaluate 代码块 并不会返回，所以最后给一个 b 作为表达式来返回
            return await CSharpScript.EvaluateAsync("var a = 1+1;var b = a+1;b");
        }

        /// <summary>
        /// 需要显示使用 using
        /// </summary>
        /// <returns></returns>
        static async Task RunWithExplictUsingAsync()
        {
            string script = @"
using System; // Console 需要这个名称空间
var coll = new string[]{}; 
foreach(var a in coll) 
{
    Console.WriteLine(a);
}";
            var obj = await CSharpScript.RunAsync(script);

        }

        /// <summary>
        /// 通过 WithImports 来使用 using
        /// </summary>
        /// <returns></returns>
        static async Task RunWithImportsAsync()
        {
            string script = @"
var coll = new string[]{}; 
    Console.WriteLine(coll);

foreach(var a in coll) 
{
    Console.WriteLine(a);
}";
            ScriptOptions scriptOptions = ScriptOptions.Default;
            //Add namespaces
            scriptOptions = scriptOptions.WithImports("System"); // WithImports 表示只使用这个 imports
            await CSharpScript.RunAsync(script, scriptOptions);
        }

        /// <summary>
        /// 使用 async await
        /// </summary>
        /// <returns></returns>
        static async Task RunWithAsyncAwaitAsync()
        {

            string script = @"await Task.Delay(5000);";
            ScriptOptions scriptOptions = ScriptOptions.Default;
            //Add namespaces
            scriptOptions = scriptOptions.WithImports("System", "System.Threading.Tasks");
            await CSharpScript.RunAsync(script, scriptOptions);
        }

        /// <summary>
        /// 使用 async await 发起 http 请求
        /// </summary>
        /// <returns></returns>
        static async Task RunWithAsyncAwaitHttpReqAsync()
        {
            string script = @"System.Net.Http.HttpClient hc = new System.Net.Http.HttpClient();
var resp = await hc.GetAsync(""http://www.baidu.com"");
return resp.StatusCode;";
            ScriptOptions scriptOptions = ScriptOptions.Default;
            //Add namespaces
            scriptOptions = scriptOptions
                .AddReferences(typeof(System.Net.Http.HttpClient).Assembly)
                .WithImports("System", "System.Threading.Tasks", "System.Net.Http");
            var rslt = await CSharpScript.RunAsync(script, scriptOptions);
        }

        static async Task RunExpandoObjectAsync()
        {
            string scripts = @"
dynamic obj = new ExpandoObject();
obj.A = 1;
Console.WriteLine(obj.A);
";

            var expressions = typeof(System.Dynamic.ExpandoObject).Assembly; // 使用 ExpandObject 需要的 dll
            var csharp = typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly; // 使用 dynamic 需要的 dll
            ScriptOptions scriptOptions = ScriptOptions.Default
                .WithLanguageVersion(Microsoft.CodeAnalysis.CSharp.LanguageVersion.LatestMajor)
                .AddReferences(expressions, csharp)
                .AddImports("System", "System.Dynamic"); // 与 withImports 不同，这个是在已有的 imports 基础上增加其他 imports

            await CSharpScript.RunAsync(scripts, scriptOptions);
        }

        static async Task RunExpandoObjectWithGlobalsAsync()
        {
            var global = new GLOBALExtended
            {
                GlobalVarsA = "this is a var from global"
            };

            string scripts = @"
dynamic obj = new ExpandoObject();
obj.A = 1;
Console.WriteLine(GlobalVarsA);
GlobalM1();
GlobalM2(""aaa"");
";

            var expressions = typeof(System.Dynamic.ExpandoObject).Assembly; // 使用 ExpandObject 需要的 dll
            var csharp = typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly; // 使用 dynamic 需要的 dll
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddReferences(expressions, csharp)
                .AddImports("System", "System.Dynamic"); // 与 withImports 不同，这个是在已有的 imports 基础上增加其他 imports

            await CSharpScript.RunAsync(scripts, scriptOptions, global);
        }

        static async Task RunWithGlobalExpandoAsync()
        {
            dynamic data = new ExpandoObject();
            data.GlobalVarsA = "this is a global var.";

            // Roslyn 目前不支持直接把 expandoObject\匿名类型 作为 global, 所以通过一个固定类型中加一个属性来使用 Expnaod
            var global = new GLOBAL
            {
                _ = data
            };

            // 先生成一个 script 用于定义 global 中在变量, 这样第二个脚本就可以直接使用这些变量
            StringBuilder sb = new StringBuilder();
            foreach (var itm in data)
            {
                sb.Append("var " + itm.Key + "=_." + itm.Key + ";");
            }

            var expressions = typeof(System.Dynamic.ExpandoObject).Assembly; // 使用 ExpandObject 需要的 dll
            var csharp = typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly; // 使用 dynamic 需要的 dll
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddReferences(expressions, csharp)
                .AddImports("System", "System.Dynamic");

            var script = await CSharpScript.RunAsync(sb.ToString(), scriptOptions, global);

            // 这里可以直接使用 global 中的变量
            string scriptStr = "Console.WriteLine(GlobalVarsA);";
            await script.ContinueWithAsync(scriptStr, scriptOptions);
        }

        static async Task RunExtensionMethodsAsync()
        {
            string scripts = @"
dynamic obj = new ExpandoObject();
obj.A = 1;
((ExpandoObject)obj).MyConsole();
";
            var myExt = typeof(Exts).Assembly; // MyConsole 需要这个 dll
            var expressions = typeof(System.Dynamic.ExpandoObject).Assembly; // 使用 ExpandObject 需要的 dll
            var csharp = typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly; // 使用 dynamic 需要的 dll
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddReferences(expressions, csharp, myExt)
                .AddImports("System", "System.Dynamic", "Demo1"); // MyConsole 需要使用这个 namespace

            await CSharpScript.RunAsync(scripts, scriptOptions);
        }

        static async Task RunCustomMethodsFromOtherAssemblyAsync()
        {
            string scripts = @"
var str =  M1(""xxx"");
var str2 = M1(""yyy"",""zzz"");
";
            var myExt = typeof(Exts).Assembly;
            var myExt2 = typeof(Exts2).Assembly;
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddReferences(myExt2)
                .AddReferences(myExt)
                .AddImports("System", "Demo1.Exts", "Demo1.Exts2");

            var rslt = await CSharpScript.RunAsync(scripts, scriptOptions);

            dynamic @in = new { };

            System.Dynamic.ExpandoObject input1 = @in.input1; System.String _input = @in._input; System.Dynamic.ExpandoObject _inputObj = @in._inputObj; System.String _tenantId = @in._tenantId; System.String _hostTenantId = @in._hostTenantId; System.String _subTenantId = @in._subTenantId; var _userId = @in._userId; System.Boolean _isTesting = @in._isTesting; System.String _connectionId = @in._connectionId; System.Boolean _isSync = @in._isSync; var _impersonatorUserId = @in._impersonatorUserId; var _item = @in._item; System.Collections.Generic.List<System.Object> table1 = @in.table1; System.String input_ogrmqgd6 = @in.input_ogrmqgd6; System.Dynamic.ExpandoObject obj = @in.obj; var obj2 = @in.obj2; System.Collections.Generic.List<System.Object> lst = @in.lst; System.Collections.Generic.List<System.Object> lst2 = @in.lst2; System.Collections.Generic.List<System.Object> lstCopy = @in.lstCopy; System.Int32 obj3 = @in.obj3; System.Boolean existed = @in.existed; System.Collections.Generic.List<System.Object> lst3 = @in.lst3; System.Dynamic.ExpandoObject first = @in.first; System.String str1 = @in.str1; System.String str2 = @in.str2; System.String str3 = @in.str3;
        }

        static async Task RunCustomMethodsWithParamsAsync()
        {
            string scripts = @"
M(""xxx"");

void M(string val) {
    Console.WriteLine(val);
}
";
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddImports("System");

            await CSharpScript.RunAsync(scripts, scriptOptions);
        }

        static async Task ContinueRunCustomMethodsWithParamsAsync()
        {
            string scripts = @"
void M(string val) {
    Console.WriteLine(val);
}
";
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddImports("System");

            var rslt = await CSharpScript.RunAsync(scripts, scriptOptions);

            string script2 = @"M(""secondScript"")";
            await rslt.ContinueWithAsync(script2);
        }

        /// <summary>
        /// 分成2个 script
        /// </summary>
        /// <returns></returns>
        static async Task CreateScriptThenRunCustomMethodsWithParamsAsync()
        {
            string scripts = @"
void M(string val) {
    Console.WriteLine(val);
}
";
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddImports("System");

            var rslt = CSharpScript.Create(scripts, scriptOptions);

            string script2 = @"M(""secondScript"")";
            rslt = rslt.ContinueWith(script2);

            await rslt.RunAsync();
        }



        /// <summary>
        /// 分成2个 script
        /// </summary>
        /// <returns></returns>
        public async Task CodeBlockContinueWithExpressionAsync()
        {
            string scripts = @"
var m = 1; var m2=2; var m3 = m+m2;
";

            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddReferences(typeof(System.Linq.Enumerable).Assembly.Location)
                .AddImports("System")
                .AddImports("System.Linq")
                .AddImports("System.Threading.Tasks");


            var rslt = CSharpScript.Create(scripts, scriptOptions);

            // 添加一个表达式
            rslt = rslt.ContinueWith("m2+1");
            var grp = new[] { 1, 2, 3 }.GroupBy(itm => itm);
            // 添加一个Linq
            rslt = rslt.ContinueWith("var grp  = new []{1,2,3}.GroupBy(itm=>itm);");
            rslt = rslt.ContinueWith("var whereObj  = new []{1,2,3.Where(itm=>itm>2);");
            //var whereObj = new[] { 1, 2, 3 }.Where(itm => itm > 2);
            //var d =whereObj.GetType();
            //var syntaxTree = rslt.GetCompilation().SyntaxTrees.First();
            //var vars = syntaxTree.GetRoot().DescendantNodes().OfType<VariableDeclarationSyntax>();
            //var semanticModel  = rslt.GetCompilation().GetSemanticModel(syntaxTree);
            //foreach (var declared in vars)
            //{
            //    var identifier = declared.Variables.First().Identifier;
            //    var symbol = semanticModel.GetDeclaredSymbol(declared.Variables[0]);
            //    var name = semanticModel.GetTypeInfo(declared.Variables[0]);
            //}


            // 添加一个代码块
            rslt = rslt.ContinueWith("for(int i=0;i<m3;i++){Console.WriteLine(i);}");

            // 直接包含一个 await 语句
            rslt = rslt.ContinueWith("await Task.Delay(500);");
            var compileResult = rslt.Compile();
            var ret = await rslt.RunAsync();

            // 获取变量
            var variables = ret.Variables.ToDictionary(itm => itm.Name, itm => itm.Value);
            var d = "{\"Key\":\"whereObj\",\"Value\":[3]}";
            foreach (var a in variables)
            {
                string s = JsonConvert.SerializeObject(a);
            }
        }

        private Script<object> _scriptCache;

        [Benchmark]
        public async Task BenchmarkWithCacheAsync()
        {
            //  BenchmarkWithCacheAsync |     37.31 us |   1.888 us |     5.508 us
            string scripts = @"
var m = 1; var m2=2; var m3 = m+m2;
";
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddImports("System")
                .AddImports("System.Threading.Tasks");
            GLOBALExtended gLOBAL = new()
            {
                GlobalVarsA = "aa"
            };
            if (_scriptCache == null)
            {
                var rslt = CSharpScript.Create(scripts, scriptOptions, typeof(GLOBALExtended));

                // 添加一个表达式
                rslt = rslt.ContinueWith("m2+1");

                // 添加一个代码块
                rslt = rslt.ContinueWith(@"for(int i=0;i<m3;i++){Console.WriteLine(i);GlobalM2(i+"""");}");

                _scriptCache = rslt;
            }
            var ret = await _scriptCache.RunAsync(gLOBAL);
        }

        [Benchmark]
        public async Task BenchmarkWithoutCacheAsync()
        {
            // BenchmarkWithoutCacheAsync | 17,074.38 us | 419.799 us | 1,177.161 us
            string scripts = @"
var m = 1; var m2=2; var m3 = m+m2;
";
            ScriptOptions scriptOptions = ScriptOptions.Default
                .AddImports("System")
                .AddImports("System.Threading.Tasks");

            var rslt = CSharpScript.Create(scripts, scriptOptions);

            // 添加一个表达式
            rslt = rslt.ContinueWith("m2+1");

            // 添加一个代码块
            rslt = rslt.ContinueWith("for(int i=0;i<m3;i++){Console.WriteLine(i);}");

            var ret = await rslt.RunAsync();
        }


        [Benchmark]
        public void BenchmarkUsingExpressionEvaluator()
        {
            // BenchmarkUsingExpressionEvaluator |    670.72 us |   1.179 us |     1.103 us
            ExpressionEvaluator evaluator = new ExpressionEvaluator();
            evaluator.ScriptEvaluate(@"
var m = 1; var m2=2; var m3 = m+m2;
for(i=0;i<m3;i++){Console.WriteLine(i);}
                ");
        }
    }

    public static class Exts
    {
        public static void MyConsole(this object obj)
        {
            if (obj != null)
            {
                Console.WriteLine(obj);
            }
        }

        public static string M1(string input)
        {
            return "return " + input + " from M1";
        }

        public static string M1(string input1, string input2)
        {
            return "return " + input1 + " " + input2 + " from M1";
        }
    }

    public static class Exts2
    {
        public static void MyConsole(this object obj)
        {
            if (obj != null)
            {
                Console.WriteLine(obj);
            }
        }

        public static string M1(string input)
        {
            return "return " + input + " from Exts2.M1";
        }

        public static string M1(string input1, string input2)
        {
            return "return " + input1 + " " + input2 + " from Exts2.M1";
        }
    }


    public class GLOBAL
    {
        public string GlobalVarsA { get; set; }

        public dynamic _ { get; set; }

        public void GlobalM1()
        {
            Console.WriteLine("This is method from Global");
        }
    }

    public class GLOBALExtended : GLOBAL
    {
        public void GlobalM2(string param1)
        {
            Console.WriteLine("This is method from Global M2:" + param1);
        }
    }

    class CollectibleAssemblyLoadContext : AssemblyLoadContext, IDisposable
    {
        public CollectibleAssemblyLoadContext() : base(true)
        { }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            return null;
        }

        public void Dispose()
        {
            Unload();
        }
    }
}
