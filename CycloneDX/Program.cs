// This file is part of CycloneDX Tool for .NET
//
// Licensed under the Apache License, Version 2.0 (the “License”);
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an “AS IS” BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// SPDX-License-Identifier: Apache-2.0
// Copyright (c) OWASP Foundation. All Rights Reserved.

using System.CommandLine;
using System.Threading.Tasks;
using CycloneDX.Models;

namespace CycloneDX
{

    public static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var SolutionOrProjectFile = new Argument<string>("path", description: "The path to a .sln, .csproj, .fsproj, .vbproj, or packages.config file or the path to a directory which will be recursively analyzed for packages.config files.");
            var framework = new Option<string>(new[] { "--framework", "-tfm" }, "The target framework to use. If not defined, all will be aggregated.");
            var runtime = new Option<string>(new[] { "--runtime", "-rt" }, "The runtime to use. If not defined, all will be aggregated.");
            var outputDirectory = new Option<string>(new[] { "--output", "-o" }, description: "The directory to write the BOM") { IsRequired = true };
            var outputFilename = new Option<string>(new[] { "--filename", "-fn" }, "Optionally provide a filename for the BOM (default: bom.xml or bom.json)");
            var json = new Option<bool>(new[] { "--json", "-j" }, "Produce a JSON BOM instead of XML");
            var excludeDev = new Option<bool>(new[] { "--exclude-dev", "-ed" }, "Exclude development dependencies from the BOM (see https://github.com/NuGet/Home/wiki/DevelopmentDependency-support-for-PackageReference)");
            var excludetestprojects = new Option<bool>(new[] { "--exclude-test-projects", "-t" }, "Exclude test projects from the BOM");
            var baseUrl = new Option<string>(new[] { "--url", "-u" }, "Alternative NuGet repository URL to https://<yoururl>/nuget/<yourrepository>/v3/index.json");
            var baseUrlUS = new Option<string>(new[] { "--baseUrlUsername", "-us" }, "Alternative NuGet repository username");
            var baseUrlUSP = new Option<string>(new[] { "--baseUrlUserPassword", "-usp" }, "Alternative NuGet repository username password/apikey");
            var isPasswordClearText = new Option<bool>(new[] { "--isBaseUrlPasswordClearText", "-uspct" }, "Alternative NuGet repository password is cleartext");
            var scanProjectReferences = new Option<bool>(new[] { "--recursive", "-rs" }, "To be used with a single project file, it will recursively scan project references of the supplied project file");
            var noSerialNumber = new Option<bool>(new[] { "--no-serial-number", "-ns" }, "Optionally omit the serial number from the resulting BOM");
            var githubUsername = new Option<string>(new[] { "--github-username", "-gu" }, "Optionally provide a GitHub username for license resolution. If set you also need to provide a GitHub personal access token");
            var githubT = new Option<string>(new[] { "--github-token", "-gt" }, "Optionally provide a GitHub personal access token for license resolution. If set you also need to provide a GitHub username");
            var githubBT = new Option<string>(new[] { "--github-bearer-token", "-gbt" }, "Optionally provide a GitHub bearer token for license resolution. This is useful in GitHub actions");
            var disableGithubLicenses = new Option<bool>(new[] { "--disable-github-licenses", "-dgl" }, "Optionally disable GitHub license resolution");
            var disablePackageRestore = new Option<bool>(new[] { "--disable-package-restore", "-dpr" }, "Optionally disable package restore");
            var disableHashComputation = new Option<bool>(new[] { "--disable-hash-computation", "-dhc" }, "Optionally disable hash computation for packages");
            var dotnetCommandTimeout = new Option<int>(new[] { "--dotnet-command-timeout", "-dct" }, description: "dotnet command timeout in milliseconds (primarily used for long dotnet restore operations)", getDefaultValue: () => 300000);
            var baseIntermediateOutputPath = new Option<string>(new[] { "--base-intermediate-output-path", "-biop" }, "Optionally provide a folder for customized build environment. Required if folder 'obj' is relocated.");
            var importMetadataPath = new Option<string>(new[] { "--import-metadata-path", "-imp" }, "Optionally provide a metadata template which has project specific details.");
            var setName = new Option<string>(new[] { "--set-name", "-sn" }, "Override the autogenerated BOM metadata component name.");
            var setVersion = new Option<string>(new[] { "--set-version", "-sv" }, "Override the default BOM metadata component version (defaults to 0.0.0).");
            var includeProjectReferences = new Option<bool>(new[] { "--include-project-references", "-ipr" }, "Include project references as components (can only be used with project files).");
            var setType = new Option<Component.Classification>(new[] { "--set-type", "-st" }, getDefaultValue: () => Component.Classification.Application, "Override the default BOM metadata component type (defaults to application).");
            //Deprecated args
            var outputFilenameDeprecated = new Option<string>(new[] { "-f" }, "(Deprecated use -fn instead) Optionally provide a filename for the BOM (default: bom.xml or bom.json).");
            var excludeDevDeprecated = new Option<bool>(new[] {"-d" }, "(Deprecated use -ed instead) Exclude development dependencies from the BOM.");
            var scanProjectDeprecated = new Option<bool>(new[] {"-r" }, "(Deprecated use -rs instead) To be used with a single project file, it will recursively scan project references of the supplied project file.");


