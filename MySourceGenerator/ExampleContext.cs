using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Debug;
using System.Security.Claims;

namespace ExampleForMySourceGenerator
{
    public class ExampleContext : DbContext
    {
        /// <summary>
        /// 日志
        /// </summary>
        [Obsolete]
        public static readonly LoggerFactory _loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options">选项</param>
        public ExampleContext(DbContextOptions<ExampleContext> options)
            : base(options)
        {

        }
        /// <summary>
        /// 配置时
        /// </summary>
        /// <param name="optionsBuilder">选项</param>
        [Obsolete]
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(_loggerFactory);
        }
        /// <summary>
        /// 建立模型时
        /// </summary>
        /// <param name="modelBuilder">模型建立器</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
