
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bam.CoreServices;
using Bam.Encryption;
using Bam.Test;
using Org.BouncyCastle.Security;

namespace Bam.Base.Transformers.Tests
{
    public class TransformerShould : UnitTestMenuContainer
    {
        
        public TransformerShould(ServiceRegistry serviceRegistry) : base(serviceRegistry)
        {
        }
        
        [UnitTest]
        public void TransformBson()
        {
            BsonTransformer<TestMonkey> transformer = new BsonTransformer<TestMonkey>();
            TestMonkey testMonkey = new TestMonkey()
            {
                Name = "Bson Fred"
            };

            byte[] bson = transformer.Transform(testMonkey);

            TestMonkey decoded = transformer.GetReverseTransformer().ReverseTransform(bson);

            Expect.AreEqual(testMonkey.Name, decoded.Name);
        }

        [UnitTest]
        public void TransformJson()
        {
            JsonTransformer<TestMonkey> transformer = new JsonTransformer<TestMonkey>();
            TestMonkey testMonkey = new TestMonkey()
            {
                Name = "Fred"
            };

            string json = transformer.Transform(testMonkey);

            TestMonkey decoded = transformer.GetReverseTransformer().ReverseTransform(json);

            Expect.AreEqual(testMonkey.Name, decoded.Name);
        }

        [UnitTest]
        public void TransformBase64()
        {
            Base64Transformer base64Transformer = new Base64Transformer();
            SecureRandom secureRandom = new SecureRandom();
            byte[] randomBytes = secureRandom.GenerateSeed(64);

            string encoded = base64Transformer.Transform(randomBytes);

            IValueReverseTransformer<string, byte[]> base64Untransformer = base64Transformer.GetReverseTransformer();
            byte[] decoded = base64Untransformer.ReverseTransform(encoded);
            Expect.AreEqual(randomBytes, decoded);
        }

        [UnitTest]
        public void AesKeyVectorPairTest()  // TODO: move this to encryption tests
        {
            AesKeyVectorPair aesKeyVectorPair = new AesKeyVectorPair();
            string testData = "this is the test data";
            string cipher = aesKeyVectorPair.Encrypt(testData);
            string deciphered = aesKeyVectorPair.Decrypt(cipher);

            deciphered.ShouldBeEqualTo(testData);
        }

        [UnitTest]
        public void TransformAes()
        {
            AesKeyVectorPair aesKeyVectorPair = new AesKeyVectorPair();
            AesByteTransformer aesByteTransformer = new AesByteTransformer(aesKeyVectorPair);

            string testData = "this is the test data";
            byte[] testDataBytes = Encoding.UTF8.GetBytes(testData);

            byte[] cipherData = aesByteTransformer.Transform(testDataBytes);

            byte[] deciphered = aesByteTransformer.GetReverseTransformer().ReverseTransform(cipherData);

            Expect.AreEqual(testDataBytes, deciphered);
            string decipheredTestData = Encoding.UTF8.GetString(deciphered);
            Expect.AreEqual(testData, decipheredTestData);
        }

        [UnitTest]
        public void TransformAesBase64()
        {
            AesKeyVectorPair aesKeyVectorPair = new AesKeyVectorPair();
            AesBase64Transformer aesBase64Transformer = new AesBase64Transformer(aesKeyVectorPair);

            string testData = "this is the test data";
            string base64Cipher = aesBase64Transformer.Transform(testData);

            Expect.IsFalse(testData.Equals(base64Cipher));

            string decipherd = aesBase64Transformer.GetReverseTransformer().ReverseTransform(base64Cipher);

            Equals(testData, decipherd);
        }

        [UnitTest]
        public void TransformRsaBytes()
        {
            RsaPublicPrivateKeyPair rsaKey = new RsaPublicPrivateKeyPair();
            RsaByteTransformer rsaByteTransformer = new RsaByteTransformer(rsaKey);

            string testData = "this is the test data";
            byte[] testDataBytes = Encoding.UTF8.GetBytes(testData);
            byte[] cipher = rsaByteTransformer.Transform(testDataBytes);

            Expect.IsFalse(testDataBytes.Length == cipher.Length);

            string base64Data = Convert.ToBase64String(testDataBytes);
            string base64Cipher = Convert.ToBase64String(cipher);

            Expect.IsNotNullOrEmpty(base64Data);
            Expect.IsNotNullOrEmpty(base64Cipher);
            Expect.IsFalse(base64Data.Equals(base64Cipher));

            byte[] deciphered = rsaByteTransformer.GetReverseTransformer().ReverseTransform(cipher);
            string decipheredText = Encoding.UTF8.GetString(deciphered);

            Expect.AreEqual(testData, decipheredText);
        }

