using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.UI.Blazor.ExceptionHandling;
using SufiChain.SufiPlatform.UI.ExceptionHandling;
using SufiChain.SufiPlatform.UI.Localization;
using SufiChain.SufiPlatform.UI.Messages;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Http;
using Volo.Abp.Localization.ExceptionHandling;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class DefaultUserExceptionInformerTests
{
    private readonly IUiMessageService _messages = Substitute.For<IUiMessageService>();
    private readonly DictionaryLocalizer _localizer = new();
    private readonly CaptureLogger _logger = new();

    [Fact]
    public void Authorization_exception_maps_to_the_unauthorized_key()
    {
        var message = CreateInformer().GetLocalizedMessage(new AbpAuthorizationException());

        message.ShouldBe("KEY:Unauthorized");
        message.ShouldNotContain("AbpAuthorizationException");
        message.ShouldNotContain("Exception of type");
    }

    [Fact]
    public void Entity_not_found_maps_to_the_entity_not_found_key()
    {
        var message = CreateInformer()
            .GetLocalizedMessage(new EntityNotFoundException(typeof(string), Guid.Empty));

        message.ShouldBe("KEY:EntityNotFound");
        message.ShouldNotContain("There is no such an entity");
        message.ShouldNotContain(Guid.Empty.ToString());
    }

    [Fact]
    public async Task Validation_exception_includes_the_validation_error_list()
    {
        var exception = new Volo.Abp.Validation.AbpValidationException(
            "invalid",
            new List<ValidationResult> { new("Name is required", ["Name"]) });

        var informer = CreateInformer();
        informer.GetLocalizedMessage(exception).ShouldBe("KEY:PleaseCorrectErrors");

        await informer.InformAsync(new UserExceptionInformerContext(exception));

        await _messages.Received(1).ErrorAsync(
            Arg.Is<string>(body => body.Contains("Name is required", StringComparison.Ordinal)),
            Arg.Is<string?>(title => title == "KEY:ValidationError"));
    }

    [Fact]
    public void User_friendly_exception_keeps_its_message()
    {
        var message = CreateInformer()
            .GetLocalizedMessage(new UserFriendlyException("Already published."));

        message.ShouldBe("Already published.");
    }

    [Fact]
    public void Mapped_business_exception_code_uses_the_localized_resource_value()
    {
        var factory = Substitute.For<IStringLocalizerFactory>();
        var resourceLocalizer = Substitute.For<IStringLocalizer>();
        resourceLocalizer["Demo:KnownRule"]
            .Returns(new LocalizedString("Demo:KnownRule", "Rule blocked this action.", resourceNotFound: false));
        factory.Create(typeof(DemoResource)).Returns(resourceLocalizer);

        var options = Options.Create(new AbpExceptionLocalizationOptions());
        options.Value.MapCodeNamespace("Demo", typeof(DemoResource));

        var informer = new DefaultUserExceptionInformer(
            _messages,
            _localizer,
            factory,
            options,
            exceptionToErrorInfoConverter: null,
            _logger);

        informer.GetLocalizedMessage(new BusinessException("Demo:KnownRule"))
            .ShouldBe("Rule blocked this action.");
    }

    [Fact]
    public void Unmapped_business_code_never_produces_english_words_from_the_key()
    {
        var message = CreateInformer()
            .GetLocalizedMessage(new BusinessException("KnowledgeBase:ArticleHooshvareFeatureDisabled"));

        message.ShouldBe("KEY:AnErrorOccurred");
        message.ShouldNotContain("Article");
        message.ShouldNotContain("Hooshvare");
        message.ShouldNotContain("Feature Disabled");
        message.ShouldNotContain("ArticleHooshvareFeatureDisabled");
        _logger.Messages.ShouldContain(m => m.Contains("KnowledgeBase:ArticleHooshvareFeatureDisabled"));
    }

    [Fact]
    public void Empty_business_exception_maps_to_a_generic_localized_message()
    {
        var message = CreateInformer().GetLocalizedMessage(new BusinessException());

        message.ShouldBe("KEY:AnErrorOccurred");
        message.ShouldNotContain("Exception of type");
    }

    [Fact]
    public void Generic_exception_never_shows_the_raw_message_and_includes_a_correlation_id()
    {
        const string raw = "NullReferenceException at WorkspaceEditor line 12";
        var message = CreateInformer().GetLocalizedMessage(new InvalidOperationException(raw));

        message.ShouldStartWith("KEY:DefaultErrorMessageWithReference:");
        message.ShouldNotContain(raw);
        message.ShouldNotContain("NullReference");
        _logger.Messages.ShouldContain(m => m.Contains("CorrelationId="));
    }

    [Fact]
    public void Converter_path_still_overrides_authorization_to_the_localized_key()
    {
        var converter = Substitute.For<IExceptionToErrorInfoConverter>();
        converter.Convert(Arg.Any<Exception>(), Arg.Any<Action<AbpExceptionHandlingOptions>?>())
            .Returns(new RemoteServiceErrorInfo("Exception of type 'Volo.Abp.Authorization.AbpAuthorizationException' was thrown."));

        var message = CreateInformer(converter)
            .GetLocalizedMessage(new AbpAuthorizationException());

        message.ShouldBe("KEY:Unauthorized");
    }

    [Fact]
    public async Task InformAsync_presents_the_localized_message_not_the_raw_exception()
    {
        await CreateInformer().InformAsync(
            new UserExceptionInformerContext(new InvalidOperationException("secret stack detail")));

        await _messages.Received(1).ErrorAsync(
            Arg.Is<string>(m => m.StartsWith("KEY:DefaultErrorMessageWithReference:", StringComparison.Ordinal)
                                && !m.Contains("secret stack detail", StringComparison.Ordinal)),
            Arg.Any<string?>());
    }

    private DefaultUserExceptionInformer CreateInformer(
        IExceptionToErrorInfoConverter? converter = null)
    {
        return new DefaultUserExceptionInformer(
            _messages,
            _localizer,
            stringLocalizerFactory: null,
            exceptionLocalizationOptions: null,
            exceptionToErrorInfoConverter: converter,
            logger: _logger);
    }

    private sealed class DictionaryLocalizer : IStringLocalizer<SufiFrameworkResource>
    {
        public LocalizedString this[string name] => new(name, "KEY:" + name, resourceNotFound: false);

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, "KEY:" + name + ":" + string.Join(":", arguments.Select(a => Convert.ToString(a, CultureInfo.InvariantCulture))), resourceNotFound: false);

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];
    }

    private sealed class CaptureLogger : ILogger<DefaultUserExceptionInformer>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }

    private sealed class DemoResource;
}
