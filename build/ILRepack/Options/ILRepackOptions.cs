using JetBrains.Annotations;
using ModularPipelines.Attributes;
using ModularPipelines.Options;

namespace Build.ILRepack.Options;

[PublicAPI]
[Serializable]
public sealed record IlRepackOptions : CommandLineToolOptions
{
    [CliFlag("/help")]
    public bool? Help { get; init; }

    [CliOption("@", CustomSeparator = "")]
    public string? ResponseFile { get; init; }

    [CliOption("/out", CustomSeparator = ":")]
    public string? Output { get; init; }

    [CliOption("/log", CustomSeparator = ":")]
    public string? LogFile { get; init; }

    [CliFlag("/verbose")]
    public bool? Verbose { get; init; }

    [CliFlag("/wildcards")]
    public bool? Wildcards { get; init; }

    [CliOption("/lib", CustomSeparator = ":")]
    public IEnumerable<string>? SearchDirectories { get; init; }

    [CliOption("/target", CustomSeparator = ":")]
    public string? TargetKind { get; init; }

    [CliOption("/ver", CustomSeparator = ":")]
    public string? Version { get; init; }

    [CliOption("/keyfile", CustomSeparator = ":")]
    public string? KeyFile { get; init; }

    [CliOption("/keycontainer", CustomSeparator = ":")]
    public string? KeyContainer { get; init; }

    [CliFlag("/delaysign")]
    public bool? DelaySign { get; init; }

    [CliFlag("/internalize")]
    public bool? Internalize { get; init; }

    [CliOption("/internalizeassembly", CustomSeparator = ":")]
    public IEnumerable<string>? InternalizeAssemblies { get; init; }

    [CliOption("/internalize", CustomSeparator = ":")]
    public string? InternalizeExcludeFile { get; init; }

    [CliFlag("/renameinternalized")]
    public bool? RenameInternalized { get; init; }

    [CliFlag("/excludeinternalizeserializable")]
    public bool? ExcludeInternalizeSerializable { get; init; }

    [CliOption("/allowdup", CustomSeparator = ":")]
    public IEnumerable<string>? AllowDuplicatesForTypes { get; init; }

    [CliFlag("/allowdup")]
    public bool? AllowDuplicatesAll { get; init; }

    [CliFlag("/union")]
    public bool? Union { get; init; }

    [CliOption("/repackdrop", CustomSeparator = ":")]
    public string? RepackDropAttribute { get; init; }

    [CliFlag("/allowduplicateresources")]
    public bool? AllowDuplicateResources { get; init; }

    [CliFlag("/noRepackRes")]
    public bool? NoRepackResource { get; init; }

    [CliFlag("/copyattrs")]
    public bool? CopyAttributes { get; init; }

    [CliOption("/attr", CustomSeparator = ":")]
    public string? AttributesAssembly { get; init; }

    [CliFlag("/allowMultiple")]
    public bool? AllowMultipleAttributes { get; init; }

    [CliOption("/targetplatform", CustomSeparator = ":")]
    public string? TargetPlatform { get; init; }

    [CliFlag("/keepotherversionreferences")]
    public bool? KeepOtherVersionReferences { get; init; }

    [CliFlag("/preservetimestamp")]
    public bool? PreserveTimestamp { get; init; }

    [CliFlag("/skipconfig")]
    public bool? SkipConfig { get; init; }

    [CliFlag("/illink")]
    public bool? ILLink { get; init; }

    [CliFlag("/xmldocs")]
    public bool? XmlDocs { get; init; }

    [CliFlag("/ndebug")]
    public bool? NoDebugSymbols { get; init; }

    [CliFlag("/zeropekind")]
    public bool? ZeroPeKind { get; init; }

    [CliFlag("/index")]
    public bool? IndexDebugInfo { get; init; }

    [CliFlag("/parallel")]
    public bool? Parallel { get; init; }

    [CliFlag("/pause")]
    public bool? Pause { get; init; }

    [CliFlag("/usefullpublickeyforreferences")]
    public bool? UseFullPublicKeyForReferences { get; init; }

    [CliFlag("/align")]
    public bool? Align { get; init; }

    [CliFlag("/closed")]
    public bool? Closed { get; init; }

    [CliArgument(0, Placement = ArgumentPlacement.AfterOptions)]
    public string? PrimaryAssembly { get; init; }

    [CliArgument(1, Placement = ArgumentPlacement.AfterOptions)]
    public IEnumerable<string>? OtherAssemblies { get; init; }
}