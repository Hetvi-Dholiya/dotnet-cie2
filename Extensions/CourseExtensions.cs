using _091.Models;

namespace _091.Extensions
{
    public static class CourseExtensions
    {
        public static decimal CalculateFinalFees(this Course course)
        {
            if (course == null) return 0;
            return course.Fees - (course.Fees * (course.DiscountPercentage / 100m));
        }
    }
}