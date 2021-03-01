using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService.Requirements
{
    public class FuncRequirementWithServices : IRequirement
    {
        private readonly Func<CancellationToken, IServiceProvider, bool> _check;
        private readonly IServiceProvider _sp;

        public FuncRequirementWithServices(
            Func<CancellationToken, IServiceProvider, bool> check,
            IServiceProvider sp)
        {
            _check = check;
            _sp = sp;
        }

        public Task<bool> IsSatisfied(CancellationToken cancellationToken)
        {
            return Task.FromResult(_check?.Invoke(cancellationToken, _sp) ?? true);
        }
    }
}