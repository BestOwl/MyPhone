using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodTimeStudio.MyPhone.Data
{
    public interface IIdentifiable<TKey>
    {
        TKey Id { get; set; }
    }
}
