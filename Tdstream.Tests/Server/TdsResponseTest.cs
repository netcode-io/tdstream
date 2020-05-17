//using NFluent;
//using NUnit.Framework;
//using System;
//using System.Linq;

//namespace Tdstream.Server
//{
//    public class TdsResponseTest
//    {
//        [Test]
//        public void Should_build_response_with_status_code()
//        {
//            // given
//            var response = new TdsResponse();
//            // when
//            var textResponse = response.ToString();
//            // then
//            Check.That(textResponse).StartsWith("HTTP/1.1 404");
//        }

//        [Test]
//        public void Should_build_response_with_headers()
//        {
//            // given
//            var response = new TdsResponse();
//            //response.Headers.Add("header", "value");
//            // when
//            var textResponse = response.ToString();
//            // then
//            Check.That(textResponse).Contains("header: value");
//        }

//    }
//}
