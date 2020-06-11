using System;
using NUnit.Framework;
using Rezare.AuditLog.Models;
using System.Security.Cryptography;

namespace Rezare.AuditLog.Test.Models
{
    [TestFixture]
    public class AuditEntryTest
    {
        [Test]
        public void TestMessageSigning()
        {
            var message = new AuditEntry()
            {
                Sequence = 1,
                CreatedAt = DateTime.Now,
                Stream = "Test",
                Action = "Action 1",
                UserDetails = "User Details",
                DataPayload = "Data Payload"
            };

            var csp = new RSACryptoServiceProvider();
            message.SignAuditMesssage(csp, Crypto.SupportedHashAlgorithm.SHA1);

            Assert.NotNull(message.Signature);
        }

        [Test]
        public void TestMessageVerification()
        {
            var message = new AuditEntry()
            {
                Sequence = 1,
                CreatedAt = DateTime.Now,
                Stream = "Test",
                Action = "Action 1",
                UserDetails = "User Details",
                DataPayload = "Data Payload"
            };

            var csp = new RSACryptoServiceProvider();
            message.SignAuditMesssage(csp, Crypto.SupportedHashAlgorithm.SHA1);

            Assert.IsTrue(message.VerifyAuditMessage(csp, Crypto.SupportedHashAlgorithm.SHA1));
        }

        [Test]
        public void TestMessageVerificationFailure()
        {
            var message = new AuditEntry()
            {
                Sequence = 1,
                CreatedAt = DateTime.Now,
                Stream = "Test",
                Action = "Action 1",
                UserDetails = "User Details",
                DataPayload = "Data Payload"
            };

            var csp = new RSACryptoServiceProvider();
            message.SignAuditMesssage(csp, Crypto.SupportedHashAlgorithm.SHA1);

            message.Sequence = 2;

            Assert.IsFalse(message.VerifyAuditMessage(csp, Crypto.SupportedHashAlgorithm.SHA1));
        }

        [Test]
        [ExpectedException(ExpectedException = typeof(InvalidOperationException))]
        public void TestMessageSigningTwiceFails()
        {
            var message = new AuditEntry()
            {
                Sequence = 1,
                CreatedAt = DateTime.Now,
                Stream = "Test",
                Action = "Action 1",
                UserDetails = "User Details",
                DataPayload = "Data Payload"
            };

            var csp = new RSACryptoServiceProvider();
            message.SignAuditMesssage(csp, Crypto.SupportedHashAlgorithm.SHA1);

            message.SignAuditMesssage(csp, Crypto.SupportedHashAlgorithm.SHA1);
        }
    }
}
