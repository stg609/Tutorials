using System;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using CodingSeb.ExpressionEvaluator;
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

            //Console.WriteLine("Hello World!");
            //var ret1 = await Evalute1Plus1Async();
            //Console.WriteLine(ret1);

            //var ret2 = await Evalute1Plus1_ComplexAsync();
            //Console.WriteLine(ret2);

            //await RunWithExplictUsingAsync();

            //await RunWithAsyncAwaitAsync();

            await RunWithAsyncAwaitHttpReqAsync();

            //await RunWithImportsAsync();

            //await RunExpandoObjectAsync();

            //await RunExtensionMethodsAsync();

            //await RunCustomMethodsWithParamsAsync();

            //await ContinueRunCustomMethodsWithParamsAsync();

            //await CreateScriptThenRunCustomMethodsWithParamsAsync();

            await p.CodeBlockContinueWithExpressionAsync();

            //await RunExpandoObjectWithGlobalsAsync();

            //p.BenchmarkUsingExpressionEvaluator();

            //BenchmarkRunner.Run<Program>();

            await Task.CompletedTask;
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
    }

    public class GLOBAL
    {
        public string GlobalVarsA { get; set; }

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
}
