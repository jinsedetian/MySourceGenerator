using MySourceGenerator;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace ExampleDomain.Model
{
    [AutoRegister]
    public partial class ExampleModel2 : Entity<int>
    {
        public string Name { get; set; }
        public ExampleModel1 ExampleModel1 { get; set; }
    }
}