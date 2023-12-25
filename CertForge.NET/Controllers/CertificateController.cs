using CertForge.NET.DTOs;
using CertForge.NET.DTOs.HTTP.Requests;
using CertForge.NET.DTOs.HTTP.Responses;
using CertForge.NET.Services.CertificateServer;
using Microsoft.AspNetCore.Mvc;

namespace CertForge.NET.Controllers;

/// <summary>
/// 证书相关的控制器
/// </summary>
[ApiController]
[Route("api/certificate")]
public class CertificateController : ControllerBase
{
    /// <summary>
    /// 证书相关的服务
    /// </summary>
    private readonly ICertificateServices _certificateServices;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="certificateServices"></param>
    public CertificateController(ICertificateServices certificateServices)
    {
        _certificateServices = certificateServices;
    }

    /// <summary>
    /// 获取根证书
    /// </summary>
    /// <returns></returns>
    [HttpGet("root")]
    public async Task<IRe<GetRootCertificateResponse>> GetRootCertificateAsync()
    {
        return await _certificateServices.GetRootCertificateAsync();
    }

    /// <summary>
    /// 生成证书
    /// </summary>
    /// <returns></returns>
    [HttpPost("generate")]
    public async Task<IRe<GenerateCertificateResponse>> GenerateCertificateAsync(GenerateCertificateRequest request)
    {
        return await _certificateServices.GenerateCertificateAsync(request);
    }
}