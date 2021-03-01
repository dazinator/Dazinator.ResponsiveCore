using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Dazinator.ResponsiveCore.ResponsiveHostedService.Requirements
{
    public class CompositeRequirementBuilder : ICompositeRequirementBuilder
    {

        public CompositeRequirementBuilder()
        {
            IncludedChecks = new List<Func<IServiceProvider, IRequirement>>();
        }

        public CompositeRequirementBuilder Include<TCheck>()
            where TCheck : IRequirement
        {
            IncludedChecks.Add((sp) => ActivatorUtilities.CreateInstance(sp, typeof(TCheck)) as IRequirement);
            return this;
        }

        public CompositeRequirementBuilder Include<TCheck>(Func<IServiceProvider, TCheck> factory)
          where TCheck : IRequirement
        {
            IncludedChecks.Add((sp) => factory.Invoke(sp) as IRequirement);
            return this;
        }

        public CompositeRequirementBuilder IncludeFunc(Func<CancellationToken, bool> check)
        {
            IncludedChecks.Add((sp) => new FuncRequirement(check));
            return this;
        }

        public CompositeRequirementBuilder IncludeFunc(Func<CancellationToken, IServiceProvider, bool> check)
        {
            IncludedChecks.Add((sp) => new FuncRequirementWithServices(check, sp));
            return this;
        }

        private List<Func<IServiceProvider, IRequirement>> IncludedChecks { get; set; }

        public IRequirement Build(IServiceProvider sp)
        {
            var instances = IncludedChecks.Select(a => a?.Invoke(sp));
            return new CompositeRequirement(instances);
        }
    }



}