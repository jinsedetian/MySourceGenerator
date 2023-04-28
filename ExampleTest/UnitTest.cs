using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace ExampleTest
{
    public class UnitTest
    {
        [Fact]
        public async Task HelloGeneratorTest()
        {
            string code = File.ReadAllText("D:\\git\\MySourceGenerator\\ExampleDomain\\Model\\ExampleModel1.cs");
            var generatedCode = "ab";
            var tester = new CSharpSourceGeneratorTest<MySourceGenerator.AutoNotifyGenerator, XUnitVerifier>()
            {
                TestState =
                {
                    Sources = { code },
                    GeneratedSources =
                    {
                        (typeof(MySourceGenerator.AutoNotifyGenerator), $"{nameof(MySourceGenerator.AutoNotifyGenerator)}.cs", SourceText.From(generatedCode, Encoding.UTF8)),
                    }
                },
            };


            //tester.ReferenceAssemblies = tester.ReferenceAssemblies
            //.WithAssemblies(ImmutableArray.Create(new[] { typeof(DependencyResolver).Assembly.Location.Replace(".dll", "", System.StringComparison.OrdinalIgnoreCase) }));

            //tester.TestState.AdditionalReferences.Add(typeof(HosAppDbContext).Assembly);
            //tester.ReferenceAssemblies = tester.ReferenceAssemblies
            //.AddPackages(ImmutableArray.Create(new PackageIdentity[] { new PackageIdentity("Microsoft.EntityFrameworkCore", "6.0.6") }));

            await tester.RunAsync();
            Console.WriteLine("abc");
        }
    }
}