﻿namespace Deployer.Tasks
{
    public interface IDeploymentContext
    {
        IDiskLayoutPreparer DiskLayoutPreparer { get; set; }
        IDevice Device { get; set; }
        WindowsDeploymentOptions DeploymentOptions { get; set; }
    }
}