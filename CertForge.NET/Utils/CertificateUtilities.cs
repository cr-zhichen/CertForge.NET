using System.Net;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace CertForge.NET.Utils;

/// <summary>
/// 证书生成器
/// </summary>
public static class CertificateUtilities
{
    /// <summary>
    /// 创建根证书
    /// </summary>
    /// <param name="organization"></param>
    /// <param name="commonName"></param>
    /// <param name="country"></param>
    /// <param name="year"></param>
    /// <returns></returns>
    public static (string certificate, string privateKey) CreateRootCertificate(string country, string organization,
        string commonName, int year = 100)
    {
        var random = new SecureRandom();
        var certificateGenerator = new X509V3CertificateGenerator();

        var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
        certificateGenerator.SetSerialNumber(serialNumber);

        var dirName = $"C={country}, O={organization}, CN={commonName}";
        var issuerDn = new X509Name(dirName);
        var subjectDn = issuerDn;
        certificateGenerator.SetIssuerDN(issuerDn);
        certificateGenerator.SetSubjectDN(subjectDn);

        certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
        certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(year));
        
        // 添加基本限制扩展 - 表明这是一个CA证书
        certificateGenerator.AddExtension(X509Extensions.BasicConstraints.Id, true, new BasicConstraints(true));

        // 可以选择性地添加密钥用途扩展
        KeyUsage keyUsage = new KeyUsage(KeyUsage.KeyCertSign | KeyUsage.CrlSign);
        certificateGenerator.AddExtension(X509Extensions.KeyUsage.Id, false, keyUsage);

        var keyGenerationParameters = new KeyGenerationParameters(random, 2048);
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);

        var keyPair = keyPairGenerator.GenerateKeyPair();
        certificateGenerator.SetPublicKey(keyPair.Public);

        var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", keyPair.Private);
        var certificate = certificateGenerator.Generate(signatureFactory);
        byte[] certStream = DotNetUtilities.ToX509Certificate(certificate).Export(X509ContentType.Cert);

        var writer = new StringWriter();
        var pemWriter = new PemWriter(writer);
        pemWriter.WriteObject(keyPair.Private);
        var privateKey = writer.ToString();

        writer = new StringWriter();
        pemWriter = new PemWriter(writer);
        pemWriter.WriteObject(certificate);
        var certPem = writer.ToString();

        return (certPem, privateKey);
    }


    /// <summary>
    /// 创建签名证书
    /// </summary>
    /// <param name="organization"></param>
    /// <param name="commonName"></param>
    /// <param name="subjectAlternativeName"></param>
    /// <param name="issuerCertificatePem"></param>
    /// <param name="issuerPrivateKeyPem"></param>
    /// <param name="country"></param>
    /// <param name="year"></param>
    /// <returns></returns>
    public static (string certificate, string privateKey) CreateSignedCertificate(string country, string organization,
        string commonName, string subjectAlternativeName, string issuerCertificatePem, string issuerPrivateKeyPem,
        int year = 100)
    {
        var reader = new StringReader(issuerCertificatePem);
        var pemReader = new PemReader(reader);
        var issuerCertificate = (X509Certificate)pemReader.ReadObject();

        reader = new StringReader(issuerPrivateKeyPem);
        pemReader = new PemReader(reader);
        var issuerPrivateKey = (AsymmetricCipherKeyPair)pemReader.ReadObject();

        var random = new SecureRandom();
        var certificateGenerator = new X509V3CertificateGenerator();

        var serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
        certificateGenerator.SetSerialNumber(serialNumber);

        var issuerDn = new X509Name(issuerCertificate.SubjectDN.ToString());
        var subjectDn = new X509Name($"C={country}, O={organization}, CN={commonName}");
        certificateGenerator.SetIssuerDN(issuerDn);
        certificateGenerator.SetSubjectDN(subjectDn);

        certificateGenerator.SetNotBefore(DateTime.UtcNow.Date);
        certificateGenerator.SetNotAfter(DateTime.UtcNow.Date.AddYears(year));

        var keyGenerationParameters = new KeyGenerationParameters(random, 2048);
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);

        var keyPair = keyPairGenerator.GenerateKeyPair();
        certificateGenerator.SetPublicKey(keyPair.Public);

        // 添加 SAN
        var sanValues = subjectAlternativeName.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        var sanVector = new Asn1EncodableVector();
        foreach (var name in sanValues)
        {
            sanVector.Add(IPAddress.TryParse(name.Trim(), out _)
                ? new GeneralName(GeneralName.IPAddress, name.Trim())
                : new GeneralName(GeneralName.DnsName, name.Trim()));
        }

        certificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName.Id, false, new DerSequence(sanVector));

        var signatureFactory = new Asn1SignatureFactory("SHA256WITHRSA", issuerPrivateKey.Private);
        var certificate = certificateGenerator.Generate(signatureFactory);
        byte[] certStream = DotNetUtilities.ToX509Certificate(certificate).Export(X509ContentType.Cert);

        var writer = new StringWriter();
        var pemWriter = new PemWriter(writer);
        pemWriter.WriteObject(keyPair.Private);
        var privateKey = writer.ToString();

        writer = new StringWriter();
        pemWriter = new PemWriter(writer);
        pemWriter.WriteObject(certificate);
        var certPem = writer.ToString();

        return (certPem, privateKey);
    }
}