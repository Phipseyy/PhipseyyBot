namespace Phipseyy.Common.Exceptions;

public class ConfigFileNotFound : Exception
{
    public ConfigFileNotFound()
        : base($"[Error] The configuration file 'config.json' was not found and is not optional. The expected physical path was '{AppContext.BaseDirectory}config.json'.")
    {
    }
}