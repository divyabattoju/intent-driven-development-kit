using IntentDK.Core.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace IntentDK.Core.Parsing;

/// <summary>
/// Parser for intent YAML definitions.
/// </summary>
public class IntentParser
{
    private readonly IDeserializer _deserializer;
    private readonly ISerializer _serializer;

    public IntentParser()
    {
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        _serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();
    }

    /// <summary>
    /// Parses a YAML string into an Intent object.
    /// </summary>
    /// <param name="yaml">The YAML string to parse.</param>
    /// <returns>The parsed Intent object.</returns>
    /// <exception cref="IntentParseException">Thrown when parsing fails.</exception>
    public Intent Parse(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            throw new IntentParseException("YAML content cannot be empty.");
        }

        try
        {
            var intent = _deserializer.Deserialize<Intent>(yaml);
            
            if (intent == null)
            {
                throw new IntentParseException("Failed to parse YAML into Intent.");
            }

            return intent;
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new IntentParseException($"Invalid YAML format: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses a YAML string and validates the resulting Intent.
    /// </summary>
    /// <param name="yaml">The YAML string to parse.</param>
    /// <returns>A result containing the intent or validation errors.</returns>
    public ParseResult<Intent> ParseAndValidate(string yaml)
    {
        try
        {
            var intent = Parse(yaml);
            var validation = intent.Validate();

            if (!validation.IsValid)
            {
                return ParseResult<Intent>.Failure(validation.Errors);
            }

            return ParseResult<Intent>.Success(intent);
        }
        catch (IntentParseException ex)
        {
            return ParseResult<Intent>.Failure(new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Extracts an intent from text that may contain a /intent command block.
    /// </summary>
    /// <param name="text">The text containing the intent.</param>
    /// <returns>A result containing the intent or errors.</returns>
    public ParseResult<Intent> ExtractFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return ParseResult<Intent>.Failure(new List<string> { "Text cannot be empty." });
        }

        // Try to extract YAML from code fence
        var yamlContent = ExtractYamlFromCodeFence(text);
        
        if (yamlContent == null)
        {
            // Check if it starts with /intent command
            yamlContent = ExtractFromIntentCommand(text);
        }

        if (yamlContent == null)
        {
            // Try parsing the entire text as YAML
            yamlContent = text;
        }

        return ParseAndValidate(yamlContent);
    }

    /// <summary>
    /// Serializes an Intent object to YAML.
    /// </summary>
    /// <param name="intent">The intent to serialize.</param>
    /// <returns>The YAML representation.</returns>
    public string ToYaml(Intent intent)
    {
        if (intent == null)
        {
            throw new ArgumentNullException(nameof(intent));
        }

        return _serializer.Serialize(intent);
    }

    /// <summary>
    /// Parses a Plan from YAML.
    /// </summary>
    public Plan ParsePlan(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            throw new IntentParseException("YAML content cannot be empty.");
        }

        try
        {
            var plan = _deserializer.Deserialize<Plan>(yaml);
            
            if (plan == null)
            {
                throw new IntentParseException("Failed to parse YAML into Plan.");
            }

            return plan;
        }
        catch (YamlDotNet.Core.YamlException ex)
        {
            throw new IntentParseException($"Invalid YAML format: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes a Plan to YAML.
    /// </summary>
    public string ToYaml(Plan plan)
    {
        if (plan == null)
        {
            throw new ArgumentNullException(nameof(plan));
        }

        return _serializer.Serialize(plan);
    }

    private static string? ExtractYamlFromCodeFence(string text)
    {
        // Match ```yaml ... ``` or ``` ... ```
        var patterns = new[]
        {
            @"```ya?ml\s*\n([\s\S]*?)```",
            @"```\s*\n([\s\S]*?)```"
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private static string? ExtractFromIntentCommand(string text)
    {
        // Match /intent followed by YAML content
        var trimmed = text.Trim();
        
        if (trimmed.StartsWith("/intent", StringComparison.OrdinalIgnoreCase))
        {
            // Remove the /intent prefix and any following whitespace
            var content = trimmed.Substring(7).TrimStart();
            
            if (!string.IsNullOrEmpty(content))
            {
                return content;
            }
        }

        return null;
    }
}

/// <summary>
/// Result of a parse operation.
/// </summary>
/// <typeparam name="T">The type being parsed.</typeparam>
public class ParseResult<T> where T : class
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public List<string> Errors { get; private set; } = new();

    public static ParseResult<T> Success(T value)
    {
        return new ParseResult<T>
        {
            IsSuccess = true,
            Value = value
        };
    }

    public static ParseResult<T> Failure(List<string> errors)
    {
        return new ParseResult<T>
        {
            IsSuccess = false,
            Errors = errors
        };
    }
}

/// <summary>
/// Exception thrown when intent parsing fails.
/// </summary>
public class IntentParseException : Exception
{
    public IntentParseException(string message) : base(message) { }
    public IntentParseException(string message, Exception innerException) : base(message, innerException) { }
}
