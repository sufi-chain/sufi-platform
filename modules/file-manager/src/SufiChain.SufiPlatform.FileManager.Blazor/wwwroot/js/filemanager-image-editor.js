/**
 * File Manager Image Editor JavaScript Module
 * Built on Cropper.js + Canvas 2D for filters
 * All editing happens in browser; backend only receives final bytes on Save.
 */

const editors = new Map();
let editorIdCounter = 0;

let cropperLoaded = false;
let cropperLoadPromise = null;

function getContentBasePath() {
    const scripts = document.querySelectorAll('script[src*="filemanager-image-editor.js"]');
    if (scripts.length > 0) {
        const src = scripts[0].src;
        return src.substring(0, src.lastIndexOf('/'));
    }
    return '_content/SufiChain.SufiAbp.FileManager.Blazor';
}

function loadScript(src) {
    return new Promise((resolve) => {
        const script = document.createElement('script');
        script.src = src;
        script.onload = () => resolve(true);
        script.onerror = () => resolve(false);
        document.head.appendChild(script);
    });
}

function loadCss(href) {
    if (!document.querySelector(`link[href="${href}"]`)) {
        const link = document.createElement('link');
        link.rel = 'stylesheet';
        link.href = href;
        document.head.appendChild(link);
    }
}

/**
 * @param {string} [scriptUrl] - Optional script URL from bundle manager
 * @param {string} [styleUrl] - Optional stylesheet URL from bundle manager
 */
async function ensureCropperLoaded(scriptUrl, styleUrl) {
    if (typeof Cropper !== 'undefined') {
        cropperLoaded = true;
        return true;
    }
    if (cropperLoadPromise) {
        return cropperLoadPromise;
    }
    cropperLoadPromise = (async () => {
        const basePath = getContentBasePath();
        const jsUrl = scriptUrl || `${basePath}/vendor/cropper.min.js`;
        const cssUrl = styleUrl || `${basePath}/vendor/cropper.min.css`;
        loadCss(cssUrl);
        const loaded = await loadScript(jsUrl);
        if (loaded && typeof Cropper !== 'undefined') {
            cropperLoaded = true;
            return true;
        }
        console.error('FileManager: Failed to load Cropper.js. Ensure vendor/cropper.min.js and cropper.min.css exist in wwwroot.');
        return false;
    })();
    return cropperLoadPromise;
}

function getEditor(editorId) {
    const entry = editors.get(editorId);
    if (!entry) throw new Error(`Editor ${editorId} not found`);
    return entry;
}

/** Build CSS filter string from entry.filters for live preview (approximates canvas pipeline). */
function buildFilterCss(filters) {
    if (!filters) return 'none';
    const b = (filters.brightness || 0) / 100;
    const c = (filters.contrast || 0) / 100;
    const s = (filters.saturation || 0) / 100;
    const parts = [];
    parts.push(`brightness(${1 + b})`);
    parts.push(`contrast(${1 + c})`);
    parts.push(`saturate(${1 + s})`);
    if (filters.grayscale) parts.push('grayscale(1)');
    if (filters.sepia) parts.push('sepia(1)');
    return parts.length ? parts.join(' ') : 'none';
}

/** Apply current filters as CSS so the user sees a live preview on the FULL image.
 *  Must target .cropper-canvas (the image wrapper) so filters apply to the visible area
 *  inside the crop box. Applying to the raw img can affect the wrong layer in Cropper's DOM.
 */
function updateFilterPreview(editorId) {
    const entry = editors.get(editorId);
    if (!entry?.cropper) return;
    const container = entry.cropper.container;
    if (!container) return;
    const canvas = container.querySelector('.cropper-canvas');
    if (canvas) {
        canvas.style.filter = buildFilterCss(entry.filters);
    } else {
        const img = entry.cropper.image ?? entry.cropper.element ?? container.querySelector('img');
        if (img) img.style.filter = buildFilterCss(entry.filters);
    }
}

function pushHistory(entry) {
    if (!entry.cropper) return;
    try {
        const data = entry.cropper.getData();
        const containerData = entry.cropper.getContainerData();
        const snapshot = {
            data,
            rotate: entry.rotate || 0,
            scaleX: entry.scaleX ?? 1,
            scaleY: entry.scaleY ?? 1,
            filters: { ...entry.filters }
        };
        entry.undoStack = entry.undoStack || [];
        entry.undoStack.push(snapshot);
        if (entry.undoStack.length > 50) entry.undoStack.shift();
        entry.redoStack = [];
    } catch (e) { /* ignore */ }
}

