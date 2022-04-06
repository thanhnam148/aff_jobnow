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
    public interface ILoanService : IEntityService<tblLoan>
    {
        List<LoanModel> RetrieveLoansByUser(string affCode);
        List<LoanModel> SearchLoans(string textSearch, int currentPage, int pageSize, string affCode, string sortField, string sortType, out int totalPage, DateTime? fromDate = null, DateTime? toDate = null);
        List<LoanModel> SearchLoansAdmin(string textSearch, int currentPage, int pageSize, string affCode, string sortField, string sortType, out int totalPage, int? status = null, DateTime? fromDate = null, DateTime? toDate = null);

    }

    public class LoanService : EntityService<tblLoan>, ILoanService
    {
        private readonly ILoanRepository _loanRepository;
        public LoanService(IUnitOfWork unitOfWork, ILoanRepository loanRepository)
            : base(unitOfWork, loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public List<LoanModel> RetrieveLoansByUser(string affCode)
        {
            var loans = GetAll().Where(t => t.AffCode == affCode);
            if (loans != null && loans.Any())
                return loans.ToList().MapToModels();
            return null;
        }

        public List<LoanModel> SearchLoans(string textSearch, int currentPage, int pageSize, string affCode, string sortField, string sortType, out int totalPage, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var transactionEntities = _loanRepository.SearchLoansByUser(textSearch, currentPage, pageSize, affCode, sortField, sortType, out totalPage, fromDate, toDate);
            if (transactionEntities != null)
            {
                return transactionEntities.MapToModels();
            }
            return null;
        }

        public List<LoanModel> SearchLoansAdmin(string textSearch, int currentPage, int pageSize, string affCode, string sortField, string sortType, out int totalPage, int? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            if (fromDate.HasValue)
            {
                fromDate = new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 1, 1, 1);
            }
            if (toDate.HasValue)
            {
                toDate = new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, 23, 59, 59);
            }
            var transactionEntities = _loanRepository.SearchLoansByAdmin(textSearch, currentPage, pageSize, sortField, sortType, out totalPage, status, fromDate, toDate);
            if (transactionEntities != null)
            {
                return transactionEntities.MapToModels();
            }
            return null;
        }
    }
}
