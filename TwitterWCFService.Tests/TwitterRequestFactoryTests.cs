using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using TwitterWCFService;
using System.Net;

namespace TwitterWCFService.Tests
{
    [TestFixture]
    public class TwitterRequestFactoryTests
    {
        string urlEnding;
        string trackValue;
        public Func<string,string,string,string> authFunc;

        [SetUp]
        public void Setup()
        {
            trackValue = "tomorrow";
        }

        public void InitAuthFunc()
        {
            authFunc = delegate(string a, string b, string c) {return a+b+c;};
        }

        [Test]
        public void InitRequest_ValidInput_ReturnsObjNotNull()
        {
            InitAuthFunc();
            HttpWebRequest req = TwitterRequestFactory.InitRequest(trackValue, authFunc);
            Assert.IsNotNull(req);
        }

        
        [Test]
        public void GetAuthValue_ValidInput_ReturnsObjNotNullOrEmpty()
        {
            InitAuthFunc();
            string result = TwitterRequestFactory.GetAuthValue(urlEnding, trackValue, authFunc);
            Assert.IsNotNullOrEmpty(result);
        }

        [Test]
        public void AddParameters_ValidInput_ReturnsObjNotNull()
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create("https://stream.twitter.com/1.1/statuses/");
            req.Method = "POST";
            req = TwitterRequestFactory.AddParameters(req, trackValue);
            Assert.IsNotNull(req);
        }

        [Test]
        public void ToBytes_StringInput_ReturnsObjNotNull()
        {
            byte[] result = TwitterRequestFactory.ToBytes(trackValue);
            Assert.IsNotNull(result);
        }

        [Test]
        public void ToBytes_StringInput_ReturnsObjNotEmpty()
        {
            byte[] result = TwitterRequestFactory.ToBytes(trackValue);
            Assert.IsTrue(result.Length>0);
        }

    }
}
