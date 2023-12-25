using CertForge.NET.Data;
using CertForge.NET.Enum;
using CertForge.NET.Infrastructure;
using CertForge.NET.Models;
using CertForge.NET.Utils;
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
        if (!await _context.Certificate.AnyAsync())
        {
            return false;
        }

        // 如果数据库中有数据，则判断第一条数据的类型是否为根证书
        var firstCertificate = await _context.Certificate.FirstAsync();
        if (firstCertificate.Type == CertificateType.Root)
        {
            return true;
        }

        // 如果数据库中有数据，但第一条数据的类型不是根证书，则全局搜索数据库中的根证书
        return await _context.Certificate.AnyAsync(certificate => certificate.Type == CertificateType.Root);
    }

    /// <summary>
    /// 获取根证书
    /// </summary>
    /// <returns></returns>
    public async Task<Certificate> GetRootCertificateAsync()
    {
        // 如果数据库中无数据，则抛出异常
        if (!await _context.Certificate.AnyAsync())
        {
            throw new Exception("数据库中无证书");
        }

        // 如果数据库中有数据，则判断第一条数据的类型是否为根证书
        var firstCertificate = await _context.Certificate.FirstAsync();
        if (firstCertificate.Type == CertificateType.Root)
        {
            return firstCertificate;
        }

        // 如果数据库中有数据，但第一条数据的类型不是根证书，则全局搜索数据库中的根证书
        var rootCertificate =
            await _context.Certificate.FirstOrDefaultAsync(certificate => certificate.Type == CertificateType.Root);
        if (rootCertificate == null)
        {
            throw new Exception("数据库中无根证书");
        }
        else
        {
            return rootCertificate;
        }
    }

    /// <summary>
    /// 创建证书
    /// </summary>
    /// <param name="cn"></param>
    /// <param name="certificatePem"></param>
    /// <param name="privateKey"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<Certificate> CreateCertificateAsync(string cn, string certificatePem, string privateKey,
        CertificateType type)
    {
        var certificate = new Certificate()
        {
            Cn = cn,
            CertificatePem = certificatePem,
            PrivateKey = privateKey,
            Type = type
        };

        await _context.Certificate.AddAsync(certificate);
        await _context.SaveChangesAsync();

        return certificate;
    }
}