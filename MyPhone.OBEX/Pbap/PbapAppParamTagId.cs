namespace GoodTimeStudio.MyPhone.OBEX.Pbap
{
    public enum PbapAppParamTagId : byte
    {
        Order = 0x01,
        SearchValue,
        SearchProperty,
        MaxListCount,
        ListStartOffset,
        PropertySelector,
        Format,
        PhonebookSize,
        NewMissedCalls,
        PrimaryFolderVersion,
        SecondaryFolderVersion,
        VCardSelector,
        DatabaseIdentifier,
        VCardSelectorOperator,
        ResetNewMissedCalls,
        PbapSupportedFeatures
    }
}
