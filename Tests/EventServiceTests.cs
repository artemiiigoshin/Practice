using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Practice.Controllers.DTO;
using Practice.DataAccess;
using Practice.Repositories;
using Practice.Service;

namespace Tests
{
    public class EventServiceTests
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IEventService _eventService;

        public EventServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IEventRepository, EventRepository>();

            services.AddScoped<IEventService, EventService>();

            _serviceProvider = services.BuildServiceProvider();
            _eventService = _serviceProvider.GetRequiredService<IEventService>();
        }

        [Fact]
        public async Task Create_Should_Add_Event()
        {
            var newEvent = new EventCreateDto(
                "Test Event",
                "Test Description",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10,
                10
                );

            var created = await _eventService.CreateAsync(newEvent);

            var result = await _eventService.GetAllAsync(new EventQueryParameters());

            Assert.Single(result.Items);
            Assert.Equal("Test Event", result.Items[0].Title);
        }

        [Fact]
        public async Task GetAll_Should_Return_All_Events()
        {
            var created = await _eventService.CreateAsync(new EventCreateDto(
                "Event 1",
                "Description 1",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10,
                10
                ));

            await _eventService.CreateAsync(new EventCreateDto(
                "Event 2",
                "Description 2",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10,
                10));

            var result = await _eventService.GetAllAsync(new EventQueryParameters());

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.ItemsCount);
            Assert.Equal(2, result.Items.Count);
        }

        #region GetById
        [Fact]
        public async Task GetById_Should_Return_Event_When_Event_Exists()
        {
            var created = await _eventService.CreateAsync(new EventCreateDto(
                "Test Event",
                "Test Description",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10,
                10));

            var result = await _eventService.GetByIdAsync(created.Id);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result!.Id);
            Assert.Equal("Test Event", result.Title);
            Assert.Equal("Test Description", result.Description);
        }

        [Fact]
        public async Task GetById_Should_Return_Null_When_Event_Does_Not_Exist()
        {
            var result = await _eventService.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }
        #endregion

        #region Update
        [Fact]
        public async Task Update_Should_Return_True_And_Update_Event_When_Exists()
        {
            var created = await _eventService.CreateAsync(new EventCreateDto(
                "Old Title",
                "Old Description",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10,
                10));

            var updatedEvent = new EventUpdateDto(
                created.Id,
                "New Title",
                "New Description",
                new DateTime(2025, 2, 1),
                new DateTime(2025, 2, 2),
                20);

            var result = await _eventService.UpdateAsync(updatedEvent);
            var fetched = await _eventService.GetByIdAsync(created.Id);

            Assert.True(result);
            Assert.NotNull(fetched);
            Assert.Equal("New Title", fetched!.Title);
            Assert.Equal("New Description", fetched.Description);
        }

        [Fact]
        public async Task Update_Should_Return_False_When_Event_Does_Not_Exist()
        {
            var updatedEvent = new EventUpdateDto(
                Guid.NewGuid(),
                "Doesn't matter",
                "Doesn't matter",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10);

            var result = await _eventService.UpdateAsync(updatedEvent);

            Assert.False(result);
        }
        #endregion

        #region Delete
        [Fact]
        public async Task Delete_Should_Return_True_And_Remove_Event_When_Exists()
        {
            var created = await _eventService.CreateAsync(new EventCreateDto(
                "Event to delete",
                "Description",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10,
                10));

            var result = await _eventService.DeleteAsync(created.Id);
            var fetched = await _eventService.GetByIdAsync(created.Id);
            var allEvents = await _eventService.GetAllAsync(new EventQueryParameters());

            Assert.True(result);
            Assert.Null(fetched);
            Assert.Equal(0, allEvents.TotalCount);
            Assert.Equal(0, allEvents.ItemsCount);
            Assert.Empty(allEvents.Items);
        }

        [Fact]
        public async Task Delete_Should_Return_False_When_Event_Does_Not_Exist()
        {
            var result = await _eventService.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
        }
        #endregion

        #region Filter
        [Fact]
        public async Task GetAll_Should_Filter_By_Title()
        {
            await _eventService.CreateAsync(new EventCreateDto(
                "Name 1",
                "Description 1",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10,
                10));

            await _eventService.CreateAsync(new EventCreateDto(
                "Name 2 target",
                "Description 2",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10,
                10));

            await _eventService.CreateAsync(new EventCreateDto(
                "Name 3",
                "Description 3",
                new DateTime(2025, 1, 1),
                new DateTime(2025, 1, 2),
                10,
                10));

            var query = new EventQueryParameters
            {
                Title = "tar",
                Page = 1,
                PageSize = 10
            };

            var result = await _eventService.GetAllAsync(query);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.ItemsCount);
            Assert.Single(result.Items);
            Assert.Equal("Name 2 target", result.Items[0].Title);
        }


        [Fact]
        public async Task GetAll_Should_Return_Requested_Page()
        {
            for (int i = 1; i <= 25; i++)
            {
                await _eventService.CreateAsync(new EventCreateDto(
                    $"Event {i}",
                    "Description",
                    new DateTime(2025, 1, 1),
                    new DateTime(2025, 1, 2),
                    10,
                    10));
            }

            var query = new EventQueryParameters
            {
                Page = 2,
                PageSize = 10
            };

            var result = await _eventService.GetAllAsync(query);

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(10, result.ItemsCount);
            Assert.Equal(10, result.Items.Count);
            Assert.Equal("Event 11", result.Items[0].Title);
            Assert.Equal("Event 20", result.Items[9].Title);
        }

        [Fact]
        public async Task GetAll_Should_Filter_By_Date_Range()
        {
            await _eventService.CreateAsync(new EventCreateDto(
                "Old Event",
                "Description 1",
                new DateTime(2025, 1, 10),
                new DateTime(2025, 1, 10),
                10,
                10));

            await _eventService.CreateAsync(new EventCreateDto(
                "Target Event",
                "Description 2",
                new DateTime(2025, 2, 15),
                new DateTime(2025, 2, 15),
                10,
                10));

            await _eventService.CreateAsync(new EventCreateDto(
                "Future Event",
                "Description 3",
                new DateTime(2025, 3, 20),
                new DateTime(2025, 3, 20),
                10,
                10));

            var query = new EventQueryParameters
            {
                From = new DateTime(2025, 2, 1),
                To = new DateTime(2025, 2, 28),
                Page = 1,
                PageSize = 10
            };

            var result = await _eventService.GetAllAsync(query);

            Assert.Single(result.Items);
            Assert.Equal("Target Event", result.Items[0].Title);
        }

        [Fact]
        public async Task GetAll_Should_Apply_Combined_Filters_Correctly()
        {
            await _eventService.CreateAsync(new EventCreateDto(
                "Title 1",
                "Description 1",
                new DateTime(2025, 4, 10),
                new DateTime(2025, 4, 10),
                10,
                10));

            await _eventService.CreateAsync(new EventCreateDto(
                "Title 2",
                "Description 2",
                new DateTime(2025, 4, 15),
                new DateTime(2025, 4, 15),
                10,
                10));

            await _eventService.CreateAsync(new EventCreateDto(
                "Name",
                "Description 3",
                new DateTime(2025, 5, 15),
                new DateTime(2025, 5, 15),
                10,
                10));

            var query = new EventQueryParameters
            {
                Title = "Title",
                From = new DateTime(2025, 4, 12),
                To = new DateTime(2025, 4, 30),
                Page = 1,
                PageSize = 10
            };

            var result = await _eventService.GetAllAsync(query);

            Assert.Single(result.Items);
            Assert.Equal("Title 2", result.Items[0].Title);
        }
        #endregion
    }
}
