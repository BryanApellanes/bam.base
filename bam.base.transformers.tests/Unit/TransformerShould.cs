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
            TestMonkey testMonkey = new TestMonkey { Name = "Bson Fred" };

            When.A<BsonTransformer<TestMonkey>>("round-trips a TestMonkey through BSON",
                (transformer) =>
                {
                    byte[] bson = transformer.Transform(testMonkey);
                    TestMonkey decoded = transformer.GetReverseTransformer().ReverseTransform(bson);
                    return decoded;
                })
            .TheTest
            .ShouldPass(because =>
            {
                because.TheResult.IsNotNull()
                    .As<TestMonkey>("Name equals original", m => testMonkey.Name.Equals(m?.Name));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void TransformJson()
        {
            TestMonkey testMonkey = new TestMonkey { Name = "Fred" };

            When.A<JsonTransformer<TestMonkey>>("round-trips a TestMonkey through JSON",
                (transformer) =>
                {
                    string json = transformer.Transform(testMonkey);
                    TestMonkey decoded = transformer.GetReverseTransformer().ReverseTransform(json);
                    return decoded;
                })
            .TheTest
            .ShouldPass(because =>
            {
                because.TheResult.IsNotNull()
                    .As<TestMonkey>("Name equals original", m => testMonkey.Name.Equals(m?.Name));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

        [UnitTest]
        public void TransformBase64()
        {
            SecureRandom secureRandom = new SecureRandom();
            byte[] randomBytes = secureRandom.GenerateSeed(64);

            When.A<Base64Transformer>("round-trips random bytes through Base64",
                (transformer) =>
                {
                    string encoded = transformer.Transform(randomBytes);
                    IValueReverseTransformer<string, byte[]> untransformer = transformer.GetReverseTransformer();
                    byte[] decoded = untransformer.ReverseTransform(encoded);
                    return new object[] { randomBytes, decoded };
                })
            .TheTest
            .ShouldPass(because =>
            {
                object[] results = (object[])because.Result;
                byte[] original = (byte[])results[0];
                byte[] decoded = (byte[])results[1];
                because.ItsTrue("decoded bytes equal original", original.SequenceEqual(decoded));
            })
            .SoBeHappy()
            .UnlessItFailed();
        }

    }
}
