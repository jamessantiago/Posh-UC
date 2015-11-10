using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AxlNetClient;

namespace Posh_UC
{
    public static partial class Extensions
    {
        public static XFkType NullIfEmpty(this XFkType data)
        {
            return string.IsNullOrEmpty(data.uuid) && string.IsNullOrEmpty(data.Value) ? null : data;
        }
    }
}
