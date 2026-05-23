# Account Overview

The Account module provides the user-facing entry points for authentication and personal account management in a Sufi Platform host. It is the module you extend when the product needs to shape how users sign in, register, update a password, or manage profile details without moving that responsibility into the administrative Identity module.

## What it covers

This module handles the flows a normal end user sees first:

- sign in
- registration
- profile access
- password change

## How it fits the platform

Use Account together with Identity. Account handles the self-service experience for the signed-in or signing-in user, while Identity handles operator-facing administration of users, roles, and organization units.

## Where to start in source

Open these packages first:

- `SufiChain.SufiAbp.Account.Blazor` for the user-facing pages such as `Login`, `Register`, `Profile`, and `ChangePassword`
- `SufiChain.SufiAbp.Account.Application.Contracts` for the DTOs and service contracts used by the UI
- `SufiChain.SufiAbp.Account.HttpApi` and `SufiChain.SufiAbp.Account.HttpApi.Client` for the remote surface used by hosts and front ends