        [UnitTest]
        public void TransformRsaBase64()
        {
            RsaPublicPrivateKeyPair rsaKey = new RsaPublicPrivateKeyPair();
            RsaBase64Transformer rsaBase64Transformer = new RsaBase64Transformer(rsaKey);

            string testData = "this is the test data";
            string base64Cipher = rsaBase64Transformer.Transform(testData);

            Expect.IsFalse(testData.Length == base64Cipher.Length);
            Expect.IsNotNullOrEmpty(base64Cipher);
            Expect.IsFalse(testData.Equals(base64Cipher));

            string deciphered = rsaBase64Transformer.GetReverseTransformer().ReverseTransform(base64Cipher);

            Expect.AreEqual(testData, deciphered);
        }

        [UnitTest]
        public void SymmetricEncryptorEncryptAndDecryptStringTest()  // TODO: move this to encryption tests
        {
            AesKeyVectorPair aesKey = new AesKeyVectorPair();
            SymmetricDataEncryptor<TestMonkey> encryptor = new SymmetricDataEncryptor<TestMonkey>(aesKey);

            string testValue = $"this is the test value {Guid.NewGuid()}";
            string cipherString = encryptor.EncryptString(testValue);            

            IDecryptor<TestMonkey> decryptor = encryptor.GetDecryptor();
            
            string decipheredString = decryptor.DecryptString(cipherString);
            Expect.AreEqual(testValue, decipheredString);
        }

        [UnitTest]
        public void SymmetricEncryptorEncryptAndDecryptBytesTest() // TODO: move this to encryption tests
        {
            AesKeyVectorPair aesKey = new AesKeyVectorPair();
            SymmetricDataEncryptor<TestMonkey> encryptor = new SymmetricDataEncryptor<TestMonkey>(aesKey);

            string testValue = $"this is the test value {Guid.NewGuid()}";
            byte[] utf8 = Encoding.UTF8.GetBytes(testValue);

            IDecryptor<TestMonkey> decryptor = encryptor.GetDecryptor();
            
            byte[] cipherBytes = encryptor.EncryptBytes(utf8);
            byte[] decipheredBytes = decryptor.DecryptBytes(cipherBytes);
            string deciphered = Encoding.UTF8.GetString(decipheredBytes);
            Expect.AreEqual(testValue, deciphered);
        }


        [UnitTest]
        public void AsymmetricEncryptorEncryptAndDecryptStringTest()
        {
            RsaPublicPrivateKeyPair rsaPublicPrivateKeyPair = new RsaPublicPrivateKeyPair();
            AsymmetricDataEncryptor<TestMonkey> encryptor = new AsymmetricDataEncryptor<TestMonkey>(rsaPublicPrivateKeyPair);

            string testValue = $"this is the test value {Guid.NewGuid()}";
            string cipherString = encryptor.EncryptString(testValue);

            IDecryptor<TestMonkey> decryptor = encryptor.GetDecryptor();

            string decipheredString = decryptor.DecryptString(cipherString);
            Expect.AreEqual(testValue, decipheredString);
        }

        [UnitTest]
        public void AsymmetricEncryptorEncryptAndDecryptBytesTest()
        {
            RsaPublicPrivateKeyPair rsaPublicPrivateKeyPair = new RsaPublicPrivateKeyPair();
            AsymmetricDataEncryptor<TestMonkey> encryptor = new AsymmetricDataEncryptor<TestMonkey>(rsaPublicPrivateKeyPair);

            string testValue = $"this is the test value {Guid.NewGuid()}";
            byte[] utf8 = Encoding.UTF8.GetBytes(testValue);

            IDecryptor<TestMonkey> decryptor = encryptor.GetDecryptor();

            byte[] cipherBytes = encryptor.EncryptBytes(utf8);
            byte[] decipheredBytes = decryptor.DecryptBytes(cipherBytes);
            string deciphered = Encoding.UTF8.GetString(decipheredBytes);
            Expect.AreEqual(testValue, deciphered);
        }

        [UnitTest]
        public void ValueTransformerPipelineFactoryTest()
        {
            ServiceRegistry testRegistry = new ServiceRegistry();
            testRegistry.For<IAesKeySource>().Use<AesKeyVectorPair>();

            ValueTransformerPipelineFactory factory = new ValueTransformerPipelineFactory(testRegistry);
            ValueTransformerPipeline<TestMonkey> valueTransformerPipeline = factory.Create<TestMonkey>( typeof(AesByteTransformer).Assembly,"aes");

            string testName = "test_".RandomLetters(8);
            TestMonkey testMonkey = new TestMonkey
            {
                Name = testName,
                TailCount = RandomNumber.Between(1, 9),
            };

            string cipher = valueTransformerPipeline.Transform(testMonkey).ToBase64();

            TestMonkey deciphered = valueTransformerPipeline.GetReverseTransformer().ReverseTransform(cipher.FromBase64());

            Expect.AreEqual(testMonkey.Name, testName);
            Expect.AreEqual(deciphered.Name, testName);
            Expect.AreEqual(deciphered.TailCount, testMonkey.TailCount);
            Expect.IsGreaterThan(deciphered.TailCount, 0);
        }
    }
}
