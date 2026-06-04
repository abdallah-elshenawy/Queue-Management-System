using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.Enums
{
    public enum TicketStatus
    {
        Waiting,
        Serving,
        Completed,
        Skipped,
        Requeued
    }
}
