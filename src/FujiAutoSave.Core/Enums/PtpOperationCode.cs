namespace FujiAutoSave.Core.Enums;

public enum PtpOperationCode : ushort
{
    OpenSession = 4098,

    GetImageInfo = 4104,

    GetImage = 4105,

    GetDevicePropValue = 4117,

    SetDevicePropValue = 4118,

    GetPartialImage = 4123,
}
