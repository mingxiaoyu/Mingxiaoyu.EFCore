using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mingxiaoyu.EFCore.Audit
{
    public interface IUserContext
    {
        string CurrentUser { get; }
    }
}
