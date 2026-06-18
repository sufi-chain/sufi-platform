// SufiAbp File Manager - JavaScript Interop (ES Module)
// This module provides file upload functionality that bypasses Blazor Server's SignalR circuit
// for better performance with large files.

/** Path for the versioned file-items API; must match FileItemController route. Built only here to avoid duplicate path in upload URL. */
const FILE_ITEMS_API_PATH = '/api/file-manager/file-items';

/**
 * Upload a file directly to the API using XMLHttpRequest (bypasses SignalR - no circuit timeout).
 * @param {File} file - The file to upload
 * @param {string} apiBaseUrl - API origin (scheme + host + port), or '' for same-origin
 * @param {object} metadata - Upload metadata (structureKey, entityType, etc.)
 * @param {string|null} accessToken - Bearer token for authentication (optional, falls back to cookies)
 * @param {object} dotNetRef - .NET reference for progress callbacks
 * @param {number|null} fileIndex - Optional index for multi-file progress (calls OnUploadProgress(fileIndex, progress))
 * @returns {Promise<object>} - The uploaded file result or error
 */
export async function uploadFile(file, apiBaseUrl, metadata, accessToken, dotNetRef, fileIndex = null) {
    // Validate inputs
    if (!file) {
        return { success: false, error: 'No file provided' };
    }
    // apiBaseUrl may be empty for same-origin (relative URL will be used)

    const formData = new FormData();
    formData.append('File', file);
    
    // Add metadata fields (sanitize string values)
    if (metadata.structureKey) formData.append('StructureKey', String(metadata.structureKey));
    if (metadata.entityType) formData.append('EntityType', String(metadata.entityType));
    if (metadata.entityId) formData.append('EntityId', String(metadata.entityId));
    if (metadata.folderPath) formData.append('FolderPath', String(metadata.folderPath));
    else if (metadata.folderId) formData.append('FolderId', String(metadata.folderId));
    formData.append('AutoConfirm', metadata.autoConfirm ? 'true' : 'false');
    if (metadata.alt) formData.append('Alt', String(metadata.alt));

    // apiBaseUrl is the API origin only (scheme + host + port) or '' for same-origin. Path is built here once.
    const origin = (apiBaseUrl && typeof apiBaseUrl === 'string') ? apiBaseUrl.replace(/\/+$/, '') : '';
    const url = origin ? `${origin}${FILE_ITEMS_API_PATH}/upload` : `${FILE_ITEMS_API_PATH}/upload`;
    const progressIndex = fileIndex !== undefined && fileIndex !== null ? fileIndex : 0;

    try {
        // Use XMLHttpRequest for progress tracking
        return await new Promise((resolve, reject) => {
            const xhr = new XMLHttpRequest();
            
            xhr.upload.addEventListener('progress', async (e) => {
                if (e.lengthComputable && dotNetRef) {
                    const progress = Math.round((e.loaded / e.total) * 100);
                    try {
                        await dotNetRef.invokeMethodAsync('OnUploadProgress', progressIndex, progress);
                    } catch (err) {
                        // Ignore callback errors (component may be disposed)
                    }
                }
            });

            xhr.addEventListener('load', () => {
                if (xhr.status >= 200 && xhr.status < 300) {
                    try {
                        const result = JSON.parse(xhr.responseText);
                        resolve({ success: true, data: result });
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
                        // Use default error message
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
            
            // Authentication: Prefer Bearer token, fall back to cookies
            if (accessToken) {
                xhr.setRequestHeader('Authorization', `Bearer ${accessToken}`);
            }
            
            // Include credentials for cookie-based auth (same-origin only)
            xhr.withCredentials = true;
            
            // Request ID for correlation (ABP pattern)
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            
            xhr.send(formData);
        });
    } catch (error) {
        return { success: false, error: error.message || 'Unknown error occurred' };
    }
}

/**
 * Handle file input change: read selected files, notify .NET, then upload each via HTTP (bypasses SignalR).
 * Prevents "Did not receive any data in the allotted time" by not sending file data over the circuit.
 * @param {string} inputId - ID of the file input element
 * @param {string} apiBaseUrl - API origin (scheme + host + port), or '' for same-origin
 * @param {object} metadata - Upload metadata
 * @param {string|null} accessToken - Bearer token (optional)
 * @param {object} dotNetRef - .NET reference (OnFilesSelected, OnUploadProgress, OnUploadComplete)
 */
export async function uploadFilesFromInput(inputId, apiBaseUrl, metadata, accessToken, dotNetRef) {
    const input = document.getElementById(inputId);
    if (!input || !input.files || input.files.length === 0) return;

    const files = Array.from(input.files);
    const fileInfos = files.map(f => ({ name: f.name, size: f.size }));

    try {
        await dotNetRef.invokeMethodAsync('OnFilesSelected', fileInfos);
    } catch (err) {
        return;
    }

    for (let i = 0; i < files.length; i++) {
        const result = await uploadFile(files[i], apiBaseUrl, metadata, accessToken, dotNetRef, i);
        try {
            await dotNetRef.invokeMethodAsync('OnUploadComplete', i, result);
        } catch (e) {
            // Component may be disposed
        }
    }

    input.value = '';
}

/**
 * Trigger file input click
 */
export function triggerFileInput(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.click();
    }
}

/**
 * Register change handler on file input so that when user selects files,
 * we call .NET OnFileInputChange (C# then drives HTTP upload via uploadFilesFromInput).
 * This avoids reading files in Blazor and prevents SignalR circuit timeout.
 */
export function registerFileInputChange(inputId, dotNetRef) {
    const input = document.getElementById(inputId);
    if (!input || !dotNetRef) return;
    input.addEventListener('change', async () => {
        if (!input.files || input.files.length === 0) return;
        try {
            await dotNetRef.invokeMethodAsync('OnFileInputChange');
        } catch (err) {
            input.value = '';
        }
    });
}

/**
 * Initialize drag and drop for a drop zone
 */
export function initializeDragDrop(dropZoneId, dotNetReference) {
    const dropZone = document.getElementById(dropZoneId);
    if (!dropZone) return;

    dropZone.addEventListener('dragenter', (e) => {
        e.preventDefault();
        e.stopPropagation();
        dotNetReference.invokeMethodAsync('OnDragEnter');
    });

    dropZone.addEventListener('dragleave', (e) => {
        e.preventDefault();
        e.stopPropagation();
        dotNetReference.invokeMethodAsync('OnDragLeave');
    });

    dropZone.addEventListener('dragover', (e) => {
        e.preventDefault();
        e.stopPropagation();
    });

    dropZone.addEventListener('drop', async (e) => {
        e.preventDefault();
        e.stopPropagation();
        await dotNetReference.invokeMethodAsync('OnDrop', e.dataTransfer.files.length);
    });
}

/**
 * Create image preview
 */
export function createImagePreview(imageData, elementId) {
    const element = document.getElementById(elementId);
    if (element && imageData) {
        element.src = 'data:image/png;base64,' + imageData;
    }
}

/**
 * Open lightbox with image
 */
export function openLightbox(imageUrl, title) {
    const overlay = document.createElement('div');
    overlay.className = 'sabp-fm-lightbox-overlay';
    overlay.innerHTML = `
        <div class="sabp-fm-lightbox-content">
            <span class="sabp-fm-lightbox-close">&times;</span>
            <img src="${imageUrl}" alt="${title}" class="sabp-fm-lightbox-image">
            <div class="sabp-fm-lightbox-caption">${title}</div>
        </div>
    `;

    document.body.appendChild(overlay);

    overlay.addEventListener('click', function (e) {
        if (e.target === overlay || e.target.className === 'sabp-fm-lightbox-close') {
            document.body.removeChild(overlay);
        }
    });

    document.addEventListener('keydown', function closeOnEsc(e) {
        if (e.key === 'Escape') {
            if (document.body.contains(overlay)) {
                document.body.removeChild(overlay);
            }
            document.removeEventListener('keydown', closeOnEsc);
        }
    });
}

/**
 * Download file
 */
export function downloadFile(fileName, contentBase64) {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = 'data:application/octet-stream;base64,' + contentBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

/**
 * Copy text to clipboard
 */
export async function copyToClipboard(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch {
        return false;
    }
}

/**
 * Initialize image zoom
 */
export function initializeImageZoom(imageElementId) {
    const imageElement = document.getElementById(imageElementId);
    if (!imageElement) return;

    let scale = 1;
    const maxScale = 3;
    const minScale = 0.5;

    imageElement.addEventListener('wheel', (e) => {
        e.preventDefault();
        const delta = e.deltaY > 0 ? -0.1 : 0.1;
        scale = Math.max(minScale, Math.min(maxScale, scale + delta));
        imageElement.style.transform = `scale(${scale})`;
    });

    imageElement.addEventListener('dblclick', () => {
        scale = scale === 1 ? 2 : 1;
        imageElement.style.transform = `scale(${scale})`;
    });
}

/**
 * Initialize video player
 */
export function initializeVideoPlayer(videoElementId, dotNetReference) {
    const videoElement = document.getElementById(videoElementId);
    if (!videoElement) return;

    videoElement.addEventListener('play', () => {
        dotNetReference.invokeMethodAsync('OnPlayStateChanged', true);
    });

    videoElement.addEventListener('pause', () => {
        dotNetReference.invokeMethodAsync('OnPlayStateChanged', false);
    });

    videoElement.addEventListener('timeupdate', () => {
        const progress = (videoElement.currentTime / videoElement.duration) * 100;
        dotNetReference.invokeMethodAsync('OnProgressChanged', progress);
    });

    videoElement.addEventListener('ended', () => {
        dotNetReference.invokeMethodAsync('OnVideoEnded');
    });
}

/**
 * Format file size
 */
export function formatFileSize(bytes) {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

/**
 * Validate image dimensions
 */
export function validateImageDimensions(file, minWidth, minHeight, maxWidth, maxHeight) {
    return new Promise((resolve) => {
        const img = new Image();
        const reader = new FileReader();

        reader.onload = function (e) {
            img.onload = function () {
                const valid = (!minWidth || img.width >= minWidth) &&
                    (!minHeight || img.height >= minHeight) &&
                    (!maxWidth || img.width <= maxWidth) &&
                    (!maxHeight || img.height <= maxHeight);

                resolve({
                    valid: valid,
                    width: img.width,
                    height: img.height
                });
            };
            img.src = e.target.result;
        };

        reader.readAsDataURL(file);
    });
}

/**
 * Get image data URL
 */
export function getImageDataUrl(file) {
    return new Promise((resolve) => {
        const reader = new FileReader();
        reader.onload = function (e) {
            resolve(e.target.result);
        };
        reader.readAsDataURL(file);
    });
}

/**
 * Scroll to element
 */
export function scrollToElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
}

/**
 * Show toast notification
 */
export function showToast(message, type = 'info', duration = 3000) {
    const toast = document.createElement('div');
    toast.className = `sabp-fm-toast sabp-fm-toast-${type}`;
    toast.textContent = message;

    document.body.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('sabp-fm-toast-show');
    }, 100);

    setTimeout(() => {
        toast.classList.remove('sabp-fm-toast-show');
        setTimeout(() => {
            if (document.body.contains(toast)) {
                document.body.removeChild(toast);
            }
        }, 300);
    }, duration);
}

/**
 * Initialize sortable
 */
export function initializeSortable(containerId, dotNetReference) {
    console.log('Sortable initialized for file manager');
}

/**
 * Dispose resources
 */
export function dispose(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        const clone = element.cloneNode(true);
        element.parentNode.replaceChild(clone, element);
    }
}

// Inject CSS styles for lightbox and toast
const style = document.createElement('style');
style.textContent = `
    .sabp-fm-lightbox-overlay {
        position: fixed;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        background-color: rgba(0, 0, 0, 0.9);
        z-index: 9999;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .sabp-fm-lightbox-content {
        position: relative;
        max-width: 90%;
        max-height: 90%;
    }

    .sabp-fm-lightbox-image {
        max-width: 100%;
        max-height: 80vh;
        object-fit: contain;
    }

    .sabp-fm-lightbox-close {
        position: absolute;
        top: -40px;
        right: 0;
        color: white;
        font-size: 35px;
        font-weight: bold;
        cursor: pointer;
    }

    .sabp-fm-lightbox-caption {
        text-align: center;
        color: white;
        padding: 10px;
        font-size: 16px;
    }

    .sabp-fm-toast {
        position: fixed;
        bottom: 20px;
        right: 20px;
        padding: 15px 25px;
        border-radius: 4px;
        color: white;
        font-size: 14px;
        z-index: 10000;
        opacity: 0;
        transform: translateY(20px);
        transition: all 0.3s ease;
    }

    .sabp-fm-toast-show {
        opacity: 1;
        transform: translateY(0);
    }

    .sabp-fm-toast-info {
        background-color: #17a2b8;
    }

    .sabp-fm-toast-success {
        background-color: #28a745;
    }

    .sabp-fm-toast-warning {
        background-color: #ffc107;
        color: #000;
    }

    .sabp-fm-toast-error {
        background-color: #dc3545;
    }
`;
document.head.appendChild(style);
