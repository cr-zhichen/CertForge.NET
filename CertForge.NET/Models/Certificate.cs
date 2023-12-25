using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CertForge.NET.Enum;

namespace CertForge.NET.Models;

/// <summary>
/// 表示数据库中的certificate表
/// </summary>
[Table("certificate")]
public class Certificate
{
    /// <summary>
    /// 证书ID
    /// </summary>
    [Key]
    [Column("certificate_id")]
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public int CertificateId { get; set; }

    /// <summary>
    /// 证书CN
    /// </summary>
    [Required]
    [Column("cn")]
    [MaxLength(128)]
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public string Cn { get; set; } = null!;

    /// <summary>
    /// 证书公钥
    /// </summary>
    [Required]
    [Column("certificate_pem")]
    [MaxLength(4096)]
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public string CertificatePem { get; set; } = null!;

    /// <summary>
    /// 证书私钥
    /// </summary>
    [Required]
    [Column("private_key")]
    [MaxLength(4096)]
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public string PrivateKey { get; set; } = null!;
}