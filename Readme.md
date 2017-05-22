# Overview

The `Package Builder` export tool allows more finite control over the creation of Kentico module installation packages with command-line support for continuous integration environments.

Comprehensive documentation of advanced topics and examples are available through the [Package Builder Wiki](https://github.com/Ntara/Kentico.PackageBuilder/wiki).

- [Command-line Interface](https://github.com/Ntara/Kentico.PackageBuilder/wiki/Command-Arguments)
- [NuSpec Manifests](https://github.com/Ntara/Kentico.PackageBuilder/wiki/NuSpec-Manifests)
- [Property Tokens](https://github.com/Ntara/Kentico.PackageBuilder/wiki/NuSpec-Manifests#replacement-tokens)
- [Adding Additional Files](https://github.com/Ntara/Kentico.PackageBuilder/wiki/NuSpec-Manifests#including-additional-files)
- [Metadata Overrides](https://github.com/Ntara/Kentico.PackageBuilder/wiki/Command-Arguments#-metadata)
- [Alternate Output Directory](https://github.com/Ntara/Kentico.PackageBuilder/wiki/Command-Arguments#-output)
- [Versioning by Assembly Attribute](https://github.com/Ntara/Kentico.PackageBuilder/wiki/Package-Versioning#extracting-version-information-from-an-assembly-file)

# Installation

Distributions of `Package Builder` are available for download from the [NuGet Gallery](https://www.nuget.org/) or installed by any supported NuGet client tool. The package must be installed to an existing Kentico `website` or `web application` project.

### Installing on Kentico 9.0

To install `Package Builder` for Kentico 9, run the following command in the [Package Manager Console](https://docs.microsoft.com/en-us/nuget/tools/package-manager-console)

```
PM> Install-Package Ntara.Kentico-9.0.PackageBuilder
```

### Installing on Kentico 10.0

To install `Package Builder` for Kentico 10, run the following command in the [Package Manager Console](https://docs.microsoft.com/en-us/nuget/tools/package-manager-console)

```
PM> Install-Package Ntara.Kentico-10.0.PackageBuilder
```

# Creating Kentico Modules

![Kentico Module Administration](https://github.com/Ntara/Kentico.PackageBuilder/wiki/Images/KenticoModuleAdmin_General.png)

It is critical that developers observe Kentico best practices when packaging modules for distribution. These patterns are thoroughly documented by Kentico and available online:

### Kentico 9 Resources

- [Creating Custom Modules](https://docs.kentico.com/k9/custom-development/creating-custom-modules)
- [Creating a Packageable Module](https://docs.kentico.com/k9/custom-development/creating-custom-modules/creating-installation-packages-for-modules/example-creating-a-packageable-module) (Recommended)

### Kentico 10 Resources

- [Creating Custom Modules](https://docs.kentico.com/k10/custom-development/creating-custom-modules)
- [Creating a Packageable Module](https://docs.kentico.com/k10/custom-development/creating-custom-modules/creating-installation-packages-for-modules/example-creating-a-packageable-module) (Recommended)

# Creating Module Packages

> **Important:**
The `PackageBuilder.exe` application must execute from the `bin` directory of a valid Kentico CMS installation. This allows the native module export to include all database objects, libraries, and file dependencies related to the specified module instance.