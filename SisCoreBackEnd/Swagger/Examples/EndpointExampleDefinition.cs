using System.Collections.Immutable;

namespace SisCoreBackEnd.Swagger.Examples
{
    internal sealed record EndpointExampleDefinition(
        object? RequestExample,
        ImmutableDictionary<int, object?> Responses)
    {
        public EndpointExampleDefinition() : this(null, ImmutableDictionary<int, object?>.Empty)
        {
        }

        public EndpointExampleDefinition WithRequest(object? example) =>
            this with { RequestExample = example };

        public EndpointExampleDefinition WithResponse(int statusCode, object? example) =>
            this with { Responses = Responses.SetItem(statusCode, example) };

        public EndpointExampleDefinition WithResponses(params (int StatusCode, object Example)[] responses)
        {
            var builder = Responses.ToBuilder();
            foreach (var (status, example) in responses)
            {
                builder[status] = example;
            }

            return this with { Responses = builder.ToImmutable() };
        }
    }
}


