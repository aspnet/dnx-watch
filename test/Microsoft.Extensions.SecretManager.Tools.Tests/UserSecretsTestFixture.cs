// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.ProjectModel.Tests;

namespace Microsoft.Extensions.Configuration.UserSecrets.Tests
{
    public class UserSecretsTestFixture : MsBuildFixture, IDisposable
    {
        private Stack<Action> _disposables = new Stack<Action>();

        public const string TestSecretsId = "b918174fa80346bbb7f4a386729c0eff";

        public UserSecretsTestFixture()
        {
            _disposables.Push(() => TryDelete(Path.GetDirectoryName(PathHelper.GetSecretsPathFromSecretsId(TestSecretsId))));
        }

        public void Dispose()
        {
            while (_disposables.Count > 0)
            {
                _disposables.Pop()?.Invoke();
            }
        }

        public string GetTempSecretProject()
        {
            string userSecretsId;
            return GetTempSecretProject(out userSecretsId);
        }

        private const string ProjectTemplate = @"<Project ToolsVersion=""14.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>netcoreapp1.0</TargetFrameworks>
    <UserSecretsId>{0}</UserSecretsId>
  </PropertyGroup>

  <ItemGroup>
    <Compile Include=""**\*.cs"" Exclude=""Excluded.cs"" />

    <PackageReference Include=""Microsoft.NETCore.Sdk"">
        <Version>1.0.0-*</Version>
    </PackageReference>
    <PackageReference Include=""Microsoft.NETCore.App"">
        <Version>1.0.0</Version>
    </PackageReference>
  </ItemGroup>

  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
</Project>";

        public string GetTempSecretProject(out string userSecretsId)
        {
            var projectPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "usersecretstest", Guid.NewGuid().ToString()));
            userSecretsId = Guid.NewGuid().ToString();
            File.WriteAllText(
                Path.Combine(projectPath.FullName, "TestProject.csproj"),
                string.Format(ProjectTemplate, userSecretsId));

            var id = userSecretsId;
            _disposables.Push(() => TryDelete(Path.GetDirectoryName(PathHelper.GetSecretsPathFromSecretsId(id))));
            _disposables.Push(() => TryDelete(projectPath.FullName));

            return projectPath.FullName;
        }

        private static void TryDelete(string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Directory.Delete(directory, true);
                }
            }
            catch (Exception)
            {
                // Ignore failures.
                Console.WriteLine("Failed to delete " + directory);
            }
        }
    }
}