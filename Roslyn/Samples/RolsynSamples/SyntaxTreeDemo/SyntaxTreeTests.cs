using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NJsonSchema;
using Xunit;
namespace SyntaxTreeDemo
{
    public class SyntaxTreeTests
    {
        /// <summary>
        /// 把原先脚本中的字段替换成 json schema 中的新字段（newName)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Demo_Input_ScriptAndJsonSchema_Output_LatestScript()
        {
            string script = @"
                  var f1 =  __somefield;
                  var result = """";
                  foreach(var itm in  __somecoll)
                  {
                      result +=  itm;
                  }
                  return result";

            // schema 用于保存老的定义和新的定义(newName)
            string schema = @"{
  ""$schema"": ""http://json-schema.org/draft-04/schema#"",
  ""type"": ""object"",
  ""properties"": {
    ""__somefield"": {
      ""type"": ""string"",
      ""newName"": ""xxx""
    },
    ""__somecoll"": {
      ""type"": ""array"",
      ""newName"": ""xxx2"",
      ""items"": [
        {
          ""type"": ""string""
        }
      ]
    }
 }
}";

            var code = await GerenateCSharpClassByJsonSchemaAsync(schema);

            // prefix 目的：通过“把 schema 生成的类型直接嵌入到 script 中，并使用 using static __ ”方式让 script 可以直接访问类型中的字段
            // 同时在后面的语法树中可以直接判断哪个字段是来自于 schema 的。
            string prefixCode = "using static __;" + code.Item1;

            // 转成语法树
            var tree = CSharpSyntaxTree.ParseText(prefixCode + script);
            var compilation = CSharpCompilation.Create("HelloWorld")
                     .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                     .AddSyntaxTrees(tree);

            // 更新成最新的字段
            string result = UpdateWithLatestFields(tree, compilation.GetSemanticModel(tree), code.Item2);


            #region 局部方法
            // 基于 schema 生成一个 static 的 C# 类型，及新老字段的 dic
            async Task<(string, Dictionary<string, string>)> GerenateCSharpClassByJsonSchemaAsync(string schema)
            {
                var mapping = new Dictionary<string, string>();
                var jschema = await JsonSchema.FromJsonAsync(schema);

                // 采用静态类型，这样才能在代码中直接访问其中的字段（借用 using static __）
                StringBuilder sb = new StringBuilder("public static class __{" + Environment.NewLine);

                foreach (var prop in jschema.ActualProperties)
                {
                    sb.AppendLine($"public static {GetType(prop.Value)} {prop.Key} {{get;set;}}");
                }

                sb.AppendLine("}");

                return (sb.ToString(), mapping);


                string GetType(JsonSchemaProperty property)
                {
                    if (property.ExtensionData?.ContainsKey("newName") == true)
                    {
                        mapping.Add(property.Name, property.ExtensionData["newName"].ToString());
                    }

                    switch (property.Type.ToString())
                    {
                        case "String":
                            return "string";
                        case "Array":
                            return "List<" + property.Items.First().Type.ToString().ToLower() + ">";

                        default:
                            return property.Type.ToString();
                    }
                }
            }
            string UpdateWithLatestFields(SyntaxTree tree, SemanticModel semanticModel, Dictionary<string, string> mapping)
            {
                CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
                var newUnit = SyntaxFactory.CompilationUnit();

                foreach (var mem in root.Members.OfType<GlobalStatementSyntax>())
                {
                    switch (mem.Statement)
                    {
                        // 局部变量定义
                        case LocalDeclarationStatementSyntax localDeclaration:

                            // 由于 Rolsyn 的语法树是只读的，所以要修改的话，必须重新构建一个
                            SeparatedSyntaxList<VariableDeclaratorSyntax> newList = new SeparatedSyntaxList<VariableDeclaratorSyntax>();
                            foreach (var variable in localDeclaration.Declaration.Variables)
                            {
                                var initializer = variable.Initializer.Value.ToString();
                                var typeInfo = semanticModel.GetTypeInfo(variable.Initializer.Value);
                                var symbol = semanticModel.GetSymbolInfo(variable.Initializer.Value);

                                // 如果这个字段是来自于 schema 生成的静态类型，且 mapping 信息中包含这个字段，那么说明这个字段需要被替换
                                if (symbol.Symbol?.ContainingType.Name == "__" && mapping.ContainsKey(initializer))
                                {
                                    var d = variable.Initializer.Value.ReplaceNode(variable.Initializer.Value, SyntaxFactory.IdentifierName(mapping[initializer]));
                                    var intilizer = variable.Initializer.WithValue(d);
                                    var newVar = variable.WithInitializer(intilizer);
                                    newList = newList.Add(newVar);
                                }
                            }

                            var delcare = newList.Count > 0 ? localDeclaration.Declaration.WithVariables(newList) : localDeclaration.Declaration;
                            var local = localDeclaration.WithDeclaration(delcare);

                            newUnit = newUnit.AddMembers(mem.WithStatement(local));

                            break;

                        // foreach 语句
                        case ForEachStatementSyntax foreachStatement:
                            var foreachMem = mem?.Statement as ForEachStatementSyntax;
                            if (foreachMem.Expression is IdentifierNameSyntax identifier && mapping.ContainsKey(identifier.Identifier.ValueText))
                            {
                                foreachStatement = foreachMem.WithExpression(SyntaxFactory.IdentifierName(mapping[identifier.Identifier.ValueText]));
                            }
                            newUnit = newUnit.AddMembers(mem.WithStatement(foreachStatement));
                            break;
                        default:
                            newUnit = newUnit.AddMembers(mem);

                            break;
                    }
                }

                var newCode = newUnit.ToFullString();
                return newCode;
            }
            #endregion
        }

    }


}
