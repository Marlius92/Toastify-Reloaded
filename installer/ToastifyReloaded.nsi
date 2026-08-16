Unicode True

!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"

!ifndef APP_EXE
  !error "APP_EXE is required"
!endif
!ifndef APP_VERSION
  !error "APP_VERSION is required"
!endif
!ifndef APP_VERSION4
  !error "APP_VERSION4 is required"
!endif
!ifndef OUT_FILE
  !error "OUT_FILE is required"
!endif
!ifndef RUNTIME
  !define RUNTIME "win-x64"
!endif

!define APP_NAME "Toastify Reloaded"
!define APP_ID "ToastifyReloaded"
!define APP_EXE_NAME "ToastifyReloaded.exe"
!define PUBLISHER "Marlius92"
!define PROJECT_URL "https://github.com/Marlius92/Toastify-Reloaded"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_ID}"
!define APP_KEY "Software\${APP_ID}"

Name "${APP_NAME}"
OutFile "${OUT_FILE}"
InstallDir "$PROGRAMFILES64\${APP_NAME}"
InstallDirRegKey HKLM "${APP_KEY}" "InstallDir"
RequestExecutionLevel admin
SetRegView 64
SetCompressor /SOLID lzma
BrandingText "${APP_NAME}"
ShowInstDetails show
ShowUninstDetails show

VIProductVersion "${APP_VERSION4}"
VIAddVersionKey /LANG=1033 "ProductName" "${APP_NAME}"
VIAddVersionKey /LANG=1033 "ProductVersion" "${APP_VERSION}"
VIAddVersionKey /LANG=1033 "FileVersion" "${APP_VERSION}"
VIAddVersionKey /LANG=1033 "CompanyName" "${PUBLISHER}"
VIAddVersionKey /LANG=1033 "FileDescription" "${APP_NAME} Setup (${RUNTIME})"
VIAddVersionKey /LANG=1033 "LegalCopyright" "Toastify Reloaded contributors"

!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"
!define MUI_FINISHPAGE_RUN "$INSTDIR\${APP_EXE_NAME}"
!define MUI_FINISHPAGE_RUN_TEXT "Avvia ${APP_NAME}"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES
!insertmacro MUI_UNPAGE_FINISH

!insertmacro MUI_LANGUAGE "Italian"

Var UpdatePid
Var IsUpdate

Function .onInit
    SetRegView 64
    StrCpy $IsUpdate "0"
    StrCpy $UpdatePid ""

    ${GetParameters} $R0
    ${GetOptions} $R0 "/UPDATEPID=" $UpdatePid
    ${If} $UpdatePid != ""
        StrCpy $IsUpdate "1"
        DetailPrint "Attendo la chiusura di ${APP_NAME} (PID $UpdatePid)..."
        nsExec::ExecToLog '"$SYSDIR\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -Command "try { Wait-Process -Id $UpdatePid -Timeout 60 -ErrorAction Stop } catch { }"'
        Sleep 600
    ${EndIf}
FunctionEnd

Section "${APP_NAME}" SecMain
    SetShellVarContext all
    SetRegView 64
    SetOutPath "$INSTDIR"

    ; Clean leftovers from old portable/package layouts without touching user data.
    RMDir /r "$INSTDIR\scripts"
    Delete "$INSTDIR\ToastifyModern.exe"

    File /oname=${APP_EXE_NAME} "${APP_EXE}"
    WriteUninstaller "$INSTDIR\Uninstall.exe"

    CreateDirectory "$SMPROGRAMS\${APP_NAME}"
    CreateShortcut "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk" "$INSTDIR\${APP_EXE_NAME}"
    CreateShortcut "$SMPROGRAMS\${APP_NAME}\Disinstalla ${APP_NAME}.lnk" "$INSTDIR\Uninstall.exe"

    WriteRegStr HKLM "${APP_KEY}" "InstallDir" "$INSTDIR"
    WriteRegStr HKLM "${APP_KEY}" "Version" "${APP_VERSION}"

    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayName" "${APP_NAME}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayVersion" "${APP_VERSION}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "Publisher" "${PUBLISHER}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "URLInfoAbout" "${PROJECT_URL}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayIcon" '"$INSTDIR\${APP_EXE_NAME}"'
    WriteRegStr HKLM "${UNINSTALL_KEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
    WriteRegStr HKLM "${UNINSTALL_KEY}" "QuietUninstallString" '"$INSTDIR\Uninstall.exe" /S'
    WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoModify" 1
    WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoRepair" 1

    ; Preserve the user's Start-with-Windows choice while correcting an old
    ; portable path to the newly installed executable.
    ReadRegStr $R1 HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "ToastifyReloaded"
    ${If} $R1 != ""
        WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "ToastifyReloaded" '"$INSTDIR\${APP_EXE_NAME}" --minimized'
    ${EndIf}

    ; Silent installer launches are used by the in-app updater. The normal
    ; interactive installer uses the Finish-page checkbox instead.
    ${If} $IsUpdate == "1"
        Exec '"$INSTDIR\${APP_EXE_NAME}"'
    ${EndIf}
SectionEnd

Section "Uninstall"
    SetShellVarContext all
    SetRegView 64

    ; Stop Toastify if it is still running so files can be removed cleanly.
    nsExec::ExecToLog '"$SYSDIR\taskkill.exe" /IM ${APP_EXE_NAME} /F'
    Sleep 300

    DeleteRegValue HKCU "Software\Microsoft\Windows\CurrentVersion\Run" "ToastifyReloaded"
    DeleteRegKey HKLM "${UNINSTALL_KEY}"
    DeleteRegKey HKLM "${APP_KEY}"

    Delete "$SMPROGRAMS\${APP_NAME}\${APP_NAME}.lnk"
    Delete "$SMPROGRAMS\${APP_NAME}\Disinstalla ${APP_NAME}.lnk"
    RMDir "$SMPROGRAMS\${APP_NAME}"

    Delete "$INSTDIR\${APP_EXE_NAME}"
    Delete "$INSTDIR\Uninstall.exe"
    RMDir "$INSTDIR"

    ; User preferences in %APPDATA% are intentionally preserved, so reinstalling
    ; or upgrading does not erase hotkeys, popup settings or compatibility state.
SectionEnd
