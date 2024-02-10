using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Deployer.Core;
using Deployer.Core.Exceptions;
using Deployer.Core.FileSystem;
using Deployer.Core.Services;
using Deployer.Core.Utils;
using Serilog;
using Zafiro.Core;
using Zafiro.Core.FileSystem;

namespace Deployer.NetFx
{
    public class DismImageService : ImageServiceBase
    {
        private readonly Regex percentRegex = new Regex(@"(\d*.\d*)%");

        public DismImageService(IFileSystemOperations fileSystemOperations) : base(fileSystemOperations)
        {
        }

        public override Task ApplyImage(IPartition target, string imagePath, int imageIndex = 1,
            bool useCompact = false,
            IOperationProgress progressObserver = null, CancellationToken token = default(CancellationToken))
        {
            return ApplyImage(target.Root, imagePath, imageIndex, useCompact, progressObserver, token);
        }

        public override Task ApplyImage(string targetDriveRoot, string imagePath, int imageIndex = 1,
            bool useCompact = false,
            IOperationProgress progressObserver = null, CancellationToken token = default(CancellationToken))
        {
            EnsureValidParameters(targetDriveRoot, imagePath, imageIndex);

            var compact = useCompact ? "/compact" : "";
            var args =
                $@"/Apply-Image {compact} /ImageFile:""{imagePath}"" /Index:{imageIndex} /ApplyDir:{targetDriveRoot}";

            return Run(args, progressObserver, token);
        }

        //dism.exe /Capture-Image /ImageFile:D:\Image_of_Windows_10.wim /CaptureDir:C:\ /Name:Windows_10 /compress:fast
        public override Task CaptureImage(IPartition source, string destination,
            IOperationProgress progressObserver = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var capturePath = source?.Root;

            if (capturePath == null)
            {
                throw new ApplicationException("The capture path cannot be null");
            }

            var args = $@"/Capture-Image /ImageFile:""{destination}"" /CaptureDir:{capturePath} /Name:WOA /compress:fast";

            return Run(args, progressObserver, cancellationToken);
        }

        private async Task Run(string args, IOperationProgress progressObserver, CancellationToken token)
        {
            var dismName = WindowsCommandLineUtils.Dism;
            ISubject<string> outputSubject = new Subject<string>();
            IDisposable stdOutputSubscription = null;
            if (progressObserver != null)
            {
                stdOutputSubscription = outputSubject
                    .Select(GetPercentage)
                    .Where(d => !double.IsNaN(d))
                    .Subscribe(progressObserver.Percentage);
            }

            Log.Verbose("We are about to run DISM: {ExecName} {Parameters}", dismName, args);
            var processResults = await ProcessMixin.RunProcess(dismName, args, outputSubject, cancellationToken: token);

            progressObserver?.Percentage.OnNext(double.NaN);

            if (processResults.ExitCode != 0)
            {
                Log.Error("There has been a problem during deployment: DISM failed {Results}", processResults);
                throw new DeploymentException($"There has been a problem during deployment: DISM exited with code {processResults.ExitCode}");
            }

            stdOutputSubscription?.Dispose();
        }

        private double GetPercentage(string dismOutput)
        {
            if (dismOutput == null)
            {
                return double.NaN;
            }

            var matches = percentRegex.Match(dismOutput);

            if (matches.Success)
            {
                var value = matches.Groups[1].Value;
                try
                {
                    var percentage = double.Parse(value, CultureInfo.InvariantCulture) / 100D;
                    return percentage;
                }
                catch (FormatException)
                {
                    Log.Warning($"Cannot convert {value} to double");
                }
            }

            return double.NaN;
        }
    }
}