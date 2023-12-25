using CertForge.NET.Data;
using CertForge.NET.Infrastructure;
using CertForge.NET.Models;
using Microsoft.EntityFrameworkCore;

namespace CertForge.NET.Repositories.CertificateRepositories;

/// <summary>
/// 和TestUser相关的数据库操作
/// </summary>
public class CertificateRepository : ICertificateRepository, IMarker
{
    private readonly AppDbContext _context;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="context"></param>
    public CertificateRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// 判断根证书是否存在
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsRootCertificateExistAsync()
    {
        // 如果数据库中无数据，则返回false
        return await _context.Certificate.AnyAsync();
    }

    /// <summary>
    /// 获取根证书
    /// </summary>
    /// <returns></returns>
    public async Task<Certificate> GetRootCertificateAsync()
    {
        // 返回第一条数据
        var rootCertificate = await _context.Certificate.FirstAsync();
        return rootCertificate;
    }

    /// <summary>
    /// 创建证书
    /// </summary>
    /// <param name="cn"></param>
    /// <param name="certificatePem"></param>
    /// <param name="privateKey"></param>
    /// <returns></returns>
    public async Task<Certificate> CreateCertificateAsync(string cn, string certificatePem, string privateKey)
    {
        var certificate = new Certificate()
        {
            Cn = cn,
            CertificatePem = certificatePem,
            PrivateKey = privateKey,
        };

        await _context.Certificate.AddAsync(certificate);
        await _context.SaveChangesAsync();

        return certificate;
    }
}