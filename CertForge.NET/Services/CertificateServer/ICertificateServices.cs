using CertForge.NET.DTOs;
using CertForge.NET.DTOs.HTTP.Requests;
using CertForge.NET.DTOs.HTTP.Responses;

namespace CertForge.NET.Services.CertificateServer;

/// <summary>
/// 和测试相关的服务接口
/// </summary>
public interface ICertificateServices
{
    /// <summary>
    /// 获取根证书
    /// </summary>
    /// <returns></returns>
    Task<IRe<GetRootCertificateResponse>> GetRootCertificateAsync();

    /// <summary>
    /// 生成证书
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    Task<IRe<GenerateCertificateResponse>> GenerateCertificateAsync(GenerateCertificateRequest request);
}