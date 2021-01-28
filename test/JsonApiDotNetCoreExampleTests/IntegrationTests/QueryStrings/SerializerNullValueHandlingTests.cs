using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Serialization.Objects;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Xunit;

namespace JsonApiDotNetCoreExampleTests.IntegrationTests.QueryStrings
{
    public sealed class SerializerNullValueHandlingTests
        : IClassFixture<IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext>>
    {
        private readonly IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> _testContext;
        private readonly QueryStringFakers _fakers = new QueryStringFakers();

        public SerializerNullValueHandlingTests(IntegrationTestContext<TestableStartup<QueryStringDbContext>, QueryStringDbContext> testContext)
        {
            _testContext = testContext;
        }

        [Fact]
        public async Task Cannot_override_from_query_string()
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowQueryStringOverrideForSerializerNullValueHandling = false;

            var route = "/calendars?nulls=true";

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<ErrorDocument>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.BadRequest);

            responseDocument.Errors.Should().HaveCount(1);
            responseDocument.Errors[0].StatusCode.Should().Be(HttpStatusCode.BadRequest);
            responseDocument.Errors[0].Title.Should().Be("Usage of one or more query string parameters is not allowed at the requested endpoint.");
            responseDocument.Errors[0].Detail.Should().Be("The parameter 'nulls' cannot be used at this endpoint.");
            responseDocument.Errors[0].Source.Parameter.Should().Be("nulls");
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData(null, "false", false)]
        [InlineData(null, "true", true)]
        [InlineData(NullValueHandling.Ignore, null, false)]
        [InlineData(NullValueHandling.Ignore, "false", false)]
        [InlineData(NullValueHandling.Ignore, "true", true)]
        [InlineData(NullValueHandling.Include, null, true)]
        [InlineData(NullValueHandling.Include, "false", false)]
        [InlineData(NullValueHandling.Include, "true", true)]
        public async Task Can_override_from_query_string(NullValueHandling? configurationValue, string queryStringValue, bool expectInDocument)
        {
            // Arrange
            var options = (JsonApiOptions) _testContext.Factory.Services.GetRequiredService<IJsonApiOptions>();
            options.AllowQueryStringOverrideForSerializerNullValueHandling = true;
            options.SerializerSettings.NullValueHandling = configurationValue ?? NullValueHandling.Include;

            var calendar = _fakers.Calendar.Generate();
            calendar.TimeZone = null;
            calendar.Appointments = _fakers.Appointment.Generate(1).ToHashSet();
            calendar.Appointments.Single().Title = null;

            await _testContext.RunOnDatabaseAsync(async dbContext =>
            {
                dbContext.Calendars.Add(calendar);
                await dbContext.SaveChangesAsync();
            });

            var route = $"/calendars/{calendar.StringId}?include=appointments" + (queryStringValue != null ? "&nulls=" + queryStringValue : "");

            // Act
            var (httpResponse, responseDocument) = await _testContext.ExecuteGetAsync<Document>(route);

            // Assert
            httpResponse.Should().HaveStatusCode(HttpStatusCode.OK);

            responseDocument.SingleData.Should().NotBeNull();
            responseDocument.Included.Should().HaveCount(1);

            if (expectInDocument)
            {
                responseDocument.SingleData.Attributes.Should().ContainKey("timeZone");
                responseDocument.Included[0].Attributes.Should().ContainKey("title");
            }
            else
            {
                responseDocument.SingleData.Attributes.Should().NotContainKey("timeZone");
                responseDocument.Included[0].Attributes.Should().NotContainKey("title");
            }
        }
    }
}