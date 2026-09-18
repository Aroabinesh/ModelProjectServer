using ModelProject.Entities.Enums;
using ModelProject.Services.WorkOrders;
using Xunit;

namespace ModelProject.Tests;

public class WorkOrderStatusWorkflowTests
{
    [Theory]
    [InlineData(WorkOrderStatus.New, WorkOrderStatus.Assigned, true)]
    [InlineData(WorkOrderStatus.Assigned, WorkOrderStatus.InProgress, true)]
    [InlineData(WorkOrderStatus.InProgress, WorkOrderStatus.Completed, true)]
    [InlineData(WorkOrderStatus.New, WorkOrderStatus.InProgress, false)]
    [InlineData(WorkOrderStatus.New, WorkOrderStatus.Completed, false)]
    [InlineData(WorkOrderStatus.Assigned, WorkOrderStatus.Completed, false)]
    [InlineData(WorkOrderStatus.Assigned, WorkOrderStatus.New, false)]
    [InlineData(WorkOrderStatus.Completed, WorkOrderStatus.New, false)]
    [InlineData(WorkOrderStatus.Completed, WorkOrderStatus.InProgress, false)]
    public void IsValidTransition_MatchesTheDefinedWorkflow(WorkOrderStatus from, WorkOrderStatus to, bool expected)
    {
        Assert.Equal(expected, WorkOrderStatusWorkflow.IsValidTransition(from, to));
    }
}
