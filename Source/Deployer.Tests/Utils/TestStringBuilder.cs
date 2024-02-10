﻿using System.Threading.Tasks;

namespace Deployer.Tests.Utils
{
    public class TestStringBuilder : IPathBuilder
    {
        public Task<string> Replace(string str)
        {
            return Task.FromResult(str);
        }
    }
}