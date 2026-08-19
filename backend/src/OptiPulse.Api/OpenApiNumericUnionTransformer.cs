using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OptiPulse.Api;

/// <summary>
/// Publishes numeric properties as plain numbers instead of `integer | string` unions.
///
/// <para><see cref="WebApplication.CreateSlimBuilder(string[])"/> configures System.Text.Json with
/// the web defaults, which include <c>JsonNumberHandling.AllowReadingFromString</c>. The OpenAPI
/// document generator reports that faithfully: every numeric member is typed as accepting a number
/// OR a string. That is true of what the API will *parse*, and false of what it ever *emits* —
/// responses are always numbers.</para>
///
/// <para>The cost of publishing it lands entirely on clients. openapi-typescript renders the union
/// as <c>number | string</c>, forcing a cast at every use site; openapi-generator's Dart target
/// emits a whole oneOf wrapper class per field (LoginResponseExpiresInSeconds and friends). And in
/// OpenAPI 3.0 — which this document targets, because the Dart generator cannot read 3.1 type
/// unions — a type union has no representation at all, so the type is dropped and the property
/// degrades to <c>unknown</c>. A contract that says "unknown" is worse than one that says
/// "integer", and both are worse than the truth.</para>
///
/// <para>Runtime leniency is unchanged: the API still accepts <c>"42"</c> where it accepted it
/// before. What changes is the published contract, which now describes the canonical wire format
/// rather than every input the parser tolerates.</para>
/// </summary>
internal sealed class OpenApiNumericUnionTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        if (schema.Type is { } type
            && (type.HasFlag(JsonSchemaType.Integer) || type.HasFlag(JsonSchemaType.Number))
            && type.HasFlag(JsonSchemaType.String))
        {
            // Clear ONLY the string alternative. The null flag is deliberately preserved: a
            // nullable integer is genuinely nullable on the wire, and dropping that would make
            // the contract lie in the opposite direction.
            schema.Type = type & ~JsonSchemaType.String;

            // The companion pattern only exists to validate the string form.
            schema.Pattern = null;
        }

        return Task.CompletedTask;
    }
}
