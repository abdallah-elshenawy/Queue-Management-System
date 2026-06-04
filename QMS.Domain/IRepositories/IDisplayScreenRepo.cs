using QMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace QMS.Domain.IRepositories
{
    public interface IDisplayScreenRepo : IBaseRepository<DisplayScreen>
    {
        public Task<List<DisplayTicket>> GetDisplayData(int branchId);
    }
}
