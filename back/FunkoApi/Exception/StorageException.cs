namespace FunkoApi.exception;

/// <summary>
/// Excepción lanzada cuando el archivo excede el tamaño máximo permitido
/// </summary>
public class FileSizeExceededException : Exception
{
    public long MaxSize { get; }
    public long ActualSize { get; }

    public FileSizeExceededException(string message) : base(message) { }

    public FileSizeExceededException(string message, long maxSize, long actualSize)
        : base(message)
    {
        MaxSize = maxSize;
        ActualSize = actualSize;
    }
}

/// <summary>
/// Excepción lanzada cuando el tipo de archivo no está permitido
/// </summary>
public class InvalidFileTypeException : Exception
{
    public string? FileType { get; }
    public string[]? AllowedTypes { get; }

    public InvalidFileTypeException(string message) : base(message) { }

    public InvalidFileTypeException(string message, string fileType, string[] allowedTypes)
        : base(message)
    {
        FileType = fileType;
        AllowedTypes = allowedTypes;
    }
}

/// <summary>
/// Excepción lanzada cuando el archivo no existe
/// </summary>
public class FileNotFoundStorageException : Exception
{
    public string FileName { get; }

    public FileNotFoundStorageException(string fileName)
        : base($"Archivo no encontrado: {fileName}")
    {
        FileName = fileName;
    }
}

/// <summary>
/// Excepción lanzada cuando hay un error de almacenamiento
/// </summary>
public class StorageException : Exception
{
    public string Operation { get; }

    public StorageException(string message, string operation) : base(message)
    {
        Operation = operation;
    }
}