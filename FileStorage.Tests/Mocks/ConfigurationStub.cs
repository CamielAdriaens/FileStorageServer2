// FileStorage.Tests/Mocks/ConfigurationStub.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileStorage.Tests.Mocks
{
    public class ConfigurationStub : IConfiguration
    {
        public string this[string key]
        {
            get => key switch
            {
                "JwtSettings:SecretKey" => "YourTestSecretKey",
                "JwtSettings:Issuer" => "YourTestIssuer",
                "JwtSettings:Audience" => "YourTestAudience",
                _ => null
            };
            set => throw new NotImplementedException();
        }

        public IConfigurationSection GetSection(string key) => new ConfigurationSectionStub(key, this[key]);

        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();

        public IChangeToken GetReloadToken() => null;
    }

    public class ConfigurationSectionStub : IConfigurationSection
    {
        private readonly string _value;

        public ConfigurationSectionStub(string key, string value)
        {
            Key = key;
            _value = value;
        }

        public string this[string key] { get => null; set { } }

        public string Key { get; }

        public string Path => Key;

        public string Value
        {
            get => _value;
            set => throw new NotImplementedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();

        public IChangeToken GetReloadToken() => null;

        public IConfigurationSection GetSection(string key) => new ConfigurationSectionStub(key, null);
    }
}
