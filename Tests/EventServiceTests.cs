using Practice.Controllers.DTO;
using Practice.Models;
using Practice.Service;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public class EventServiceTests
    {
        [Fact]
        public void Create_Should_Add_Event()
        {
            var service = new EventService();

            var newEvent = new Event
            {
                Title = "Test Event",
                Description = "Test Description",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            };

            var created = service.Create(newEvent);

            var result = service.GetAll(new EventQueryParameters());

            Assert.Single(result.Items);
            Assert.Equal("Test Event", result.Items[0].Title);
        }

        [Fact]
        public void GetAll_Should_Return_All_Events()
        {
            var service = new EventService();

            service.Create(new Event
            {
                Title = "Event 1",
                Description = "Description 1",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            });

            service.Create(new Event
            {
                Title = "Event 2",
                Description = "Description 2",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            });

            var result = service.GetAll(new EventQueryParameters());

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.ItemsCount);
            Assert.Equal(2, result.Items.Count);
        }

        #region GetById
        [Fact]
        public void GetById_Should_Return_Event_When_Event_Exists()
        {
            var service = new EventService();

            var created = service.Create(new Event
            {
                Title = "Test Event",
                Description = "Test Description",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            });

            var result = service.GetById(created.Id);

            Assert.NotNull(result);
            Assert.Equal(created.Id, result!.Id);
            Assert.Equal("Test Event", result.Title);
            Assert.Equal("Test Description", result.Description);
        }

        [Fact]
        public void GetById_Should_Return_Null_When_Event_Does_Not_Exist()
        {
            var service = new EventService();

            var result = service.GetById(Guid.NewGuid());

            Assert.Null(result);
        }
        #endregion

        #region Update
        [Fact]
        public void Update_Should_Return_True_And_Update_Event_When_Exists()
        {
            var service = new EventService();

            var created = service.Create(new Event
            {
                Title = "Old Title",
                Description = "Old Description",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            });

            var updatedEvent = new Event
            {
                Id = created.Id,
                Title = "New Title",
                Description = "New Description",
                StartAt = new DateTime(2025, 2, 1),
                EndAt = new DateTime(2025, 2, 2)
            };

            var result = service.Update(updatedEvent);
            var fetched = service.GetById(created.Id);

            Assert.True(result);
            Assert.NotNull(fetched);
            Assert.Equal("New Title", fetched!.Title);
            Assert.Equal("New Description", fetched.Description);
        }

        [Fact]
        public void Update_Should_Return_False_When_Event_Does_Not_Exist()
        {
            var service = new EventService();

            var updatedEvent = new Event
            {
                Id = Guid.NewGuid(),
                Title = "Doesn't matter",
                Description = "Doesn't matter",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            };

            var result = service.Update(updatedEvent);

            Assert.False(result);
        }
        #endregion

        #region Delete
        [Fact]
        public void Delete_Should_Return_True_And_Remove_Event_When_Exists()
        {
            var service = new EventService();

            var created = service.Create(new Event
            {
                Title = "Event to delete",
                Description = "Description",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            });

            var result = service.Delete(created.Id);
            var fetched = service.GetById(created.Id);
            var allEvents = service.GetAll(new EventQueryParameters());

            Assert.True(result);
            Assert.Null(fetched);
            Assert.Equal(0, allEvents.TotalCount);
            Assert.Equal(0, allEvents.ItemsCount);
            Assert.Empty(allEvents.Items);
        }

        [Fact]
        public void Delete_Should_Return_False_When_Event_Does_Not_Exist()
        {
            var service = new EventService();

            var result = service.Delete(Guid.NewGuid());

            Assert.False(result);
        }
        #endregion

        #region Filter
        [Fact]
        public void GetAll_Should_Filter_By_Title()
        {
            var service = new EventService();

            service.Create(new Event
            {
                Title = "Name 1",
                Description = "Description 1",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            });

            service.Create(new Event
            {
                Title = "Name 2 target",
                Description = "Description 2",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            });

            service.Create(new Event
            {
                Title = "Name 3",
                Description = "Description 3",
                StartAt = new DateTime(2025, 1, 1),
                EndAt = new DateTime(2025, 1, 2)
            });

            var query = new EventQueryParameters
            {
                Title = "tar",
                Page = 1,
                PageSize = 10
            };

            var result = service.GetAll(query);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal(1, result.ItemsCount);
            Assert.Single(result.Items);
            Assert.Equal("Name 2 target", result.Items[0].Title);
        }


        [Fact]
        public void GetAll_Should_Return_Requested_Page()
        {
            var service = new EventService();

            for (int i = 1; i <= 25; i++)
            {
                service.Create(new Event
                {
                    Title = $"Event {i}",
                    Description = $"Description",
                    StartAt = new DateTime(2025, 1, 1),
                    EndAt = new DateTime(2025, 1, 2)
                });
            }

            var query = new EventQueryParameters
            {
                Page = 2,
                PageSize = 10
            };

            var result = service.GetAll(query);

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(2, result.Page);
            Assert.Equal(10, result.PageSize);
            Assert.Equal(10, result.ItemsCount);
            Assert.Equal(10, result.Items.Count);
            Assert.Equal("Event 11", result.Items[0].Title);
            Assert.Equal("Event 20", result.Items[9].Title);
        }

        [Fact]
        public void GetAll_Should_Filter_By_Date_Range()
        {
            var service = new EventService();

            service.Create(new Event
            {
                Title = "Old Event",
                Description = "Description 1",
                StartAt = new DateTime(2025, 1, 10),
                EndAt = new DateTime(2025, 1, 10)
            });

            service.Create(new Event
            {
                Title = "Target Event",
                Description = "Description 2",
                StartAt = new DateTime(2025, 2, 15),
                EndAt = new DateTime(2025, 2, 15)
            });

            service.Create(new Event
            {
                Title = "Future Event",
                Description = "Description 3",
                StartAt = new DateTime(2025, 3, 20),
                EndAt = new DateTime(2025, 3, 20)
            });

            var query = new EventQueryParameters
            {
                From = new DateTime(2025, 2, 1),
                To = new DateTime(2025, 2, 28),
                Page = 1,
                PageSize = 10
            };

            var result = service.GetAll(query);

            Assert.Single(result.Items);
            Assert.Equal("Target Event", result.Items[0].Title);
        }

        [Fact]
        public void GetAll_Should_Apply_Combined_Filters_Correctly()
        {
            var service = new EventService();

            service.Create(new Event
            {
                Title = "Title 1",
                Description = "Description 1",
                StartAt = new DateTime(2025, 4, 10),
                EndAt = new DateTime(2025, 4, 10)
            });

            service.Create(new Event
            {
                Title = "Title 2",
                Description = "Description 2",
                StartAt = new DateTime(2025, 4, 15),
                EndAt = new DateTime(2025, 4, 15)
            });

            service.Create(new Event
            {
                Title = "Name",
                Description = "Description 3",
                StartAt = new DateTime(2025, 4, 20),
                EndAt = new DateTime(2025, 4, 20)
            });

            var query = new EventQueryParameters
            {
                Title = "Title",
                From = new DateTime(2025, 4, 12),
                To = new DateTime(2025, 4, 30),
                Page = 1,
                PageSize = 10
            };

            var result = service.GetAll(query);

            Assert.Single(result.Items);
            Assert.Equal("Title 2", result.Items[0].Title);
        }
        #endregion
    }
}
