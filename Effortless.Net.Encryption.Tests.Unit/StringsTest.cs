namespace Effortless.Net.Encryption.Tests.Unit
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System.Security.Cryptography;
    using NUnit.Framework;
    using System.Collections.Generic;

    [TestFixture]
    public class StringsTest
    {
        [Test]
        [TestCase(1, true)]
        [TestCase(1, false)]
        [TestCase(10, true)]
        [TestCase(10, false)]
        [TestCase(100, true)]
        [TestCase(100, false)]
        [TestCase(1000, true)]
        [TestCase(1000, false)]
        [TestCase(10000, true)]
        [TestCase(10000, false)]
        public void Create_long_password(int size, bool allowPunctuation)
        {
            // Check for a long password, so it causes multiple CreateSaltsFull() function calls
            var s = Strings.CreatePassword(size, allowPunctuation);
            Assert.AreEqual(size, s.Length);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void Create_password(bool allowPunctuation)
        {
            const int passLen = 10;
            var list = new List<string>();
            for (var n = 0; n < 100; n++)
            {
                var password = Strings.CreatePassword(passLen, allowPunctuation);
                Assert.AreEqual(passLen, password.Length);

                foreach (var c in password)
                    if (allowPunctuation)
                        Assert.IsTrue(char.IsLetterOrDigit(c) || char.IsPunctuation(c));
                    else
                        Assert.IsTrue(char.IsLetterOrDigit(c));

                Assert.IsFalse(list.Contains(password));
                list.Add(password);
            }
        }

        [Test]
        public void Create_salt()
        {
            const int numChars = 30;
            var list = new SortedList<string, string>();
            for (var n = 0; n < 10000; n++)
            {
                var salt = Strings.CreateSalt(numChars);
                Assert.AreEqual(numChars, salt.Length);

                Assert.IsFalse(list.ContainsKey(salt));
                list.Add(salt, salt);
            }
        }

        [Test]
        public void Create_salt_full()
        {
            const int numBytes = 30;
            var list = new SortedList<string, string>();
            for (var n = 0; n < 10000; n++)
            {
                var salt = Strings.CreateSaltFull(numBytes);
                Assert.AreEqual(40, salt.Length);

                Assert.IsFalse(list.ContainsKey(salt));
                list.Add(salt, salt);
            }
        }

        [Test]
        public void Encrypt_decrypt_using_a_very_long_string()
        {
            var key = Bytes.GenerateKey();
            var iv = Bytes.GenerateIV();
            var random = new Random();

            for (var n = 0; n < 1000; n++)
            {
                var size = random.Next(4096) + 1;
                var data = Strings.CreateSalt(size);
                var encrypted = Strings.Encrypt(data, key, iv);
                var decrypted = Strings.Decrypt(encrypted, key, iv);
                Assert.AreEqual(data, decrypted);
            }
        }

        [Test]
        [TestCase(Bytes.KeySize.Size128)]
        [TestCase(Bytes.KeySize.Size192)]
        [TestCase(Bytes.KeySize.Size256)]
        public void Encrypt_decrypt_using_different_key_sizes(Bytes.KeySize keySize)
        {
            const string password = "Hello world";
            const string salt = "saltsaltsalt";
            var iv = string.Empty.PadLeft(16, '#'); //todo
            const string original = "My secret text";

            var encrypted = Strings.Encrypt(original, password, salt, iv, keySize, 1);
            var decrypted = Strings.Decrypt(encrypted, password, salt, iv, keySize, 1);
            Assert.AreEqual(original, decrypted);
        }

        [Test]
        [TestCase(@"$L;R*gM0\3+%@!#pLR4!@b#ryu'E+Wre_6r^i,b?2-mQ hu|^8ZnQ[_rw._i6%C")]
        [TestCase("Hello world")]
        [TestCase("My secret text")]
        public void Encrypt_decrypt_using_key_iv(string data)
        {
            var key = Bytes.GenerateKey();
            var iv = Bytes.GenerateIV();

            var encrypted = Strings.Encrypt(data, key, iv);
            var decrypted = Strings.Decrypt(encrypted, key, iv);

            Assert.AreEqual(data, decrypted);
        }
    }
}