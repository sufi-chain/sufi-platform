# File Structure

A **File Structure** is a named configuration that defines validation and processing rules for file uploads in the File Manager module.

## Why File Structures?

Different upload scenarios need different rules:

- **General storage** – Mixed content (images, videos, documents, audio) with permissive settings
- **Profile pictures** – Images only, smaller max size, single file
- **Public attachments** – Documents for download, specific extensions

File Structures enable:

- Consistent validation (allowed types, extensions, MIME types, max size)
- Per-structure processing (thumbnails, WebP conversion, image resizing)
- Clear separation of use cases without duplicating validation logic

## Well-Known Keys

From `FileStructureKeys` in configuration:

| Key | Purpose |
|-----|---------|
| `General` | General-purpose file storage |
| `ProfilePicture` | Profile pictures / avatars |
| `PublicAttachment` | Public attachments (documents) |

Structures are configured via `FileManagerOptions` or in the admin **File Structures** page (`/admin/file-manager/structures`).

## Components Using StructureKey

| Component | Parameter | Description |
|-----------|-----------|-------------|
| `FileUploader` | `StructureKey` | Target structure for uploads |
| `QuickImageUploader` | `StructureKey` | Target structure for image uploads |
| `FileBrowser` | `StructureKey` | Filter browseable files by structure |
| `FileManager` | `StructureKey` | Filter toolbar via structure dropdown |
| `FileSelector` | `StructureKey` | Initial filter when modal opens |

When `ShowStructureSelector` is true on upload components, users can choose the structure from a dropdown.

## Structure Configuration

Each structure defines:

- **Allowed file types** – Image, Video, Document, Audio (flags)
- **Allowed extensions** – e.g. jpg, png, pdf
- **Allowed MIME types** – e.g. image/jpeg, application/pdf
- **Max file size** – Per-file limit
- **Image dimensions** – Optional min/max width and height
- **Single vs multiple** – Single file or multiple (with optional max count)
- **Processing** – Thumbnail generation, WebP conversion, resize large images
- **Storage** – Custom provider, public access

Configure via module options or the admin File Structures CRUD page.
