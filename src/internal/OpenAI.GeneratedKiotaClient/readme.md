# Install Kiota
```shell
dotnet tool install --global Microsoft.OpenApi.Kiota
```

```shell
dotnet tool update --global Microsoft.OpenApi.Kiota
```

# OpenAI OpenAPI specification

```shell
kiota generate -d https://github.com/openai/openai-openapi/raw/master/openapi.yaml -o generated/openai -l CSharp -c GeneratedOpenAiClient --namespace-name OpenAI.GeneratedKiotaClient --serializer Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory --deserializer Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory
```

## Update the generated code
```shell
kiota update -o generated/openai
```

# Azure OpenAI specification

```shell
kiota generate -d https://github.com/Azure/azure-rest-api-specs/raw/main/specification/cognitiveservices/data-plane/AzureOpenAI/inference/preview/2023-12-01-preview/inference.yaml -o generated/azure_openai -l CSharp -c GeneratedAzureOpenAiClient --namespace-name OpenAI.Azure.GeneratedKiotaClient --serializer Microsoft.Kiota.Serialization.Json.JsonSerializationWriterFactory --deserializer Microsoft.Kiota.Serialization.Json.JsonParseNodeFactory
```

## Update the generated code
```shell
kiota update -o azure_openai
```

# Replace public with internal

```shell
powershell -ExecutionPolicy Bypass -File scripts\public_to_internal.ps1
```