﻿using OpenAI;
using OpenAI.ChatGpt.Extensions;

var client = new OpenAiClient(Environment.GetEnvironmentVariable("openai_api_key_paid")!);

var testsCode = await client.GenerateTestsForCodeInFolder(
@"C:\Users\rodio\RiderProjects\OpenAI_DotNet\OpenAI",
"C#", "xunit and FluentAssertions"
);
//var testsCode = await client.GenerateTestsForCode("int Add(int a, int b) => a + b;", "C#", "xunit and FluentAssertions");

File.WriteAllText("test.txt", testsCode);

Console.WriteLine(testsCode);