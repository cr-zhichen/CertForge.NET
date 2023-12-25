namespace CertForge.NET.DTOs.HTTP.Responses;

/// <summary>
/// 获取根证书 返回响应体
/// </summary>
public class GetRootCertificateResponse
{
    /// <summary>
    /// 公钥
    /// </summary>
    public string PublicKey { get; set; } = null!;
}

/// <summary>
/// 生成证书 返回响应体
/// </summary>
public class GenerateCertificateResponse
{
    /// <summary>
    /// 私钥
    /// </summary>
    public string PrivateKey { get; set; } = null!;

    /// <summary>
    /// 公钥
    /// </summary>
    public string PublicKey { get; set; } = null!;
}