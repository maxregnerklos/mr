﻿using System.Threading;
using System.Threading.Tasks;
using Deployer.Execution.Testing;
using Deployer.Tasks;
using Deployer.Tests.Real.Tasks.Azure;
using Xunit;

namespace Deployer.Tests.Real.Tasks.GitHub
{
    public class FetchGitHubBranchTests
    {
        [Fact]
        [Trait("Category", "Real")]
        public async Task Test()
        {
            var task = new FetchGitHubBranch("https://github.com/gus33000/MSM8994-8992-NT-ARM64-Drivers", "master", new ZipExtractor(new FileSystemOperations()), null, null, null, new TestDeploymentContext(), new TestFileSystemOperations(), new TestOperationContext());

            await task.Execute();
        }
    }
}