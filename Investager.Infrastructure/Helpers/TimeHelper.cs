using Investager.Core.Interfaces;
using System;

namespace Investager.Infrastructure.Helpers
{
    public class TimeHelper : ITimeHelper
    {
        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
