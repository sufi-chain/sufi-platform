---
title: SufiChain.SufiPlatform.SufiCom.Channels.Tests
type: project-readme
area: platform
status: active
source_path: "sufi-platform/test/suficom-channels/SufiChain.SufiPlatform.SufiCom.Channels.Tests/SufiChain.SufiPlatform.SufiCom.Channels.Tests.csproj"
tags:
  - project
  - ai-kb
  - kb/area/platform
---

# SufiChain.SufiPlatform.SufiCom.Channels.Tests

Public (OSS) channel tests for Kavenegar SMS + Voice and IdehPardazan (SMS.ir).
Project references stay inside `sufi-platform` only (Abstractionsions + the three OSS providers).

The private commercial SMS OTP provider lives under `commercial-modules/sufi-channels-*`.
Email connector tests live in
`pro-modules/suficom/channels/test/SufiChain.SufiPlatform.SufiCom.Channels.Email.Tests`.

## Project metadata

| Field | Value |
| --- | --- |
| Project file | `sufi-platform/test/suficom-channels/SufiChain.SufiPlatform.SufiCom.Channels.Tests/SufiChain.SufiPlatform.SufiCom.Channels.Tests.csproj` |
| Target framework(s) | `net10.0` |
| Project references | Abstractions; Sms.Kavenegar; Sms.IdehPardazan; Voice.Kavenegar |

## No-pro guard

Run from the `sufi-platform` root (or any cwd):

```bash
bash checks/no-pro-references.sh
```

There is no checks/ job in `.github/workflows` yet (only `publish-nuget.yml`), so this script is the
local/CI gate for the OSS OTP surface until a workflow step is added.

## Validation

```bash
dotnet test test/suficom-channels/SufiChain.SufiPlatform.SufiCom.Channels.Tests -c Release
bash checks/no-pro-references.sh
```
