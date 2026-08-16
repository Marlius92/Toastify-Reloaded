# Toastify Reloaded — Full Rename Notice

Version 1.0.3 completes the project-wide rename to **Toastify Reloaded**.

The repository now consistently uses:

- `ToastifyReloaded.sln`
- `src/ToastifyReloaded/`
- `ToastifyReloaded.csproj`
- `ToastifyReloaded.*` namespaces
- `%APPDATA%\ToastifyReloaded`
- Windows startup value `ToastifyReloaded`
- mutex `ToastifyReloaded.SingleInstance`

Earlier development-build settings are not automatically migrated. Reconfigure the application once after upgrading if necessary.
