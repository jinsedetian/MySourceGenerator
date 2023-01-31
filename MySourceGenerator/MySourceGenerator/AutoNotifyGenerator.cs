using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Volo.Abp.Domain.Entities;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace MySourceGenerator
{
    [Generator]
    public class AutoNotifyGenerator : ISourceGenerator
    {
        private string Template = "";
        private string DomainName = "你的命名空间";
        public void Initialize(GeneratorInitializationContext context)
        {
            //if (Debugger.IsAttached)
            //{
            //Debugger.Launch();
            //}
            //context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Template = @"///此文件由Source Generator生成
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EntityFrameworkCore.Modeling;
using System;
using System.Linq;
using {2}.Domain.Model.{0};

namespace {2}.EntityFrameworkCore
{{
    public static partial class {0}AutoCreating
    {{
        public static void AutoBuild(ModelBuilder modelBuilder)
        {{
            {1}
        }}
    }}
}}";

            var compilation = context.Compilation;
            var dataList = new List<INamedTypeSymbol>();
            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                //GetSemanticModel() and DescendantNodesAndSelf() are less-performant,
                //so check the source code first to improve the performance of SourceGenerator
                if (syntaxTree.TryGetText(out var sourceText) &&
                    !sourceText.ToString().Contains("[AutoRegister]"))
                {
                    continue;
                }
                var semanticModel = compilation.GetSemanticModel(syntaxTree);
                if (semanticModel == null)
                {
                    continue;
                }
                //[HasStronglyTypedId] can be applied to class, record, interface or struct.
                //TypeDeclarationSyntax is the base class of ClassDeclarationSyntax, InterfaceDeclarationSyntax, RecordDeclarationSyntax,and StructDeclarationSyntax.
                var classDefs = syntaxTree.GetRoot().DescendantNodesAndSelf().OfType<TypeDeclarationSyntax>();
                foreach (var item in classDefs)
                {
                    dataList.Add(semanticModel.GetDeclaredSymbol(item));
                }
            }

            var groupClassDefs = dataList.GroupBy(w => w.ContainingNamespace.Name);
            foreach (var groupClassDef in groupClassDefs)
            {
                var code = "";
                foreach (var classDef in groupClassDef)
                {
                    code = $"{code}{ProcessClass(context, classDef)}";
                }
                context.AddSource(groupClassDef.Key + "AutoCreating", string.Format(Template, groupClassDef.Key, code, DomainName));
            }
        }

        private string ProcessClass(GeneratorExecutionContext context, INamedTypeSymbol namedTypeSymbol/* SemanticModel semanticModel, TypeDeclarationSyntax classDef*/)
        {
            //var namedTypeSymbol = semanticModel.GetDeclaredSymbol(classDef);
            var code = "";
            var hasAutoRegisterAttribute = namedTypeSymbol.GetAttributes()
                .SingleOrDefault(t => t.AttributeClass.Name == nameof(AutoRegisterAttribute));
            if (hasAutoRegisterAttribute == null) return "";
            string className = namedTypeSymbol.Name;
            if (HasEFCoreReference(context))
            {
                code = $@"{code}
            modelBuilder.Entity<{className}>(entity =>
            {{
                entity.ToTable(""{ToLowerDownLine(className)}"");";
                var properties = namedTypeSymbol.GetMembers().OfType<IPropertySymbol>();
                foreach (var property in properties.Where(w => w.IsVirtual == false))
                {
                    code = $@"{code}
                entity.Property(e => e.{property.Name})
                {HasColumnType(TypeChange(property))}
                .HasColumnName(""{(namedTypeSymbol.ContainingNamespace.Name == DomainName ? ToLowerDownLine(property.Name) : ToUpperCamelCase(property.Name))}"");//{string.Join(",", property.GetAttributes().Select(w => w.AttributeClass.Name))}
                ";
                }
                code = $@"{code}
                //{string.Join(",", properties.Where(w => w.IsVirtual == true && w.Type.Name != "List").Select(w => w.Name))}";
                var tempKeyName = properties.FirstOrDefault(w => w.GetAttributes().Any(u => u.AttributeClass.Name == nameof(KeyAttribute)));
                var keyName = tempKeyName == null ? "" : tempKeyName.Name;
                foreach (var property in properties.Where(w => w.IsVirtual == true && w.Type.Name != "List"))//一对？
                {
                    var child = property.Type.GetMembers().OfType<IPropertySymbol>()/*.Where(w => w.IsVirtual == true)*/;//获取子类的虚拟类成员的Type
                    var tempChildKeyName = child.FirstOrDefault(w => w.GetAttributes().Any(u => u.AttributeClass.Name == nameof(KeyAttribute)));
                    var childKeyName = tempChildKeyName == null ? "" : tempChildKeyName.Name;
                    if (child.FirstOrDefault(w => w.Type.Name == className) != null)//一对一
                    {
                        code = $@"{code}
                entity.HasOne(e => e.{property.Name}).WithOne(e => e.{child.FirstOrDefault(w => w.Type.Name == className).Name}).HasForeignKey<{className}>(e => e.{keyName}).HasPrincipalKey<{property.Name}>(e => e.{childKeyName}); ";
                    }
                    else//一对多
                    {
                        var a1 = child.Where(w => w.Type.Name == nameof(List<object>));
                        var a2 = a1.Select(w => new
                        {
                            InListName = (w.Type.ToDisplayParts()[w.Type.ToDisplayParts().Count() - 2]).ToString(),
                            Name = w.Name,
                            //IsKey = w.GetAttributes().Any(u => u.AttributeClass.Name == nameof(KeyAttribute))
                        }).FirstOrDefault(w => w.InListName == className);
                        if (a2 != null)
                            code = $@"{code}
                entity.HasOne(e => e.{property.Name}).WithMany(e => e.{a2.Name}).HasForeignKey(e => e.{keyName}).HasPrincipalKey(e => e.{childKeyName});";
                        else
                            code = $@"{code}
                //数组类型不符合预期;{string.Join(",", a1.Select(w => w.Type.ToDisplayParts()[w.Type.ToDisplayParts().Count() - 2].ToString()))}";

                    }
                }
                foreach (var property in properties.Where(w => w.IsVirtual == true && w.Type.Name == "List"))//多对一
                {
                    var child = property.Type.GetMembers().OfType<IPropertySymbol>()/*.Where(w => w.IsVirtual == true)*/;//获取子类的虚拟类成员的Type
                    var childKeyName = child.FirstOrDefault(w => w.GetAttributes().Any(u => u.AttributeClass.Name == nameof(KeyAttribute))) == null ? "" : child.FirstOrDefault(w => w.GetAttributes().Any(u => u.AttributeClass.Name == nameof(KeyAttribute))).Name;
                    if (child.FirstOrDefault(w => w.Type.Name == className) != null)//？对一
                    {
                        code = $@"{code}
                entity.HasOne(e => e.{property.Name}).WithOne(e => e.{child.FirstOrDefault(w => w.Type.Name == className).Name}).HasForeignKey<{className}>(e => e.TaskId).HasPrincipalKey(e => e.Id); ";
                    }
                }

                code = $@"{code}
            }});
";
                return code;
            }
            else
            {
                var projectName = context.Compilation.AssemblyName;
                context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("ZK001", "EF Core", $"Assembly Microsoft.EntityFrameworkCore is not added into the project '{projectName}', so ValueConverter types will not be automatically generated. Please add reference Microsoft.EntityFrameworkCore to '{projectName}' or write the ValueConverter types manually.", "", DiagnosticSeverity.Warning, true), null));
                return "";
            }
        }

        // 语法接收器，将在每次生成代码时被按需创建
        public class MySyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // any method with at least one attribute is a candidate for property generation
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax
                    && classDeclarationSyntax.AttributeLists.Count >= 0)
                {
                    CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
        private static bool HasEFCoreReference(GeneratorExecutionContext context)
        {
            return context.Compilation.ReferencedAssemblyNames.Any(r => r.Name == "Microsoft.EntityFrameworkCore" /*&& r.Name == "System.ComponentModel.DataAnnotations"*/);
        }

        public static string HasColumnType(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            else
                return $@".HasColumnType(""{input}"")";
        }

        /// <summary>
        /// 类型转换
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TypeChange(IPropertySymbol input)
        {
            var typeName = input.Type.Name;// input.MetadataName.ToLower();
            switch (typeName)
            {
                case "String":
                    {
                        var attr = input.GetAttributes()
                .SingleOrDefault(t => t.AttributeClass.Name == nameof(MaxLengthAttribute));
                        if (attr == null)
                            return "ntext";
                        else
                        {
                            var args = attr.ConstructorArguments;
                            return $"varchar({Convert.ToInt32(args.Single().Value)})";
                        }
                    }
                case "Int64":
                    return "bigint";
                case "Decimal":
                    return "float";
                case "Int32":
                    return "int";
                case "Boolean":
                    return "bit";
                case "DateTime":
                    return "datetime";
                default:
                    return typeName;
            }
        }

        /// <summary>
        /// 转换为小写下划线
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToLowerDownLine(string input)
        {
            var result = Regex.Replace(input, @"(?x)( [A-Z][a-z,0-9]+ | [A-Z]+(?![a-z]))", "_$0").ToUpper();
            if (result.StartsWith("_"))
                result = result.Substring(1, result.Length - 1);
            return result.ToLower();
        }

        /// <summary>
        /// 转换为大写下划线
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToUpperDownLine(string input)
        {
            var result = Regex.Replace(input, @"(?x)( [A-Z][a-z,0-9]+ | [A-Z]+(?![a-z]))", "_$0").ToUpper();
            if (result.StartsWith("_"))
                result = result.Substring(1, result.Length - 1);
            return result.ToUpper();
        }

        /// <summary>
        /// 转换为大驼峰
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToUpperCamelCase(string input)
        {
            var result = input.Split('_');
            for (var i = 0; i < result.Length; i++)
            {
                if (result[i].Length <= 1)
                {
                    result[i] = result[i].ToUpper();
                }
                else
                    result[i] = $"{result[i].Substring(0, 1).ToUpper()}{result[i].Substring(1, result[i].Length - 1).ToLower()}";
            }
            return string.Join(null, result);
        }

        /// <summary>
        /// 转换为小驼峰
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToLowerCamelCase(string input)
        {
            var result = input.Split('_');
            for (var i = 0; i < result.Length; i++)
            {
                if (i == 0)
                    result[i] = result[i].ToLower();
                if (result[i].Length <= 1)
                {
                    result[i] = result[i].ToUpper();
                }
                else
                    result[i] = $"{result[i].Substring(0, 1).ToUpper()}{result[i].Substring(1, result[i].Length - 1).ToLower()}";
            }
            return string.Join(null, result);
        }
    }
}