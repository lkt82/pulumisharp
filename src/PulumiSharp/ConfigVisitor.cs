using Pulumi;
using PulumiSharp.Reflection;
using System.Reflection;
using System.Text.Json;

namespace PulumiSharp;

internal class ConfigVisitor: MemberVisitor<object?>
{
    public Dictionary<string, Config> Configs = new();

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public override object? VisitProperty(PropertyInfo propertyInfo)
    {
        var configAttribute = propertyInfo.GetCustomAttribute<ConfigAttribute>();

        if (configAttribute != null && !string.IsNullOrEmpty(configAttribute.Key))
        {
            Config config;
            if (string.IsNullOrEmpty(configAttribute.Name))
            {
                var name = propertyInfo.DeclaringType!.GetCustomAttribute<ConfigAttribute>()?.Name;
                config = GetConfig(name ?? throw new InvalidOperationException());
            }
            else
            {
                config = GetConfig(configAttribute.Name);
            }

            if (configAttribute.Secret && IsOutput(propertyInfo))
            {
                var type = propertyInfo.PropertyType.GenericTypeArguments.First();

                var typeCode = Type.GetTypeCode(type);

                switch (typeCode)
                {
                    case TypeCode.String:
                        return config.GetSecret(configAttribute.Key);
                    case TypeCode.Boolean:
                        return config.GetSecretBoolean(configAttribute.Key);
                    case TypeCode.Int32:
                        return config.GetSecretInt32(configAttribute.Key);
                    case TypeCode.Double:
                        return config.GetSecretDouble(configAttribute.Key);
                    case TypeCode.Object:
                        return GetSecretObject(type, config, configAttribute.Key);
                    default:
                        throw new NotSupportedException();
                }
            }
            else
            {
                var type = propertyInfo.PropertyType;

                var typeCode = Type.GetTypeCode(type);

                switch (typeCode)
                {
                    case TypeCode.String:
                        return config.Get(configAttribute.Key);
                    case TypeCode.Boolean:
                        return config.GetBoolean(configAttribute.Key);
                    case TypeCode.Int32:
                        return config.GetInt32(configAttribute.Key);
                    case TypeCode.Double:
                        return config.GetDouble(configAttribute.Key);
                    case TypeCode.Object:
                        return GetObject(type, config, configAttribute.Key);
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        return null;
    }

    private Config GetConfig(string name)
    {
        if (!Configs.TryGetValue(name, out var config))
        {
            config = new Config(name);
            Configs.Add(name, config);
        }

        return config;
    }

    private T? GetObjectGeneric<T>(Config config,string key)
    {
        var value = config.Get(key);
        return value != null ? JsonSerializer.Deserialize<T>(value, _options) : default;
    }

    private Output<T>? GetObjectSecretGeneric<T>(Config config, string key)
    {
        var secret = config.GetSecret(key);
        if (secret != null)
        {
            return secret.Apply(c => JsonSerializer.Deserialize<T>(c, _options))!;
        }

        return null;
    }

    private readonly MethodInfo _getObjectMethod;
    private readonly MethodInfo _getSecretObjectMethod;

    public ConfigVisitor()
    {
        _getObjectMethod = GetType().GetMethod(nameof(GetObjectGeneric), BindingFlags.Instance | BindingFlags.NonPublic)!;
        _getSecretObjectMethod = GetType().GetMethod(nameof(GetObjectSecretGeneric), BindingFlags.Instance | BindingFlags.NonPublic)!;
    }

    private object? GetObject(Type type, Config config,string key)
    {
        return _getObjectMethod.MakeGenericMethod(type).Invoke(this, [config, key]);
    }

    private object? GetSecretObject(Type type, Config config, string key)
    {
        return _getSecretObjectMethod.MakeGenericMethod(type).Invoke(this, [config, key]);
    }

    private static bool IsOutput(PropertyInfo propertyInfo)
    {
        return propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Output<>);
    }

    public override object? VisitType(Type type)
    {
        var configAttribute = type.GetCustomAttribute<ConfigAttribute>();

        if (configAttribute != null && !string.IsNullOrEmpty(configAttribute.Key))
        {
            var config = GetConfig(configAttribute.Name ?? throw new InvalidOperationException());

            var key = configAttribute.Key;

            return configAttribute.Secret ? GetSecretObject(type, config, key) : GetObject(type, config, key);
        }

        var dto = Activator.CreateInstance(type);

        foreach (var propertyInfo in type.GetAllProperties())
        {
            var propertyConfigAttribute = propertyInfo.GetCustomAttribute<ConfigAttribute>();

            if (propertyConfigAttribute != null)
            {
                var value2 = propertyInfo.Accept(this);

                propertyInfo.SetValue(dto, value2);

                continue;
            }

            configAttribute = propertyInfo.PropertyType.GetCustomAttribute<ConfigAttribute>();

            if (configAttribute != null)
            {
                var value2 = propertyInfo.PropertyType.Accept(this);

                propertyInfo.SetValue(dto, value2);
            }

            if (IsOutput(propertyInfo))
            {
                var outputType = propertyInfo.PropertyType.GenericTypeArguments.First();

                configAttribute = outputType.GetCustomAttribute<ConfigAttribute>();

                if (configAttribute != null)
                {
                    var value2 = outputType.Accept(this);

                    propertyInfo.SetValue(dto, value2);
                }
            }
        }


        return dto;
    }
}