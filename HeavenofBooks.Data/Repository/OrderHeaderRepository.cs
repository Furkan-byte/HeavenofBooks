﻿using HeavenofBooks.DataAccess.Data;
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

        public void UpdateStripePaymentId(int id, string sessionId, string? paymentIntentId)
        {
            var orderfromDb = _context.OrderHeaders.FirstOrDefault(u => u.Id == id);
            orderfromDb.PaymentDate = DateTime.Now;
            orderfromDb.SessionId = sessionId;
            orderfromDb.PaymentIntentId = paymentIntentId;
        }
    }
}
