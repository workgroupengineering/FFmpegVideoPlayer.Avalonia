Place FFmpeg native binaries in this folder using the standard NuGet RID layout:

- `runtimes/win-x64/native/*.dll`
- `runtimes/win-x86/native/*.dll`
- `runtimes/osx-arm64/native/*.dylib`
- `runtimes/osx-x64/native/*.dylib`
- `runtimes/linux-x64/native/*.so`
- `runtimes/linux-arm64/native/*.so`

These files are packed into the NuGet so the control is self-contained on each platform. The runtime loader will automatically prefer these copies before falling back to system installations.

## macOS dylibs: beware of hardcoded paths

Do **not** just copy dylibs out of `/opt/homebrew/Cellar/ffmpeg/*/lib`. Those files have
absolute `LC_LOAD_DYLIB` entries pointing at `/opt/homebrew/opt/<formula>/lib/...`, so
`dlopen` silently fails on any Mac that doesn't have the exact same set of Homebrew
formulae installed. FFmpeg.AutoGen swallows the failure and replaces every function with
a stub that throws `NotSupportedException("Specified method is not supported.")` at call
time (e.g. during `avformat_open_input`).

If you want to bundle dylibs, you must rewrite them with
`install_name_tool -id @rpath/...` and `install_name_tool -change <abs> @loader_path/...`
for every transitive dependency, and ship all of those dependencies alongside them. The
simpler, more reliable option (and the one this project uses by default) is to **not**
bundle macOS binaries and require end users to `brew install ffmpeg`. The initializer
will auto-install via Homebrew on macOS when `autoInstall: true` (the default).

