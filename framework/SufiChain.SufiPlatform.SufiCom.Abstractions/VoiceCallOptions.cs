namespace SufiChain.SufiPlatform.SufiCom.VoiceCall;

/// <summary>
/// Options for voice call configuration
/// </summary>
public class VoiceCallOptions
{
    /// <summary>
    /// Voice gender (Male, Female, Neutral)
    /// </summary>
    public string? VoiceGender { get; set; }
    
    /// <summary>
    /// Language code (e.g., "en-US", "ar-SA")
    /// </summary>
    public string? Language { get; set; }
    
    /// <summary>
    /// Speech rate (0.5 to 2.0, default 1.0)
    /// </summary>
    public double SpeechRate { get; set; } = 1.0;
    
    /// <summary>
    /// Number of times to repeat the message
    /// </summary>
    public int RepeatCount { get; set; } = 1;
    
    /// <summary>
    /// Whether to wait for user input (DTMF)
    /// </summary>
    public bool WaitForInput { get; set; }
    
    /// <summary>
    /// Timeout in seconds for user input
    /// </summary>
    public int InputTimeoutSeconds { get; set; } = 10;
    
    /// <summary>
    /// Custom voice name (provider-specific)
    /// </summary>
    public string? VoiceName { get; set; }
}
