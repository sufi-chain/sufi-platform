// SufiChain Media Management - JavaScript Interop

window.sufiChainSpFileManager = {
    // File input trigger
    triggerFileInput: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.click();
        }
    },

    // Drag and drop handlers
    initializeDragDrop: function (dropZoneElement, dotNetReference) {
        if (!dropZoneElement) return;

        dropZoneElement.addEventListener('dragenter', (e) => {
            e.preventDefault();
            e.stopPropagation();
            dotNetReference.invokeMethodAsync('OnDragEnter');
        });

        dropZoneElement.addEventListener('dragleave', (e) => {
            e.preventDefault();
            e.stopPropagation();
            dotNetReference.invokeMethodAsync('OnDragLeave');
        });

        dropZoneElement.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.stopPropagation();
        });

        dropZoneElement.addEventListener('drop', async (e) => {
            e.preventDefault();
            e.stopPropagation();
            
            const files = Array.from(e.dataTransfer.files);
            await dotNetReference.invokeMethodAsync('OnDrop', files.length);
        });
    },

    // Image preview
    createImagePreview: function (imageData, elementId) {
        const element = document.getElementById(elementId);
        if (element && imageData) {
            element.src = 'data:image/png;base64,' + imageData;
        }
    },

    // Lightbox functionality
    openLightbox: function (imageUrl, title) {
        // Create lightbox overlay
        const overlay = document.createElement('div');
        overlay.className = 'sufichain-lightbox-overlay';
        overlay.innerHTML = `
            <div class="sufichain-lightbox-content">
                <span class="sufichain-lightbox-close">&times;</span>
                <img src="${imageUrl}" alt="${title}" class="sufichain-lightbox-image">
                <div class="sufichain-lightbox-caption">${title}</div>
            </div>
        `;

        document.body.appendChild(overlay);

        // Close on click
        overlay.addEventListener('click', function (e) {
            if (e.target === overlay || e.target.className === 'sufichain-lightbox-close') {
                document.body.removeChild(overlay);
            }
        });

        // Close on ESC key
        document.addEventListener('keydown', function closeOnEsc(e) {
            if (e.key === 'Escape') {
                if (document.body.contains(overlay)) {
                    document.body.removeChild(overlay);
                }
                document.removeEventListener('keydown', closeOnEsc);
            }
        });
    },

    // Download file
    downloadFile: function (fileName, contentBase64) {
        const link = document.createElement('a');
        link.download = fileName;
        link.href = 'data:application/octet-stream;base64,' + contentBase64;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },

    // Copy to clipboard
    copyToClipboard: function (text) {
        return navigator.clipboard.writeText(text).then(() => {
            return true;
        }).catch(() => {
            return false;
        });
    },

    // Image zoom
    initializeImageZoom: function (imageElement) {
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
    },

    // Video player controls
    initializeVideoPlayer: function (videoElement, dotNetReference) {
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
    },

    // File size formatter
    formatFileSize: function (bytes) {
        if (bytes === 0) return '0 B';
        const k = 1024;
        const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
    },

    // Validate image dimensions
    validateImageDimensions: async function (file, minWidth, minHeight, maxWidth, maxHeight) {
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
    },

    // Get image data URL
    getImageDataUrl: function (file) {
        return new Promise((resolve) => {
            const reader = new FileReader();
            reader.onload = function (e) {
                resolve(e.target.result);
            };
            reader.readAsDataURL(file);
        });
    },

    // Scroll to element
    scrollToElement: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    },

    // Show toast notification
    showToast: function (message, type = 'info', duration = 3000) {
        const toast = document.createElement('div');
        toast.className = `sufichain-toast sufichain-toast-${type}`;
        toast.textContent = message;
        
        document.body.appendChild(toast);

        setTimeout(() => {
            toast.classList.add('sufichain-toast-show');
        }, 100);

        setTimeout(() => {
            toast.classList.remove('sufichain-toast-show');
            setTimeout(() => {
                document.body.removeChild(toast);
            }, 300);
        }, duration);
    },

    // Initialize sortable
    initializeSortable: function (containerElement, dotNetReference) {
        // This would integrate with a library like Sortable.js
        // For now, just a placeholder
        console.log('Sortable initialized for media gallery');
    },

    // Dispose resources
    dispose: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            // Remove all event listeners
            const clone = element.cloneNode(true);
            element.parentNode.replaceChild(clone, element);
        }
    }
};

// CSS for lightbox and toast
const style = document.createElement('style');
style.textContent = `
    .sufichain-lightbox-overlay {
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

    .sufichain-lightbox-content {
        position: relative;
        max-width: 90%;
        max-height: 90%;
    }

    .sufichain-lightbox-image {
        max-width: 100%;
        max-height: 80vh;
        object-fit: contain;
    }

    .sufichain-lightbox-close {
        position: absolute;
        top: -40px;
        right: 0;
        color: white;
        font-size: 35px;
        font-weight: bold;
        cursor: pointer;
    }

    .sufichain-lightbox-caption {
        text-align: center;
        color: white;
        padding: 10px;
        font-size: 16px;
    }

    .sufichain-toast {
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

    .sufichain-toast-show {
        opacity: 1;
        transform: translateY(0);
    }

    .sufichain-toast-info {
        background-color: #17a2b8;
    }

    .sufichain-toast-success {
        background-color: #28a745;
    }

    .sufichain-toast-warning {
        background-color: #ffc107;
        color: #000;
    }

    .sufichain-toast-error {
        background-color: #dc3545;
    }
`;
document.head.appendChild(style);

