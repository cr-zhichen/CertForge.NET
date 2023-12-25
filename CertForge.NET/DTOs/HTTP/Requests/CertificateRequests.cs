namespace CertForge.NET.DTOs.HTTP.Requests;

/// <summary>
/// 生成证书 请求体
/// </summary>
public class GenerateCertificateRequest
{
    /// <summary>
    /// 国家代码
    /// </summary>
    public string? C { get; set; }

    /// <summary>
    /// 组织名称
    /// </summary>
    public string? O { get; set; }

    /// <summary>
    /// CN
    /// </summary>
    public string Cn { get; set; } = null!;
}