# Background Jobs Usage

## Typical operator workflows

- open the background jobs management page
- inspect current or failed jobs
- retry jobs when appropriate
- delete stale jobs when cleanup is needed

## UI in source

The Blazor package contains `BackgroundJobsManagement`, which is the main administrative entry point for the module.

## Why it matters

Background Jobs is an operational module that helps teams keep asynchronous processing manageable from the platform UI.
