﻿using System;
using System.Threading.Tasks;

namespace Deployer.Services
{
    public interface IArchiveUncompressor
    {
        Task Extract(string archivePath, string destination, IOperationProgress progressObserver = null);
        Task<string> ReadToEnd(string archivePath, string key);
    }
}