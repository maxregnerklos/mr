﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using Serilog;

namespace Deployer.Tasks
{
    [TaskDescription("Fetching asset {1} from latest release at {0}")]
    public class FetchGitHubLatestReleaseAsset : DownloaderTask
    {
        private readonly string repoUrl;
        private readonly string assetName;
        private readonly IZipExtractor extractor;
        private readonly IGitHubClient gitHubClient;
        private readonly IDownloader downloader;
        private readonly IOperationProgress progressObserver;

        public FetchGitHubLatestReleaseAsset(string repoUrl, string assetName, IZipExtractor extractor,
            IGitHubClient gitHubClient, IDownloader downloader, IOperationProgress progressObserver,
            IDeploymentContext deploymentContext, IFileSystemOperations fileSystemOperations,
            IOperationContext operationContext) : base(deploymentContext,
            fileSystemOperations, operationContext)
        {
            this.repoUrl = repoUrl ?? throw new ArgumentNullException(nameof(repoUrl));
            this.assetName = assetName ?? throw new ArgumentNullException(nameof(assetName));
            this.extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            this.gitHubClient = gitHubClient ?? throw new ArgumentNullException(nameof(gitHubClient));
            this.downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
            this.progressObserver = progressObserver ?? throw new ArgumentNullException(nameof(progressObserver));
        }

        protected override string ArtifactName => Path.GetFileNameWithoutExtension(assetName);

        protected override async Task ExecuteCore()
        {
            if (Directory.Exists(ArtifactPath))
            {
                Log.Warning("{Pack} was already downloaded. Skipping download.", ArtifactPath);
                return;
            }

            var repoInf = GitHubMixin.GetRepoInfo(repoUrl);
            var releases = await gitHubClient.Repository.Release.GetAll(repoInf.Owner, repoInf.Repository);
            var latestRelease = releases.OrderByDescending(x => x.CreatedAt).First();
            var asset = latestRelease.Assets.First(x => string.Equals(x.Name, assetName, StringComparison.OrdinalIgnoreCase));

            using (var stream = await downloader.GetStream(asset.BrowserDownloadUrl, progressObserver))
            {
                await extractor.ExtractFirstChildToFolder(stream, ArtifactPath, progressObserver);
            }

            SaveMetadata(new
            {
                asset.Name,
                asset.Id,
                asset.Size,
                asset.UpdatedAt,
                asset.Label,
                asset.Url,
            });
        }
    }
}