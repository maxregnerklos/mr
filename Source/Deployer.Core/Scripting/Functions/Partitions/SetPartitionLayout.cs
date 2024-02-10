﻿using System;
using System.Threading.Tasks;
using Deployer.Core.FileSystem;
using Deployer.Core.Scripting.Core;
using Zafiro.Core.FileSystem;

namespace Deployer.Core.Scripting.Functions.Partitions
{
    public class SetPartitionLayout : DeployerFunction
    {
        private readonly IFileSystem fileSystem;

        public SetPartitionLayout(IFileSystem fileSystem, IFileSystemOperations fileSystemOperations,
            IOperationContext operationContext) : base(fileSystemOperations, operationContext)
        {
            this.fileSystem = fileSystem;
        }

        public async Task Execute(int diskNumber, string diskType)
        {
            var disk = await fileSystem.GetDisk(diskNumber);
            var type = (DiskType) Enum.Parse(typeof(DiskType), diskType, true);
            await disk.ClearAs(type);
        }
    }
}