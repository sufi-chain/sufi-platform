namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Turns an OpenRouter pricing object into the amount and unit shown on a model route.
/// OpenRouter does not send a unit. <c>pricing.prompt</c> is USD per token for text,
/// USD per second or per hour for duration speech-to-text, and USD per character for most text-to-speech.
/// </summary>
public readonly record struct CatalogPriceQuote(
    decimal? InputPrice,
    AIPriceUnit InputUnit,
    decimal? OutputPrice,
    AIPriceUnit OutputUnit,
    bool ShowOutput)
{
    /// <summary>
    /// Duration speech-to-text stores USD per second in <c>prompt</c> up to about 0.0003.
    /// Microsoft MAI stores USD per hour at 0.1 and 0.36. 0.01 sits in the gap observed on 2026-09-29.
    /// </summary>
    public const decimal HourlyAudioPromptFloor = 0.01m;

    /// <summary>
    /// Gemini speech sets both sides at token rates, with prompt under this ceiling.
    /// Character-priced speech sets prompt only, from about 0.000004 USD per character.
    /// </summary>
    public const decimal SpeechTokenPromptCeiling = 0.000002m;

    /// <summary>
    /// A completion rate at or above this, with no token-scale prompt, is USD per second of generated audio.
    /// Token rates in the speech catalog stay below it.
    /// </summary>
    public const decimal SpeechOutputSecondsFloor = 0.0001m;

    /// <summary>
    /// <c>image_output</c> at or above this is USD per image. Smaller values match <c>image_token</c> and are per token.
    /// </summary>
    public const decimal PerImageOutputFloor = 0.001m;

    public static CatalogPriceQuote Default(AICapabilityType capability)
    {
        return capability switch
        {
            AICapabilityType.AudioTranscription => new CatalogPriceQuote(null, AIPriceUnit.PerMinute, null, AIPriceUnit.PerMillionTokens, false),
            AICapabilityType.TextToSpeech => new CatalogPriceQuote(null, AIPriceUnit.PerMillionCharacters, null, AIPriceUnit.PerMillionTokens, false),
            AICapabilityType.ImageGeneration => new CatalogPriceQuote(null, AIPriceUnit.PerMillionTokens, null, AIPriceUnit.PerImage, true),
            AICapabilityType.WebSearch or AICapabilityType.WebFetch => new CatalogPriceQuote(null, AIPriceUnit.PerRequest, null, AIPriceUnit.PerRequest, false),
            AICapabilityType.Embeddings => new CatalogPriceQuote(null, AIPriceUnit.PerMillionTokens, null, AIPriceUnit.PerMillionTokens, false),
            _ => new CatalogPriceQuote(null, AIPriceUnit.PerMillionTokens, null, AIPriceUnit.PerMillionTokens, true)
        };
    }

    public static CatalogPriceQuote From(
        AICapabilityType capability,
        decimal? prompt,
        decimal? completion,
        decimal markupPercent,
        decimal? image = null,
        decimal? imageOutput = null,
        decimal? imageToken = null,
        decimal? requestPrice = null,
        decimal? webSearch = null)
    {
        return capability switch
        {
            AICapabilityType.AudioTranscription => Transcription(prompt, completion, markupPercent),
            AICapabilityType.TextToSpeech => Speech(prompt, completion, markupPercent),
            AICapabilityType.ImageGeneration => Image(prompt, completion, image, imageOutput, imageToken, markupPercent),
            AICapabilityType.Embeddings => Tokens(prompt, completion, markupPercent, showOutput: false),
            AICapabilityType.WebSearch or AICapabilityType.WebFetch => Request(webSearch ?? requestPrice, markupPercent),
            _ => Tokens(prompt, completion, markupPercent, showOutput: true)
        };
    }

    private static CatalogPriceQuote Transcription(decimal? prompt, decimal? completion, decimal markupPercent)
    {
        if (completion is > 0)
        {
            return Tokens(prompt, completion, markupPercent, showOutput: true);
        }

        if (prompt is >= HourlyAudioPromptFloor)
        {
            return new CatalogPriceQuote(
                ModelCatalogPriceCalculator.MarkUpAmount(prompt, markupPercent),
                AIPriceUnit.PerHour,
                null,
                AIPriceUnit.PerMillionTokens,
                false);
        }

        var perMinute = prompt is > 0 ? prompt * 60m : prompt;
        return new CatalogPriceQuote(
            ModelCatalogPriceCalculator.MarkUpAmount(perMinute, markupPercent),
            AIPriceUnit.PerMinute,
            null,
            AIPriceUnit.PerMillionTokens,
            false);
    }

    private static CatalogPriceQuote Speech(decimal? prompt, decimal? completion, decimal markupPercent)
    {
        if (completion is > 0 && prompt is > 0 and < SpeechTokenPromptCeiling)
        {
            return Tokens(prompt, completion, markupPercent, showOutput: true);
        }

        if (completion is > 0 and < SpeechOutputSecondsFloor && prompt is null or 0)
        {
            return Tokens(null, completion, markupPercent, showOutput: true);
        }

        if (completion is >= SpeechOutputSecondsFloor && prompt is null or 0)
        {
            return new CatalogPriceQuote(
                null,
                AIPriceUnit.PerMillionCharacters,
                ModelCatalogPriceCalculator.MarkUpAmount(completion * 60m, markupPercent),
                AIPriceUnit.PerMinute,
                true);
        }

        var perMillionCharacters = prompt is > 0 ? prompt * 1_000_000m : prompt;
        return new CatalogPriceQuote(
            ModelCatalogPriceCalculator.MarkUpAmount(perMillionCharacters, markupPercent),
            AIPriceUnit.PerMillionCharacters,
            null,
            AIPriceUnit.PerMillionTokens,
            false);
    }

    private static CatalogPriceQuote Image(
        decimal? prompt,
        decimal? completion,
        decimal? image,
        decimal? imageOutput,
        decimal? imageToken,
        decimal markupPercent)
    {
        decimal? input = null;
        var inputUnit = AIPriceUnit.PerMillionTokens;
        if (prompt is > 0)
        {
            input = ModelCatalogPriceCalculator.MarkUpPerMillion(prompt, markupPercent);
        }
        else if (image is > 0)
        {
            input = ModelCatalogPriceCalculator.MarkUpAmount(image, markupPercent);
            inputUnit = AIPriceUnit.PerImage;
        }

        decimal? output = null;
        var outputUnit = AIPriceUnit.PerImage;
        if (completion is > 0)
        {
            output = ModelCatalogPriceCalculator.MarkUpPerMillion(completion, markupPercent);
            outputUnit = AIPriceUnit.PerMillionTokens;
        }
        else if (imageToken is > 0)
        {
            output = ModelCatalogPriceCalculator.MarkUpPerMillion(imageToken, markupPercent);
            outputUnit = AIPriceUnit.PerMillionTokens;
        }
        else if (imageOutput is >= PerImageOutputFloor)
        {
            output = ModelCatalogPriceCalculator.MarkUpAmount(imageOutput, markupPercent);
        }
        else if (imageOutput is > 0)
        {
            output = ModelCatalogPriceCalculator.MarkUpPerMillion(imageOutput, markupPercent);
            outputUnit = AIPriceUnit.PerMillionTokens;
        }

        return new CatalogPriceQuote(input, inputUnit, output, outputUnit, true);
    }

    private static CatalogPriceQuote Tokens(decimal? prompt, decimal? completion, decimal markupPercent, bool showOutput)
    {
        return new CatalogPriceQuote(
            ModelCatalogPriceCalculator.MarkUpPerMillion(prompt, markupPercent),
            AIPriceUnit.PerMillionTokens,
            showOutput ? ModelCatalogPriceCalculator.MarkUpPerMillion(completion, markupPercent) : null,
            AIPriceUnit.PerMillionTokens,
            showOutput);
    }

    private static CatalogPriceQuote Request(decimal? price, decimal markupPercent)
    {
        return new CatalogPriceQuote(
            ModelCatalogPriceCalculator.MarkUpAmount(price, markupPercent),
            AIPriceUnit.PerRequest,
            null,
            AIPriceUnit.PerRequest,
            false);
    }
}
