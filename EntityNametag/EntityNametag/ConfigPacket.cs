using ProtoBuf;

namespace EntityNametag;

[ProtoContract]
public class ConfigPacket
{
    [ProtoMember(1)] public required string[] NotApplicableToEntityClasses;
}