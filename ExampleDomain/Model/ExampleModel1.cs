using MySourceGenerator;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace ExampleDomain.Model
{
    [AutoRegister]
    public partial class ExampleModel1 : Entity
    {
        [Key]
        public int ExampleId { get; set; }
        public string ExampleName { get; set; } = string.Empty;
        public override object[] GetKeys()
        {
            return new object[] { ExampleId };
        }
    }
}