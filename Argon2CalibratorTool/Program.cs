﻿using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Linq;
using Argon2CalibratorTool.Argon;
using Argon2CalibratorTool.Utilities;

namespace Argon2CalibratorTool
{
    partial class Program
    {
        static void Main(string[] args)
        {
            var app = new CommandLineApplication();

            app.Description = "Perform an Argon2 calibration to determine the best combination of iterations, memory usage, and parallel threads for password storage using Argon2.";
            app.HelpOption("-?|-h|--help");

            var timeOption = app.Option(
                "-t|--time",
                "The maximum time in milliseconds it should take to calculate the password hash. Defaults to 1000.",
                CommandOptionType.SingleValue
                );
            var parallelismOption = app.Option(
                "-p|--parallelism",
                "The degree of parallelism. Defaults to twice the number of CPU cores.",
                CommandOptionType.SingleValue
                );
            var iterationsOption = app.Option(
                "-i|--iterations",
                "The minimum number of iterations. Defaults to 2.",
                CommandOptionType.SingleValue
                );
            var maxMemoryOption = app.Option(
                "-m|--memory",
                "The max amount of memory supported to count down from. Default begins at 1024KB and increases instead.",
                CommandOptionType.SingleValue
                );
            var modeOption = app.Option(
                "--mode",
                "The mode of operation. The default is Argon2id. Advanced usage only.",
                CommandOptionType.SingleValue
                );
            var saltLengthOption = app.Option(
                "--saltlength",
                "The length of the salt and password, in bytes. Defaults to 16. Advanced usage only.",
                CommandOptionType.SingleValue
                );
            var hashLengthOption = app.Option(
                "--hashlength",
                "The length of the hash, in bytes. Defaults to 16. Advanced usage only.",
                CommandOptionType.SingleValue
                );

            app.OnExecute(() =>
            {
                var factory = new Argon2Factory();
                var logger = new Argon2Logger();
                var input = new Argon2CalibrationInput()
                {
                    MaximumTime = ReadOption(timeOption, () => 1000),
                    DegreeOfParallelism = ReadOption(parallelismOption, () => SystemManagement.GetTotalCpuCores() * 2),
                    MinimumIterations = ReadOption(iterationsOption, () => 2),
                    MaximumMemory = ReadOption(maxMemoryOption, () => -1),
                    Mode = ReadOption(modeOption, () => Argon2Mode.Argon2id),
                    SaltAndPasswordLength = ReadOption(saltLengthOption, () => 16),
                    HashLength = ReadOption(hashLengthOption, () => 16)
                };

                var calibrator = new Argon.Argon2Calibrator(factory, logger, input);
                var results = calibrator.Run();

                logger.WriteLine();
                logger.WriteLine("Best results:");
                results.ToList().ForEach(result => logger.WriteCalibrationResult(result));

                return 0;
            });

            app.Execute(args);
        }

        private static long ReadOption(CommandOption option, Func<long> defaultValue)
        {
            if (!option.HasValue())
                return defaultValue();

            return long.Parse(option.Value());
        }

        private static int ReadOption(CommandOption option, Func<int> defaultValue)
        {
            if (!option.HasValue())
                return defaultValue();

            return int.Parse(option.Value());
        }

        private static TEnum ReadOption<TEnum>(CommandOption option, Func<TEnum> defaultValue)
            where TEnum : struct
        {
            if (!option.HasValue())
                return defaultValue();

            return Enum.Parse<TEnum>(option.Value());
        }
    }
}
