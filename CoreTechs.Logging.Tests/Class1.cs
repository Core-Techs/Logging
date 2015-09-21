using System.Linq;
using System.Net;
using System.Xml.Linq;
using CoreTechs.Logging.Targets;
using NUnit.Framework;

namespace CoreTechs.Logging.Tests
{
    class Class1
    {
        [Test]
        public void CanCreateEmailTargetWithInlinedSmtpClient()
        {
            var t = new EmailTarget();
            t.Configure(
                XElement.Parse(
                    "<target type=\"Email\" host=\"testhost\" port=\"215\" username=\"ronnie\" password=\"abc123\" /> "));

            var smtp = t.SmtpClientFactory.CreateSmtpClient();

            Assert.AreEqual("testhost",smtp.Host);
            Assert.AreEqual(215, smtp.Port);
            Assert.AreEqual("ronnie", ((NetworkCredential) smtp.Credentials).UserName);
            Assert.AreEqual("abc123", ((NetworkCredential) smtp.Credentials).Password);
        }

        [Test]
        public void CanCreateEmailTargetWithoutInlinedSmtpClient()
        {
            var t = new EmailTarget();
            t.Configure(XElement.Parse("<target type=\"Email\"  /> "));

            Assert.AreEqual(typeof(DefaultSmtpClientCreator),t.SmtpClientFactory.GetType());
        }

        [Test]
        public void ConcurrentCollectionAlwaysInOrder()
        {
            var range = Enumerable.Range(0, 10000);
            var coll = new ConcurrentList<int>(range);

            CollectionAssert.AreEqual(range,coll);
        }
    }
}
