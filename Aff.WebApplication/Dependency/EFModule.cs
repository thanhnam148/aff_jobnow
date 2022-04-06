using Aff.DataAccess;
using Aff.DataAccess.Common;
using Autofac;
namespace Aff.WebApplication.Dependency
{
    public class EFModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule(new RepositoryModule());

            builder.RegisterType(typeof(TimaAffiliateEntities)).InstancePerRequest();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerRequest();
        }
    }
}