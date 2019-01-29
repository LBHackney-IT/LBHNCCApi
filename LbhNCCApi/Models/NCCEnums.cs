using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LbhNCCApi.Models
{
    public enum GovNotifierChannelTypes
    {
        DontNeed = 0,
        Email = 1,
        SMS = 2,
        Post = 3
    }

    public enum NotesType
    {
        Manual = 1,
        Automatic = 2,
        Callback = 3,
        ActionDiary = 4
    }

    public enum PaymentStatus
    {
        DontNeed = 0,
        Initiated = 1,
        Failed = 2,
        Successful = 3
    }

    public enum CallbackResponse
    {
        NotAssigned = 0,
        CalledThemBack = 1,
        CouldntGetThrough = 2
    }

}
