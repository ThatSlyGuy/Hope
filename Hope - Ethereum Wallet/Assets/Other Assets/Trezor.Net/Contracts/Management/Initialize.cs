// This file was generated by a tool; you should avoid making direct changes.
// Consider using 'partial classes' to extend these types
// Input: messages-management.proto

#pragma warning disable CS1591, CS0612, CS3021, IDE1006
namespace Trezor.Net.Contracts.Management
{

    [ProtoBuf.ProtoContract()]
    public class Initialize : ProtoBuf.IExtensible
    {
        private ProtoBuf.IExtension __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoBuf.ProtoMember(1, Name = @"state")]
        public byte[] State { get; set; }
        public bool ShouldSerializeState() => State != null;
        public void ResetState() => State = null;

        [ProtoBuf.ProtoMember(2, Name = @"skip_passphrase")]
        public bool SkipPassphrase
        {
            get { return __pbn__SkipPassphrase.GetValueOrDefault();
            } set { __pbn__SkipPassphrase = value; }
        }
        public bool ShouldSerializeSkipPassphrase() => __pbn__SkipPassphrase != null;
        public void ResetSkipPassphrase() => __pbn__SkipPassphrase = null;
        private bool? __pbn__SkipPassphrase;

    }
}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006
