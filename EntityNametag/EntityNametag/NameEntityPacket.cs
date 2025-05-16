using ProtoBuf;

namespace EntityNametag;

[ProtoContract]
public class NameEntityPacket
{
    [ProtoMember(1)] public required long EntityId;
    [ProtoMember(2)] public required string NewName;
    [ProtoMember(3)] public required bool ShouldHaveOwnership;
}