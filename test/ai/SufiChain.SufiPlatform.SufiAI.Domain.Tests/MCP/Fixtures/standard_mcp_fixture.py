#!/usr/bin/env python3
"""Minimal MCP peer: stdio JSON-RPC and a streamable HTTP gateway in front of that stdio peer.

The initialize check follows the MCP lifecycle: protocolVersion, capabilities, and clientInfo
are required. protocolVersion "1.0" is rejected. Secrets are read from the environment only.
"""
from __future__ import annotations

import json
import os
import sys
import threading
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from urllib.parse import urlparse


PROTOCOL = "2025-03-26"


def fail(request_id, message):
    return {
        "jsonrpc": "2.0",
        "id": request_id,
        "error": {"code": -32602, "message": message},
    }


def handle(message: dict) -> dict | None:
    method = message.get("method")
    request_id = message.get("id")
    if method == "notifications/initialized" or request_id is None:
        return None

    params = message.get("params") if isinstance(message.get("params"), dict) else {}
    if method == "initialize":
        version = params.get("protocolVersion")
        capabilities = params.get("capabilities")
        client_info = params.get("clientInfo") if isinstance(params.get("clientInfo"), dict) else None
        if version in (None, "", "1.0"):
            return fail(request_id, f"unsupported protocolVersion {version!r}; expected {PROTOCOL}")
        if not isinstance(capabilities, dict):
            return fail(request_id, "initialize params.capabilities is required")
        if not client_info or not client_info.get("name") or not client_info.get("version"):
            return fail(request_id, "initialize params.clientInfo.name and version are required")
        return {
            "jsonrpc": "2.0",
            "id": request_id,
            "result": {
                "protocolVersion": PROTOCOL,
                "capabilities": {"tools": {"listChanged": False}},
                "serverInfo": {"name": "standard-mcp-fixture", "version": "1.0"},
            },
        }

    if method == "tools/list":
        return {
            "jsonrpc": "2.0",
            "id": request_id,
            "result": {
                "tools": [
                    {
                        "name": "echo_env",
                        "description": "Returns a configured env value without reading argv.",
                        "inputSchema": {"type": "object"},
                        "annotations": {"readOnlyHint": True},
                    },
                    {
                        "name": "write_note",
                        "description": "Writes a note.",
                        "inputSchema": {"type": "object", "properties": {"text": {"type": "string"}}},
                    },
                ]
            },
        }

    if method == "tools/call":
        name = params.get("name")
        token = os.environ.get("SUFI_MCP_FIXTURE_TOKEN", "")
        argv_blob = " ".join(sys.argv)
        if name == "echo_env":
            payload = {
                "present": bool(token),
                "token": token,
                "argvHasToken": bool(token) and token in argv_blob,
                "parentLeak": bool(os.environ.get("SUFI_PARENT_API_TOKEN")),
            }
            if token:
                print(f"fixture stderr token {token}", file=sys.stderr, flush=True)
            text = json.dumps(payload)
        else:
            text = "wrote"
        return {
            "jsonrpc": "2.0",
            "id": request_id,
            "result": {"content": [{"type": "text", "text": text}], "isError": False},
        }

    return fail(request_id, f"unknown method {method}")


def serve_stdio() -> None:
    for line in sys.stdin:
        if not line.strip():
            continue
        try:
            message = json.loads(line)
        except json.JSONDecodeError:
            sys.stdout.write(json.dumps(fail(None, "invalid json")) + "\n")
            sys.stdout.flush()
            continue
        response = handle(message)
        if response is None:
            continue
        sys.stdout.write(json.dumps(response) + "\n")
        sys.stdout.flush()


class Gateway(BaseHTTPRequestHandler):
    child_lock = threading.Lock()
    child = None
    transcript = None

    def log_message(self, fmt, *args):
        return

    def do_DELETE(self):
        self.send_response(204)
        self.end_headers()

    def do_POST(self):
        length = int(self.headers.get("Content-Length", "0"))
        raw = self.rfile.read(length).decode("utf-8")
        path = urlparse(self.path).path
        if self.transcript is not None:
            self.transcript.write(
                json.dumps(
                    {
                        "path": path,
                        "authorization": self.headers.get("Authorization"),
                        "session": self.headers.get("Mcp-Session-Id"),
                        "body": raw,
                    }
                )
                + "\n"
            )
            self.transcript.flush()
        if path != "/mcp":
            self._text(404, "not the mcp endpoint")
            return
        try:
            message = json.loads(raw or "{}")
        except json.JSONDecodeError:
            self._text(400, "invalid json")
            return
        method = message.get("method")
        with self.child_lock:
            self.child.stdin.write(raw.strip() + "\n")
            self.child.stdin.flush()
            if method == "notifications/initialized" or message.get("id") is None:
                self.send_response(202)
                self.end_headers()
                return
            reply = self.child.stdout.readline()
        if not reply:
            self._text(502, "stdio child closed")
            return
        body = reply.encode("utf-8")
        self.send_response(200)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(body)))
        if method == "initialize":
            self.send_header("Mcp-Session-Id", "sess-gateway")
        self.end_headers()
        self.wfile.write(body)

    def _text(self, status, text):
        data = text.encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "text/plain")
        self.send_header("Content-Length", str(len(data)))
        self.end_headers()
        self.wfile.write(data)


def serve_http(port: int, transcript_path: str | None) -> None:
    import subprocess

    child = subprocess.Popen(
        [sys.executable, __file__, "stdio"],
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.DEVNULL,
        text=True,
        bufsize=1,
    )
    Gateway.child = child
    if transcript_path:
        Gateway.transcript = open(transcript_path, "w", encoding="utf-8")
    server = ThreadingHTTPServer(("127.0.0.1", port), Gateway)
    bound = server.server_address[1]
    print(f"PORT {bound}", flush=True)
    try:
        server.serve_forever()
    finally:
        child.kill()


def main() -> None:
    mode = sys.argv[1] if len(sys.argv) > 1 else "stdio"
    if mode == "stdio":
        serve_stdio()
        return
    if mode == "http":
        port = int(sys.argv[2]) if len(sys.argv) > 2 else 0
        transcript = sys.argv[3] if len(sys.argv) > 3 else None
        serve_http(port, transcript)
        return
    raise SystemExit(f"unknown mode {mode}")


if __name__ == "__main__":
    main()
