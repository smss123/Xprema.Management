using Microsoft.EntityFrameworkCore;
using Xprema.Framework.Entities.Common;

namespace Xprema.Framework.tests.Enitiies.Common
{
    public class BaseRepositoryTests
    {
        private int RandomId() => new Random().Next(1, 1000);
        private class TestEntity : BaseEntity<int>
        {
        }

        private class TestDbContext : DbContext
        {
            public DbSet<TestEntity> TestEntities { get; set; }

            public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
            {
            }
        }

        private class TestRepository : BaseRepository<TestEntity, int>
        {
            public TestRepository(DbContext context) : base(context)
            {
            }
        }

        [Fact]
        public async Task AddAsync_ShouldAddEntity()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using var context = new TestDbContext(options);
            var repository = new TestRepository(context);

            var id = RandomId();
            var entity = new TestEntity { Id = id, CreatedBy = "User1", CreatedDate = DateTime.UtcNow };
            await repository.AddAsync(entity, "User1");

            var addedEntity = await context.TestEntities.FirstOrDefaultAsync(e => e.Id == id);
            Assert.NotNull(addedEntity);
            Assert.Equal("User1", addedEntity.CreatedBy);
        }

        [Fact]
        public async Task GetAll_ShouldReturnEntities()
        {
            var options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            await using var context = new TestDbContext(options);
            var repository = new TestRepository(context);
var id = RandomId();
var id1 = RandomId();
            var entity1 = new TestEntity { Id =id , CreatedBy = "User1", CreatedDate = DateTime.UtcNow };
            var entity2 = new TestEntity
                { Id = id1, CreatedBy = "User2", CreatedDate = DateTime.UtcNow, IsDeleted = true };
            await context.TestEntities.AddRangeAsync(entity1, entity2);
            await context.SaveChangesAsync();

            var entities = repository.GetAll(null).ToList();
            Assert.Equal(2,entities.Count);
            Assert.Equal("User1", entities[0].CreatedBy);
        }


    [Fact]
public async Task FirstOrDefaultAsync_ShouldReturnEntity()
{
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

    using var context = new TestDbContext(options);
    var repository = new TestRepository(context);
var id = RandomId();
    var entity = new TestEntity { Id = id, CreatedBy = "User1", CreatedDate = DateTime.UtcNow };
    await context.TestEntities.AddAsync(entity);
    await context.SaveChangesAsync();

    var result = await repository.FirstOrDefaultAsync(e => e.Id == id);
    Assert.NotNull(result);
    Assert.Equal("User1", result.CreatedBy);
}

[Fact]
public async Task AddAsync_ShouldAddHistoryRecord()
{
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

    await using var context = new TestDbContext(options);
    var repository = new TestRepository(context);
    var id = RandomId();
    var entity = new TestEntity { Id =id, CreatedBy = "User1", CreatedDate = DateTime.UtcNow };
    await repository.AddAsync(entity, "User1");

    var addedEntity = await context.TestEntities.FirstOrDefaultAsync(e => e.Id == id);
    Assert.NotNull(addedEntity);
    Assert.Single(addedEntity.HistoryRecords);
    Assert.Equal("User1", addedEntity.HistoryRecords[0].ChangedBy);
    Assert.Equal("Created", addedEntity.HistoryRecords[0].ChangeType);
}

[Fact]
public async Task UpdateAsync_ShouldAddHistoryRecord()
{
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

    using var context = new TestDbContext(options);
    var repository = new TestRepository(context);

    var id = RandomId();
    var entity = new TestEntity { Id =  id, CreatedBy = "User1", CreatedDate = DateTime.UtcNow };
    await context.TestEntities.AddAsync(entity);
    await context.SaveChangesAsync();

    entity.ModifiedBy = "User2";
    entity.ModifiedDate = DateTime.UtcNow;
    await repository.UpdateAsync(entity, "User2");

    var updatedEntity = await context.TestEntities.FirstOrDefaultAsync(e => e.Id == id);
    Assert.NotNull(updatedEntity);
    Assert.Equal(2, updatedEntity.HistoryRecords.Count);
    Assert.Equal("User2", updatedEntity.HistoryRecords[1].ChangedBy);
    Assert.Equal("Updated", updatedEntity.HistoryRecords[1].ChangeType);
}

[Fact]
public async Task DeleteAsync_ShouldAddHistoryRecord()
{
    var options = new DbContextOptionsBuilder<TestDbContext>()
        .UseInMemoryDatabase(databaseName: "TestDatabase")
        .Options;

    using var context = new TestDbContext(options);
    var repository = new TestRepository(context);

    var id = RandomId();
    var entity = new TestEntity { Id = id, CreatedBy = "User1", CreatedDate = DateTime.UtcNow };
    await context.TestEntities.AddAsync(entity);
    await context.SaveChangesAsync();

    await repository.DeleteAsync(entity, "User3");
await repository.DeleteAsync(entity, "User3");

    var deletedEntity = await context.TestEntities.IgnoreQueryFilters().Include(baseEntity => baseEntity.HistoryRecords)
        .FirstOrDefaultAsync(e => e.Id == id);
    Assert.NotNull(deletedEntity);
    Assert.True(deletedEntity.IsDeleted);
    Assert.Equal("User3", deletedEntity.DeletedBy);
    Assert.Equal(2, deletedEntity.HistoryRecords.Count);
    Assert.Equal("User3", deletedEntity.HistoryRecords[1].ChangedBy);
    Assert.Equal("Deleted", deletedEntity.HistoryRecords[1].ChangeType);
}

    }
}
