using HeavenofBooks.DataAccess.Data;
using HeavenofBooks.DataAccess.Repository.IRepository;
using HeavenofBooks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeavenofBooks.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader> ,IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderHeader item)
        {
            _context.OrderHeaders.Update(item);

        }

        public void UpdateStatus(int id, string orderStatus, string? paymentstatus=null)
        {
            var orderfromDb = _context.OrderHeaders.FirstOrDefault(u=>u.Id == id);
            if (orderStatus!=null)
            {
                orderfromDb.OrderStatus = orderStatus;
                if (paymentstatus!=null)
                {
                    orderfromDb.PaymentStatus = paymentstatus;

                }
            }
        }

        public void UpdateSessionId(int id, string sessionId)
        {
            var orderfromDb = _context.OrderHeaders.FirstOrDefault(u => u.Id == id);
            orderfromDb.SessionId = sessionId;
        }

        public void UpdatePaymentIntentId(int id,string paymentIntentId)
        {
            var orderfromDb = _context.OrderHeaders.FirstOrDefault(u => u.Id == id);
            orderfromDb.PaymentIntentId = paymentIntentId;
            orderfromDb.PaymentDate = DateTime.Now;
        }
    }
}
