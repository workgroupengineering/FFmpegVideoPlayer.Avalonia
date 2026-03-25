Place FFmpeg native binaries in this folder using the standard NuGet RID layout:

- `runtimes/win-x64/native/*.dll`
- `runtimes/win-x86/native/*.dll`
- `runtimes/osx-arm64/native/*.dylib`
- `runtimes/osx-x64/native/*.dylib`
- `runtimes/linux-x64/native/*.so`
- `runtimes/linux-arm64/native/*.so`

These files are packed into the NuGet so the control is self-contained on each platform. The runtime loader will automatically prefer these copies before falling back to system installations.