            RootCommand rootCommand = new RootCommand
            {
                SolutionOrProjectFile,
                framework,
                runtime,
                outputDirectory,
                outputFilename,
                json,
                excludeDev,
                excludetestprojects,
                baseUrl,
                baseUrlUS,
                baseUrlUSP,
                isPasswordClearText,
                scanProjectReferences,
                noSerialNumber,
                githubUsername,
                githubT,
                githubBT,
                disableGithubLicenses,
                disablePackageRestore,
                disableHashComputation,
                dotnetCommandTimeout,
                baseIntermediateOutputPath,
                importMetadataPath,
                setName,
                setVersion,
                setType,
                includeProjectReferences,
                outputFilenameDeprecated,
                excludeDevDeprecated,
                scanProjectDeprecated
            };
            rootCommand.Description = "A .NET Core global tool which creates CycloneDX Software Bill-of-Materials (SBOM) from .NET projects.";
            rootCommand.SetHandler(async (context) =>
            {
                RunOptions options = new RunOptions
                {
                    SolutionOrProjectFile = context.ParseResult.GetValueForArgument(SolutionOrProjectFile),
                    runtime = context.ParseResult.GetValueForOption(runtime),
                    framework = context.ParseResult.GetValueForOption(framework),
                    outputDirectory = context.ParseResult.GetValueForOption(outputDirectory),
                    outputFilename = context.ParseResult.GetValueForOption(outputFilename) ?? context.ParseResult.GetValueForOption(outputFilenameDeprecated),
                    json = context.ParseResult.GetValueForOption(json),
                    excludeDev = context.ParseResult.GetValueForOption(excludeDev) | context.ParseResult.GetValueForOption(excludeDevDeprecated),
                    excludeTestProjects = context.ParseResult.GetValueForOption(excludetestprojects),
                    baseUrl = context.ParseResult.GetValueForOption(baseUrl),
                    baseUrlUserName = context.ParseResult.GetValueForOption(baseUrlUS),
                    baseUrlUSP = context.ParseResult.GetValueForOption(baseUrlUSP),
                    isPasswordClearText = context.ParseResult.GetValueForOption(isPasswordClearText),
                    scanProjectReferences = context.ParseResult.GetValueForOption(scanProjectReferences) | context.ParseResult.GetValueForOption(scanProjectDeprecated),
                    noSerialNumber = context.ParseResult.GetValueForOption(noSerialNumber),
                    githubUsername = context.ParseResult.GetValueForOption(githubUsername),
                    githubT = context.ParseResult.GetValueForOption(githubT),
                    githubBT = context.ParseResult.GetValueForOption(githubBT),
                    disableGithubLicenses = context.ParseResult.GetValueForOption(disableGithubLicenses),
                    disablePackageRestore = context.ParseResult.GetValueForOption(disablePackageRestore),
                    disableHashComputation = context.ParseResult.GetValueForOption(disableHashComputation),
                    dotnetCommandTimeout = context.ParseResult.GetValueForOption(dotnetCommandTimeout),
                    baseIntermediateOutputPath = context.ParseResult.GetValueForOption(baseIntermediateOutputPath),
                    importMetadataPath = context.ParseResult.GetValueForOption(importMetadataPath),
                    setName = context.ParseResult.GetValueForOption(setName),
                    setVersion = context.ParseResult.GetValueForOption(setVersion),
                    setType = context.ParseResult.GetValueForOption(setType),
                    includeProjectReferences = context.ParseResult.GetValueForOption(includeProjectReferences)
                };                

                Runner runner = new Runner();
                var taskStatus = await runner.HandleCommandAsync(options);
                context.ExitCode = taskStatus;

            });
            return Task.FromResult(rootCommand.Invoke(args));
        }
    }
}
