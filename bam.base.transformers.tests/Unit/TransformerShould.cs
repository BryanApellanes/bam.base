using System.Text;
using Bam.DependencyInjection;
using Bam.Encryption;
using Bam.Services;
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

    }
}
