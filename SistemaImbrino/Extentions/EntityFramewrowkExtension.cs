using System.Collections.Generic;


namespace System.Linq
{
    public static class EntityFramewrowkExtension
    {
        public static int page { get; set; }
        public static int limit { get; set; }

        public static List<TSource> ToPagination<TSource>(this IEnumerable<TSource> source, int page, int limit) 
        {
            if (page == 0)
                page = 1;

            if (limit == 0)
                limit = int.MaxValue;
            var skip = (page - 1) * limit;
            var result = source.Skip(skip).Take(limit).ToList();
            return result;
        }

        public static List<TSource> ToPagination<TSource>(this IEnumerable<TSource> source)
        {
            if (page == 0)
                page = 1;

            if (limit == 0)
                limit = int.MaxValue;
            var skip = (page - 1) * limit;
            var result = source.Skip(skip).Take(limit).ToList();
            return result;
        }
    }
}