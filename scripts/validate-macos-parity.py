#!/usr/bin/env python3
from __future__ import annotations

import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
LINUX = ROOT / "src" / "ToastifyReloaded.Linux"
MAC = ROOT / "src" / "ToastifyReloaded.Mac"


def fail(message: str) -> None:
    print(f"FAIL: {message}", file=sys.stderr)
    raise SystemExit(1)


def props(path: Path) -> set[str]:
    if not path.is_file():
        fail(f"missing source file: {path.relative_to(ROOT)}")
    text = path.read_text(encoding="utf-8")
    return set(re.findall(r"public\s+[\w?<>]+\s+(\w+)\s*\{\s*get;", text))


def xaml_names(path: Path) -> set[str]:
    text = path.read_text(encoding="utf-8")
    return set(re.findall(r'x:Name="([^"]+)"', text))


linux_props = props(LINUX / "Models" / "LinuxSettings.cs")
mac_props = props(MAC / "Models" / "MacSettings.cs")

linux_platform_only = {
    "EnableX11GlobalHotkeys",
    "EnableWaylandPortalHotkeys",
    "AutoCheckLinuxUpdates",
    "AutoInstallLinuxUpdates",
}
mac_platform_only = {
    "EnableGlobalHotkeys",
    "AutoCheckMacUpdates",
    "AutoInstallMacUpdates",
}

linux_common = linux_props - linux_platform_only
mac_common = mac_props - mac_platform_only

if linux_common != mac_common:
    missing = sorted(linux_common - mac_common)
    extra = sorted(mac_common - linux_common)
    fail(f"settings drift; missing on macOS={missing}, extra on macOS={extra}")

if not mac_platform_only.issubset(mac_props):
    fail("macOS platform settings mapping is incomplete")

linux_names = xaml_names(LINUX / "MainWindow.axaml")
mac_names = xaml_names(MAC / "MainWindow.axaml")
if linux_names != mac_names:
    fail(
        "settings UI control drift; "
        f"missing on macOS={sorted(linux_names - mac_names)}, "
        f"extra on macOS={sorted(mac_names - linux_names)}"
    )

linux_toast = (LINUX / "ToastWindow.axaml").read_text(encoding="utf-8")
mac_toast = (MAC / "ToastWindow.axaml").read_text(encoding="utf-8")
# The only intended XAML difference in the toast is the x:Class namespace.
normalize = lambda text: re.sub(
    r'x:Class="ToastifyReloaded\.(?:Linux|Mac)\.ToastWindow"',
    'x:Class="ToastifyReloaded.Platform.ToastWindow"',
    text,
)
if normalize(linux_toast) != normalize(mac_toast):
    fail("ToastWindow.axaml is no longer structurally identical to Linux stable")

required_theme_names = {
    "Classic Toastify",
    "Spotify Green",
    "Midnight Blue",
    "Neon Purple",
    "Cyberpunk",
    "Crimson Night",
    "Amber Gold",
    "Emerald",
    "Ocean",
    "Sakura",
    "Arctic",
    "Monochrome",
    "Retro Synthwave",
    "Custom",
}
mac_settings_text = (MAC / "MainWindow.axaml").read_text(encoding="utf-8")
missing_themes = sorted(name for name in required_theme_names if name not in mac_settings_text)
if missing_themes:
    fail(f"missing macOS toast theme options: {missing_themes}")

print(f"PASS: {len(mac_common)} common settings match Linux stable")
print(f"PASS: {len(mac_names)} named settings controls match Linux stable")
print("PASS: toast XAML remains structurally identical")
print("PASS: 13 built-in toast presets plus Custom are present")
print("PARITY RESULT: PASS")
