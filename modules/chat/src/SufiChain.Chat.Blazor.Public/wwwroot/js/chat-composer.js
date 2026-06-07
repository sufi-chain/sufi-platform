// Chat composer interop — popover overlay positioning, file reading, geolocation, and voice recording.
//
// DEBUG: This module logs every interop entry/exit and error to the browser console
// with the "[chat-composer]" prefix. Bump MODULE_VERSION whenever this file changes so
// the version banner below confirms the browser loaded the fresh (non-cached) module.

const MODULE_VERSION = "chat-composer-v6";
const LOG_PREFIX = "[chat-composer]";

function log(...args) {
  console.debug(LOG_PREFIX, ...args);
}

function logError(label, error) {
  console.error(LOG_PREFIX, label, error);
}

// Banner: if you do NOT see this line in the console after a rebuild, the browser is
// serving a stale cached module (check the ?v= cache-busting query in ChatComposerJsInterop).
console.info(`${LOG_PREFIX} module loaded (version=${MODULE_VERSION})`);

let mediaRecorder = null;
let recordedChunks = [];
let activeStream = null;
let recordedMimeType = "audio/webm";

function getDirection(element) {
  if (element && typeof getComputedStyle === "function") {
    const direction = getComputedStyle(element).direction;
    if (direction === "rtl" || direction === "ltr") {
      return direction;
    }
  }

  const rootDir = document.documentElement.getAttribute("dir");
  if (rootDir === "rtl" || rootDir === "ltr") {
    return rootDir;
  }

  return getComputedStyle(document.documentElement).direction || "ltr";
}

function isRtl(element) {
  return getDirection(element) === "rtl";
}

function clearHorizontalPosition(popover) {
  popover.style.left = "";
  popover.style.right = "";
  popover.style.insetInlineStart = "";
  popover.style.insetInlineEnd = "";
}

/**
 * Applies physical left/right coordinates derived from getBoundingClientRect().
 * Logical inset properties are avoided because fixed overlays may not inherit RTL direction.
 */
function applyHorizontalPosition(popover, rect, padding, rtl) {
  clearHorizontalPosition(popover);

  if (rtl) {
    popover.style.right = `${window.innerWidth - rect.right + padding}px`;
  } else {
    popover.style.left = `${rect.left + padding}px`;
  }
}

/**
 * Positions a composer popover as a fixed overlay above the footer trigger,
 * visually on top of the composer shell without affecting shell layout.
 *
 * @param {HTMLElement} anchor - Footer icon button wrapper.
 * @param {HTMLElement} popover - Popover panel element.
 * @param {HTMLElement|null} shell - Composer shell box (optional).
 * @param {"anchor"|"shell"} mode - "anchor" aligns to trigger; "shell" spans shell width.
 */
export function positionOverlayPopover(anchor, popover, shell, mode) {
  log("positionOverlayPopover() called", { hasAnchor: !!anchor, hasPopover: !!popover, mode });

  if (!anchor || !popover) {
    log("positionOverlayPopover() skipped — missing anchor or popover element");
    return;
  }

  const gap = 8;
  const anchorRect = anchor.getBoundingClientRect();
  const shellRect =
    shell && typeof shell.getBoundingClientRect === "function"
      ? shell.getBoundingClientRect()
      : null;
  const bounds = shellRect ?? anchorRect;
  const rtl = isRtl(anchor || shell);
  const shellPadding = 8;
  const alignRect = mode === "shell" ? bounds : anchorRect;
  const inlinePadding = mode === "shell" ? shellPadding : 0;

  popover.style.position = "fixed";
  popover.style.zIndex = "2000";
  popover.style.margin = "0";
  popover.style.top = "auto";
  popover.style.bottom = `${window.innerHeight - anchorRect.top + gap}px`;
  applyHorizontalPosition(popover, alignRect, inlinePadding, rtl);
  popover.setAttribute("dir", rtl ? "rtl" : "ltr");

  if (mode === "shell") {
    popover.style.maxWidth = `${Math.max(bounds.width - shellPadding * 2, 160)}px`;
  } else {
    popover.style.maxWidth = "";
  }

  log("positionOverlayPopover() done", {
    left: popover.style.left,
    right: popover.style.right,
    bottom: popover.style.bottom,
    rtl,
  });
}

/**
 * Reads the selected files from a file input element as base64 payloads,
 * then clears the input so the same file can be picked again.
 *
 * @param {HTMLInputElement} input - The file input element.
 * @returns {Promise<Array<{name: string, type: string, base64: string}>>}
 */
export async function readInputFiles(input) {
  log("readInputFiles() called", {
    hasInput: !!input,
    fileCount: input && input.files ? input.files.length : 0,
  });

  if (!input || !input.files || input.files.length === 0) {
    log("readInputFiles() returning empty — no files selected");
    return [];
  }

  try {
    const files = Array.from(input.files);
    log("readInputFiles() reading files", files.map((f) => ({ name: f.name, type: f.type, size: f.size })));

    const payloads = await Promise.all(files.map(readFileAsPayload));

    // Reset so selecting the same file again still raises a change event.
    input.value = "";

    log("readInputFiles() done", { count: payloads.length });
    return payloads;
  } catch (error) {
    logError("readInputFiles() failed", error);
    throw error;
  }
}

function readFileAsPayload(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => {
      resolve({
        name: file.name,
        type: file.type || "application/octet-stream",
        base64: extractBase64(reader.result),
      });
    };
    reader.onerror = () => {
      logError(`readFileAsPayload() failed for "${file.name}"`, reader.error);
      reject(reader.error ?? new Error("Failed to read file."));
    };
    reader.readAsDataURL(file);
  });
}

