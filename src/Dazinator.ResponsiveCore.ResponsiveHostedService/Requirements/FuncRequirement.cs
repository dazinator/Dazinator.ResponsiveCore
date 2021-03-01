using System;
using System.Threading;
using System.Threading.Tasks;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService.Requirements
{
    public class FuncRequirement : IRequirement
    {
        private readonly Func<CancellationToken, bool> _check;

        public FuncRequirement(           
            Func<CancellationToken, bool> check)
        {
            _check = check;
        }

        public Task<bool> IsSatisfied(CancellationToken cancellationToken)
        {
            return Task.FromResult(_check?.Invoke(cancellationToken) ?? true);
        }
    }
}