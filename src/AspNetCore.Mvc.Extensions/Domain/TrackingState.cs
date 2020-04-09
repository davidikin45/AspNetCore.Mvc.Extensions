namespace AspNetCore.Mvc.Extensions.Domain
{
    public enum TrackingState
    {
        Unchanged = 0,
        Added = 1,
        Modified = 2,
        Deleted = 3
    }
}

//foreach (var appointment in schedule.Appointments)
// {
//    if (appointment.State == TrackingState.Added)
//    {
//        _context.Entry(appointment).State = EntityState.Added;
//    }
//    if (appointment.State == TrackingState.Modified)
//    {
//        _context.Entry(appointment).State = EntityState.Modified;
//    }
//    if (appointment.State == TrackingState.Deleted)
//    {
//        _context.Entry(appointment).State = EntityState.Deleted;
//    }
//}