/**
 * Resolves the current geolocation coordinates.
 *
 * @returns {Promise<{latitude: number, longitude: number, accuracyMeters: number|null}>}
 */
export function getGeolocation() {
  log("getGeolocation() called");

  return new Promise((resolve, reject) => {
    if (!navigator.geolocation) {
      logError("getGeolocation() unsupported", "navigator.geolocation is undefined");
      reject(new Error("Geolocation is not supported by this browser."));
      return;
    }

    navigator.geolocation.getCurrentPosition(
      (position) => {
        log("getGeolocation() success", {
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          accuracy: position.coords.accuracy,
        });
        resolve({
          latitude: position.coords.latitude,
          longitude: position.coords.longitude,
          accuracyMeters: position.coords.accuracy ?? null,
        });
      },
      (error) => {
        logError("getGeolocation() error", error);
        reject(new Error(error.message || "Unable to retrieve location."));
      },
      { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 }
    );
  });
}

/**
 * Starts microphone recording using the MediaRecorder API.
 */
export async function startVoiceRecording() {
  log("startVoiceRecording() called", { currentState: mediaRecorder ? mediaRecorder.state : "none" });

  if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
    logError("startVoiceRecording() unsupported", "navigator.mediaDevices.getUserMedia is undefined");
    throw new Error("Microphone recording is not supported by this browser.");
  }

  if (mediaRecorder && mediaRecorder.state === "recording") {
    log("startVoiceRecording() ignored — already recording");
    return;
  }

  try {
    log("startVoiceRecording() requesting microphone permission...");
    activeStream = await navigator.mediaDevices.getUserMedia({ audio: true });
    log("startVoiceRecording() microphone granted");

    recordedChunks = [];
    recordedMimeType = pickAudioMimeType() || "audio/webm";

    const options = recordedMimeType ? { mimeType: recordedMimeType } : undefined;
    mediaRecorder = new MediaRecorder(activeStream, options);
    recordedMimeType = mediaRecorder.mimeType || recordedMimeType;

    mediaRecorder.ondataavailable = (event) => {
      if (event.data && event.data.size > 0) {
        recordedChunks.push(event.data);
        log("startVoiceRecording() chunk captured", { size: event.data.size, totalChunks: recordedChunks.length });
      }
    };

    mediaRecorder.start();
    log("startVoiceRecording() recording started", { mimeType: recordedMimeType });
  } catch (error) {
    logError("startVoiceRecording() failed", error);
    stopActiveStream();
    mediaRecorder = null;
    throw error;
  }
}

/**
 * Stops recording and returns the captured audio as a base64 payload.
 *
 * @returns {Promise<{base64: string, mimeType: string, size: number}>}
 */
export function stopVoiceRecording() {
  log("stopVoiceRecording() called", { currentState: mediaRecorder ? mediaRecorder.state : "none" });

  return new Promise((resolve) => {
    if (!mediaRecorder) {
      log("stopVoiceRecording() no active recorder — returning empty");
      stopActiveStream();
      resolve({ base64: "", mimeType: recordedMimeType, size: 0 });
      return;
    }

    const recorder = mediaRecorder;

    const finalize = async () => {
      stopActiveStream();
      mediaRecorder = null;

      const blob = new Blob(recordedChunks, { type: recordedMimeType });
      log("stopVoiceRecording() finalizing", { blobSize: blob.size, chunks: recordedChunks.length });
      recordedChunks = [];

      if (blob.size === 0) {
        log("stopVoiceRecording() empty blob — returning empty");
        resolve({ base64: "", mimeType: recordedMimeType, size: 0 });
        return;
      }

      try {
        const base64 = await blobToBase64(blob);
        log("stopVoiceRecording() done", { mimeType: recordedMimeType, size: blob.size, base64Length: base64.length });
        resolve({ base64, mimeType: recordedMimeType, size: blob.size });
      } catch (error) {
        logError("stopVoiceRecording() base64 conversion failed", error);
        resolve({ base64: "", mimeType: recordedMimeType, size: 0 });
      }
    };

    recorder.onstop = finalize;

    if (recorder.state !== "inactive") {
      recorder.stop();
    } else {
      finalize();
    }
  });
}

function pickAudioMimeType() {
  if (typeof MediaRecorder === "undefined" || !MediaRecorder.isTypeSupported) {
    return "audio/webm";
  }

  const candidates = [
    "audio/webm;codecs=opus",
    "audio/webm",
    "audio/ogg;codecs=opus",
    "audio/ogg",
  ];

  const picked = candidates.find((type) => MediaRecorder.isTypeSupported(type)) || "";
  log("pickAudioMimeType() selected", { picked });
  return picked;
}

function stopActiveStream() {
  if (activeStream) {
    activeStream.getTracks().forEach((track) => track.stop());
    activeStream = null;
    log("stopActiveStream() microphone tracks stopped");
  }
}

function blobToBase64(blob) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = () => resolve(extractBase64(reader.result));
    reader.onerror = () => reject(reader.error ?? new Error("Failed to read recording."));
    reader.readAsDataURL(blob);
  });
}

function extractBase64(dataUrl) {
  const value = String(dataUrl || "");
  const commaIndex = value.indexOf(",");
  return commaIndex >= 0 ? value.slice(commaIndex + 1) : "";
}
