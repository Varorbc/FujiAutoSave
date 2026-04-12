namespace FujiAutoSave.Core.Enums;

public enum PtpResponseCode
{
    Unknown = 0,

    OK = 8193,

    GeneralError = 8194,

    SessionNotOpen = 8195,

    InvalidTransactionId = 8196,

    IncompleteTransfer = 8197,

    InvalidStorageId = 8198,

    InvalidImageIndex = 8199,

    DevicePropNotSupported = 8200,

    InvalidImageFormatCode = 8201,

    StoreFull = 8202,

    ImageWriteProtected = 8203,

    StoreReadOnly = 8204,

    AccessDenied = 8205,

    NoThumbnailPresent = 8206,

    SelfTestFailed = 8207,

    PartialDeletion = 8208,

    StoreNotAvailable = 8209,

    SpecificationByFormatUnsupported = 8210,

    NoValidImageInfo = 8211,

    InvalidCodeFormat = 8212,

    UnknownVendorCode = 8213,

    CaptureAlreadyTerminated = 8214,

    DeviceBusy = 8215,

    InvalidParentImage = 8216,

    InvalidDevicePropFormat = 8217,

    InvalidDevicePropValue = 8218,

    InvalidParameter = 8219,

    SessionAlreadyOpened = 8220,

    TransactionCanceled = 8221,

    SpecificationOfDestinationUnsupported = 8222,
}
