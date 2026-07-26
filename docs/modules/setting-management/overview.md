# Setting Management Overview

The Setting Management module gives Sufi Platform a centralized UI and extensibility model for configuration. It is the module to inspect when a team wants operators to manage settings in a standard screen instead of scattering configuration forms throughout a host application.

## What it enables

- centralized settings management
- grouped setting contributors
- email settings management
- time-zone settings management

## How it fits the platform

This module is part of the baseline configuration story. It often works together with Feature Management and Tenant Management so hosts can control behavior from one consistent administration surface.

## Where to start in source

Open these packages first:

- `SufiChain.SufiPlatform.Settings.Blazor` for the `SettingsManagement` page and group components
- `SufiChain.SufiPlatform.Settings.Application.Contracts` for settings DTOs and contracts
- `SufiChain.SufiPlatform.Settings.HttpApi` for the remote surface used by hosts and clients