function applyFiltersToCanvas(sourceCanvas, filters) {
    if (!filters || (filters.brightness === 0 && filters.contrast === 0 && filters.saturation === 0 && !filters.grayscale && !filters.sepia)) {
        return sourceCanvas;
    }
    const w = sourceCanvas.width;
    const h = sourceCanvas.height;
    const offscreen = document.createElement('canvas');
    offscreen.width = w;
    offscreen.height = h;
    const ctx = offscreen.getContext('2d');
    ctx.drawImage(sourceCanvas, 0, 0);
    const imgData = ctx.getImageData(0, 0, w, h);
    const data = imgData.data;
    const b = (filters.brightness || 0) / 100;
    const c = (filters.contrast || 0) / 100;
    const s = (filters.saturation || 0) / 100;
    const grayscale = filters.grayscale || false;
    const sepia = filters.sepia || false;

    for (let i = 0; i < data.length; i += 4) {
        let r = data[i], g = data[i + 1], b_ = data[i + 2];
        r = Math.min(255, Math.max(0, r * (1 + b) + 128 * b));
        g = Math.min(255, Math.max(0, g * (1 + b) + 128 * b));
        b_ = Math.min(255, Math.max(0, b_ * (1 + b) + 128 * b));
        const contrastFactor = (259 * (c * 100 + 255)) / (255 * (259 - c * 100));
        r = Math.min(255, Math.max(0, ((r / 255 - 0.5) * contrastFactor + 0.5) * 255));
        g = Math.min(255, Math.max(0, ((g / 255 - 0.5) * contrastFactor + 0.5) * 255));
        b_ = Math.min(255, Math.max(0, ((b_ / 255 - 0.5) * contrastFactor + 0.5) * 255));
        if (s !== 0) {
            const gray = 0.299 * r + 0.587 * g + 0.114 * b_;
            r = Math.min(255, Math.max(0, gray + (r - gray) * (1 + s)));
            g = Math.min(255, Math.max(0, gray + (g - gray) * (1 + s)));
            b_ = Math.min(255, Math.max(0, gray + (b_ - gray) * (1 + s)));
        }
        if (grayscale) {
            const gray = 0.299 * r + 0.587 * g + 0.114 * b_;
            r = g = b_ = gray;
        }
        if (sepia) {
            const tr = 0.393 * r + 0.769 * g + 0.189 * b_;
            const tg = 0.349 * r + 0.686 * g + 0.168 * b_;
            const tb = 0.272 * r + 0.534 * g + 0.131 * b_;
            r = Math.min(255, tr);
            g = Math.min(255, tg);
            b_ = Math.min(255, tb);
        }
        data[i] = r;
        data[i + 1] = g;
        data[i + 2] = b_;
    }
    ctx.putImageData(imgData, 0, 0);
    return offscreen;
}

export async function initCropper(imageElement, options) {
    const scriptUrl = options?.scriptUrl;
    const styleUrl = options?.styleUrl;
    const loaded = await ensureCropperLoaded(scriptUrl, styleUrl);
    if (!loaded || typeof Cropper === 'undefined') return null;

    const editorId = `fm-editor-${++editorIdCounter}`;
    const cropper = new Cropper(imageElement, {
        viewMode: 0, /* 0 = no restriction; allows image to fit (contain) within container instead of filling it */
        dragMode: 'crop',
        aspectRatio: options?.aspectRatio ?? NaN,
        autoCropArea: options?.autoCropArea ?? 0.8,
        autoCrop: options?.autoCrop !== false,
        restore: false,
        guides: true,
        center: true,
        highlight: true,
        cropBoxMovable: true,
        cropBoxResizable: true,
        toggleDragModeOnDblclick: false,
        ...options
    });

    const entry = {
        cropper,
        rotate: 0,
        scaleX: 1,
        scaleY: 1,
        resizeWidth: null,
        resizeHeight: null,
        filters: { brightness: 0, contrast: 0, saturation: 0, grayscale: false, sepia: false },
        undoStack: [],
        redoStack: []
    };
    editors.set(editorId, entry);
    setTimeout(() => updateFilterPreview(editorId), 0);
    return editorId;
}

export function getCroppedDataUrl(editorId, format, quality) {
    const entry = getEditor(editorId);
    const cropper = entry.cropper;
    const opts = { imageSmoothingQuality: 'high' };
    if (entry.resizeWidth > 0 && entry.resizeHeight > 0) {
        opts.width = entry.resizeWidth;
        opts.height = entry.resizeHeight;
    }
    let canvas = cropper.getCroppedCanvas(opts);
    if (!canvas) return '';
    canvas = applyFiltersToCanvas(canvas, entry.filters);
    const mime = format || 'image/png';
    const q = typeof quality === 'number' ? quality : 0.92;
    return canvas.toDataURL(mime, q);
}

export function rotate(editorId, deg) {
    const entry = getEditor(editorId);
    entry.cropper.rotate(deg);
    entry.rotate = (entry.rotate || 0) + deg;
    pushHistory(entry);
}

