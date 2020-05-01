using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClerkBot.Helpers.temp
{
    public interface IHandleWorkflowFormAdded
    {
        /// <summary>
        /// the engine has applied a new workflow form - this is your opportunity to modify the form.
        /// for example you may want to assign default answers based on business logic and lookups against the flow
        /// </summary>
        /// <param name="pluginHelper"></param>
        /// <param name="flowContext"></param>
        /// <param name="formContext"></param>
        /// <param name="formTask"></param>
        /// <returns></returns>
        void Handle(IAmAPluginHelper pluginHelper, IFlowContext flowContext, IFormContext formContext, TaskDef formTask);
    }
}
