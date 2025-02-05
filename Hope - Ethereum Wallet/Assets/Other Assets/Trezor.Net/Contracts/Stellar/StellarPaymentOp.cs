namespace Trezor.Net.Contracts.Stellar
{
    [ProtoBuf.ProtoContract()]
    public class StellarPaymentOp : ProtoBuf.IExtensible
    {
        private ProtoBuf.IExtension __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoBuf.ProtoMember(1, Name = @"source_account")]
        [System.ComponentModel.DefaultValue("")]
        public string SourceAccount
        {
            get { return __pbn__SourceAccount ?? "";
            } set { __pbn__SourceAccount = value; }
        }
        public bool ShouldSerializeSourceAccount() => __pbn__SourceAccount != null;
        public void ResetSourceAccount() => __pbn__SourceAccount = null;
        private string __pbn__SourceAccount;

        [ProtoBuf.ProtoMember(2, Name = @"destination_account")]
        [System.ComponentModel.DefaultValue("")]
        public string DestinationAccount
        {
            get { return __pbn__DestinationAccount ?? "";
            } set { __pbn__DestinationAccount = value; }
        }
        public bool ShouldSerializeDestinationAccount() => __pbn__DestinationAccount != null;
        public void ResetDestinationAccount() => __pbn__DestinationAccount = null;
        private string __pbn__DestinationAccount;

        [ProtoBuf.ProtoMember(3, Name = @"asset")]
        public StellarAssetType Asset { get; set; }

        [ProtoBuf.ProtoMember(4, Name = @"amount", DataFormat = ProtoBuf.DataFormat.ZigZag)]
        public long Amount
        {
            get { return __pbn__Amount.GetValueOrDefault();
            } set { __pbn__Amount = value; }
        }
        public bool ShouldSerializeAmount() => __pbn__Amount != null;
        public void ResetAmount() => __pbn__Amount = null;
        private long? __pbn__Amount;

    }
}