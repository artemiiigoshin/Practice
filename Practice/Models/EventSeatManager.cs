using Microsoft.EntityFrameworkCore;
using Practice.Models;

namespace Practice.Service
{
    public static class EventSeatManager
    {
        public static bool TryReserveSeats(Event? evt, int count = 1)
        {
            if (evt is null)
                return false;

            if (count <= 0)
                throw new ArgumentException("Количество мест должно быть больше нуля.");

            if (evt.AvailableSeats < count)
                return false;

            evt.AvailableSeats -= count;

            return true;
        }

        public static bool ReleaseSeats(Event? evt, int count = 1)
        {
            if (evt is null)
                return false;

            if (count <= 0)
                throw new ArgumentException("Количество мест должно быть больше нуля.");

            evt.AvailableSeats += count;

            if (evt.AvailableSeats > evt.TotalSeats)
                throw new InvalidOperationException("AvailableSeats не может быть больше TotalSeats.");

            return true;
        }
    }   
}