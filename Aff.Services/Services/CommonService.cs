using EcommerceSystem.DataAccess.Common;
using EcommerceSystem.DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceSystem.Services.Services
{
    public interface ICommonService
    {
        decimal GetServiceFee(int customerId, decimal currentSubTotal, int month, int year);
    }
    public class CommonService : ICommonService
    {
        private readonly IServiceFeeRuleRepository _serviceFeeRuleRepository;
        private readonly ICustomerTransactionByMonthRepository _customerTransactionByMonthRepository;
        public CommonService(IUnitOfWork unitOfWork, IServiceFeeRuleRepository serviceFeeRuleRepository
            , ICustomerTransactionByMonthRepository customerTransactionByMonthRepository)
        {
            _serviceFeeRuleRepository = serviceFeeRuleRepository;
            _customerTransactionByMonthRepository = customerTransactionByMonthRepository;
        }

        public decimal GetServiceFee(int customerId, decimal currentSubTotal, int month, int year)
        {
            decimal totalTransactionAmount = 0;
            var transactionByMonth = _customerTransactionByMonthRepository.Find(x => x.CustomerId == customerId && x.Month == month && x.Year == year);
            if(transactionByMonth != null)
            {
                totalTransactionAmount = transactionByMonth.SubTotalAmount;
            }
            var subTotal = totalTransactionAmount + currentSubTotal;
            var rule = _serviceFeeRuleRepository.Find(x => x.MinOrderTotal <= subTotal && x.MaxOrderTotal > subTotal);
            var fees = rule != null ? rule.ServiceFees : 0;
            return fees;
        }
    }
}
