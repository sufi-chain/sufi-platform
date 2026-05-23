# File Manager Troubleshooting

Common issues for File-Manager with AI Management integration.

## Archiving does not run

- Confirm `FileManager.Archiving.Enabled` is `true`.
- Verify background worker registration in the host module.
- Check logs for `FileArchivingWorker` and `FileArchivingBackgroundJob`.
- Validate retention settings (`RetentionDays`, `BatchSize`).

## Files are not selected for archiving

- Ensure files are not temporary (`IsTemp = false`).
- Check file creation time is older than retention cutoff.
- Confirm directory/structure filters match the stored `BlobName` and `StructureKey`.
- Verify files are not already archived (`IsArchived = false`).

## AI files not archived with separate retention

- Confirm `FileManager.Archiving.ArchiveAIFiles = true`.
- Set `FileManager.Archiving.AIFilesRetentionDays` to a numeric value.
- Ensure AI files use expected structure/source tagging.

## File events not observed by consumers

- Confirm distributed event bus is configured in the host.
- Verify consumer module references File-Manager ETO contracts.
- Check handler registration and permission/tenant context.
- Inspect logs around file create/delete/archive operations.

## Archive/restore API fails

- Verify user has required permissions:
  - archive: `FileManager.FileItems.Delete`
  - restore: `FileManager.FileItems.Update`
  - archived list: `FileManager.FileItems.Default`
- Confirm the file exists and state is valid (not already archived for archive action, archived for restore action).

## Performance considerations

- Reduce `BatchSize` to lower DB/job pressure.
- Schedule worker during off-peak usage.
- Ensure indexes exist on archive/source columns.
- Monitor background job queue throughput and retries.
