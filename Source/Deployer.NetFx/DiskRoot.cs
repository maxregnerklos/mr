﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ByteSizeLib;
using Deployer.FileSystem;
using Serilog;

namespace Deployer.NetFx
{
    public class DiskRoot : IDiskRoot
    {
        public async Task<IList<IDisk>> GetDisks()
        {
            var results = await PowerShellMixin.ExecuteScript("Get-Disk");

            var disks = results
                .Select(x => x.ImmediateBaseObject)
                .Select(ToDisk);

            return disks.ToList();
        }

        private IDisk ToDisk(object disk)
        {
            var number = (uint)disk.GetPropertyValue("Number");
            var size = new ByteSize((ulong)disk.GetPropertyValue("Size"));
            var allocatedSize = new ByteSize((ulong)disk.GetPropertyValue("AllocatedSize"));

            var diskInfo = new DiskInfo
            {
                Number = number,
                Size = size,
                AllocatedSize = allocatedSize,
                FriendlyName = (string)disk.GetPropertyValue("FriendlyName"),
                IsSystem = (bool)disk.GetPropertyValue("IsSystem"),
                IsBoot = (bool)disk.GetPropertyValue("IsBoot"),
                IsOffline = (bool)disk.GetPropertyValue("IsOffline"),
                IsReadOnly = (bool)disk.GetPropertyValue("IsReadOnly"),
                UniqueId = (string)disk.GetPropertyValue("UniqueId"),
            };

            return new Disk(diskInfo);
        }

        public async Task<IDisk> GetDisk(int n)
        {
            Log.Verbose("Getting disk by index {Id}", n);
            var results = await PowerShellMixin.ExecuteScript($"Get-Disk -Number {n}");

            var disks = results
                .Select(x => x.ImmediateBaseObject)
                .Select(ToDisk);

            var disk = disks.First();

            Log.Verbose("Returned disk {Disk}", disk);

            return disk;
        }
    }
}