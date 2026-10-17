# ارائه‌دهندگان کانال SufiCom (متن‌باز)

ارائه‌دهندگان پیامک و OTP صوتی متن‌باز برای Sufi Platform.

## چه چیزی متن‌باز است

- **Kavenegar** — پیامک و OTP تراکنشی (Verify Lookup) و تماس صوتی
- **IdehPardazan (SMS.ir)** — OTP از مسیر verify

مجوز: LGPL-3.0-only. در نمونه‌ها فقط placeholder؛ کلید واقعی commit نشود.

## نصب

ورودی ModuleRegistry با نام نمایشی **SMS/OTP providers (Kavenegar, IdehPardazan)** را از CLI نصب کنید، یا ماژول‌های زیر را در میزبان Blazor compose کنید:

- `SufiComChannelsSmsKavenegarModule`
- `SufiComChannelsSmsIdehPardazanModule`
- `SufiComChannelsVoiceKavenegarModule`

## تنظیمات

فیلدهای رایج: `ApiKey`، `SenderNumber`، `OtpTemplate`، `OtpCodeParameterName`، `TimeoutSeconds` (جزئیات در کد هر ارائه‌دهنده).

## نرمال‌سازی شماره

شماره موبایل ایران قبل از ارسال نرمال می‌شود (`09…` → `+98…`).

## ماژول تجاری

ارائه‌دهنده OTP تجاری جدا در `commercial-modules` است. برای نصب فقط OSS، OTP ورود را روی Kavenegar یا IdehPardazan بگذارید.
