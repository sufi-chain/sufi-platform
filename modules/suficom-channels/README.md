# SufiCom channel providers (OSS)

Open-source SMS and voice OTP providers for Sufi Platform.

## What is included

| Package | Channel / provider code | Role |
| --- | --- | --- |
| `SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar` | `Sms.Kavenegar` / `Kavenegar` | SMS + transactional OTP (Verify Lookup) |
| `SufiChain.SufiPlatform.SufiCom.Channels.Voice.Kavenegar` | `Voice.Kavenegar` / `Kavenegar` | Voice OTP |
| `SufiChain.SufiPlatform.SufiCom.Channels.Sms.IdehPardazan` | `Sms.IdehPardazan` / `SmsIr` | SMS.ir (IdehPardazan) verify OTP |

License: LGPL-3.0-only. Placeholders only in samples — never commit real API keys.

## Install

CLI ModuleRegistry entry display name: **SMS/OTP providers (Kavenegar, IdehPardazan)**.

Depends on the host module types:

- `SufiComChannelsSmsKavenegarModule`
- `SufiComChannelsSmsIdehPardazanModule`
- `SufiComChannelsVoiceKavenegarModule`

Compose those modules (or install via the CLI module catalog) into the Blazor WebApp host. Package ids match the table above.

## Settings fields

Common OTP setting keys (see `OtpProviderSettingKeys` in SufiCom.Abstractions):

| Key | Purpose |
| --- | --- |
| `ApiKey` | Provider API credential |
| `SenderNumber` | Default sender line |
| `OtpTemplate` | Default OTP template (Kavenegar name, or SMS.ir numeric template id) |
| `OtpTemplateLogin` / `OtpTemplateRegistration` / … | Optional per-purpose templates |
| `OtpCodeParameterName` | SMS.ir verify parameter name (default `Code`) |
| `OtpSenderNumber` | Optional OTP-specific sender line |
| `TimeoutSeconds` | HTTP timeout for the OTP request |

Confirm exact labels in each provider’s `GetRequiredSettingsAsync` implementation.

## Phone normalization

Iranian mobiles are normalized before send (`09…` → national / `+98…`). See issue #379 and `IranianMobileNumber` in SufiCom.Abstractions.

## Commercial SMS OTP module

A private commercial SMS OTP provider lives under `commercial-modules` (separate package id and install). OSS hosts should configure login OTP to **Kavenegar** or **IdehPardazan**. If a saved setting still names an uninstalled commercial provider, OTP send fails softly with an admin warning (no exception to the login caller).

## Validation

```bash
bash checks/no-pro-references.sh
dotnet test test/suficom-channels/SufiChain.SufiPlatform.SufiCom.Channels.Tests -c Release
```
