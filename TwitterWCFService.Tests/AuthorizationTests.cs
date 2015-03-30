using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TwitterWCFService.Tests
{
    [TestFixture]
    class AuthorizationTests
    {
        private string timestampValue;
        private string nonceValue;
        private string trackValue;
        private string httpMethodValue;
        private string urlValue;

        [SetUp]
        public void Setup()
        {
            timestampValue = "1424518297";
            nonceValue = "88d0fa4bf17f67f041b2bb9ffba7f849";
            trackValue = "Unit Tests";
            httpMethodValue = "POST";
            urlValue = "https://stream.twitter.com/1.1/statuses/filter.json";
        }

        [Test]
        public void InitSignatureBaseString_ValidInput_ContainsTimestamp() 
        {
            string result = Authorization.InitSignatureBaseString(timestampValue, nonceValue, trackValue, httpMethodValue, urlValue);
            StringAssert.Contains(timestampValue, result);
        }

        [Test]
        public void InitSignatureBaseString_ValidInput_ContainsNonceValue()
        {
            string result = Authorization.InitSignatureBaseString(timestampValue, nonceValue, trackValue, httpMethodValue, urlValue);
            StringAssert.Contains(nonceValue, result);
        }

        [Test]
        public void InitSignatureBaseString_ValidInput_ContainsTrackValue()
        {
            string result = Authorization.InitSignatureBaseString(timestampValue, nonceValue, trackValue, httpMethodValue, urlValue);
            StringAssert.Contains(nonceValue, result);
        }

        [Test]
        public void InitSignatureBaseString_ValidInput_ContainsHttpMethodValue()
        {
            string result = Authorization.InitSignatureBaseString(timestampValue, nonceValue, trackValue, httpMethodValue, urlValue);
            StringAssert.Contains(httpMethodValue, result);
        }
    }
}
