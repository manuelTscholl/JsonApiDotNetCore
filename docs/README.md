# Intro
Documentation for JsonApiDotNetCore is produced using [DocFX](https://dotnet.github.io/docfx/) from several files in this directory.
In addition, the example request/response pairs are generated by executing `curl` commands against the GettingStarted project.

# Installation
Run the following commands once to setup your system:
```
choco install docfx -y
```
```
npm install -g httpserver
```

# Running
The next command regenerates the documentation website and opens it in your default browser:
```
pwsh ./build-dev.ps1
```
