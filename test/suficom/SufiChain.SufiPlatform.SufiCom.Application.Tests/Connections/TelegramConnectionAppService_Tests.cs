using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Connections;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Connections;

public class TelegramConnectionAppService_Tests : SufiComTestBase<SufiComTelegramApplicationTestModule>
{
    private readonly ITelegramConnectionAppService _connectionAppService;
    private readonly ITelegramConnectionRepository _repository;

    public TelegramConnectionAppService_Tests()
    {
        _connectionAppService = GetRequiredService<ITelegramConnectionAppService>();
        _repository = GetRequiredService<ITelegramConnectionRepository>();
    }

    [Fact]
    public async Task Should_Create_Connection_Disconnected()
    {
        var dto = await CreateAsync();

        dto.AuthState.ShouldBe(TelegramAuthState.Disconnected);
        dto.IsEnabled.ShouldBeTrue();
        dto.IsDefaultOutbound.ShouldBeFalse();
    }

    [Fact]
    public async Task PrepareConnection_Should_Transition_To_WaitCode_Via_Fake_Gateway()
    {
        var dto = await CreateAsync();

        var prepared = await _connectionAppService.PrepareConnectionAsync(dto.Id);

        prepared.AuthState.ShouldBe(TelegramAuthState.WaitCode);
        prepared.ForeignSessionKey.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Full_Auth_Flow_Should_Reach_Ready()
    {
        var dto = await CreateAsync();

        await _connectionAppService.PrepareConnectionAsync(dto.Id);
        var ready = await _connectionAppService.SubmitCodeAsync(dto.Id, new SubmitTelegramCodeInput { Code = "12345" });

        ready.AuthState.ShouldBe(TelegramAuthState.Ready);
        ready.LastHealthAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task SetDefaultOutbound_Should_Clear_Other_Defaults()
    {
        var first = await CreateAsync(isDefault: true);
        var second = await CreateAsync(phone: "+989120000002", isDefault: true);

        var reloadedFirst = await _repository.GetAsync(first.Id);
        reloadedFirst.IsDefaultOutbound.ShouldBeFalse();
        second.IsDefaultOutbound.ShouldBeTrue();
    }

    [Fact]
    public async Task Disconnect_Should_Reset_To_Disconnected()
    {
        var dto = await CreateAsync();
        await _connectionAppService.PrepareConnectionAsync(dto.Id);
        await _connectionAppService.SubmitCodeAsync(dto.Id, new SubmitTelegramCodeInput { Code = "12345" });

        var disconnected = await _connectionAppService.DisconnectAsync(dto.Id);

        disconnected.AuthState.ShouldBe(TelegramAuthState.Disconnected);
        disconnected.ForeignSessionKey.ShouldBeNull();
    }

    [Fact]
    public async Task Get_Unknown_Should_Throw()
    {
        await Should.ThrowAsync<BusinessException>(() =>
            _connectionAppService.GetAsync(Guid.NewGuid()));
    }

    private async Task<TelegramConnectionDto> CreateAsync(
        string phone = "+989120000001",
        bool isDefault = false)
    {
        return await _connectionAppService.CreateAsync(new CreateTelegramConnectionInput
        {
            PhoneNumber = phone,
            DisplayName = "Support",
            RoleLabel = "Support",
            IsEnabled = true,
            IsDefaultOutbound = isDefault
        });
    }
}
