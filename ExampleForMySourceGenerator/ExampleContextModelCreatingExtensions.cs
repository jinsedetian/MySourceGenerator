using Microsoft.EntityFrameworkCore;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore.Modeling;

namespace ExampleForMySourceGenerator;

public static partial class ExampleContextModelCreatingExtensions
{
    public static void Configure(
        this ModelBuilder modelBuilder,
        Action<AbpModelBuilderConfigurationOptions> optionsAction = null)
    {
        Check.NotNull(modelBuilder, nameof(modelBuilder));
        ModelAutoCreating.AutoBuild(modelBuilder);
    }
}
