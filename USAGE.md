# Usage

## Requirements

Your consuming .NET-project must meet the following requirements:

- targeting .NET-runtime 8 or above:
	
	e.g. `<TargetFramework>net8.0</TargetFramework>` in your `.*proj` file

The platform, you want your consuming project to be executed on, must meet one of the following requirements with it's dependencies:

- Windows `x64`, `x86`, `arm64`, or `arm`:
  - OpenGL (`opengl32`)
  - Visual C++ Redistributable Runtime (2015+) (at least `vcruntime140`)
- Linux `x64`, `x86`, `arm64` (`aarch64`), or `arm` (`armhf`):
  - X11 (`libX11`)
  - GLX for OpenGL (`libGL`)
  - a C runtime (`libc`)

##