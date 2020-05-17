using NFluent;
using NUnit.Framework;

namespace Tdstream.Server
{
    public class TdsRequestTest
    {
        [Test]
        public void Should_parse_request()
        {
            // given
            var rawQuery = "Query";

            // when
            var tdsRequest = TdsRequest.Parse(rawQuery);

            // then
            Check.That(tdsRequest).IsNotNull();
            Check.That(tdsRequest.Query).IsEqualTo("Query");
        }
    }
}
