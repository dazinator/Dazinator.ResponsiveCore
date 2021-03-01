using System.Threading;
using System.Threading.Tasks;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService.Requirements
{
    /// <summary>
    /// Some requirement that must be satisfied.
    /// </summary>
    public interface IRequirement
    {
        Task<bool> IsSatisfied(CancellationToken cancellationToken);
    }
}