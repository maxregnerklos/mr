﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Deployer.Core;
using Deployer.Core.Interaction;
using Deployer.Gui.ViewModels.Common;
using Grace.DependencyInjection.Attributes;
using ReactiveUI;
using Serilog;
using Zafiro.Core.UI;

namespace Deployer.Gui.ViewModels.Sections
{
    [Metadata("Name", "Main")]
    [Metadata("Order", 1)]
    public class DeviceDeploymentViewModel : ReactiveObject, ISection
    {
        private readonly DeviceDeployer deployer;
        private readonly IDeviceRepository deviceRepository;
        private readonly IDialogService dialogService;
        private readonly CompositeDisposable disposables = new CompositeDisposable();
        private Deployment deployment;
        private ObservableAsPropertyHelper<IEnumerable<Deployment>> devices;

        public DeviceDeploymentViewModel(DeviceDeployer deployer, IDialogService dialogService,
            IDeviceRepository deviceRepository, OperationProgressViewModel operationProgress)
        {
            this.deployer = deployer;
            this.dialogService = dialogService;
            this.deviceRepository = deviceRepository;
            OperationProgress = operationProgress;

            ConfigureCommands();

            IsBusyObservable = Deploy.IsExecuting;
        }

        public OperationProgressViewModel OperationProgress { get; }

        public Deployment Deployment
        {
            get => deployment;
            set => this.RaiseAndSetIfChanged(ref deployment, value);
        }

        public ReactiveCommand<Unit, Unit> Deploy { get; set; }

        public IEnumerable<Deployment> Deployments => devices.Value;

        public ReactiveCommand<Unit, DeployerStore> FetchDevices { get; set; }

        public IObservable<bool> IsBusyObservable { get; }

        private void ConfigureCommands()
        {
            ConfigureDeployCommand();
            ConfigureFetchDevices();
        }

        private void ConfigureFetchDevices()
        {
            FetchDevices = ReactiveCommand.CreateFromTask(() => deviceRepository.Get());
            devices = FetchDevices.Select(x => x.Deployments).ToProperty(this, model => model.Deployments);
            dialogService.HandleExceptionsFromCommand(FetchDevices, "Error", "Cannot fetch supported devices");
        }
        
        private void ConfigureDeployCommand()
        {
            var hasDevice = this.WhenAnyValue(model => model.Deployment).Select(d => d != null);
            Deploy = ReactiveCommand.CreateFromTask(
                () => deployer.Deploy(Deployment.ScriptPath, Deployment.Devices.First()), hasDevice);
            dialogService.HandleExceptionsFromCommand(Deploy, exception =>
            {
                Log.Error(exception, exception.Message);

                if (exception is DeploymentCancelledException)
                {
                    return ("Deployment cancelled", "Deployment cancelled");
                }

                return ("Deployment failed", exception.Message);
            });

            Deploy.OnSuccess(() => dialogService.Notice("Deployment finished", "Deployment finished"))
                .DisposeWith(disposables);
        }
    }
}