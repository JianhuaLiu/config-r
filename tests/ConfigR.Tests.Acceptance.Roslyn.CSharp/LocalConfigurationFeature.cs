﻿// <copyright file="LocalConfigurationFeature.cs" company="ConfigR contributors">
//  Copyright (c) ConfigR contributors. (configr.net@gmail.com)
// </copyright>

namespace ConfigR.Tests.Acceptance.Roslyn.CSharp
{
    using System;
    using System.IO;
    using ConfigR.Tests.Acceptance.Roslyn.CSharp.Support;
    using FluentAssertions;
    using Xbehave;
    using Xunit;

    public static class LocalConfigurationFeature
    {
        [Scenario]
        public static void RetrievingAnObject(Foo result)
        {
            dynamic config = null;

            "Given a local config file containing a Foo with a Bar of 'baz'"
                .x(c =>
                {
                    var code =
@"using ConfigR.Tests.Acceptance.Roslyn.CSharp.Support;
Config.Foo = new Foo { Bar = ""baz"" };
";

                    ConfigFile.Create(code).Using(c);
                });

            "When I load the config"
                .x(async () => config = await new Config().UseRoslynCSharpLoader().LoadDynamic());

            "And I get the Foo"
                .x(() => result = config.Foo<Foo>());

            "Then the Foo has a Bar of 'baz'"
                .x(() => result.Bar.Should().Be("baz"));
        }

        [Scenario]
        public static void ScriptFailsToCompile(Exception exception)
        {
            "Given a local config file which fails to compile"
                .x(c => ConfigFile.Create(@"This is not C#!").Using(c));

            "When I load the config file"
                .x(async () => exception = await Record.ExceptionAsync(async () => await new Config().UseRoslynCSharpLoader().LoadDynamic()));

            "Then an exception is thrown"
                .x(() => exception.Should().NotBeNull());

            "And the exception should be a compilation error exception"
                .x(() => exception.GetType().Name.Should().Be("CompilationErrorException"));
        }

        [Scenario]
        public static void ScriptFailsToExecute(Exception exception)
        {
            "Given a local config file which throws an exception with the message 'Boo!'"
                .x(c => ConfigFile.Create(@"throw new System.Exception(""Boo!"");").Using(c));

            "When I load the config file"
                .x(async () => exception = await Record.ExceptionAsync(async () => await new Config().UseRoslynCSharpLoader().LoadDynamic()));

            "Then an exception is thrown"
                .x(() => exception.Should().NotBeNull());

            "And the exception message is 'Boo!'"
                .x(() => exception.Message.Should().Be("Boo!"));
        }

        [Scenario]
        public static void ConfigurationFileIsNull(Exception exception)
        {
            "Given the app domain configuration file is null"
                .x(c => AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", null))
                .Teardown(() => AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", Path.GetFileName(ConfigFile.GetDefaultPath())));

            "When I load the config file"
                .x(async () => exception = await Record.ExceptionAsync(async () => await new Config().UseRoslynCSharpLoader().LoadDynamic()));

            "Then an invalid operation exception is thrown"
                .x(() => exception.Should().NotBeNull());

            "And the exception message tells us that the app domain config file is null"
                .x(() => exception.Message.Should().Be("AppDomain.CurrentDomain.SetupInformation.ConfigurationFile is null."));
        }
    }
}
