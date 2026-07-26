// Public file uploader — HTTP multipart upload (bypasses Blazor Server SignalR circuit).
// Prevents "Did not receive any data in the allotted time" from BrowserFileStream.

const FILE_ITEMS_API_PATH = '/api/file-manager/file-items';

/**
 * Upload a file via XMLHttpRequest (bypasses SignalR).
 */
export async function uploadFile(file, apiBaseUrl, metadata, accessToken, dotNetRef, fileIndex = null) {
    if (!file) {
        return { success: false, error: 'No file provided' };
    }

    const formData = new FormData();
    formData.append('File', file);

    if (metadata.structureKey) formData.append('StructureKey', String(metadata.structureKey));
    if (metadata.entityType) formData.append('EntityType', String(metadata.entityType));
    if (metadata.entityId) formData.append('EntityId', String(metadata.entityId));
    if (metadata.folderPath) formData.append('FolderPath', String(metadata.folderPath));
    else if (metadata.folderId) formData.append('FolderId', String(metadata.folderId));
    formData.append('AutoConfirm', metadata.autoConfirm ? 'true' : 'false');
    if (metadata.alt) formData.append('Alt', String(metadata.alt));

    const origin = (apiBaseUrl && typeof apiBaseUrl === 'string') ? apiBaseUrl.replace(/\/+$/, '') : '';
    const url = origin ? `${origin}${FILE_ITEMS_API_PATH}/upload` : `${FILE_ITEMS_API_PATH}/upload`;
    const progressIndex = fileIndex !== undefined && fileIndex !== null ? fileIndex : 0;

    try {
        return await new Promise((resolve) => {
            const xhr = new XMLHttpRequest();

            xhr.upload.addEventListener('progress', async (e) => {
                if (e.lengthComputable && dotNetRef) {
                    const progress = Math.round((e.loaded / e.total) * 100);
                    try {
                        await dotNetRef.invokeMethodAsync('OnUploadProgress', progressIndex, progress);
                    } catch {
                        // Component may be disposed
                    }
                }
            });

            xhr.addEventListener('load', () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    try {
                        resolve({ success: true, data: JSON.parse(xhr.responseText) });
                    } catch {
                        resolve({ success: true, data: xhr.responseText });
                    }
                } else if (xhr.status === 401) {
                    resolve({ success: false, error: 'Authentication required. Please log in again.' });
                } else {
                    let errorMessage = `Upload failed with status ${xhr.status}`;
                    try {
                        const errorResponse = JSON.parse(xhr.responseText);
                        errorMessage = errorResponse.error?.message || errorResponse.message || errorMessage;
                    } catch {
                        // keep default
                    }
                    if (xhr.status === 403 && errorMessage === `Upload failed with status ${xhr.status}`) {
                        errorMessage = 'You do not have permission to upload files.';
                    }
                    resolve({ success: false, error: errorMessage });
                }
            });

            xhr.addEventListener('error', () => {
                resolve({ success: false, error: 'Network error occurred during upload' });
            });

            xhr.addEventListener('abort', () => {
                resolve({ success: false, error: 'Upload was cancelled' });
            });

            xhr.open('POST', url);
            if (accessToken) {
                xhr.setRequestHeader('Authorization', `Bearer ${accessToken}`);
            }
            xhr.withCredentials = true;
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            xhr.send(formData);
        });
    } catch (error) {
        return { success: false, error: error.message || 'Unknown error occurred' };
    }
}

/**
 * Read selected files, notify .NET, upload each via HTTP.
 */
export async function uploadFilesFromInput(inputId, apiBaseUrl, metadata, accessToken, dotNetRef) {
    const input = document.getElementById(inputId);
    if (!input || !input.files || input.files.length === 0) return;

    const files = Array.from(input.files);
    const fileInfos = files.map(f => ({ name: f.name, size: f.size, type: f.type || '' }));

    try {
        await dotNetRef.invokeMethodAsync('OnFilesSelected', fileInfos);
    } catch {
        input.value = '';
        return;
    }

    const maxFileSize = metadata && metadata.maxFileSize ? Number(metadata.maxFileSize) : 0;

    for (let i = 0; i < files.length; i++) {
        if (maxFileSize > 0 && files[i].size > maxFileSize) {
            try {
                await dotNetRef.invokeMethodAsync('OnUploadComplete', i, {
                    success: false,
                    error: `File exceeds the maximum size.`
                });
            } catch {
                // Component may be disposed
            }
            continue;
        }

        const result = await uploadFile(files[i], apiBaseUrl, metadata, accessToken, dotNetRef, i);
        try {
            await dotNetRef.invokeMethodAsync('OnUploadComplete', i, result);
        } catch {
            // Component may be disposed
        }
    }

    input.value = '';
}

export function triggerFileInput(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.click();
    }
}

/**
 * When the user selects files, call .NET OnFileInputChange so C# can start HTTP upload.
 */
export function registerFileInputChange(inputId, dotNetRef) {
    const input = document.getElementById(inputId);
    if (!input || !dotNetRef) return;
    input.addEventListener('change', async () => {
        if (!input.files || input.files.length === 0) return;
        try {
            await dotNetRef.invokeMethodAsync('OnFileInputChange');
        } catch {
            input.value = '';
        }
    });
}
