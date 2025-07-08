using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public enum ServicesErrorTypeEnum
    {
        None = 0,
        Warning = 100,
        PartialData = 300,
        ApplicationError = 500,
        UserRequestError = 401,
        OtherErrors = 400,
        DataUnavailable = 404,
        AuthenticationError = 403,
        SubscriptionError = 501
    }
}
