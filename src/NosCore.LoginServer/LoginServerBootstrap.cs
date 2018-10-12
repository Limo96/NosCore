﻿using System;
using System.IO;
using System.Reflection;
using Autofac;
using DotNetty.Buffers;
using DotNetty.Codecs;
using FastExpressionCompiler;
using log4net;
using log4net.Config;
using Mapster;
using Microsoft.Extensions.Configuration;
using NosCore.Configuration;
using NosCore.Controllers;
using NosCore.Core.Encryption;
using NosCore.Core.Handling;
using NosCore.Core.Serializing;
using NosCore.GameObject.Networking;
using NosCore.Packets.ClientPackets;
using NosCore.Shared.I18N;

namespace NosCore.LoginServer
{
    public static class LoginServerBootstrap
    {
        private const string ConfigurationPath = @"../../../configuration";
        private const string Title = "NosCore - LoginServer";
        const string consoleText = "LOGIN SERVER - NosCoreIO";

        private static LoginConfiguration InitializeConfiguration()
        {
            var builder = new ConfigurationBuilder();
            var loginConfiguration = new LoginConfiguration();
            builder.SetBasePath(Directory.GetCurrentDirectory() + ConfigurationPath);
            builder.AddJsonFile("login.json", false);
            builder.Build().Bind(loginConfiguration);
            LogLanguage.Language = loginConfiguration.Language;
            Logger.Log.Info(LogLanguage.Instance.GetMessageFromKey(LanguageKey.SUCCESSFULLY_LOADED));
            return loginConfiguration;
        }

        private static void InitializeLogger()
        {
            // LOGGER
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("../../configuration/log4net.config"));
            Logger.InitializeLogger(LogManager.GetLogger(typeof(LoginServer)));
        }

        private static void InitializePackets()
        {
            PacketFactory.Initialize<NoS0575Packet>();
        }

        public static void Main()
        {
            Console.Title = Title;
            InitializeLogger();
            Logger.PrintHeader(consoleText);
            InitializePackets();
            var container = InitializeContainer();
            var loginServer = container.Resolve<LoginServer>();
            TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileFast();
            loginServer.Run();
        }

        private static IContainer InitializeContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterInstance(InitializeConfiguration()).As<LoginConfiguration>().As<GameServerConfiguration>();
            containerBuilder.RegisterAssemblyTypes(typeof(DefaultPacketController).Assembly).As<IPacketController>();
            containerBuilder.RegisterType<LoginDecoder>().As<MessageToMessageDecoder<IByteBuffer>>();
            containerBuilder.RegisterType<LoginEncoder>().As<MessageToMessageEncoder<string>>();
            containerBuilder.RegisterType<LoginServer>().PropertiesAutowired();
            containerBuilder.RegisterType<ClientSession>();
            containerBuilder.RegisterType<NetworkManager>();
            containerBuilder.RegisterType<PipelineFactory>();

            return containerBuilder.Build();
        }
    }
}