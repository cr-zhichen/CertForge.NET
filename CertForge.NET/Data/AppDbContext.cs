using CertForge.NET.Models;
using Microsoft.EntityFrameworkCore;

namespace CertForge.NET.Data;

/// <summary>
/// 数据库上下文
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="options"></param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// CertificateRepositories 表
    /// </summary>
    public DbSet<Certificate> Certificate { get; set; }

    /// <summary>
    /// 数据库配置
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
    }

    /// <summary>
    /// 配置模型
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 配置 CertificateRepositories 表的映射
        modelBuilder
            .Entity<Certificate>(entity =>
            {
                entity.HasKey(e => e.CertificateId).HasName("PRIMARY");
                entity.Property(e => e.CertificateId).ValueGeneratedOnAdd();
                entity.HasIndex(u => u.Cn).IsUnique();
                entity.Property(e => e.Type).HasConversion<string>();
            });
    }
}