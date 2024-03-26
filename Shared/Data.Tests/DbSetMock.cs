using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Data.Tests
{
    public static class DbSetMock
    {
        public static Mock<DbSet<T>> Create<T>(IEnumerable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            var queryableData = data.AsQueryable();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryableData.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());

            mockSet.Setup(set => set.FindAsync(It.IsAny<object>())).Returns((object key) =>
            {
                var result = data.FirstOrDefault(x => x.Equals(key));
                return Task.FromResult(result);
            });
            return mockSet;
        }
    }
}
