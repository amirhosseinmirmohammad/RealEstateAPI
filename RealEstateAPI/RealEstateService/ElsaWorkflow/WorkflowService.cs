//using Elsa.Services;

//namespace RealEstateService.ElsaWorkflow
//{
//    public class WorkflowService : IWorkflowService
//    {
//        private readonly IWorkflowLaunchpad _workflowLaunchpad;

//        public WorkflowService(IWorkflowLaunchpad workflowLaunchpad)
//        {
//            _workflowLaunchpad = workflowLaunchpad;
//        }

//        public async Task StartWorkflowAsync()
//        {
//            var startableWorkflow = await _workflowLaunchpad.FindStartableWorkflowAsync("SpecialOrder", null, null, default);
//            if (startableWorkflow != null)
//            {
//                await _workflowLaunchpad.ExecuteStartableWorkflowAsync(startableWorkflow, new Elsa.Models.WorkflowInput());
//            }
//        }
//    }
//}
