using CertForge.NET.DTOs;
using CertForge.NET.DTOs.HTTP.Requests;
using CertForge.NET.DTOs.HTTP.Responses;
using CertForge.NET.Infrastructure;
using CertForge.NET.Repositories.CertificateRepositories;
using CertForge.NET.Utils;

namespace CertForge.NET.Services.CertificateServer;

/// <summary>
/// 和测试相关的服务
/// </summary>
public class CertificateServices : ICertificateServices, IMarker
{
    /// <summary>
    /// 用户相关的数据库操作
    /// </summary>
    private readonly ICertificateRepository _certificateRepository;

    /// <summary>
    /// 配置文件
    /// </summary>
    private readonly IConfiguration _configuration;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="certificateRepository"></param>
    /// <param name="configuration"></param>
    public CertificateServices(ICertificateRepository certificateRepository, IConfiguration configuration)
    {
        _certificateRepository = certificateRepository;
        _configuration = configuration;
    }

    /// <summary>
    /// 获取根证书
    /// </summary>
    /// <returns></returns>
    public async Task<IRe<GetRootCertificateResponse>> GetRootCertificateAsync()
    {
        // 判断是否已经存在根证书
        var exists = await _certificateRepository.IsRootCertificateExistAsync();
        // 如果存在，则返回根证书
        if (exists)
        {
            var rootCertificate = await _certificateRepository.GetRootCertificateAsync();
            return new Ok<GetRootCertificateResponse>()
            {
                Message = "获取根证书成功",
                Data = new GetRootCertificateResponse()
                {
                    PublicKey = rootCertificate.CertificatePem,
                }
            };
        }
        else
        {
            //从配置文件中获取根证书的CN
            var c = _configuration["RootCertificate:C"] ?? "CN";
            var o = _configuration["RootCertificate:O"] ?? "CertForgeDotNET";
            var cn = _configuration["RootCertificate:CN"] ?? "CertForgeDotNET";
            // 不存在，则生成根证书
            var rootCertificate = CertificateUtilities.CreateRootCertificate(c, o, cn);
            // 将根证书保存到数据库
            await _certificateRepository.CreateCertificateAsync(cn, rootCertificate.certificate,
                rootCertificate.privateKey);
            // 返回根证书
            return new Ok<GetRootCertificateResponse>()
            {
                Message = "获取根证书成功",
                Data = new GetRootCertificateResponse()
                {
                    PublicKey = rootCertificate.certificate,
                }
            };
        }
    }

    /// <summary>
    /// 生成证书
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<IRe<GenerateCertificateResponse>> GenerateCertificateAsync(GenerateCertificateRequest request)
    {
        var c = request.C ?? (_configuration["RootCertificate:C"] ?? "CN");
        var o = request.O ?? (_configuration["RootCertificate:O"] ?? "CertForgeDotNET");
        var cn = request.Cn;
        var san = request.San ?? "";

        // 判断是否已经存在根证书
        var exists = await _certificateRepository.IsRootCertificateExistAsync();
        // 如果不存在，则创建根证书
        if (!exists)
        {
            //从配置文件中获取根证书的CN
            var rootC = _configuration["RootCertificate:C"] ?? "CN";
            var rootO = _configuration["RootCertificate:O"] ?? "CertForgeDotNET";
            var rootCn = _configuration["RootCertificate:CN"] ?? "CertForgeDotNET";

            // 不存在，则生成根证书
            var rootCertificate =
                CertificateUtilities.CreateRootCertificate(rootC, rootO, rootCn);
            // 将根证书保存到数据库
            await _certificateRepository.CreateCertificateAsync(rootCn, rootCertificate.certificate,
                rootCertificate.privateKey);
        }

        // 从数据库中获取根证书
        var rootCertificateFromDb = await _certificateRepository.GetRootCertificateAsync();

        // 创建签名证书
        var signedCertificate = CertificateUtilities.CreateSignedCertificate(c, o, cn, san,
            rootCertificateFromDb.CertificatePem, rootCertificateFromDb.PrivateKey, request.ValidityYear);
        // 返回签名证书
        return new Ok<GenerateCertificateResponse>()
        {
            Message = "生成证书成功",
            Data = new GenerateCertificateResponse()
            {
                PublicKey = signedCertificate.certificate,
                PrivateKey = signedCertificate.privateKey
            }
        };
    }
}