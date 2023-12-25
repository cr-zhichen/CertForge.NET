namespace CertForge.NET.DTOs.HTTP.Requests;

/// <summary>
/// 生成证书 请求体
/// </summary>
public class GenerateCertificateRequest
{
    /// <summary>
    /// 国家代码
    /// </summary>
    public string? C { get; set; } = null;

    /// <summary>
    /// 组织名称
    /// </summary>
    public string? O { get; set; } = null;

    /// <summary>
    /// CN
    /// </summary>
    public string Cn { get; set; } = null!;
    
    /// <summary>
    /// SAN
    /// </summary>
    public string? San { get; set; } = null;

    /// <summary>
    /// 证书有效期(年)
    /// </summary>
    public int ValidityYear { get; set; } = 100;
}