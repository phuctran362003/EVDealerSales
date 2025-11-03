namespace EVDealerSales.DataAccess.Entities
{
    public class Feedback : BaseEntity
    {
        // The user who gave the feedback (customer)
        public Guid CustomerId { get; set; }
        public User Customer { get; set; }

        // Optional reference to an order
        public Guid? OrderId { get; set; }
        public Order Order { get; set; }

        public string Content { get; set; }

        // The user (staff/manager) who created this record in the system (for internal use)
        public Guid? CreatedBy { get; set; }
        public User Creator { get; set; }

        // The user (manager) who resolved or responded to the feedback
        public Guid? ResolvedBy { get; set; }
        public User Resolver { get; set; }
    }
}
