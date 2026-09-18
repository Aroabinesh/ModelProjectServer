namespace ModelProject.Entities.Enums;

// Allowed forward transitions: New -> Assigned -> InProgress -> Completed.
// See Services/WorkOrders/WorkOrderStatusWorkflow.cs for the enforced transition table.
public enum WorkOrderStatus
{
    New = 0,
    Assigned = 1,
    InProgress = 2,
    Completed = 3
}