export function flipX(editorId) {
    const entry = getEditor(editorId);
    const scale = entry.scaleX ?? 1;
    entry.scaleX = scale * -1;
    entry.cropper.scaleX(entry.scaleX);
    pushHistory(entry);
}

export function flipY(editorId) {
    const entry = getEditor(editorId);
    const scale = entry.scaleY ?? 1;
    entry.scaleY = scale * -1;
    entry.cropper.scaleY(entry.scaleY);
    pushHistory(entry);
}

export function setAspectRatio(editorId, ratio) {
    const entry = getEditor(editorId);
    entry.cropper.setAspectRatio(ratio);
    pushHistory(entry);
}

export function resize(editorId, width, height) {
    const entry = getEditor(editorId);
    entry.resizeWidth = width > 0 ? width : null;
    entry.resizeHeight = height > 0 ? height : null;
}

export function applyFilter(editorId, name, value) {
    const entry = getEditor(editorId);
    if (name === 'brightness' || name === 'contrast' || name === 'saturation') {
        entry.filters[name] = typeof value === 'number' ? value : 0;
    } else if (name === 'grayscale' || name === 'sepia') {
        entry.filters[name] = !!value;
    }
    updateFilterPreview(editorId);
    pushHistory(entry);
}

export function undo(editorId) {
    const entry = getEditor(editorId);
    const stack = entry.undoStack;
    if (!stack || stack.length === 0) return false;
    const snapshot = stack.pop();
    entry.redoStack = entry.redoStack || [];
    entry.redoStack.push(snapshot);
    try {
        entry.cropper.setData(snapshot.data);
        entry.rotate = snapshot.rotate;
        entry.scaleX = snapshot.scaleX;
        entry.scaleY = snapshot.scaleY;
        entry.cropper.scaleX(snapshot.scaleX);
        entry.cropper.scaleY(snapshot.scaleY);
        entry.filters = { ...snapshot.filters };
        updateFilterPreview(editorId);
    } catch (e) { return false; }
    return true;
}

export function redo(editorId) {
    const entry = getEditor(editorId);
    const stack = entry.redoStack;
    if (!stack || stack.length === 0) return false;
    const snapshot = stack.pop();
    entry.undoStack = entry.undoStack || [];
    entry.undoStack.push(snapshot);
    try {
        entry.cropper.setData(snapshot.data);
        entry.rotate = snapshot.rotate;
        entry.scaleX = snapshot.scaleX;
        entry.scaleY = snapshot.scaleY;
        entry.cropper.scaleX(snapshot.scaleX);
        entry.cropper.scaleY(snapshot.scaleY);
        entry.filters = { ...snapshot.filters };
        updateFilterPreview(editorId);
    } catch (e) { return false; }
    return true;
}

export function canUndo(editorId) {
    const entry = editors.get(editorId);
    return entry && entry.undoStack && entry.undoStack.length > 0;
}

export function canRedo(editorId) {
    const entry = editors.get(editorId);
    return entry && entry.redoStack && entry.redoStack.length > 0;
}

export function reset(editorId) {
    const entry = getEditor(editorId);
    entry.cropper.reset();
    entry.rotate = 0;
    entry.scaleX = 1;
    entry.scaleY = 1;
    entry.resizeWidth = null;
    entry.resizeHeight = null;
    entry.filters = { brightness: 0, contrast: 0, saturation: 0, grayscale: false, sepia: false };
    entry.undoStack = [];
    entry.redoStack = [];
    updateFilterPreview(editorId);
}

export function zoom(editorId, ratio) {
    const entry = getEditor(editorId);
    entry.cropper.zoom(ratio);
}

/** Show the crop box (use after init with autoCrop: false). */
export function showCropBox(editorId) {
    const entry = getEditor(editorId);
    entry.cropper.crop();
}

/** Hide/clear the crop box. */
export function clearCropBox(editorId) {
    const entry = getEditor(editorId);
    entry.cropper.clear();
}

/** Toggle crop box visibility. Returns true if now shown, false if now hidden. */
export function toggleCropBox(editorId) {
    const entry = getEditor(editorId);
    const cropBoxData = entry.cropper.getCropBoxData();
    const isVisible = cropBoxData && cropBoxData.width > 0 && cropBoxData.height > 0;
    if (isVisible) {
        entry.cropper.clear();
        return false;
    } else {
        entry.cropper.crop();
        return true;
    }
}

/** Check if crop box is currently visible. */
export function isCropBoxVisible(editorId) {
    const entry = editors.get(editorId);
    if (!entry?.cropper) return false;
    const cropBoxData = entry.cropper.getCropBoxData();
    return cropBoxData && cropBoxData.width > 0 && cropBoxData.height > 0;
}

export function destroy(editorId) {
    const entry = editors.get(editorId);
    if (entry && entry.cropper) {
        entry.cropper.destroy();
        editors.delete(editorId);
    }
}
