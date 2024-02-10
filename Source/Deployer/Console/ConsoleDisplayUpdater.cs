﻿using System;
using System.Linq;

namespace Deployer.Console
{
    public class ConsoleDisplayUpdater : IDisposable
    {
        private readonly IDisposable progressUpdater;

        public ConsoleDisplayUpdater(OperationProgress progress)
        {
            progressUpdater = progress.Percentage.Subscribe(DisplayProgress);
        }

        public int Width { get; set; } = 50;

        private void DisplayProgress(double progress)
        {
            if (double.IsNaN(progress))
            {
                System.Console.WriteLine();
                return;
            }

            var progressBarLenght = progress * Width;
            System.Console.CursorLeft = 0;
            System.Console.Write("[");
            var bar = new string(Enumerable.Range(1, (int) progressBarLenght).Select(_ => '=').ToArray());
            
            System.Console.Write(bar);
            
            var label = $@"{progress:P0}";
            System.Console.CursorLeft = (Width -label.Length) / 2;
            System.Console.Write(label);
            System.Console.CursorLeft = Width;
            System.Console.Write("]");
        }

        public void Dispose()
        {
            progressUpdater?.Dispose();            
        }
    }
}