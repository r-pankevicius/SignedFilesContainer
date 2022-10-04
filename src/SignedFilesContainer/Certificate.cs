using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SignedFilesContainer
{
    /// <summary>
    /// A wrapper for X509 certificate and its RSA public key providing simple API
    /// not requiring deep cryptography knownledge.
    /// </summary>
    public class Certificate
    {
        private readonly X509Certificate2? _certificate;
        private readonly string? _password;
        private byte[]? _RSAPublicKey;

        public Certificate(X509Certificate2 certificate, string? password)
        {
            _certificate = certificate ?? throw new ArgumentNullException(nameof(certificate));
            _password = password;
        }

        private Certificate(byte[]? pfxCertificateBytes, byte[]? publicKeyBytes, string? password)
        {
            if (pfxCertificateBytes is null && publicKeyBytes is null)
                throw new ArgumentException($"One of {nameof(pfxCertificateBytes)} | {nameof(publicKeyBytes)} must be not null.");

            if (pfxCertificateBytes is not null)
            {
                _certificate = new X509Certificate2(pfxCertificateBytes, password);
            }

            if (publicKeyBytes is not null)
            {
                _RSAPublicKey = publicKeyBytes;
            }

            _password = password;
        }

        public static Certificate FromPfxBytes(byte[] pfxCertificateBytes, string? password)
        {
            if (pfxCertificateBytes is null)
                throw new ArgumentNullException(nameof(pfxCertificateBytes));

            return new(pfxCertificateBytes, null, password);
        }

        public static Certificate FromRSAPublicKeyBytes(byte[] publicKeyBytes)
        {
            if (publicKeyBytes is null)
                throw new ArgumentNullException(nameof(publicKeyBytes));

            return new(null, publicKeyBytes, null);
        }

        public bool HasCertificate => _certificate is not null;

        public bool HasPublicKey => _RSAPublicKey is not null || _certificate is not null;

        /// <summary>
        /// Gets certificate bytes for pfx format. You can save the to file like "MyCertificate.pfx".
        /// </summary>
        /// <returns>Bytes in pfx format</returns>
        public ReadOnlySpan<byte> GetPfxBytes()
        {
            if (_certificate is null)
                throw new InvalidOperationException("No certificate");

            byte[] certBytes = _certificate.Export(X509ContentType.Pfx, _password);
            return certBytes;
        }

        /// <summary>
        /// Gets bytes for RSA public key.
        /// </summary>
        /// <returns>RSA public key bytes</returns>
        public ReadOnlySpan<byte> GetRSAPublicKeyBytes()
        {
            if (_RSAPublicKey is not null)
                return _RSAPublicKey;

            RSA? rsaPublicKey = _certificate!.PublicKey.GetRSAPublicKey();
            if (rsaPublicKey == null)
                throw new InvalidOperationException("Failed to get RSA Public Key from certificate.");

            _RSAPublicKey = rsaPublicKey.ExportSubjectPublicKeyInfo();
            return _RSAPublicKey;
        }
    }
}
