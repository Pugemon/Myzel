namespace Myzel.Lib.FileFormats.Bcsv.Attributes;

/// <summary>
/// Instructs the BCSV serializer to ignore this class property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BcsvIgnoreAttribute : Attribute
{ }