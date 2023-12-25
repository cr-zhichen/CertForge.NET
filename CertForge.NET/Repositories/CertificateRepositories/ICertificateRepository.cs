using CertForge.NET.Enum;
using CertForge.NET.Models;

namespace CertForge.NET.Repositories.CertificateRepositories;

/// <summary>
/// 和Certificate表相关的数据库操作
/// </summary>
public interface ICertificateRepository
{
    /// <summary>
    /// 判断根证书是否存在
    /// </summary>
    /// <returns></returns>
    Task<bool> IsRootCertificateExistAsync();

    /// <summary>
    /// 获取根证书
    /// </summary>
    /// <returns></returns>
    Task<Certificate> GetRootCertificateAsync();

    /// <summary>
    /// 创建证书
    /// </summary>
    /// <param name="cn"></param>
    /// <param name="certificatePem"></param>
    /// <param name="privateKey"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    Task<Certificate> CreateCertificateAsync(string cn, string certificatePem, string privateKey, CertificateType type);
}