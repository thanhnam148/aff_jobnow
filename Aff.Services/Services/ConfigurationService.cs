using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aff.Services
{
    public interface IConfigurationService : IEntityService<Configuration>
    {
        int GetAcceptanceBalancePercentage(bool isAdminProcess);
        decimal GetExchangeRate();
        Dictionary<string, object> GetConfig(bool isAdminProcess = true);
        ConfigurationModel GetConfiguration();
        bool UpdateConfiguration(ConfigurationModel model);
    }
    public class ConfigurationService : EntityService<Configuration>, IConfigurationService
    {
        private readonly IConfigurationRepository _configurationRepository;
        public ConfigurationService(IUnitOfWork unitOfWork, IConfigurationRepository configurationRepository)
            : base(unitOfWork, configurationRepository)
        {
            _configurationRepository = configurationRepository;
        }

        public int GetAcceptanceBalancePercentage(bool isAdminProcess)
        {
            var config = _configurationRepository.GetAll().FirstOrDefault();

            if (config == null)
                return 0;
            return isAdminProcess ? config.AdminAcceptanceBalancePercentage
                : config.ClientAcceptanceBalancePercentage;
        }

        public decimal GetExchangeRate()
        {
            var config = _configurationRepository.GetAll().FirstOrDefault();

            if (config == null)
                return 0;
            return config.ExchangeRate ?? 0;
        }

        public Dictionary<string, object> GetConfig(bool isAdminProcess = true)
        {
            var resultDict = new Dictionary<string, object>();
            var config = _configurationRepository.GetAll().FirstOrDefault();

            if (config == null)
                return null;
            var acceptanceBalance = isAdminProcess ? config.AdminAcceptanceBalancePercentage
                 : config.ClientAcceptanceBalancePercentage;
            resultDict.Add("AcceptanceBalancePercentage", acceptanceBalance);
            resultDict.Add("ExchangeRate", config.ExchangeRate);
            return resultDict;
        }

        public ConfigurationModel GetConfiguration()
        {
            var configuration = _configurationRepository.GetAll().FirstOrDefault();
            return configuration != null ? configuration.MaptoModel() : new ConfigurationModel();
        }

        public bool UpdateConfiguration(ConfigurationModel model)
        {
            try
            {
                var configuration = _configurationRepository.GetAll().FirstOrDefault();
                if (configuration == null)
                {
                    configuration = new Configuration()
                    {
                        AdminAcceptanceBalancePercentage = model.AdminAcceptanceBalancePercentage,
                        ClientAcceptanceBalancePercentage = model.ClientAcceptanceBalancePercentage,
                        ExchangeRate = model.ExchangeRate
                    };
                    _configurationRepository.Insert(configuration);
                    UnitOfWork.SaveChanges();
                    return true;
                }
                configuration.AdminAcceptanceBalancePercentage = model.AdminAcceptanceBalancePercentage;
                configuration.ClientAcceptanceBalancePercentage = model.ClientAcceptanceBalancePercentage;
                configuration.ExchangeRate = model.ExchangeRate;
                UnitOfWork.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return false;
            }
        }
    }

}
