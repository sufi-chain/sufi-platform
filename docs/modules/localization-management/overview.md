# Localization Management Overview

The Localization Management module gives the platform a runtime administration surface for translation data. It is the module to use when a team wants to review resources, edit localized texts, or support an operational translation workflow without relying only on static files in source control.

## What it enables

- inspect localization resources
- manage localization texts
- import and export localization data
- support a platform-managed translation workflow

## How it fits the platform

This module is part of the configuration and operations toolset. It complements the platform-wide localization rules by giving operators a managed UI for translation content.

## Where to start in source

Open these packages first:

- `SufiChain.SufiPlatform.LocalizationManagement.Blazor` for the resource and text management pages
- `SufiChain.SufiPlatform.LocalizationManagement.Application` and `.Application.Contracts` for service workflows and contracts
- `SufiChain.SufiPlatform.LocalizationManagement.Domain`, `.EntityFrameworkCore`, and `.MongoDB` for persisted localization data
