using System.Linq;
using System;
namespace Queries
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new PlutoContext();

            /***************************************************************************/
            //// w/LINQ there are 2 ways to get data - LINQ syntax and extension methods
            //// LINQ syntax
            //var query =
            //    from c in context.Courses
            //    where c.Name.Contains("c#")
            //    orderby c.Name
            //    select c;

            //foreach (var course in query)
            //    System.Console.WriteLine(course.Name);
            //System.Console.WriteLine();

            //// Extension methods
            //var courses = context.Courses
            //    .Where(c => c.Name.Contains("c#"))
            //    .OrderBy(c => c.Name);

            //foreach (var course in courses)
            //    System.Console.WriteLine(course.Name);
            /***************************************************************************/


            /*************************  LINQ Syntax  ***********************************/
            /***************************************************************************/

            // SELECTING, filtering, and ordering courses
            var query1 =
                from c in context.Courses
                where c.Author.Id == 1
                orderby c.Level descending, c.Name
                select c;

            /***************************************************************************/

            // PROJECTION - might be used for optimization - instead of returning
            // the entire object just return the data needed
            var query2 =
                from c in context.Courses
                where c.Author.Id == 1
                orderby c.Level descending, c.Name
                select new { Name = c.Name, Author = c.Author.Name };

            /***************************************************************************/

            // GROUPING - group by operator in LINQ that is different than the one in SQL
            // In SQL use of 'group by' is w/aggregate functions like Count, sum, max, etc...
            // In LINQ 'group by' is used to break down a list of objects into 1+ groups
            // this query returns a list of groups
            var query3 =
                from c in context.Courses
                group c by c.Level into g
                select g;

            // display courses in each group by group key
            //foreach (var group in query3)
            //{
            //    System.Console.WriteLine(group.Key);

            //    foreach (var course in group)
            //        System.Console.WriteLine("\t{0}", course.Name);
            //}


            //// using an aggregate function with groups
            //// display all levels and the # of courses in each
            //foreach (var group in query3)
            //{
            //    System.Console.WriteLine("{0}, ({1})", group.Key, group.Count());
            //}

            /***************************************************************************/

            // JOINING - join 2 tables together to display some data from each

            // Inner Join - in LINQ use the navigation properties of the entities to display
            // related properties
            // display list of courses w/their authors - LINQ provider at runtime automatically
            // translates it into an inner join in SQL
            var query4 =
                from c in context.Courses
                select new { CourseName = c.Name, AuthorName = c.Author.Name };

            // INNER join - use when 2 entities do not have a navigation property
            // use inner join to link a course w/its author
            var query5 =
                from c in context.Courses
                join a in context.Authors on c.AuthorId equals a.Id
                select new { CourseName = c.Name, AuthorName = a.Name };

            // GROUP join - associate an object from one list with 1+ objects in another list
            // such as a list of authors associated with courses 
            // useful when you would use a left join and aggregate function in SQL
            var query6 =
                from a in context.Authors
                join c in context.Courses on a.Id equals c.AuthorId into g
                select new { AuthorName = a.Name, Courses = g.Count() };

            //foreach ( var x in query6)
            //    System.Console.WriteLine("{0} ({1})", x.AuthorName, x.Courses);

            // CROSS join - same as cross join in SQL
            // Ex - get a list of authors and cross join with courses
            // Will be a full combo of every author and every course
            var query7 =
                from a in context.Authors
                from c in context.Courses
                select new { AuthorName = a.Name, CourseName = c.Name };

            //foreach (var x in query7)
            //    System.Console.WriteLine("{0} - {1}", x.AuthorName, x.CourseName);

            /***************************************************************************/


            /************************  LINQ Extension Methods  *************************/
            /***************************************************************************/

            // RESTRICTION
            // EX - get all courses in level one
            var courses = context.Courses.Where(c => c.Level == 1);

            /***************************************************************************/

            // ORDERING
            var courses2 = context.Courses
                .Where(c => c.Level == 1)
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Level);

            /***************************************************************************/

            // PROJECTION w/select
            var courses3 = context.Courses
                .Where(c => c.Level == 1)
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Level)
                // call select method and project objects into a different type
                .Select(c => new { CourseName = c.Name, AuthorName = c.Name });

            // PROJECTION w/select many - used to flatten a hierarchical object
            // using 'select' will return a list of lists of tags
            // flatten it with 'select many' - will end up w/a flat list of tags 
            var tags = context.Courses
                .Where(c => c.Level == 1)
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Level)
                .SelectMany(c => c.Tags);

            //foreach (var t in tags)
            //{
            //    System.Console.WriteLine(t.Name);
            //}

            /***************************************************************************/

            // SET OPERATIONS
            var tags2 = context.Courses
                .Where(c => c.Level == 1)
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Level)
                .SelectMany(c => c.Tags)
                // prevents what???
                .Distinct();

            /***************************************************************************/

            // GROUPING
            // breaking courses into groups by LEVEL - each has its own KEY
            var groups = context.Courses
                .GroupBy(c => c.Level);

            foreach (var group in groups)
            {
                Console.WriteLine("key: " + group.Key);
                foreach (var course in group)
                    Console.WriteLine("\t" + course.Name);
            }
            Console.ReadKey();

            /***************************************************************************/

            // JOINING

            // Inner Join
            // use when there is no relationshsip btw objects and you need to relate them
            // EX - Join Courses with Authors 
            // 1st ARG - the list you want to join Courses with - the list of Authors
            // 2nd ARG - Func<Course, TKey> - what property of the course class do we want to join with Authors?
            // 3rd ARG - Func<Author, int> - pass an Author and return an integer which is the key in Author used for joining
            // Using AuthorId on Course and Id on Author to join these together
            // 4th ARG - Func<Course, Author, TResult> - pass a Course obj, Author obj, and returns the join result
            // use the Course and Author objects to set the values of the properties CourseName and AuthorName
            context.Courses.Join(context.Authors, 
                c => c.AuthorId, 
                a => a.Id, 
                (course, author) => new
                    {
                        CourseName = course.Name,
                        AuthorName = author.Name
                    });

            // GROUP Join
            // useful for situations (in SQL) when doing a left join, w/aggregate func, and groupBy
            // EX - get all authors and count their courses
            // 1st ARG - IEnumerable<TInner> - target list you want to do a group join to (Courses)
            // 2nd ARG - Func<Author, TKey> - a key in the author entity 
            // 3rd ARG - Func<Course, int> - key in Courses that corresponds to the key in the previous argument
            // What should happen once the records are matched? Each object in the first list is mathced 
            //  w/1+ objects from the 2nd list
            // 4th ARG - specifies what should happen once objects are matched - 
            //  Func<Author, IEnurmerable<Course>, TResult> - takes 2 args and returns result
            context.Authors.GroupJoin(context.Courses, 
                a => a.Id, 
                c => c.AuthorId, 
                (author, courses1) => new
                    {
                        AuthorName = author.Name,
                        Courses = courses1.Count()
                    });


            // CROSS Join
            // join every combination of the elements of 2 lists
            // (a,b) X (1,2,3) = (a1, a2, a3, b1, b2, b3)
            // There is no 'cross join' in extension methods - must use SelectMany
            // EX - get a list of authors and courses and combine them
            // 1st ARG - Func<Author, IEnumerable<TResult> - don't select a property from Author
            //  instead return all courses in the database
            // 2nd ARG - Func<Author, Course, TResult> - create lambda expression that takes an 
            //  author and course that is one combination and returns a result
            context.Authors.SelectMany(a => context.Courses, 
                (author, course) => new
                    {
                        AuthorName = author.Name,
                        CourseName = course.Name
                    });


            /*************  Other Methods Not Supported By LINQ Syntax  *****************/
            /***************************************************************************/

            // PARTITIONING
            // useful when you want to return a page of records
            // EX - display 'courses' in pages of size 10
            // return courses in the second page
            var coursesPage2 = context.Courses.Skip(10).Take(10);

            // ELEMENT OPERATORS
            // useful for returning a single object or the first object in a list

            // first record in a table (throws an exception if the table is empty)
            context.Courses.OrderBy(c => c.Level).First(c => c.FullPrice > 100);

            // first course w/price > 100 (returns null if there are no records in the target source)
            context.Courses.OrderBy(c => c.Level).FirstOrDefault(c => c.FullPrice > 100);

            // cannot use last when quering database b/c there is no SQL operator to get the last 
            // record in a table
            // sort in a descending way first, then select the first record 
            context.Courses.Last();
            context.Courses.LastOrDefault();

            // single returns one object based on given argument, otherwise throws exception
            // if the condition you supply returns multiple records, single also throws exception
            // singleOrDefault does the same but returns null if no object is found
            context.Courses.Single(c => c.Id == 1);
            context.Courses.SingleOrDefault(c => c.Id == 2);

            // QUANTIFYING
            bool allAbove10Dollars = context.Courses.All(c => c.FullPrice > 10);
            bool anyCoursesInLevelOne = context.Courses.Any(c => c.Level == 1);

            // AGGREGATING
            // Count, average, sum, max, min
            var numOfCoursesInLevelOne = context.Courses.Where(c => c.Level == 1).Count();
            var mostExpensiveCourse = context.Courses.Max(c => c.FullPrice);
            var leastExpensiveCourse = context.Courses.Min(c => c.FullPrice);
            var averageCourseCost = context.Courses.Average(c => c.FullPrice);


        }
    }
}
