using ModularPipelines.Attributes;
using ModularPipelines.Options;

namespace Build.ILRepack.Options;

[Serializable]
public sealed record IlRepackOptions() : CommandLineToolOptions("ilrepack")
{
    [BooleanCommandSwitch("/help")]
    public bool? Help { get; init; }

    [CommandSwitch("@", SwitchValueSeparator = "")]
    public string? ResponseFile { get; init; }

    [CommandSwitch("/out", SwitchValueSeparator = ":")]
    public string? Output { get; init; }

    [CommandSwitch("/log", SwitchValueSeparator = ":")]
    public string? LogFile { get; init; }

    [BooleanCommandSwitch("/verbose")]
    public bool? Verbose { get; init; }

    [BooleanCommandSwitch("/wildcards")]
    public bool? Wildcards { get; init; }

    [CommandSwitch("/lib", SwitchValueSeparator = ":")]
    public IEnumerable<string>? SearchDirectories { get; init; }

    [CommandSwitch("/target", SwitchValueSeparator = ":")]
    public string? TargetKind { get; init; }

    [CommandSwitch("/ver", SwitchValueSeparator = ":")]
    public string? Version { get; init; }

    [CommandSwitch("/keyfile", SwitchValueSeparator = ":")]
    public string? KeyFile { get; init; }

    [CommandSwitch("/keycontainer", SwitchValueSeparator = ":")]
    public string? KeyContainer { get; init; }

    [BooleanCommandSwitch("/delaysign")]
    public bool? DelaySign { get; init; }

    [BooleanCommandSwitch("/internalize")]
    public bool? Internalize { get; init; }

    [CommandSwitch("/internalizeassembly", SwitchValueSeparator = ":")]
    public IEnumerable<string>? InternalizeAssemblies { get; init; }

    [CommandSwitch("/internalize", SwitchValueSeparator = ":")]
    public string? InternalizeExcludeFile { get; init; }

    [BooleanCommandSwitch("/renameinternalized")]
    public bool? RenameInternalized { get; init; }

    [BooleanCommandSwitch("/excludeinternalizeserializable")]
    public bool? ExcludeInternalizeSerializable { get; init; }

    [CommandSwitch("/allowdup", SwitchValueSeparator = ":")]
    public IEnumerable<string>? AllowDuplicatesForTypes { get; init; }

    [BooleanCommandSwitch("/allowdup")]
    public bool? AllowDuplicatesAll { get; init; }

    [BooleanCommandSwitch("/union")]
    public bool? Union { get; init; }

    [CommandSwitch("/repackdrop", SwitchValueSeparator = ":")]
    public string? RepackDropAttribute { get; init; }

    [BooleanCommandSwitch("/allowduplicateresources")]
    public bool? AllowDuplicateResources { get; init; }

    [BooleanCommandSwitch("/noRepackRes")]
    public bool? NoRepackResource { get; init; }

    [BooleanCommandSwitch("/copyattrs")]
    public bool? CopyAttributes { get; init; }

    [CommandSwitch("/attr", SwitchValueSeparator = ":")]
    public string? AttributesAssembly { get; init; }

    [BooleanCommandSwitch("/allowMultiple")]
    public bool? AllowMultipleAttributes { get; init; }

    [CommandSwitch("/targetplatform", SwitchValueSeparator = ":")]
    public string? TargetPlatform { get; init; }

    [BooleanCommandSwitch("/keepotherversionreferences")]
    public bool? KeepOtherVersionReferences { get; init; }

    [BooleanCommandSwitch("/preservetimestamp")]
    public bool? PreserveTimestamp { get; init; }

    [BooleanCommandSwitch("/skipconfig")]
    public bool? SkipConfig { get; init; }

    [BooleanCommandSwitch("/illink")]
    public bool? ILLink { get; init; }

    [BooleanCommandSwitch("/xmldocs")]
    public bool? XmlDocs { get; init; }

    [BooleanCommandSwitch("/ndebug")]
    public bool? NoDebugSymbols { get; init; }

    [BooleanCommandSwitch("/zeropekind")]
    public bool? ZeroPeKind { get; init; }

    [BooleanCommandSwitch("/index")]
    public bool? IndexDebugInfo { get; init; }

    [BooleanCommandSwitch("/parallel")]
    public bool? Parallel { get; init; }

    [BooleanCommandSwitch("/pause")]
    public bool? Pause { get; init; }

    [BooleanCommandSwitch("/usefullpublickeyforreferences")]
    public bool? UseFullPublicKeyForReferences { get; init; }

    [BooleanCommandSwitch("/align")]
    public bool? Align { get; init; }

    [BooleanCommandSwitch("/closed")]
    public bool? Closed { get; init; }

    [PositionalArgument]
    public string? PrimaryAssembly { get; init; }

    [PositionalArgument]
    public IEnumerable<string>? OtherAssemblies { get; init; }
}