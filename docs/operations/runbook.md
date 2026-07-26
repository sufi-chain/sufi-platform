# Operational Runbook

## Monitoring

- Expose ASP.NET Core health checks (`/health`, `/health/ready`) for database and critical dependencies
- Prefer structured Serilog logging; optionally export OpenTelemetry metrics for request duration and error rate
- Watch Blazor circuit count and memory when hosting Blazor Server

### Useful alerts

- Database connection pool exhaustion
- Blob/S3 upload failures
- Token issuance / OIDC refresh failures
- High Blazor circuit count (memory pressure)

## Troubleshooting

| Issue | Symptom | Resolution |
|-------|---------|------------|
| Module not loading | Missing menus or blank pages | Check `DependsOn` / module registration and NuGet restore |
| Localization missing | Raw keys in UI | Verify resource JSON and culture configuration |
| Blazor circuit crash | “Connection lost” | Guard JS interop with interactivity checks; inspect circuit logs |
| S3 upload fails | 403/400 on upload | Credentials, bucket policy, region, and endpoint |
| Auth redirect loop | Endless login | Client secrets, redirect URIs, and clock skew |

Module-specific notes:

- [AI Troubleshooting](../modules/ai/troubleshooting.md)
- [File Manager Troubleshooting](../modules/file-manager/troubleshooting.md)

## Backup and recovery

- **Relational DB:** regular dumps plus PITR where available
- **MongoDB:** dumps and/or replica-set backups
- **Blob storage:** enable object versioning / cross-region replication as required
- Settings stored in the database are covered by database backups

## Rollback

1. Identify the failed deployment version
2. Shift traffic to the previous healthy revision
3. Confirm health checks
4. Investigate from CI/CD and structured logs

## Maintenance cadence

- Review NuGet updates against `versions.props`
- Align ABP upgrades with the pinned ABP minor line (currently 10.3.0)
- Rotate TLS certificates with overlap so hosts stay available

## Related

- [Deployment](deployment.md)
- [Security](security.md)
- [Technology stack](../reference/technology-stack.md)
