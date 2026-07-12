# Background Jobs Overview

The Background Jobs module gives a Sufi Platform host an operational UI around asynchronous work. It is the module to use when support or operations teams must inspect job status, retry a failed job, or remove jobs that are no longer relevant.

## What it enables

- browse background jobs
- inspect operational job state
- retry jobs when the workflow allows it
- delete jobs that are stale or no longer useful

## How it fits the platform

This module is part of the baseline operations story. It keeps background processing visible and manageable instead of leaving job behavior buried in logs or provider-specific tooling.

## Where to start in source

Open these packages first:

- `SufiChain.SufiPlatform.BackgroundJobs.Blazor` for the `BackgroundJobsManagement` page
- `SufiChain.SufiPlatform.BackgroundJobs.Application.Contracts` for DTOs and permission definitions
- `SufiChain.SufiPlatform.BackgroundJobs.HttpApi` for the public management API
