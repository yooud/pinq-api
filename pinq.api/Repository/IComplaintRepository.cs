using pinq.api.Models.Entities;

namespace pinq.api.Repository;

public interface IComplaintRepository
{
    public Task<Complaint> CreateComplaintAsync(Complaint complaint);
}