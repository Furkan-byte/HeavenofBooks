using HeavenofBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenofBooks.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader item);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus=null);
        void UpdateSessionId(int id, string sessionId);
        void UpdatePaymentIntentId(int id, string paymentIntentId);
    }
}
