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
        /// ��ԭ�Ƚű��е��ֶ��滻�� json schema �е����ֶΣ�newName)
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

            // schema ���ڱ����ϵĶ�����µĶ���(newName)
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

            // prefix Ŀ�ģ�ͨ������ schema ���ɵ�����ֱ��Ƕ�뵽 script �У���ʹ�� using static __ ����ʽ�� script ����ֱ�ӷ��������е��ֶ�
            // ͬʱ�ں�����﷨���п���ֱ���ж��ĸ��ֶ��������� schema �ġ�
            string prefixCode = "using static __;" + code.Item1;

            // ת���﷨��
            var tree = CSharpSyntaxTree.ParseText(prefixCode + script);
            var compilation = CSharpCompilation.Create("HelloWorld")
                     .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                     .AddSyntaxTrees(tree);

            // ���³����µ��ֶ�
            string result = UpdateWithLatestFields(tree, compilation.GetSemanticModel(tree), code.Item2);


            #region �ֲ�����
            // ���� schema ����һ�� static �� C# ���ͣ��������ֶε� dic
            async Task<(string, Dictionary<string, string>)> GerenateCSharpClassByJsonSchemaAsync(string schema)
            {
                var mapping = new Dictionary<string, string>();
                var jschema = await JsonSchema.FromJsonAsync(schema);

                // ���þ�̬���ͣ����������ڴ�����ֱ�ӷ������е��ֶΣ����� using static __��
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
                        // �ֲ���������
                        case LocalDeclarationStatementSyntax localDeclaration:

                            // ���� Rolsyn ���﷨����ֻ���ģ�����Ҫ�޸ĵĻ����������¹���һ��
                            SeparatedSyntaxList<VariableDeclaratorSyntax> newList = new SeparatedSyntaxList<VariableDeclaratorSyntax>();
                            foreach (var variable in localDeclaration.Declaration.Variables)
                            {
                                var initializer = variable.Initializer.Value.ToString();
                                var typeInfo = semanticModel.GetTypeInfo(variable.Initializer.Value);
                                var symbol = semanticModel.GetSymbolInfo(variable.Initializer.Value);

                                // �������ֶ��������� schema ���ɵľ�̬���ͣ��� mapping ��Ϣ�а�������ֶΣ���ô˵������ֶ���Ҫ���滻
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

                        // foreach ���
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
