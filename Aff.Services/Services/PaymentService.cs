using Aff.DataAccess;
using Aff.DataAccess.Common;
using Aff.DataAccess.Repositories;
using Aff.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Services.Services
{
    public interface IPaymentService : IEntityService<tblPayment>
    {
        List<PaymentModel> RetrievePaymentsByUser(int userId);
        List<PaymentModel> SearchPayments(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, List<int> status, out int totalPage);
        List<PaymentModel> SearchPaymentByStatus(string textSearch, int currentPage, int pageSize, string sortField, string sortType, List<int> status, out int totalPage);
        List<PaymentModel> GetAmountWithDrawStatus(int userId, List<int> status);
        bool CreatePayment(PaymentModel payment);
    }

    public class PaymentService : EntityService<tblPayment>, IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        public PaymentService(IUnitOfWork unitOfWork, IPaymentRepository paymentRepository)
            : base(unitOfWork, paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public List<PaymentModel> RetrievePaymentsByUser(int userId)
        {
            var Payments = GetAll().Where(t => t.UserId == userId);
            if (Payments != null && Payments.Any())
                return Payments.ToList().MapToModels();
            return null;
        }

        public List<PaymentModel> SearchPayments(string textSearch, int currentPage, int pageSize, int userId, string sortField, string sortType, List<int> status, out int totalPage)
        {
            var transactionEntities = _paymentRepository.SearchPaymentByUser(textSearch, currentPage, pageSize, userId, sortField, sortType, status, out totalPage);
            if (transactionEntities != null)
            {
                return transactionEntities.MapToModels();
            }
            return null;
        }

        public List<PaymentModel> SearchPaymentByStatus(string textSearch, int currentPage, int pageSize, string sortField, string sortType, List<int> status, out int totalPage)
        {
            var transactionEntities = _paymentRepository.SearchPaymentByStatus(textSearch, currentPage, pageSize, sortField, sortType, status, out totalPage);
            if (transactionEntities != null)
            {
                return transactionEntities.MapToModels();
            }
            return null;
        }

        public List<PaymentModel> GetAmountWithDrawStatus(int userId, List<int> status)
        {
            var paymentUser = _paymentRepository.GetPaymentByStatus(userId, status);
            if(paymentUser != null && paymentUser.Any())
            {
                return paymentUser.ToList().MapToModels();
            }
            return null;
        }

        public bool CreatePayment(PaymentModel payment)
        {
            _paymentRepository.Insert(payment.MapToEntity());
            UnitOfWork.SaveChanges();

            return true;
        }
    }
}
