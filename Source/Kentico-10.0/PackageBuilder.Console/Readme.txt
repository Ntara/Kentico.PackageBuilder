==================================================================================
# Overview
==================================================================================

The 'Package Builder' export tool allows more finite control over the creation
of Kentico module installation packages with command-line support for continuous
integration environments.

Comprehensive documentation of advanced topics and examples are available through
the Package Builder Wiki:

https://github.com/Ntara/Kentico.PackageBuilder/wiki


==================================================================================
# Installation
==================================================================================

Distributions of 'Package Builder' are available for download from
the 'NuGet Gallery' (https://www.nuget.org/) or installed by any supported NuGet
client tool. The package must be installed to an existing Kentico 'website'
or 'web application' project.

To install 'Package Builder' for Kentico 10, run the following command in
the 'Package Manager Console'

 |---------------------------------------------------------|
 | PM> Install-Package Ntara.Kentico-10.0.PackageBuilder   |
 |---------------------------------------------------------|


==================================================================================
# Creating Kentico Modules
==================================================================================

It is critical that developers observe Kentico best practices when packaging
modules for distribution. These patterns are thoroughly documented by Kentico
and available online:

Kentico 10 Resources
____________________

Creating Custom Modules:
https://docs.kentico.com/k10/custom-development/creating-custom-modules

Creating a Packageable Module (Recommended):
https://docs.kentico.com/k10/custom-development/creating-custom-modules/creating-installation-packages-for-modules/example-creating-a-packageable-module


==================================================================================
# Creating Module Packages
==================================================================================

**********************************************************************************
* Important: The 'PackageBuilder.exe' application must execute from the 'bin'    *
* directory of a valid Kentico CMS installation. This allows the native module   *
* export to include all database objects, libraries, and file dependencies       *
* related to the specified module instance.                                      *
**********************************************************************************

The '-module' command is the only *required* command-line argument. The following
command will produce a *NuGet* (.nupkg) package identical to one produced by
clicking the 'Create installation package' button.

 |---------------------------------------------------------|
 | PackageBuilder.exe -module:Acme.Module                  |
 |---------------------------------------------------------|

In this example, the package 'Acme.Module_1.0.0.nupkg' will be written to the
website 'CMSSiteUtils\Export' directory.


==================================================================================
# Licensing
==================================================================================

Source code is made available under terms of the MIT license:
https://github.com/Ntara/Kentico.PackageBuilder/wiki/Licensing